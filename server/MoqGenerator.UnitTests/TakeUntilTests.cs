using System.Linq;
using NUnit.Framework;

using MoqGenerator.UnitTests.Utils;

namespace MoqGenerator.UnitTests
{
	public class TakeUntilTests
	{
		[TestCase(new [] {1, 2, 3}, 2, true, new [] { 1, 2 })]
		[TestCase(new [] {1, 2, 3}, 2, false, new [] { 1 })]
		[TestCase(new [] {1, 2, 3}, 5, false, new [] { 1, 2, 3 })]
		[TestCase(new [] {1, 2, 3}, 5, true, new [] { 1, 2, 3 })]
		[TestCase(new [] {1, 2, 3}, 1, true, new [] { 1 })]
		[TestCase(new [] {1, 2, 3}, 1, false, new int[] { })]
		public void Go(int[] input, int until, bool inclusive, int[] expected)
		{
			var actual = input
				.TakeUntil(x => x == until, inclusive)
				.ToArray();

			CollectionAssert.AreEquivalent(expected, actual);
		}
	}
}