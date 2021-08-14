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

#warning write a LOT more tests for the whole spaces as tabs stuff:
			/*
				fakeTabCounts:
					3, 6, 9, 12
					3, 6, 8, 9, 12
					1, 3, 6, 8, 9, 12, 14

					etc...
			*/
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
	}
}