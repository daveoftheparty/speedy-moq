using System.Threading.Tasks;
using NUnit.Framework;

using Features;
using Features.MoqGenerator;
using Features.Model.Lsp;

using UnitTests.Features;

namespace IntegrationTests.Features
{
	public class Tests
	{


		[Test]
		public async Task Go()
		{
			var logger = new LoggerDouble<InterfaceStore>();
			var store = new InterfaceStore(logger);
			var textDoc = new TextDocumentItem(
				new TextDocumentIdentifier(@"C:\src\daveoftheparty\boiler-moq\server\UnitTests.Features\LoggerDouble.cs", 0),
				Constants.LanguageId,
				"foo");

			await store.LoadDefinitionsIfNecessaryAsync(textDoc);
			Assert.Fail();
		}
	}
}