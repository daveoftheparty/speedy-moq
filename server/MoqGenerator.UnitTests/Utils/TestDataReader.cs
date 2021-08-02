using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MoqGenerator.UnitTests.Utils
{
	public class TestDataReader
	{
		public static IEnumerable<(string testIdMessage, string[] testInputs)> GetTestInputs(string path)
		{
			if(!path.EndsWith('/') && !path.EndsWith('\\'))
				throw new ArgumentOutOfRangeException(nameof(path), path, "Use a trailing slash or this won't work the way you want!");
				
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
					return new
					{
						TestId = g.Key,
						Files = g
							.OrderBy(name => name.FileName)
							.Select(n => File.ReadAllText(path + n.FileName))
							.ToArray()
					};
				});

			foreach(var file in files)
			{
				yield return ($"TestId: {file.TestId}", file.Files);
			}
		}
	}
}