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
		// VS Code sends the drive colon as %3A, but OmniSharp's DocumentUri.ToString() has already decoded it by the time we get here
		[TestCase(@"file:///c:/src/My%20Code/IFoo.cs", @"c:\src\My Code\IFoo.cs")]
		[TestCase(@"file:///Users/me/TwoInterface/Two.Lib/IDealio.cs", @"/Users/me/TwoInterface/Two.Lib/IDealio.cs")]
		[TestCase(@"file:///Users/me/My%20Code/IFoo.cs", @"/Users/me/My Code/IFoo.cs")]
		public void TestHandler(string uri, string expected)
		{
			var handler = new UriHandler();
			Assert.AreEqual(expected, handler.GetFilePath(new TextDocumentIdentifier(uri, 0)));
		}
	}
}