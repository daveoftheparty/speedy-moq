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
			var hereDict = new HereDict();
			var rawInput = hereDict.GetDictionary(test.testInputs[0]).First();
			var sourceText = rawInput.Value;
			var currentLine = UInt32.Parse(rawInput.Key);

			var expected = JsonSerializer.Deserialize<IndentationConfig>(test.testInputs[1]);


			var indenter = new Indentation(new LoggerDouble<Indentation>());
			var actual = indenter.GetIndentationConfig(sourceText, currentLine);

			Assert.AreEqual(expected, actual, test.testIdMessage);
		}
	}
}