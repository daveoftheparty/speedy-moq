using System.Collections.Generic;
using System.IO;
using System.Linq;
using Features.Interfaces.Lsp;
using Features.Model.Lsp;
using Microsoft.CodeAnalysis.CSharp;

namespace Features.MoqGenerator
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

		public IEnumerable<Diagnostic> GetDiagnostics(TextDocumentItem item)
		{
			/*
				do we need to store a dict of the TextDocIdentifier, or are we always guaranteed to get a new version?

				first, see if we're dealing with a "test" file-- because if we aren't, we should bail before
				trying to fire up the roslyn libraries

				if we are dealing with a test file, perform our analysis
			*/

			var lines = SplitToTrimmedLines(item.Text).ToList();

			// check if we're dealing with a test file
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
						Diagnostic = new Diagnostic
						(
							new Range
							(
								new Position((uint)roslynRange.StartLinePosition.Line, (uint)roslynRange.StartLinePosition.Character),
								new Position((uint)roslynRange.EndLinePosition.Line, (uint)roslynRange.EndLinePosition.Character)
							),
							DiagnosticSeverity.Error,
							Constants.DiagnosticCode_CanMoq,
							Constants.DiagnosticSource,
							Constants.MessagesByDiagnosticCode[Constants.DiagnosticCode_CanMoq],
							candidateInterface
						)
					};
				})
				.Where(isEntireLine => isEntireLine.Diagnostic.Data == lines[(int)isEntireLine.Diagnostic.Range.start.line])
				.GroupBy(candidate => candidate.candidateInterface)
				.Select(grp => grp.First().Diagnostic)
				;

			
			var publishableDiagnostics = diagnostics
				.Where(candidate => _interfaceStore.Exists(candidate.Data)) // can't gen text if the interface hasn't been loaded
				.Select(loadable =>
				{
					var config = _indentation.GetIndentationConfig(item.Text, loadable.Range);
					var newText = _mockText.GetMockText(loadable.Data, config);
					
					// we're gonna reset the range start char to 0 so that the first
					// line of our generated text doesn't get double-indented

					return loadable with
					{
						Data = newText,
						Range = new Range
						(
							new Position(loadable.Range.start.line, 0),
							loadable.Range.end
						)
					};
				})
				;

			return publishableDiagnostics;
		}

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