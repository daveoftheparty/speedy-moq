using System.Collections.Generic;
using System.IO;

namespace MoqGenerator.Util
{
	public class TextLineSplitter
	{
		public IEnumerable<string> SplitToLines(string input)
		{
			if (input == null)
			{
				yield break;
			}

			using (StringReader reader = new StringReader(input))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					yield return line;
				}
			}
		}
	}
}