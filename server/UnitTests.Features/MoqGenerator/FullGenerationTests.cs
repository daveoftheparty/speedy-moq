using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

namespace UnitTests.Features.MoqGenerator
{
	public class FullGenerationTests
	{
		[TestCaseSource(nameof(FileTests))]
		public void Go((string input, string expected, string testId) test)
		{
			Assert.AreEqual(test.expected, test.input, $"TestId: {test.testId}");
		}

		public static IEnumerable<(string input, string expected, string testId)> FileTests
		{
			get
			{
				const string path = "TestData/";

				var files = Directory
					.EnumerateFiles(path)
					.Select(f => 
					{
						var fileOnly = f.Replace(path, "");
						return new
						{
							TestNumber = fileOnly.Substring(0,3),
							FileName = fileOnly
						};
					})
					.GroupBy(t => t.TestNumber)
					.OrderBy(o => o.Key)
					.Select(g => 
					{
						var files = g.Select(n => n.FileName).ToList();
						return new
						{
							Input = files[1],
							Expected = files[0],
							TestId = g.Key
						};
					});

				foreach(var file in files)
				{
					yield return (File.ReadAllText(path + file.Input), File.ReadAllText(path + file.Expected), file.TestId);
				}
			}
		}
	}
}