using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MoqGenerator.UnitTests.Utils
{
	public class TestDataSingleFileReader
	{
		public static IEnumerable<(string testFile, string[] inputLines)> GetTestInputs(string path)
		{
			if(!path.EndsWith('/') && !path.EndsWith('\\'))
				throw new ArgumentOutOfRangeException(nameof(path), path, "Use a trailing slash or this won't work the way you want!");
				
			var files = Directory
				.EnumerateFiles(path)
				.Select(f => f.Replace(path, ""))
				.OrderBy(o => o)
				.Select(g => 
				{
					return new
					{
						testFile = g,
						inputLines = File.ReadAllLines(path + g),
					};
				});

			foreach(var file in files)
			{
				yield return (file.testFile, file.inputLines);
			}
		}
	}
}