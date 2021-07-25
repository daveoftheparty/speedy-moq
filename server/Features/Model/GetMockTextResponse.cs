using System.Collections.Generic;

namespace Features.Model
{
	public class GetMockTextResponse
	{
		public IEnumerable<MockTextReplacements> Replacements { get; set; }
	}
}