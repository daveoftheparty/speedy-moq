using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Features.MoqGenerator;

namespace UnitTests.Features.MoqGenerator
{
	public class FullGenerationTests
	{
		[TestCaseSource(nameof(FileTests))]
		public async Task Go((string testId, string input, string expected) test)
		{
			var generator = new Generator();

			var actual = await generator.GetReplacementAsync(Guid.NewGuid().ToString(), test.input);
			
			Assert.IsTrue(actual.IsChanged, test.testId);
			Assert.AreEqual(test.expected, actual.Text, test.testId);
		}

		public static IEnumerable<(string input, string expected, string testId)> FileTests
		{
			get
			{
				const string path = "TestData/FullGenerationTests/";
				var data = TestDataReader.GetTests(path);

				foreach(var test in data)
				{
					yield return (test.testId, test.tests[1], test.tests[0]);
				}
			}
		}
	}
}