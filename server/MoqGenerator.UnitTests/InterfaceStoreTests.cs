using Moq;
using System.Threading.Tasks;
using MoqGenerator.Services;
using NUnit.Framework;
using MoqGenerator.Model.Lsp;
using System.Text.Json;
using MoqGenerator.Model;
using System;
using MoqGenerator.UnitTests.Utils;
using MoqGenerator.Interfaces.Lsp;

namespace MoqGenerator.UnitTests
{
	public class InterfaceStoreTests
	{
		[TestCase(true)]
		[TestCase(false)]
		public async Task SlowDownTurbo(bool go)
		{
			var text = 
			@"
			namespace Foo
			{
				public interface IBar
				{
					public string FizzBuzz { get; }
				}
			}";

			var whoaCowboy = new Mock<IWhoaCowboy>();
			whoaCowboy.SetupGet(x => x.GiddyUp).Returns(go);

			var logMock = new LoggerDouble<InterfaceStore>();

			var store = new InterfaceStore(logMock, whoaCowboy.Object);
			await store.LoadDefinitionsIfNecessaryAsync(
				new TextDocumentItem
				(
					new TextDocumentIdentifier("", 0),
					Constants.LanguageId,
					text
				)
			);

			if(go)
				Assert.AreEqual("FizzBuzz", store.GetInterfaceDefinition("IBar").Properties[0]);
			else
				Assert.IsNull(store.GetInterfaceDefinition("IBar"));
		}

		[TestCaseSource(typeof(TestDataReader), nameof(TestDataReader.GetTestInputs), new object[] {"TestData/InterfaceStore/"})]
		public async Task GoAsync((string testIdMessage, string[] testInputs) test)
		{
			var interfaceName = test.testInputs[0];
			var inputText = test.testInputs[1];
			var expected = JsonSerializer.Deserialize<InterfaceDefinition>(test.testInputs[2]);

			var logMock = new LoggerDouble<InterfaceStore>();
			
			var whoaCowboy = new Mock<IWhoaCowboy>();
			whoaCowboy.SetupGet(x => x.GiddyUp).Returns(true);

			var store = new InterfaceStore(logMock, whoaCowboy.Object);
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

			try
			{
				// surprisingly??, the new record type in C# 9.0 isn't "smart enough" to do nested collection asserts...
				
				// Assert.AreEqual(expected.InterfaceName, actual.InterfaceName, test.testIdMessage);
				// Assert.AreEqual(expected.SourceFile, actual.SourceFile, test.testIdMessage);
				// CollectionAssert.AreEquivalent(expected.Methods, actual.Methods, test.testIdMessage);
				// CollectionAssert.AreEquivalent(expected.Properties, actual.Properties, test.testIdMessage);
				Assert.AreEqual(JsonSerializer.Serialize(expected), JsonSerializer.Serialize(actual), test.testIdMessage);
			}
			catch
			{
				Console.WriteLine("actual output:");
				Console.WriteLine(JsonSerializer.Serialize(actual));
				throw;
			}
		}
	}
}