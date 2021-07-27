using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Features.Model.Lsp;
using NUnit.Framework;
using Features.MoqGenerator;

namespace UnitTests.Features.MoqGenerator
{
	public class MockTextTests
	{
		#warning clean me up later when/if no longer needed....
		[Test]
		public void RecordFoo()
		{
			var expected = new TextDocumentItem(
				new TextDocumentIdentifier("file:///somewhere/a.txt", 0),
				"csharp",
				@"using System;
using System.Linq.Expressions;

using Moq;
using NUnit.Framework;

namespace UnitTests.Features.TestData
{
	public interface IStringAnalyzer
	{
		int HowManyItems(string patient, char charToCount);
	}

	public class StringAnalyzerTests
	{
		[Test]
		public void HappyPath()
		{
			IStringAnalyzer
		}
	}
}"
				);
			var expectedJson = JsonSerializer.Serialize(expected);
			var actual = JsonSerializer.Deserialize<TextDocumentItem>(expectedJson);
			Assert.AreEqual(expected, actual);
			Console.WriteLine(expectedJson);
		}
	
	
		[TestCaseSource(nameof(MockTestFiles))]
		public async Task GoMocks((string testId, string textDocJson, string diagnosticDataJson, string expected) test)
		{
			var mockText = new MockText();

			var docId = JsonSerializer.Deserialize<TextDocumentIdentifier>(test.textDocJson);
			var diagnosticData = JsonSerializer.Deserialize<DiagnosticData>(test.diagnosticDataJson);
			
			var actual = await mockText.GetMockTextAsync(docId, diagnosticData);
			
			Assert.AreEqual(test.expected, actual, test.testId);
		}

		public static IEnumerable<(string testId, string textDocJson, string textDocText, string expected)> MockTestFiles
		{
			get
			{
				const string path = "TestData/MockTests/";
				var data = TestDataReader.GetTests(path);

				foreach(var test in data)
				{
					yield return (test.testId, test.tests[0], test.tests[1], test.tests[2]);
				}
			}
		}
	}
}