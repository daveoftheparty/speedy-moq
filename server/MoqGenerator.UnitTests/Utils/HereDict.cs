using System;
using System.Collections.Generic;
using System.Linq;
using MoqGenerator.Util;

namespace MoqGenerator.UnitTests.Utils
{
	// kinda like "heredoc" -- get it??
	public class HereDict
	{
		private const string NewKeyDeclaration = "<< begin ";
		public Dictionary<string, string> GetDictionary(string input)
		{
			var result = new Dictionary<string, string>();

			var splitter = new TextLineSplitter();
			var lines = splitter
				.SplitToLines(input)
				.Select((line, i) => new
				{
					lineIndex = i,
					text = line
				})
				.ToList();

			var keyIndices = lines
				.Where(line => line.text.StartsWith(NewKeyDeclaration))
				.Select(line => new
				{
					line.lineIndex,
					Key = line.text.Replace(NewKeyDeclaration, "")
				})
				.ToList();

			foreach(var keyIndex in keyIndices)
			{
				var endTextForKey = $"end {keyIndex.Key} >>";

				var value = string.Join(
					Environment.NewLine,
					lines
						.Skip(keyIndex.lineIndex + 1)
						.TakeUntil(line => line.text == endTextForKey, false)
						.Select(line => line.text)
				);

				result.Add(keyIndex.Key, value);
			}

			return result;
		}
	}
}