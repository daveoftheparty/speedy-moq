using System.Collections.Generic;

namespace Demo.Lib
{
	public interface IGenericService<TSource, TResult>
	{
		IEnumerable<TResult> TransformSource(IEnumerable<TSource> items);
	}
}