using System;
using System.Diagnostics;
using NUnit.Framework;
using MoqGenerator.Util;
using System.Collections.Generic;
using MoqGenerator.UnitTests.Utils;

namespace MoqGenerator.UnitTests
{
	public class StopwatchHelperTests
	{
		[TestCase("hi", "hi ")]
		[TestCase("already gots whitespace\t", "already gots whitespace\t")]
		[TestCase(null, "Elapsed Time: ")]
		[TestCase("", "Elapsed Time: ")]
		public void TestStopAndLog(string messagePrefix, string expectedStartsWith)
		{
			var watch = new Stopwatch();
			watch.Start();
			var logger = new LoggerDouble<StopwatchHelperTests>();

			watch.StopAndLogInformation(logger, messagePrefix);
			Assert.AreEqual(1, logger.LogEntries.Count);
			Assert.IsTrue(logger.LogEntries[0].ToString().StartsWith(expectedStartsWith));
		}

		[TestCaseSource(nameof(FormatTests))]
		public void TestFormats((TimeSpan span, string expected) test)
		{
			Assert.AreEqual(test.expected, StopwatchHelper.FormatSpan(test.span));
		}

		public static IEnumerable<(TimeSpan span, string expected)> FormatTests
		{
			get
			{
				yield return (TimeSpan.FromMilliseconds(0), "0ms");
				yield return (TimeSpan.FromMilliseconds(3), "3ms");
				yield return (TimeSpan.FromMilliseconds(999), "999ms");

				yield return (TimeSpan.FromSeconds(37), "[ss.ms] 37.000");
				yield return (TimeSpan.FromSeconds(61), "[mm:ss.ms] 01:01.000");

				yield return (TimeSpan.FromMinutes(13), "[mm:ss.ms] 13:00.000");

				yield return (TimeSpan.FromHours(11), "[hh:mm:ss.ms] 11:00:00.000");
				yield return (TimeSpan.FromHours(25), "[days:hh:mm:ss.ms] 1:01:00:00.000");

				yield return (new TimeSpan(1369, 1, 2, 3, 5), "[days:hh:mm:ss.ms] 1369:01:02:03.005");
			}
		}
	}
}