using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

using NUnit.Framework;

using Features;
using Features.Model.Lsp;
using Features.MoqGenerator;

namespace UnitTests.Features.MoqGenerator
{
	public class DiagnosticsTests
	{
		[TestCaseSource(nameof(DiagnosticTestFiles))]
		public async Task GoDiagnostics((string testId, string input, string expected) test)
		{
			var expected = JsonSerializer.Deserialize<IEnumerable<Diagnostic>>(test.expected);

			var diagnoser = new Diagnoser();
			var textDoc = new TextDocumentItem(new TextDocumentIdentifier("somefile.cs", 0), Constants.LanguageId, test.input);
			var actual = await diagnoser.GetDiagnosticsAsync(textDoc);

			CollectionAssert.AreEquivalent(expected, actual, test.testId);
		}

		public static IEnumerable<(string testId, string input, string expected)> DiagnosticTestFiles
		{
			get
			{
				const string path = "TestData/Diagnostics/";
				var data = TestDataReader.GetTests(path);

				foreach(var test in data)
				{
					yield return (test.testId, test.tests[0], test.tests[1]);
				}
			}
		}
	}
}