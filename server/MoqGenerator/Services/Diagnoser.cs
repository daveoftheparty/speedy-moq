using System.Collections.Generic;
using System.Linq;
using MoqGenerator.Interfaces.Lsp;
using MoqGenerator.Model.Lsp;
using Microsoft.CodeAnalysis.CSharp;
using System;
using MoqGenerator.Util;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Diagnostics;

namespace MoqGenerator.Services
{
	public class Diagnoser : IDiagnoser
	{
		private readonly IInterfaceStore _interfaceStore;
		private readonly IMockText _mockText;
		private readonly IIndentation _indentation;
		private readonly IInterfaceGenericsBuilder _genericsBuilder;
		private readonly ILogger<Diagnoser> _logger;

		public Diagnoser(
			IInterfaceStore interfaceStore,
			IMockText mockText,
			IIndentation indentation,
			IInterfaceGenericsBuilder genericsBuilder,
			ILogger<Diagnoser> logger)
		{
			_interfaceStore = interfaceStore;
			_mockText = mockText;
			_indentation = indentation;
			_logger = logger;
			_genericsBuilder = genericsBuilder;
		}

		private readonly HashSet<string> _testFrameworks = new()
		{
			"using NUnit.Framework;",
			"using Xunit;",
			"using Microsoft.VisualStudio.TestTools.UnitTesting;"
		};

		private readonly List<string> _requiredImports = new()
		{
			"using System;",
			"using System.Linq.Expressions;",
			"using Moq;",
		};

		public IEnumerable<Diagnostic> GetDiagnostics(TextDocumentItem item)
		{
			var watch = new Stopwatch();
			watch.Start();

			var splitter = new TextLineSplitter();
			var lines = splitter
				.SplitToLines(item.Text)
				.Select(line => line.Trim())
				.ToList();

			// check if we're dealing with a test file, otherwise bail early...
			if(!lines.Any(line => _testFrameworks.Contains(line)))
			{
				_logger.LogInformation($"{item.Identifier.Uri} does not appear to be a test file");
				watch.StopAndLogInformation(_logger, $"discarding {item.Identifier.Uri} for not being a test file took: ");
				return new List<Diagnostic>();
			}

			watch.StopAndLogInformation(_logger, $"deciding that {item.Identifier.Uri} is a test file took: ");
			watch.Restart();

			// use roslyn to find any diagnostics for the file,
			// will also do some of the heavy lifting for us on line location/range

			var tree = CSharpSyntaxTree.ParseText(item.Text);

			var compilation = CSharpCompilation
				.Create(null)
				.AddSyntaxTrees(tree);

			watch.StopAndLogInformation(_logger, $"building syntax tree for {item.Identifier.Uri} took: ");
			watch.Restart();

			var generics = _genericsBuilder.BuildFast(compilation, tree);
			watch.StopAndLogInformation(_logger, $"building InterfaceGenerics for {item.Identifier.Uri} took: ");
			watch.Restart();

			var diagnostics = compilation
				.GetDiagnostics()
				.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
				.Select(x =>
				{
					// check for a generic:
					var generic = generics
						.Values
						.FirstOrDefault(v => v.Location == x.Location.SourceSpan);
					

					// Location refers to the substring indices of the text document
					var candidateInterface = item.Text.Substring(x.Location.SourceSpan.Start, x.Location.SourceSpan.Length);
					
					// use this to calculate the range in the document where we want to eventually
					// request our TextEdit when publishing a QuickFix from CodeActionHandler
					var roslynRange = x.Location.GetLineSpan();
					return new
					{
						candidateInterface,
						genericInterface = generic.Generics?.InterfaceNameKey,
						diagnosticRange = new Model.Lsp.Range
							(
								new Position((uint)roslynRange.StartLinePosition.Line, (uint)roslynRange.StartLinePosition.Character),
								new Position((uint)roslynRange.EndLinePosition.Line, (uint)roslynRange.EndLinePosition.Character)
							)
					};
				})
				.Where(isEntireLine => isEntireLine.candidateInterface == lines[(int)isEntireLine.diagnosticRange.start.line])
				.GroupBy(candidate => candidate.candidateInterface)
				.Select(grp => grp.First())
				;

#warning either in Linq statement above, or here, match up our location with the generics builder stuff so we can pass the right stuff to interfaceStore.exists()
			
			var publishableDiagnostics = diagnostics
				.Where(candidate => 
					_interfaceStore.Exists(candidate.candidateInterface) ||
					_interfaceStore.Exists(candidate.genericInterface)
				) // can't gen text if the interface hasn't been loaded
				.Select(loadable =>
				{
					var config = _indentation.GetIndentationConfig(item.Text, loadable.diagnosticRange.start.line);
					var mockedTextByNamespace = _mockText.GetMockTextByNamespace(loadable.candidateInterface, config);

					var dataDict = mockedTextByNamespace
						.Select(pair => new
						{
							NamespaceName = pair.Key,
							Text = GetEdits(loadable.diagnosticRange, pair.Value, lines)
						})
						.ToDictionary(textByNs => textByNs.NamespaceName, textByNs => textByNs.Text);

					return new Diagnostic
					(
						loadable.diagnosticRange,
						DiagnosticSeverity.Error,
						Constants.DiagnosticCode_CanMoq,
						Constants.DiagnosticSource,
						Constants.MessagesByDiagnosticCode[Constants.DiagnosticCode_CanMoq],
						dataDict
					);
				})
				.ToList()
				;


			if(publishableDiagnostics.Count > 0)
				_logger.LogInformation("Diagnostics we're going to publish:");

			foreach(var diagnostic in publishableDiagnostics)
			{
				_logger.LogInformation(JsonSerializer.Serialize(diagnostic));
			}
			
			watch.StopAndLogInformation(_logger, $"calculating diagnostics from syntax tree for {item.Identifier.Uri} took: ");
			return publishableDiagnostics;
		}

		private IReadOnlyList<TextEdit> GetEdits(Model.Lsp.Range diagnosticRange, string mockedText, List<string> trimmedLines)
		{
			var result = new List<TextEdit>
			{
				new TextEdit
				(
					new Model.Lsp.Range
					(
						// we're gonna reset the range start char to 0 so that the first
						// line of our generated text doesn't get double-indented
						new Position(diagnosticRange.start.line, 0),
						diagnosticRange.end
					),
					mockedText
				)
			};


			// check if we need to import any of our usings
			var imports = string.Join(
					Environment.NewLine,
					_requiredImports
						.Where(r => !trimmedLines.Contains(r))
					);
			
			if(!string.IsNullOrWhiteSpace(imports))
			{
				result.Add
				(
					new TextEdit(
						new Model.Lsp.Range(
							new Position(0, 0),
							new Position(0, 0)
						),
						$"{imports}{Environment.NewLine}"
					)
				);
			}

			return result;
		}
	}
}