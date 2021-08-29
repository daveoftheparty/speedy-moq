using System.Collections.Generic;

namespace Demo.Lib
{
	public interface IHasListStringByLongIndexer
	{
		List<string> this[long key] { get; set; }
	}
}