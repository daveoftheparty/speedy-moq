using System;
using System.Linq;
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
			var actuals = builder.BuildSlow(code);
			
			try
			{
				Assert.AreEqual(1, actuals.Count);
				var actual = actuals.First();

				Assert.AreEqual(expectedName, actual.Generics.InterfaceName);
				Assert.AreEqual($"{expectedName};{expectedArgs.Length}", actual.Generics.InterfaceNameKey);
				Assert.AreEqual(0, actual.Location.Start);
				Assert.AreEqual(code.Length, actual.Location.Length);
				CollectionAssert.AreEqual(expectedArgs, actual.Generics.GenericTypeArguments);
			}
			catch
			{
				Console.WriteLine($"Actual results serialized: {JsonSerializer.Serialize(actuals)}");
				throw;
			}
		}
	}
}