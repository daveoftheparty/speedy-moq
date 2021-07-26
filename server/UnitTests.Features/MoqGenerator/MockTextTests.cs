using System;
using System.Text.Json;
using Features.Model.Lsp;
using NUnit.Framework;

namespace UnitTests.Features.MoqGenerator
{
	public class MockTextTests
	{
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
	}
}