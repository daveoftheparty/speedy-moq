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
		[TestCaseSource(typeof(TestDataReader), nameof(TestDataReader.GetTestInputs), new object[] {"TestData/FullGenerationTests/"})]
		public async Task Go((string testIdMessage, string[] testInputs) test)
		{
			var input = test.testInputs[0];
			var expected = test.testInputs[1];

			var generator = new Generator();

			var actual = await generator.GetReplacementAsync(Guid.NewGuid().ToString(), input);
			
			Assert.IsTrue(actual.IsChanged, test.testIdMessage);
			Assert.AreEqual(expected, actual.Text, test.testIdMessage);
		}
	}
}