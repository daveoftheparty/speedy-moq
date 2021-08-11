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

			if(result == null)
				_logger.LogWarning(
					$"{nameof(GetInterfaceDefinitionByNamespace)} is looking for interfaceName {interfaceName}." +
					$" Interfaces loaded: {{GetInterfacesForLogging()}}."
				);

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
		private readonly IUriHandler _uriHandler;

		public InterfaceStore(ILogger<InterfaceStore> logger, IWhoaCowboy whoaCowboy, IProjectHandler projectHandler, IUriHandler uriHandler)
		{
			_logger = logger;
			_logger.LogTrace($"hello from {nameof(InterfaceStore)}:{_thisInstance} ctor...");
			_whoaCowboy = whoaCowboy;
			_projectHandler = projectHandler;
			_uriHandler = uriHandler;
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
			var definitions = GetInterfaceDefinitionsByName(new List<SemanticModel> { model }, textDocItem.Identifier);
			watch.StopAndLogDebug(_logger, "(single file) time to get interface definitions from semantic models: ");

			UpdateInterfaceDictionary(definitions);

			if(definitions.Count > 0)
			{
				_logger.LogInformation($"method {nameof(LoadCsInterfaceIfNecessaryAsync)} loaded for uri {textDocItem.Identifier.Uri}");
				LogDefinitions(nameof(LoadCsInterfaceIfNecessaryAsync));
			}
			
			await Task.CompletedTask;
		}


		private async Task LoadCSProjAsync(string csProjPath, TextDocumentIdentifier textDocId)
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
			var definitions = GetInterfaceDefinitionsByName(models.ToList(), textDocId);
			watch.StopAndLogDebug(_logger, "time to get interface definitions from semantic models: ");

			UpdateInterfaceDictionary(definitions);

			// and finally, load up our hashset so that we don't have to do this again
			foreach(var proj in projectsAdded)
				_csProjectsAlreadyLoaded.Add(proj);

			if(definitions.Count > 0)
				LogDefinitions(nameof(LoadCSProjAsync));
		}


		private Dictionary<string, Dictionary<string, InterfaceDefinition>> GetInterfaceMethods(List<SemanticModel> models, string uriAsFile)
		{
			var interfaceGroups = models
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
								Namespace = node.Model.GetDeclaredSymbol(method.Parent)?.ContainingSymbol?.ToString(),
								InterfaceName = node.Model.GetDeclaredSymbol(method.Parent).Name,
								SourceFile = string.IsNullOrWhiteSpace(method?.Parent.SyntaxTree.FilePath) ? uriAsFile : method.Parent.SyntaxTree.FilePath,
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
				.GroupBy(x => x.InterfaceName)
				.ToList();

			var dict = interfaceGroups
				.Select(interfaceGroup => new
				{
					InterfaceName = interfaceGroup.Key,
					NamespaceDict = interfaceGroup
						.GroupBy(y => y.Namespace)
						.Select(namespaceGroup => new
						{
							Namespace = namespaceGroup.Key,
							InterfaceDefinition = new InterfaceDefinition
							(
								interfaceGroup.Key,
								namespaceGroup.First().SourceFile,
								namespaceGroup
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
						})
						.ToDictionary(nsPair => nsPair.Namespace, nsPair => nsPair.InterfaceDefinition)
				})
				.ToDictionary(pair => pair.InterfaceName, pair => pair.NamespaceDict);

			return dict;
		}


		private Dictionary<string, Dictionary<string, InterfaceDefinition>> GetInterfacePropertiesByName(List<SemanticModel> models, string uriAsFile)
		{
			var interfaceGroups = models
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
							Namespace = node.Model.GetDeclaredSymbol(prop.Parent)?.ContainingSymbol?.ToString(),
							InterfaceName = node.Model.GetDeclaredSymbol(prop.Parent).Name,
							SourceFile = string.IsNullOrWhiteSpace(prop?.Parent.SyntaxTree.FilePath) ? uriAsFile : prop.Parent.SyntaxTree.FilePath,
							PropertyName = prop.Identifier.Text
						})
				)
				.GroupBy(x => x.InterfaceName)
				.ToList();

			var dict = interfaceGroups
				.Select(interfaceGroup => new
				{
					InterfaceName = interfaceGroup.Key,
					NamespaceDict = interfaceGroup
						.GroupBy(y => y.Namespace)
						.Select(namespaceGroup => new
						{
							Namespace = namespaceGroup.Key,
							InterfaceDefinition = new InterfaceDefinition
							(
								interfaceGroup.Key,
								namespaceGroup.First().SourceFile,
								new List<InterfaceMethod>(), // methods to be filled in by other method...
								namespaceGroup.Select(p => p.PropertyName).ToList()
							)
						})
						.ToDictionary(nsPair => nsPair.Namespace, nsPair => nsPair.InterfaceDefinition)

				})
				.ToDictionary(pair => pair.InterfaceName, pair => pair.NamespaceDict);
			
			return dict;
		}


		private Dictionary<string, Dictionary<string, InterfaceDefinition>> GetInterfaceDefinitionsByName(List<SemanticModel> models, TextDocumentIdentifier textDocId)
		{
			var uriAsFile = _uriHandler.GetFilePath(textDocId);

			var methods = GetInterfaceMethods(models, uriAsFile);
			var properties = GetInterfacePropertiesByName(models, uriAsFile);

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


		private void UpdateInterfaceDictionary(Dictionary<string, Dictionary<string, InterfaceDefinition>> definitions)
		{
			foreach(var definition in definitions)
			{
				if(!_definitionsByNameSpaceByInterface.TryGetValue(definition.Key, out var masterNamespaceDict))
				{
					// this interface doesn't even exist in master dictionary, this incoming must be 'the truth'
					_definitionsByNameSpaceByInterface[definition.Key] = definition.Value;
				}
				else
				{
					// update individual namespace KeyValuePairs from incoming since they're now 'the truth'
					foreach(var definitionByNamespace in definition.Value)
					{
						_definitionsByNameSpaceByInterface[definition.Key][definitionByNamespace.Key] = definitionByNamespace.Value;
					}
				}
			}
		}

		private async Task LoadCsProjAsyncIfNecessaryAsync(TextDocumentItem textDocItem)
		{
			// the csProj may or may not have been loaded already. if it's in our HashSet, no reason to load,
			// because we'll detect changes on individual files with the LoadCsInterfaceIfNecessaryAsync method

			var csProjPath = _projectHandler.GetCsProjFromCsFile(textDocItem.Identifier);
			if(csProjPath != null && !_csProjectsAlreadyLoaded.Contains(csProjPath))
				await LoadCSProjAsync(csProjPath, textDocItem.Identifier);
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


		private string GetInterfacesForLogging()
		{
			return 
				Environment.NewLine + 
				string.Join(Environment.NewLine,
					_definitionsByNameSpaceByInterface
						.SelectMany(entry => entry
						.Value
						.Select(nsPair => $"{nsPair.Key}.{entry.Key} : {nsPair.Value.SourceFile}")
					)
				);
		}

		private void LogDefinitions(string v)
		{
			_logger.LogInformation($"{nameof(LogDefinitions)} was called by {v}. CsProjs loaded: {string.Join('|', _csProjectsAlreadyLoaded)}");
			_logger.LogInformation($"{nameof(LogDefinitions)} was called by {v}. Interfaces loaded: {GetInterfacesForLogging()}");
		}
	}
}