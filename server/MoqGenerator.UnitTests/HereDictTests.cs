using System.Collections.Generic;
using System.Text.Json;
using MoqGenerator.UnitTests.Utils;
using NUnit.Framework;

namespace MoqGenerator.UnitTests
{
	public class HereDictTests
	{
		[TestCaseSource(typeof(TestDataReader), nameof(TestDataReader.GetTestInputs), new object[] {"TestData/HereDict/"})]
		public void Go((string testIdMessage, string[] testInputs) test)
		{
			var input = test.testInputs[0];
			var expected = JsonSerializer.Deserialize<Dictionary<string, string>>(test.testInputs[1]);

			var hereDict = new HereDict();
			var actual = hereDict.GetDictionary();

			CollectionAssert.AreEquivalent(expected, actual);
		}
	}
}