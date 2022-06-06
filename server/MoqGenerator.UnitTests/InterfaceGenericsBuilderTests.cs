using System;
using System.Text.Json;
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
		[TestCase("IService<UserClassA, IReadOnlyDictionary<UserClassB>>", "IService", "UserClassA", "IReadOnlyDictionary<UserClassB>")]
		[TestCase("IService1<string, IService2<int>, IService3<char, IService4<double>>>", "IService1", "string", "IService2<int>", "IService3<char, IService4<double>>")]
		public void BuildSlowest(string code, string expectedName, params string[] expectedArgs)
		{
			var builder = new InterfaceGenericsBuilder();
			var actual = builder.BuildSlowest(code);
			
			try
			{
				Assert.AreEqual(1, actual.Count);
				Assert.True(actual.ContainsKey(expectedName));
				CollectionAssert.AreEqual(expectedArgs, actual[expectedName].GenericTypeArguments);
			}
			catch
			{
				Console.WriteLine($"Actual results serialized: {JsonSerializer.Serialize(actual)}");
				throw;
			}
		}
	}
}