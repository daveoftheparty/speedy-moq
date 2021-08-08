using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		public Dictionary<string, InterfaceDefinition> GetInterfaceDefinitionByNamespace(string interfaceName)
		{
			_definitionsByNameSpaceByInterface.TryGetValue(interfaceName, out var result);
			#warning fix up this log statement:
			// _logger.LogDebug($"{nameof(GetInterfaceDefinitionByNamespace)} is looking for interfaceName {interfaceName}." +
			// 	$" Interfaces loaded: {string.Join('|', _definitionsByNameSpaceByInterface.Keys)}." +
			// 	$" Interface Source file:{result?.SourceFile ?? "not found"}");
			return result;
		}

		public async Task LoadDefinitionsIfNecessaryAsync(TextDocumentItem textDocItem)
		{
			if(!_whoaCowboy.GiddyUp)
				return;

			var csProjTask = LoadCsProjAsyncIfNecessaryAsync(textDocItem);
			var csInterfaceFileTask = LoadCsInterfaceIfNecessaryAsync(textDocItem);
			await Task.WhenAll(csProjTask, csInterfaceFileTask);
		}

		public bool Exists(string interfaceName) => _definitionsByNameSpaceByInterface.ContainsKey(interfaceName);

		#endregion IInterfaceStore

		private readonly Dictionary<string, Dictionary<string, InterfaceDefinition>> _definitionsByNameSpaceByInterface = new();
		private readonly HashSet<string> _csProjectsAlreadyLoaded = new();
		private readonly ILogger<InterfaceStore> _logger;
		private readonly string _thisInstance = Guid.NewGuid().ToString();
		private readonly IWhoaCowboy _whoaCowboy;
		private readonly IProjectHandler _projectHandler;

		public InterfaceStore(ILogger<InterfaceStore> logger, IWhoaCowboy whoaCowboy, IProjectHandler projectHandler)
		{
			_logger = logger;
			_logger.LogTrace($"hello from {nameof(InterfaceStore)}:{_thisInstance} ctor...");
			_whoaCowboy = whoaCowboy;
			_projectHandler = projectHandler;
		}

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
				_definitionsByNameSpaceByInterface[definition.Key] = definition.Value;
			}

			if(definitions.Count > 0)
			{
				_logger.LogInformation($"method {nameof(LoadCsInterfaceIfNecessaryAsync)} loaded for uri {textDocItem.Identifier.Uri}");
				LogDefinitions(nameof(LoadCsInterfaceIfNecessaryAsync));
			}
			
			await Task.CompletedTask;
		}


		private async Task LoadCSProjAsync(string csProjPath)
		{

			var (projectsAdded, workspace) = GetProjectsAndWorkspace(csProjPath);

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
				_definitionsByNameSpaceByInterface[definition.Key] = definition.Value;
			}

			// and finally, load up our hashset so that we don't have to do this again
			foreach(var proj in projectsAdded)
				_csProjectsAlreadyLoaded.Add(proj);

			if(definitions.Count > 0)
				LogDefinitions(nameof(LoadCSProjAsync));
		}


		private Dictionary<string, Dictionary<string, InterfaceDefinition>> GetInterfaceMethods(List<SemanticModel> models)
		{
			return models
				.Select(semanticModel => new
				{
					Model = semanticModel,
					Methods = semanticModel
						.SyntaxTree
						.GetRoot()
						.DescendantNodes()
						.OfType<MethodDeclarationSyntax>()
						.Where(meth => meth.Parent.GetType() == typeof(InterfaceDeclarationSyntax))
				})
				.SelectMany(node => 
					node
						.Methods
						.Select(method =>
							new
							{
								Namespace = node.Model.GetDeclaredSymbol(method).ContainingNamespace.Name,
								InterfaceName = node.Model.GetDeclaredSymbol(method.Parent).Name,
								SourceFile = method.Parent.SyntaxTree.FilePath,
								MethodName = method.Identifier.Text,
								ReturnType = method.ReturnType.ToString(),
								Parameters = method
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
				)
				.GroupBy(x => new { x.InterfaceName, x.Namespace })
				.Select(grouping => new
				{
					InterfaceName = grouping.Key.InterfaceName,
					NamespaceDict = grouping
					.GroupBy(justTheSpace => justTheSpace.Namespace)
						.ToDictionary(
							pair => pair.Key,
							pair => new InterfaceDefinition
							(
								grouping.Key.InterfaceName,
								grouping.First().SourceFile,
								grouping
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
								new List<string>() // Properties, to be filled in by other method...
							)

						)
				})
				.ToDictionary(pair => pair.InterfaceName, pair => pair.NamespaceDict);
		}


		private Dictionary<string, Dictionary<string, InterfaceDefinition>> GetInterfacePropertiesByName(List<SemanticModel> models)
		{
			// debuggin:
			// var firstModel = models.First();
			// var firstNode = firstModel
			// 	.SyntaxTree
			// 	.GetRoot()
			// 	.DescendantNodes()
			// 	.OfType<InterfaceDeclarationSyntax>()
			// 	.First();
			
			// this throws exception:
			// var namespaceName = firstModel.GetTypeInfo(firstNode).Type.ContainingNamespace.Name;

			/*
				this gets what we want...
				firstModel.GetDeclaredSymbol(firstNode).ContainingNamespace.Name => "Foo"
				firstModel.GetDeclaredSymbol(firstNode).Name => "IConfigProps"
				firstModel.GetDeclaredSymbol(firstNode).ToString() => "Foo.IConfigProps"
			*/

			return models
				.Select(semanticModel => new
				{
					Model = semanticModel,
					Properties = semanticModel
						.SyntaxTree
						.GetRoot()
						.DescendantNodes()
						.OfType<PropertyDeclarationSyntax>()
						.Where(meth => meth.Parent.GetType() == typeof(InterfaceDeclarationSyntax)),
					
				})
				.SelectMany(node => 
					node
						.Properties
						.Select(prop => new 
						{
							Namespace = node.Model.GetDeclaredSymbol(prop).ContainingNamespace.Name,
							InterfaceName = node.Model.GetDeclaredSymbol(prop.Parent).Name,
							SourceFile = prop.Parent.SyntaxTree.FilePath,
							PropertyName = prop.Identifier.Text
						})
				)
				.GroupBy(x => new { x.InterfaceName, x.Namespace })
				.Select(grouping => new
				{
					InterfaceName = grouping.Key.InterfaceName,
					NamespaceDict = grouping
						.GroupBy(justTheSpace => justTheSpace.Namespace)
						.ToDictionary(
							pair => pair.Key,
							pair => new InterfaceDefinition
							(
								grouping.Key.InterfaceName,
								grouping.First().SourceFile,
								new List<InterfaceMethod>(), // methods to be filled in by other method...
								grouping.Select(p => p.PropertyName).ToList()
							))
				})
				.ToDictionary(pair => pair.InterfaceName, pair => pair.NamespaceDict);
		}


		private Dictionary<string, Dictionary<string, InterfaceDefinition>> GetInterfaceDefinitionsByName(List<SemanticModel> models)
		{
			var methods = GetInterfaceMethods(models);
			var properties = GetInterfacePropertiesByName(models);

			// merge the two dictionaries
			var result = new Dictionary<string, Dictionary<string, InterfaceDefinition>>();

			

			var interfaceKeys = methods.Keys.Concat(properties.Keys);
			foreach(var interfaceKey in interfaceKeys)
			{
				if(methods.TryGetValue(interfaceKey, out var methodNamespaceDict))
				{
					if(properties.TryGetValue(interfaceKey, out var propertyNamespaceDict))
					{
						// merge namespace dictionaries
						result[interfaceKey] = MergeNamespaceDictionaries(methodNamespaceDict, propertyNamespaceDict);
					}
					else
					{
						result[interfaceKey] = methodNamespaceDict;
					}
				}
				else
					result[interfaceKey] = properties[interfaceKey];
			}
			return result;
		}

		private Dictionary<string, InterfaceDefinition> MergeNamespaceDictionaries(
			Dictionary<string, InterfaceDefinition> methodNamespaceDict,
			Dictionary<string, InterfaceDefinition> propertyNamespaceDict)
		{
			var result = new Dictionary<string, InterfaceDefinition>();

			var namespaceKeys = methodNamespaceDict.Keys.Concat(propertyNamespaceDict.Keys);

			foreach (var namespaceKey in namespaceKeys)
			{
				if(methodNamespaceDict.TryGetValue(namespaceKey, out var methodDef))
				{
					if(propertyNamespaceDict.TryGetValue(namespaceKey, out var propDef))
					{
						result[namespaceKey] = methodDef with { Properties = propDef.Properties };
					}
					else
						result[namespaceKey] = methodDef;
				}
				else
					result[namespaceKey] = propertyNamespaceDict[namespaceKey];
			}

			return result;
		}

		private async Task LoadCsProjAsyncIfNecessaryAsync(TextDocumentItem textDocItem)
		{
			// the csProj may or may not have been loaded already. if it's in our HashSet, no reason to load,
			// because we'll detect changes on individual files with the LoadCsInterfaceIfNecessaryAsync method

			var csProjPath = _projectHandler.GetCsProjFromCsFile(textDocItem.Identifier.Uri);
			if(csProjPath != null && !_csProjectsAlreadyLoaded.Contains(csProjPath))
				await LoadCSProjAsync(csProjPath);
		}


		private (IReadOnlyList<string> projects, AdhocWorkspace workspace) GetProjectsAndWorkspace(string csProjPath)
		{
			var manager = new AnalyzerManager();
			var workspace = new AdhocWorkspace();
			var projectsAdded = new List<string>();

			var buildWatch = new Stopwatch();
			var addToWorkspaceWatch = new Stopwatch();

			var projects = _projectHandler.GetProjectAndProjectReferences(csProjPath);

			foreach(var projectName in projects)
			{
				if(projectsAdded.Contains(projectName))
					break;

				buildWatch.Start();
				var project = manager.GetProject(projectName);
				buildWatch.Stop();

				addToWorkspaceWatch.Start();
				project.AddToWorkspace(workspace);
				addToWorkspaceWatch.Stop();

				projectsAdded.Add(projectName);
			}

			buildWatch.StopAndLogDebug(_logger, "time to build projs to get refs: ");
			addToWorkspaceWatch.StopAndLogDebug(_logger, "time to add projs to workspace: ");
			
			return (projectsAdded, workspace);
		}


		private void LogDefinitions(string v)
		{
			#warning this logging probably needs to change, and also, we should just dump the whole dict (serialized?)
			_logger.LogInformation($"{nameof(LogDefinitions)} was called by {v}. CsProjs loaded: {string.Join('|', _csProjectsAlreadyLoaded)}");
			_logger.LogInformation($"{nameof(LogDefinitions)} was called by {v}. Interfaces loaded: {string.Join('|', _definitionsByNameSpaceByInterface.Keys)}");
		}
	}
}