using MoqGenerator.Services;
using NUnit.Framework;

namespace MoqGenerator.UnitTests
{
	public class InterfaceGenericsBuilderTests
	{
		[TestCase("IList<int>", "IList", "int")]
		public void BuildSlowest(string code, string expectedName, params string[] expectedArgs)
		{
			var builder = new InterfaceGenericsBuilder();
			var actual = builder.BuildSlowest(code);
			
			Assert.True(actual.ContainsKey(expectedName));
			CollectionAssert.AreEqual(expectedArgs, actual[expectedName].GenericTypeArguments);
		}

	}
}