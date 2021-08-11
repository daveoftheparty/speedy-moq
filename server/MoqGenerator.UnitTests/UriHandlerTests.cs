using MoqGenerator.Model.Lsp;
using MoqGenerator.Services;
using NUnit.Framework;

namespace MoqGenerator.UnitTests
{
	public class UriHandlerTests
	{

		[TestCase(@"file:///e:/temp/TwoInterface/Two.Lib/IDealio.cs", @"e:\temp\TwoInterface\Two.Lib\IDealio.cs")]
		[TestCase(@"file:///e:\temp/TwoInterface\Two.Lib/IDealio.cs", @"e:\temp\TwoInterface\Two.Lib\IDealio.cs")]
		[TestCase(@"file:///e:\temp\TwoInterface\Two.Lib\IDealio.cs", @"e:\temp\TwoInterface\Two.Lib\IDealio.cs")]
		public void TestHandler(string uri, string expected)
		{
			var handler = new UriHandler();
			Assert.AreEqual(expected, handler.GetFilePath(new TextDocumentIdentifier(uri, 0)));
		}
	}
}