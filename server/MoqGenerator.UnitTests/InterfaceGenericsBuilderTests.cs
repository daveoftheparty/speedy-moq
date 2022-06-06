using MoqGenerator.Services;
using NUnit.Framework;

namespace MoqGenerator.UnitTests
{
	public class InterfaceGenericsBuilderTests
	{
		[TestCase("IList<int>")]
		public void BuildSlowest(string code)
		{
			var builder = new InterfaceGenericsBuilder();
			builder.BuildSlowest(code);
		}

	}
}