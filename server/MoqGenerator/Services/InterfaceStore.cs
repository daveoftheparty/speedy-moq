using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

using Buildalyzer;
using Buildalyzer.Workspaces;

using MoqGenerator.Interfaces.Lsp;
using MoqGenerator.Model;
using MoqGenerator.Model.Lsp;
using MoqGenerator.Util;

namespace MoqGenerator.Services
{
	public class InterfaceStore : IInterfaceStore
	{
		#region IInterfaceStore
		public InterfaceDefinition GetInterfaceDefinition(string interfaceName)
		{
			_definitionsByInterfaceName.TryGetValue(interfaceName, out var result);
			_logger.LogDebug($"{nameof(GetInterfaceDefinition)} is looking for interfaceName {interfaceName}." +
				$" Interfaces loaded: {string.Join('|', _definitionsByInterfaceName.Keys)}." +
				$" Interface Source file:{result?.SourceFile ?? "not found"}");
			return result;
		}

		public async Task LoadDefinitionsIfNecessaryAsync(TextDocumentItem textDocItem)
		{
			var csProjTask = LoadCsProjAsyncIfNecessaryAsync(textDocItem);
			var csInterfaceFileTask = LoadCsInterfaceIfNecessaryAsync(textDocItem);
			await Task.WhenAll(csProjTask, csInterfaceFileTask);
		}

		public bool Exists(string interfaceName) => _definitionsByInterfaceName.ContainsKey(interfaceName);

		#endregion IInterfaceStore

		private readonly Dictionary<string, InterfaceDefinition> _definitionsByInterfaceName = new();
		private readonly HashSet<string> _csProjectsAlreadyLoaded = new();
		private readonly ILogger<InterfaceStore> _logger;
		private readonly string _thisInstance = Guid.NewGuid().ToString();

		public InterfaceStore(ILogger<InterfaceStore> logger)
		{
			_logger = logger;
			_logger.LogTrace($"hello from {nameof(InterfaceStore)}:{_thisInstance} ctor...");
		}


		#warning here's an interesting question. If I open a folder in VSCode that already has a .cs file open, will we get a notification for textDocument/didOpen!?!?!
		#warning change some/all LogInformation to LogTraces later... want it to show in messages for now while I'm actively debugging

		private async Task LoadCsInterfaceIfNecessaryAsync(TextDocumentItem textDocItem)
		{
			// this file may or may not contain interface definitions, and there's no way to tell until we parse it

			// we might want to convert this method to a Task.Run() so it can actually be run in parallel...

			var watch = new Stopwatch();
			watch.Start();

			var tree = CSharpSyntaxTree.ParseText(textDocItem.Text);
			var root = tree.GetCompilationUnitRoot();
			var compilation = CSharpCompilation
				.Create(null)
				.AddSyntaxTrees(tree);
			
			var model = compilation.GetSemanticModel(tree);
			watch.StopAndLogDebug(_logger, "(single file) time to get semantic models: ");

			// load our interface dict...
			watch.Restart();
			var definitions = GetInterfaceDefinitionsByName(new List<SemanticModel> { model });
			watch.StopAndLogDebug(_logger, "(single file) time to get interface definitions from semantic models: ");

			foreach(var definition in definitions)
			{
				_definitionsByInterfaceName[definition.Key] = definition.Value;
			}

			if(definitions.Count > 0)
			{
				_logger.LogInformation($"method {nameof(LoadCsInterfaceIfNecessaryAsync)} loaded for uri {textDocItem.Identifier.Uri}");
				LogDefinitions(nameof(LoadCsInterfaceIfNecessaryAsync));
			}
			
			await Task.CompletedTask;
		}


		private async Task LoadCsProjAsyncIfNecessaryAsync(TextDocumentItem textDocItem)
		{
			// the csProj may or may not have been loaded already. if it's in our HashSet, no reason to load,
			// because we'll detect changes on individual files with the LoadCsInterfaceIfNecessaryAsync method

			var csProjPath = GetCsProjFromCsFile(textDocItem.Identifier.Uri);
			if(csProjPath != null && !_csProjectsAlreadyLoaded.Contains(csProjPath))
				await LoadCSProjAsync(csProjPath);
		}


		private void AddToWorkspace(AnalyzerManager manager, AdhocWorkspace workspace, Queue<string> projects, HashSet<string> projectsAdded,
			Stopwatch buildWatch, Stopwatch addToWorkspaceWatch)
		{
			if(projects.Count == 0)
				return;

			var projectName = projects.Dequeue();

			if(!projectsAdded.Contains(projectName))
			{
				buildWatch.Start();
				var project = manager.GetProject(projectName);
				var buildResult = project.Build();

				buildResult
					.SelectMany(br => br.ProjectReferences)
					.ToList()
					.ForEach(pr => projects.Enqueue(pr))
					;
				buildWatch.Stop();

				addToWorkspaceWatch.Start();
				project.AddToWorkspace(workspace);
				addToWorkspaceWatch.Stop();

				projectsAdded.Add(projectName);
			}
			
			AddToWorkspace(manager, workspace, projects, projectsAdded, buildWatch, addToWorkspaceWatch);
		}

