using System;
using System.Collections.Generic;

namespace MoqGenerator.UnitTests.Utils
{
	public static class LinqExtensions
	{
		public static IEnumerable<T> TakeUntil<T>(this IEnumerable<T> source, Func<T, bool> predicate, bool inclusive = true)
		{
			foreach(T item in source)
			{
				if(!predicate(item))
				{
					yield return item;
				}
				else
				{
					if(inclusive) yield return item;
					yield break;
				}
			}
		}
	}
}