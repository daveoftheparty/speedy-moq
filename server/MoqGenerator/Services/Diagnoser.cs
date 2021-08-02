using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoqGenerator.Interfaces.Lsp;
using MoqGenerator.Model.Lsp;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace MoqGenerator.Services
{
	public class Diagnoser : IDiagnoser
	{
		private readonly IInterfaceStore _interfaceStore;
		private readonly IMockText _mockText;
		private readonly IIndentation _indentation;

		public Diagnoser(IInterfaceStore interfaceStore, IMockText mockText, IIndentation indentation)
		{
			_interfaceStore = interfaceStore;
			_mockText = mockText;
			_indentation = indentation;
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
			var lines = SplitToTrimmedLines(item.Text).ToList();

			// check if we're dealing with a test file, otherwise bail early...
			if(!lines.Any(line => _testFrameworks.Contains(line)))
				return new List<Diagnostic>();

			// use roslyn to find any diagnostics for the file,
			// will also do some of the heavy lifting for us on line location/range

			var tree = CSharpSyntaxTree.ParseText(item.Text);
			var root = tree.GetCompilationUnitRoot();

			var compilation = CSharpCompilation
				.Create(null)
				.AddSyntaxTrees(tree);

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
				;

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


#warning DRY... combine this logic with the dupe StringReader logic in this sln and extract to own class...
		private IEnumerable<string> SplitToTrimmedLines(string input)
		{
			if (input == null)
			{
				yield break;
			}

			using (StringReader reader = new StringReader(input))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					yield return line.Trim();
				}
			}
		}
	}
}