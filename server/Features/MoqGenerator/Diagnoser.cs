using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Features.Interfaces.Lsp;
using Features.Model.Lsp;
using Microsoft.CodeAnalysis.CSharp;

namespace Features.MoqGenerator
{
	public class Diagnoser : IDiagnoser
	{
		private readonly HashSet<string> _testFrameworks = new()
		{
			"using NUnit.Framework;",
			"using Xunit;",
			"using Microsoft.VisualStudio.TestTools.UnitTesting;"
		};

#warning there's really nothing awaitable in this method, and it's causing some weirdness / warnings where it's called by OmniLsp, so, refactor to synchronous method
		public async Task<IEnumerable<Diagnostic>> GetDiagnosticsAsync(TextDocumentItem item)
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
				return await Task.FromResult(new List<Diagnostic>());

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
					)
				)
				.Select(x =>
				{
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
						item.Text.Substring(x.Location.SourceSpan.Start, x.Location.SourceSpan.Length)
					);
				})
				;

			return await Task.FromResult(diagnostics);
		}
	}
}