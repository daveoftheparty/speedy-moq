using System.Collections.Generic;

namespace Features.Model
{
	public class GetMockTextResponse
	{
		public IEnumerable<MockTextReplacement> Replacements { get; set; }
	}
}