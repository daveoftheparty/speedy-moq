using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Buildalyzer;
using Buildalyzer.Workspaces;

using Features.Interfaces.Lsp;
using Features.Model;
using Features.Model.Lsp;

namespace Features.MoqGenerator
{
	public class InterfaceStore : IInterfaceStore
	{
		#region IInterfaceStore
		public InterfaceDefinition GetInterfaceDefinition(string interfaceName)
		{
			_definitionsByInterfaceName.TryGetValue(interfaceName, out var result);
			return result;
		}

		public async Task LoadDefinitionsIfNecessaryAsync(TextDocumentItem textDocItem)
		{
			var csProjTask = LoadCsProjAsyncIfNecessaryAsync(textDocItem);
			var csInterfaceFileTask = LoadCsInterfaceIfNecessaryAsync(textDocItem);
			await Task.WhenAll(csProjTask, csInterfaceFileTask);
		}


		#endregion IInterfaceStore

		private readonly Dictionary<string, InterfaceDefinition> _definitionsByInterfaceName = new();
		private readonly HashSet<string> _csProjectsAlreadyLoaded = new();


		private async Task LoadCsInterfaceIfNecessaryAsync(TextDocumentItem textDocItem)
		{
			// this file may or may not contain interface definitions...
			throw new NotImplementedException();
		}

		private async Task LoadCsProjAsyncIfNecessaryAsync(TextDocumentItem textDocItem)
		{
			// the csProj may or may not have been loaded already. if it's in our HashSet, no reason to load,
			// because we'll detect changes on individual files with the LoadCsInterfaceIfNecessaryAsync method
			throw new NotImplementedException();
		}


		#warning here's an interesting question. If I open a folder in VSCode that already has a .cs file open, will we get a notification for textDocument/didOpen!?!?!

		private string GetCsProjFromCsFile(string uri)
		{
			throw new NotImplementedException();
		}

		private async Task LoadCSProjAsync(string csProjPath)
		{
			// probably need to be called from the client the first time a .cs file is opened,
			// and again, each time a new .cs file is opened to see if it is attached to the same 
			// or new .csproj

			var manager = new AnalyzerManager();
			var analyzer = manager.GetProject(csProjPath);
			var workspace = new AdhocWorkspace();
			var project = analyzer.AddToWorkspace(workspace);
			
			var compilations = await Task.WhenAll(workspace.CurrentSolution.Projects.Select(x => x.GetCompilationAsync()));

			var definitions = compilations
				.SelectMany(compilation => compilation.SyntaxTrees.Select(syntaxTree => compilation.GetSemanticModel(syntaxTree)))
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
							.ToList()
					)
				});

				// load our interface dict...
				foreach(var definition in definitions)
				{
					_definitionsByInterfaceName[definition.InterfaceName] = definition.InterfaceDefinition;
				}

				// and finally, load up our hashset so that we don't have to do this again
				_csProjectsAlreadyLoaded.Add(csProjPath);
		}
	}
}