		private async Task LoadCSProjAsync(string csProjPath)
		{
			var manager = new AnalyzerManager();
			var workspace = new AdhocWorkspace();

			var projectQueue = new Queue<string>(new[] {csProjPath});
			var projectsAdded = new HashSet<string>();
			var buildWatch = new Stopwatch();
			var addToWorkspaceWatch = new Stopwatch();
			AddToWorkspace(manager, workspace, projectQueue, projectsAdded, buildWatch, addToWorkspaceWatch);
			
			buildWatch.StopAndLogDebug(_logger, "time to build projs to get refs: ");
			addToWorkspaceWatch.StopAndLogDebug(_logger, "time to add projs to workspace: ");

			var watch = new Stopwatch();
			watch.Start();

			var compilations = await Task.WhenAll(workspace.CurrentSolution.Projects.Select(x => x.GetCompilationAsync()));

			var models = compilations
				.SelectMany(compilation => compilation.SyntaxTrees.Select(syntaxTree => compilation.GetSemanticModel(syntaxTree)))
				;

			watch.StopAndLogDebug(_logger, "time to get semantic models: ");

			// load our interface dict...
			watch.Restart();
			var definitions = GetInterfaceDefinitionsByName(models.ToList());
			watch.StopAndLogDebug(_logger, "time to get interface definitions from semantic models: ");

			foreach(var definition in definitions)
			{
				_definitionsByInterfaceName[definition.Key] = definition.Value;
			}

			// and finally, load up our hashset so that we don't have to do this again
			foreach(var proj in projectsAdded)
				_csProjectsAlreadyLoaded.Add(proj);

			if(definitions.Count > 0)
				LogDefinitions(nameof(LoadCSProjAsync));
		}


		private Dictionary<string, InterfaceDefinition> GetInterfaceDefinitionsByName(List<SemanticModel> models)
		{
			var methods = GetInterfaceMethods(models);
			var properties = GetInterfacePropertiesByName(models);

			// merge the two dictionaries
			var result = new Dictionary<string, InterfaceDefinition>();

			var allKeys = methods.Keys.Concat(properties.Keys);

			foreach (var key in allKeys)
			{
				if(methods.TryGetValue(key, out var methodDef))
				{
					if(properties.TryGetValue(key, out var propDef))
					{
						result[key] = methodDef with { Properties = propDef.Properties };
					}
					else
						result[key] = methodDef;
				}
				else
					result[key] = properties[key];
			}

			return result;
		}

		private Dictionary<string, InterfaceDefinition> GetInterfaceMethods(List<SemanticModel> models)
		{
			return models
				.SelectMany(
					semanticModel => semanticModel
						.SyntaxTree
						.GetRoot()
						.DescendantNodes()
						.OfType<MethodDeclarationSyntax>()
						.Where(meth => meth.Parent.GetType() == typeof(InterfaceDeclarationSyntax))
				)
				.Select(node => new
				{
					InterfaceName = ((InterfaceDeclarationSyntax)((SyntaxNode)node).Parent).Identifier.Text,
					SourceFile = node.Parent.SyntaxTree.FilePath,
					MethodName = node.Identifier.Text,
					ReturnType = node.ReturnType.ToString(),
					Parameters = node
						.ParameterList
						.Parameters
						.Select(param => new
						{
							ParameterType = param.Type.ToString(),
							ParameterName = param.Identifier.Text,
							ParameterDefinition = param.ToString()
						}
						)
				}
				)
				.GroupBy(x => x.InterfaceName)
				.Select(interfaceName => new
				{
					InterfaceName = interfaceName.Key,
					InterfaceDefinition = new InterfaceDefinition
					(
						interfaceName.Key,
						interfaceName.First().SourceFile,
						interfaceName
							.GroupBy(y => y.MethodName)
							.Select(method => new InterfaceMethod
							(
								method.Key,
								method.First().ReturnType,
								method.First().Parameters
									.Select(parameter => new InterfaceMethodParameter
									(
										parameter.ParameterType,
										parameter.ParameterName,
										parameter.ParameterDefinition
									))
									.ToList()
							))
							.ToList(),
						new List<string>()
					)
				})
				.ToDictionary(pair => pair.InterfaceName, pair => pair.InterfaceDefinition);
		}

		private Dictionary<string, InterfaceDefinition> GetInterfacePropertiesByName(List<SemanticModel> models)
		{
			return models
				.SelectMany(
					semanticModel => semanticModel
						.SyntaxTree
						.GetRoot()
						.DescendantNodes()
						.OfType<PropertyDeclarationSyntax>()
						.Where(meth => meth.Parent.GetType() == typeof(InterfaceDeclarationSyntax))
				)
				.Select(node => new
				{
					InterfaceName = ((InterfaceDeclarationSyntax)((SyntaxNode)node).Parent).Identifier.Text,
					SourceFile = node.Parent.SyntaxTree.FilePath,
					PropertyName = node.Identifier.Text
				}
				)
				.GroupBy(x => x.InterfaceName)
				.Select(interfaceName => new
				{
					InterfaceName = interfaceName.Key,
					InterfaceDefinition = new InterfaceDefinition
					(
						interfaceName.Key,
						interfaceName.First().SourceFile,
						new List<InterfaceMethod>(),
						interfaceName.Select(p => p.PropertyName).ToList()
					)
				})
				.ToDictionary(pair => pair.InterfaceName, pair => pair.InterfaceDefinition);
		}

		private string GetCsProjFromCsFile(string uri)
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


		private void LogDefinitions(string v)
		{
			_logger.LogInformation($"{nameof(LogDefinitions)} was called by {v}. CsProjs loaded: {string.Join('|', _csProjectsAlreadyLoaded)}");
			_logger.LogInformation($"{nameof(LogDefinitions)} was called by {v}. Interfaces loaded: {string.Join('|', _definitionsByInterfaceName.Keys)}");
		}
	}
}