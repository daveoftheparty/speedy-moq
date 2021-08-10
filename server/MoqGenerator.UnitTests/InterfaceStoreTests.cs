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
using System.Collections.Generic;
using System.Linq;

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

			var store = new InterfaceStore(logMock, whoaCowboy.Object, new Mock<IProjectHandler>().Object);
			await store.LoadDefinitionsIfNecessaryAsync(
				new TextDocumentItem
				(
					new TextDocumentIdentifier("", 0),
					Constants.LanguageId,
					text
				)
			);

			if(go)
				Assert.AreEqual("FizzBuzz", store.GetInterfaceDefinitionByNamespace("IBar")["Foo"].Properties[0]);
			else
				Assert.IsNull(store.GetInterfaceDefinitionByNamespace("IBar"));
		}

		[TestCaseSource(typeof(TestDataReader), nameof(TestDataReader.GetTestInputs), new object[] {"TestData/InterfaceStore/"})]
		public async Task GoAsync((string testIdMessage, string[] testInputs) test)
		{
			// if(test.testIdMessage != "TestId: 005")
			// 	return;

			var interfaceDict = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(test.testInputs[0]);
			var inputText = test.testInputs[1];
			var expected = JsonSerializer.Deserialize<Dictionary<string, InterfaceDefinition>>(test.testInputs[2]);

			var logMock = new LoggerDouble<InterfaceStore>();
			
			var whoaCowboy = new Mock<IWhoaCowboy>();
			whoaCowboy.SetupGet(x => x.GiddyUp).Returns(true);

			var store = new InterfaceStore(logMock, whoaCowboy.Object, new Mock<IProjectHandler>().Object);
			await store.LoadDefinitionsIfNecessaryAsync(
				new TextDocumentItem
				(
					new TextDocumentIdentifier("", 0),
					Constants.LanguageId,
					inputText
				)
			);

			try
			{
				foreach(var interfaceByNamespace in interfaceDict)
				{
					var interfaceName = interfaceByNamespace.Key;
					Assert.IsTrue(store.Exists(interfaceName), test.testIdMessage);
					var namespaceDict = store.GetInterfaceDefinitionByNamespace(interfaceName);

					foreach(var namespaceName in interfaceByNamespace.Value)
					{
						Assert.IsTrue(namespaceDict.ContainsKey(namespaceName), test.testIdMessage);

						var actualDefinition = namespaceDict[namespaceName];
						var expectedDefinition = expected[namespaceName];
						
						Assert.AreEqual(
							JsonSerializer.Serialize(expectedDefinition),
							JsonSerializer.Serialize(actualDefinition),
							test.testIdMessage
							);
					}
				}
			}
			catch
			{
				Console.WriteLine("actual output:");
				interfaceDict
					.Keys
					.Select(iName => new
					{
						InterfaceName = iName,
						NamespaceDict = store.GetInterfaceDefinitionByNamespace(iName)
					})
					.ToList()
					.ForEach(iNameAndDict => {
						var result = iNameAndDict.NamespaceDict == null
							? $"{iNameAndDict.InterfaceName} : null"
							: $"{iNameAndDict.InterfaceName} : {JsonSerializer.Serialize(iNameAndDict.NamespaceDict)}"
							;
						Console.WriteLine(result);
					});
				throw;
			}
		}
	}
}