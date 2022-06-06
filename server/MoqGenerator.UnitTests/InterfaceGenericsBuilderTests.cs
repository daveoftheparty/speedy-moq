using MoqGenerator.Services;
using NUnit.Framework;

namespace MoqGenerator.UnitTests
{
	public class InterfaceGenericsBuilderTests
	{
		[TestCase("IList<int>", "IList", "int")]
		[TestCase("IGenericService<TSource, TResult>", "IGenericService", "TSource", "TResult")]
		[TestCase("IService<T, K, V>", "IService", "T", "K", "V")]
		[TestCase("IService\t<    T,\r\tK,V\r\n>", "IService", "T", "K", "V")]
		[TestCase("IService<TSource, IReadOnlyDictionary<TResult>>", "IService", "TSource", "IReadOnlyDictionary<TResult>")]
		public void BuildSlowest(string code, string expectedName, params string[] expectedArgs)
		{
			var builder = new InterfaceGenericsBuilder();
			var actual = builder.BuildSlowest(code);
			
			Assert.AreEqual(1, actual.Count);
			Assert.True(actual.ContainsKey(expectedName));
			CollectionAssert.AreEqual(expectedArgs, actual[expectedName].GenericTypeArguments);
		}

	}
}