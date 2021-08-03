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
		private readonly ILogger<Diagnoser> _logger;

		public Diagnoser(IInterfaceStore interfaceStore, IMockText mockText, IIndentation indentation, ILogger<Diagnoser> logger)
		{
			_interfaceStore = interfaceStore;
			_mockText = mockText;
			_indentation = indentation;
			_logger = logger;
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
				_logger.LogDebug($"{item.Identifier.Uri} does not appear to be a test file");
				watch.StopAndLogDebug(_logger, $"discarding {item.Identifier.Uri} for not being a test file took: ");
				return new List<Diagnostic>();
			}

			watch.StopAndLogDebug(_logger, $"deciding that {item.Identifier.Uri} is a test file took: ");
			watch.Restart();

			// use roslyn to find any diagnostics for the file,
			// will also do some of the heavy lifting for us on line location/range

			var tree = CSharpSyntaxTree.ParseText(item.Text);
			var root = tree.GetCompilationUnitRoot();

			var compilation = CSharpCompilation
				.Create(null)
				.AddSyntaxTrees(tree);

			watch.StopAndLogDebug(_logger, $"building syntax tree for {item.Identifier.Uri} took: ");
			watch.Restart();

			var diagnostics = compilation
				.GetDiagnostics()
				.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
				.Select(x =>
				{
					// Location refers to the substring indices of the text document
					var candidateInterface = item.Text.Substring(x.Location.SourceSpan.Start, x.Location.SourceSpan.Length);
					
					// use this to calculate the range in the document where we want to eventually
					// request our TextEdit when publishing a QuickFix from CodeActionHandler
					var roslynRange = x.Location.GetLineSpan();
					return new
					{
						candidateInterface,
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

			
			var publishableDiagnostics = diagnostics
				.Where(candidate => _interfaceStore.Exists(candidate.candidateInterface)) // can't gen text if the interface hasn't been loaded
				.Select(loadable =>
				{
					var config = _indentation.GetIndentationConfig(item.Text, loadable.diagnosticRange);
					var mockedText = _mockText.GetMockText(loadable.candidateInterface, config);

					return new Diagnostic
					(
						loadable.diagnosticRange,
						DiagnosticSeverity.Error,
						Constants.DiagnosticCode_CanMoq,
						Constants.DiagnosticSource,
						Constants.MessagesByDiagnosticCode[Constants.DiagnosticCode_CanMoq],
						GetEdits(loadable.diagnosticRange, mockedText, lines)
					);
				})
				.ToList()
				;


			if(publishableDiagnostics.Count > 0)
				_logger.LogDebug("Diagnostics we're going to publish:");

			foreach(var diagnostic in publishableDiagnostics)
			{
				_logger.LogDebug(JsonSerializer.Serialize(diagnostic));
			}
			
			watch.StopAndLogDebug(_logger, $"calculating diagnostics from syntax tree for {item.Identifier.Uri} took: ");
			return publishableDiagnostics;
		}

		private List<TextEdit> GetEdits(Model.Lsp.Range diagnosticRange, string mockedText, List<string> trimmedLines)
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