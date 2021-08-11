using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using MoqGenerator.Interfaces.Lsp;
using MoqGenerator.Util;

namespace MoqGenerator.Services
{
	public class ProjectHandler : IProjectHandler
	{
		private readonly Dictionary<string, string> _csProjByFile = new();

		private readonly ILogger<ProjectHandler> _logger;

		public ProjectHandler(ILogger<ProjectHandler> logger)
		{
			_logger = logger;
		}


		public string GetCsProjFromCsFile(string uri)
		{
			var watch = new Stopwatch();
			watch.Start();

			// whelp, we're gonna hack at this, don't know if there is a better way. look for a csproj in the same directory as this file,
			// if we can't find one, traverse backwards until we do... or don't

			var currPath = uri.Replace("file:///", "");
			var filePath = currPath;
			if(_csProjByFile.TryGetValue(currPath, out var proj))
			{
				watch.StopAndLogInformation(_logger, $"method {nameof(GetCsProjFromCsFile)} found previously resolved {proj} for lookup file {currPath}");
				return proj;
			}

			_logger.LogInformation($"method {nameof(GetCsProjFromCsFile)} is looking for a .csproj related to file {currPath}");
			
			currPath = Path.GetDirectoryName(currPath);
			while(!string.IsNullOrWhiteSpace(currPath))
			{
				_logger.LogInformation($"method {nameof(GetCsProjFromCsFile)} is searching for a .csproj in {currPath}");
				
				var csProjFile = Directory
					.EnumerateFiles(currPath)
					.Select(f => new FileInfo(f))
					.FirstOrDefault(x => x.Extension == ".csproj") // I have no idea why there would be more than one csproj in a directory, or if it's even possible
					;

				if(csProjFile != null)
				{
					watch.StopAndLogInformation(_logger, $"method {nameof(GetCsProjFromCsFile)} found {csProjFile} in: ");
					
					var fullName = csProjFile.FullName;
					_csProjByFile.Add(filePath, csProjFile.FullName);
					return fullName;
				}

				currPath = Directory.GetParent(currPath).FullName;
			}

			_logger.LogError($"method {nameof(GetCsProjFromCsFile)} could not locate a parent .csproj file for {uri}");
			watch.StopAndLogError(_logger, "time that we FAILED to find csproj file: ");
			return null;
		}

		public IEnumerable<string> GetProjectAndProjectReferences(string csProjPath)
		{
			var results = new List<string>();

			var projects = new Queue<string>(new[] { csProjPath });

			while(projects.Count > 0)
			{
				var currProject = projects.Dequeue();
				results.Add(currProject);
				_logger.LogInformation($"Loading project references for project {currProject}");

				var root = XDocument.Load(currProject);
				root
					.Descendants()
					.Where(xl => xl.Name.LocalName == "ProjectReference")
					.Select(projRefPath => projRefPath.Attribute("Include").Value)
					.Select(refPath => 
					{
						if(Path.IsPathFullyQualified(refPath) && File.Exists(refPath))
							return refPath;
						
						
						var basePath = Path.GetDirectoryName(currProject);
						var fullPath = Path.GetFullPath(refPath, basePath);
						if(File.Exists(fullPath))
							return fullPath;

						var message =
							$"Having trouble building a fully qualified path. Incoming project: {csProjPath}" +
							$" Project currently processing: {currProject}" +
							$" Project we're trying to locate, because it's referenced by the current project: {refPath}" +
							$" FullPath we tried to build from the current project and the current reference: {fullPath}"
							;
						
						_logger.LogError(message);
						throw new Exception(message);
					})
					.ToList()
					.ForEach(pr => projects.Enqueue(pr));
			}

			return results;
		}
	}
}