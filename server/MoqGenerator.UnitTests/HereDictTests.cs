using System;
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
			var expected = NormalizeNewLines(JsonSerializer.Deserialize<Dictionary<string, string>>(test.testInputs[1]));

			var hereDict = new HereDict();
			var actual = NormalizeNewLines(hereDict.GetDictionary(input));

			try
			{
				CollectionAssert.AreEquivalent(expected, actual);
			}
			catch
			{
				DictionaryDumper.DumpDictionaries(expected, actual);
			}
		}

		// compare content, not line endings: input files have whatever line endings git checked out,
		// and expected json may be authored on either Windows or mac
		private static Dictionary<string, string> NormalizeNewLines(IReadOnlyDictionary<string, string> dict)
		{
			var result = new Dictionary<string, string>();
			foreach(var kvp in dict)
				result[kvp.Key] = kvp.Value.Replace("\r\n", "\n");

			return result;
		}
	}
}