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
			{
				_logger.LogWarning($"{nameof(GetInterfaceDefinitionByNamespace)} failed to lookup interfaceName {interfaceName}.");
				LogInterfacesLoaded(nameof(GetInterfaceDefinitionByNamespace));
			}

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

		public InterfaceStore(
			ILogger<InterfaceStore> logger,
			IWhoaCowboy whoaCowboy,
			IProjectHandler projectHandler,
			IUriHandler uriHandler)
		{
			_logger = logger;
			_logger.LogInformation($"hello from {nameof(InterfaceStore)}:{_thisInstance} ctor...");
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
			watch.StopAndLogInformation(_logger, "(single file) time to get semantic models: ");

			// load our interface dict...
			watch.Restart();
			var definitions = GetDefinitionsByNamespaceByInterface(new List<SemanticModel> { model }, textDocItem.Identifier);
			watch.StopAndLogInformation(_logger, "(single file) time to get interface definitions from semantic models: ");

			UpdateMainDictionary(definitions);

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

			watch.StopAndLogInformation(_logger, "time to get semantic models: ");

			// load our interface dict...
			watch.Restart();
			var definitions = GetDefinitionsByNamespaceByInterface(models.ToList(), textDocId);
			watch.StopAndLogInformation(_logger, "time to get interface definitions from semantic models: ");

			UpdateMainDictionary(definitions);

			// and finally, load up our hashset so that we don't have to do this again
			foreach(var proj in projectsAdded)
				_csProjectsAlreadyLoaded.Add(proj);

			if(definitions.Count > 0)
				LogDefinitions(nameof(LoadCSProjAsync));
		}


		private Dictionary<string, Dictionary<string, InterfaceDefinition>> GetMethods(List<SemanticModel> models, string uriAsFile)
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
						{
							var parentSymbol = node.Model.GetDeclaredSymbol(method.Parent);
							var interfaceName = parentSymbol.Name;
							var typeArgs = GetInterfaceTypeArguments(interfaceName, node.Model, method.Parent);

							return new
							{
								Namespace = parentSymbol.ContainingSymbol?.ToString(),
								InterfaceName = typeArgs?.InterfaceNameKey ?? interfaceName,
								TypeArguments = typeArgs,
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
									})
							};
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
								namespaceGroup.First().TypeArguments,
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
								new List<string>(), // Properties, to be filled in by GetProperties()
								null // Indexer to be filled in by GetIndexer()
							)
						})
						.ToDictionary(nsPair => nsPair.Namespace, nsPair => nsPair.InterfaceDefinition)
				})
				.ToDictionary(pair => pair.InterfaceName, pair => pair.NamespaceDict);

			return dict;
		}


		private Dictionary<string, Dictionary<string, InterfaceDefinition>> GetProperties(List<SemanticModel> models, string uriAsFile)
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
						.Select(prop => 
						{
							var parentSymbol = node.Model.GetDeclaredSymbol(prop.Parent);
							var interfaceName = parentSymbol.Name;
							var typeArgs = GetInterfaceTypeArguments(interfaceName, node.Model, prop.Parent);

							return new
							{
								Namespace = parentSymbol.ContainingSymbol?.ToString(),
								InterfaceName = typeArgs?.InterfaceNameKey ?? interfaceName,
								TypeArguments = typeArgs,
								SourceFile = string.IsNullOrWhiteSpace(prop?.Parent.SyntaxTree.FilePath) ? uriAsFile : prop.Parent.SyntaxTree.FilePath,
								PropertyName = prop.Identifier.Text
							};
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
								namespaceGroup.First().TypeArguments,
								namespaceGroup.First().SourceFile,
								new List<InterfaceMethod>(), // methods to be filled in by GetMethods()
								namespaceGroup.Select(p => p.PropertyName).ToList(),
								null // Indexer to be filled in by GetIndexer
							)
						})
						.ToDictionary(nsPair => nsPair.Namespace, nsPair => nsPair.InterfaceDefinition)

				})
				.ToDictionary(pair => pair.InterfaceName, pair => pair.NamespaceDict);
			
			return dict;
		}


		private Dictionary<string, Dictionary<string, InterfaceDefinition>> GetIndexer(List<SemanticModel> models, string uriAsFile)
		{
			var interfaceGroups = models
				.Select(semanticModel => new
				{
					Model = semanticModel,
					Indexer = semanticModel
						.SyntaxTree
						.GetRoot()
						.DescendantNodes()
						.OfType<IndexerDeclarationSyntax>()
						.Where(meth => meth.Parent.GetType() == typeof(InterfaceDeclarationSyntax))

				})
				.SelectMany(node => 
					node
						.Indexer
						.Select(indexer => 
						{
							var parentSymbol = node.Model.GetDeclaredSymbol(indexer.Parent);
							var interfaceName = parentSymbol.Name;
							var typeArgs = GetInterfaceTypeArguments(interfaceName, node.Model, indexer.Parent);

							return new 
							{
								Namespace = parentSymbol.ContainingSymbol?.ToString(),
								InterfaceName = typeArgs?.InterfaceNameKey ?? interfaceName,
								TypeArguments = typeArgs,
								SourceFile = string.IsNullOrWhiteSpace(indexer.Parent?.SyntaxTree.FilePath) ? uriAsFile : indexer.Parent?.SyntaxTree.FilePath,
								KeyType = indexer.ParameterList.Parameters.First().Type.ToString(),
								ReturnType = indexer.Type.ToString(),
								HasGet = indexer.AccessorList.Accessors.Any(a => ((AccessorDeclarationSyntax)a).Keyword.Text == "get"),
								HasSet = indexer.AccessorList.Accessors.Any(a => ((AccessorDeclarationSyntax)a).Keyword.Text == "set"),
							};
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
								namespaceGroup.First().TypeArguments,
								namespaceGroup.First().SourceFile,
								new List<InterfaceMethod>(), // methods to be filled in by GetMethods()
								new List<string>(), // Properties to be filled in by GetProperties()
								new InterfaceIndexer
								(
									namespaceGroup.Select(x => x.KeyType).First(),
									namespaceGroup.Select(x => x.ReturnType).First(),
									namespaceGroup.Select(x => x.HasGet).First(),
									namespaceGroup.Select(x => x.HasSet).First()
								)
							)
						})
						.ToDictionary(nsPair => nsPair.Namespace, nsPair => nsPair.InterfaceDefinition)

				})
				.ToDictionary(pair => pair.InterfaceName, pair => pair.NamespaceDict);
			
			return dict;
		}

		private InterfaceGenerics GetInterfaceTypeArguments(string interfaceName, SemanticModel model, SyntaxNode member)
		{
			var typeArgs = ((INamedTypeSymbol)model.GetDeclaredSymbol(member))?
				.TypeArguments
				.Select(t => t.Name)
				.ToList();

			return typeArgs.Count == 0
				? null
				: new InterfaceGenerics(interfaceName, typeArgs);
		}

		private Dictionary<string, Dictionary<string, InterfaceDefinition>> GetDefinitionsByNamespaceByInterface(List<SemanticModel> models, TextDocumentIdentifier textDocId)
		{
			var uriAsFile = _uriHandler.GetFilePath(textDocId);

			var methods = GetMethods(models, uriAsFile);
			var properties = GetProperties(models, uriAsFile);
			var indexer = GetIndexer(models, uriAsFile);

			// merge the dictionaries
			var result = new Dictionary<string, Dictionary<string, InterfaceDefinition>>(methods);
			MergeDictionary(result, properties, MergeType.Properties);
			MergeDictionary(result, indexer, MergeType.Indexer);
			return result;
		}

		private enum MergeType
		{
			Properties,
			Indexer
		}

		private void MergeDictionary(
			Dictionary<string, Dictionary<string, InterfaceDefinition>> result,
			IReadOnlyDictionary<string, Dictionary<string, InterfaceDefinition>> incoming,
			MergeType mergeType
			)
		{
			foreach(var interfaceKey in incoming.Keys)
			{
				if(result.TryGetValue(interfaceKey, out var resultDefByNamespace))
					result[interfaceKey] = MergeNamespaceDictionaries(resultDefByNamespace, incoming[interfaceKey], mergeType);
				else
					result[interfaceKey] = incoming[interfaceKey];
			}
		}


		private Dictionary<string, InterfaceDefinition> MergeNamespaceDictionaries(
			Dictionary<string, InterfaceDefinition> resultDefByNamespace,
			Dictionary<string, InterfaceDefinition> incomingDefByNamespace,
			MergeType mergeType)
		{
			var result = new Dictionary<string, InterfaceDefinition>();

			foreach (var namespaceKey in incomingDefByNamespace.Keys)
			{
				if(resultDefByNamespace.TryGetValue(namespaceKey, out var resultDef))
				{
					switch (mergeType)
					{
						case MergeType.Properties:
							result[namespaceKey] = resultDef with { Properties = incomingDefByNamespace[namespaceKey].Properties };
							break;
						case MergeType.Indexer:
							result[namespaceKey] = resultDef with { Indexer = incomingDefByNamespace[namespaceKey].Indexer };
							break;
					}
				}
				else
				{
					result[namespaceKey] = incomingDefByNamespace[namespaceKey];
				}
			}

			return result;
		}


		private void UpdateMainDictionary(Dictionary<string, Dictionary<string, InterfaceDefinition>> definitions)
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

			buildWatch.StopAndLogInformation(_logger, "time to build projs to get refs: ");
			addToWorkspaceWatch.StopAndLogInformation(_logger, "time to add projs to workspace: ");
			
			return (projectsAdded, workspace);
		}


		private void LogInterfacesLoaded(string who)
		{
			_logger.LogInformation($"{nameof(LogInterfacesLoaded)} was called by {who}. Interfaces loaded:");

			_definitionsByNameSpaceByInterface
				.SelectMany(entry => entry
					.Value
					.Select(nsPair => $"\t\t\t\t{nsPair.Key}.{entry.Key} : {nsPair.Value.SourceFile}")
				)
				.ToList()
				.ForEach(message => _logger.LogInformation(message))
				;
		}

		private void LogDefinitions(string who)
		{
			_logger.LogInformation($"{nameof(LogDefinitions)} was called by {who}. CsProjs loaded:");
			_csProjectsAlreadyLoaded
				.Select(proj => $"\t\t\t\t{proj}")
				.ToList()
				.ForEach(message => _logger.LogInformation(message));

			LogInterfacesLoaded(who);
		}
	}
}