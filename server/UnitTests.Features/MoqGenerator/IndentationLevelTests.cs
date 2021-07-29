using System.Text.Json;
using Features.Model.Lsp;
using Features.MoqGenerator;
using NUnit.Framework;

namespace UnitTests.Features.MoqGenerator
{
	public class IndentationLevelTests
	{
		[TestCaseSource(typeof(TestDataReader), nameof(TestDataReader.GetTestInputs), new object[] {"TestData/IndentationTests/"})]
		public void GoIndentation((string testIdMessage, string[] testInputs) test)
		{
			var sourceText = test.testInputs[0];
			var sourceRange = JsonSerializer.Deserialize<Range>(test.testInputs[1]);
			var expected = JsonSerializer.Deserialize<IndentationConfig>(test.testInputs[2]);

			var indenter = new Indentation();
			var actual = indenter.GetIndentationConfig(sourceText, sourceRange);

			Assert.AreEqual(expected, actual, test.testIdMessage);
		}
	}
}