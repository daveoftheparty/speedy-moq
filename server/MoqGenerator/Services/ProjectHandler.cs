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

			var path = uri.Replace("file:///", "");
			_logger.LogInformation($"method {nameof(GetCsProjFromCsFile)} is looking for a .csproj related to file {path}");
			
			path = Path.GetDirectoryName(path);
			while(!string.IsNullOrWhiteSpace(path))
			{
				_logger.LogInformation($"method {nameof(GetCsProjFromCsFile)} is searching for a .csproj in {path}");
				
				var csProjFile = Directory
					.EnumerateFiles(path)
					.Select(f => new FileInfo(f))
					.FirstOrDefault(x => x.Extension == ".csproj") // I have no idea why there would be more than one csproj in a directory, or if it's even possible
					;

				if(csProjFile != null)
				{
					watch.StopAndLogDebug(_logger, "time to find csproj file: ");
					_logger.LogInformation($"method {nameof(GetCsProjFromCsFile)} found {csProjFile}");
					return csProjFile.FullName;
				}

				path = Directory.GetParent(path).FullName;
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