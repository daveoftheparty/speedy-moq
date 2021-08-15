using System.Text.Json;
using MoqGenerator.Model.Lsp;
using MoqGenerator.Services;
using NUnit.Framework;
using MoqGenerator.UnitTests.Utils;
using System.Linq;
using System;

namespace MoqGenerator.UnitTests
{
	public class IndentationLevelTests
	{
		[TestCaseSource(typeof(TestDataReader), nameof(TestDataReader.GetTestInputs), new object[] {"TestData/IndentationTests/"})]
		public void GoIndentation((string testIdMessage, string[] testInputs) test)
		{
			// if(test.testIdMessage != "TestId: 009")
			// 	return;

			var hereDict = new HereDict();
			var rawInput = hereDict.GetDictionary(test.testInputs[0]).First();
			var sourceText = rawInput.Value;
			var currentLine = UInt32.Parse(rawInput.Key);

			var expected = JsonSerializer.Deserialize<IndentationConfig>(test.testInputs[1]);

			var logger = new LoggerDouble<Indentation>();
			var indenter = new Indentation(logger);
			var actual = indenter.GetIndentationConfig(sourceText, currentLine);

			Console.WriteLine("Logs: for " + test.testIdMessage);
			foreach(var entry in logger.LogEntries)
				Console.WriteLine(entry);

			Assert.AreEqual(expected, actual, test.testIdMessage);
		}


		[TestCase(new [] { 3, 6, 9, 12 }, 9, 3, 3)]
		[TestCase(new [] { 3, 6, 8, 9, 12 }, 3, 3, 1)]
		[TestCase(new [] { 1, 3, 6, 8, 9, 12, 14 }, 12, 3, 4)]
		[TestCase(new [] { 1, 2, 4, 8, 12, 16, 20, 21 }, 16, 2, 8)] // looks like user setting is 4, but, 2 is valid...
		
		// invalid currentLineSpaceCount / round up tests
		[TestCase(new [] { 1, 4, 8 }, 1, 4, 1)]
		[TestCase(new [] { 3, 4, 8 }, 3, 4, 1)]
		[TestCase(new [] { 4, 5, 8 }, 5, 4, 2)]
		[TestCase(new [] { 4, 6, 8 }, 6, 4, 2)]
		[TestCase(new [] { 4, 7, 8 }, 7, 4, 2)]
		[TestCase(new [] { 4, 8, 9 }, 9, 4, 3)]
		public void TestCalculateFakeTabStop
		(
			int[] distinctSpaceCounts,
			int currentLineSpaceCount,
			int expectedTabSize,
			int expectedCurrentIndentLevel
		)
		{
			var indenter = new Indentation(new LoggerDouble<Indentation>());
			var actual = indenter.CalculateFakeTabStop(distinctSpaceCounts.ToList(), currentLineSpaceCount);

			Assert.AreEqual(expectedTabSize, actual.tabSize);
			Assert.AreEqual(expectedCurrentIndentLevel, actual.currentIndentLevel);
		}
	}
}