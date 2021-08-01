using System.Threading.Tasks;
using Features.MoqGenerator;
using NUnit.Framework;
using Features.Model.Lsp;
using Features;
using System.Text.Json;
using Features.Model;

namespace UnitTests.Features.MoqGenerator
{
	public class InterfaceStoreTests
	{
		[TestCaseSource(typeof(TestDataReader), nameof(TestDataReader.GetTestInputs), new object[] {"TestData/InterfaceStore/"})]
		public async Task GoAsync((string testIdMessage, string[] testInputs) test)
		{
			var interfaceName = test.testInputs[0];
			var inputText = test.testInputs[1];
			var expected = JsonSerializer.Deserialize<InterfaceDefinition>(test.testInputs[2]);

			var logMock = new LoggerDouble<InterfaceStore>();

			var store = new InterfaceStore(logMock);
			await store.LoadDefinitionsIfNecessaryAsync(
				new TextDocumentItem
				(
					new TextDocumentIdentifier("", 0),
					Constants.LanguageId,
					inputText
				)
			);

			Assert.IsTrue(store.Exists(interfaceName), test.testIdMessage);
			var actual = store.GetInterfaceDefinition(interfaceName);
			Assert.AreEqual(expected, actual, test.testIdMessage);
		}
	}
}