using System.Collections.Generic;
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

			
			var tree = CSharpSyntaxTree.ParseText(item.Text);
			var root = tree.GetCompilationUnitRoot();

			var isTestFile = false;
			foreach(var usingDirective in root.Usings)
			{
				// honestly, since we're just doing a string match, it may be faster to do an IndexOf()
				// on the incoming text before even declaring a syntax tree
				if(_testFrameworks.Contains(usingDirective.ToString()))
				{
					isTestFile = true;
					break;
				}
			}
			
			if(!isTestFile)
				return new List<Diagnostic>();

			var compilation = CSharpCompilation
				.Create(null)
				.AddSyntaxTrees(tree);

			var diagnostics = compilation
				.GetDiagnostics()
				.Where(d =>
					d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error
					&&
					(
						d.Id == "CS0103" // The name '{0}' does not exist in the current context (such as The name 'IMyInterface'...)
						||
						d.Id == "CS0119" // '{0}' is a {1}, which is not valid in the given context (such as 'IMyInterface' is a type...)
						||
						d.Id == "CS0246" // The type or namespace name '{0}' could not be found (are you missing a using directive or an assembly reference?)
						||
						d.Id == "CS0518" // Predefined type '{0}' is not defined or imported
					)
				)
				.Select(x =>
				{
					// Location refers to the substring indices of the text document
					// we need those to get the interfaceName, we also need them
					// to calculate the range in the document where we want to eventually
					// request our TextEdit when publishing a QuickFix from CodeActionHandler
					var roslynRange = x.Location.GetLineSpan();
					return new Diagnostic
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
						item.Text.Substring(x.Location.SourceSpan.Start, x.Location.SourceSpan.Length) // interfaceName
					);
				})
				.ToList()
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
	}
}