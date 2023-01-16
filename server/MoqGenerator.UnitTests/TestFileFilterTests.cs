using System;
using MoqGenerator.Services;
using MoqGenerator.UnitTests.Utils;
using NUnit.Framework;

namespace MoqGenerator.UnitTests
{
	public class TestFileFilterTests
	{
		[TestCaseSource(typeof(TestDataSingleFileReader), nameof(TestDataSingleFileReader.GetTestInputs), new object[] {"TestData/TestFileFilter/"})]
		public void Go((string testFile, string[] inputLines) test)
		{
			var file = test.testFile;
			Assert.True(
				file.Contains("true", StringComparison.InvariantCulture) ||
				file.Contains("false", StringComparison.InvariantCulture),
				"Invalid test file naming convention!"
				);

			var expected = file.Contains("true", StringComparison.InvariantCulture);

			var filter = new TestFileFilter();
			Assert.AreEqual(expected, filter.IsTestFile(test.inputLines));
		}
	}
}