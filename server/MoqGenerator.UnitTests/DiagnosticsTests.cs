using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text.Json;

using NUnit.Framework;
using Moq;

using MoqGenerator.Model.Lsp;
using MoqGenerator.Services;
using MoqGenerator.Interfaces.Lsp;
using MoqGenerator.UnitTests.Utils;
using System;

namespace MoqGenerator.UnitTests
{
	public class DiagnosticsTests
	{
		[TestCaseSource(typeof(TestDataReader), nameof(TestDataReader.GetTestInputs), new object[] {"TestData/Diagnostics/"})]
		public void Go((string testIdMessage, string[] testInputs) test)
		{
			var input = test.testInputs[0];
			var expected = JsonSerializer.Deserialize<IEnumerable<Diagnostic>>(test.testInputs[1]);

			var mockReplacementDictionary = new Dictionary<string, string>
			{
				{
					"Foo",
					"-- THIS WOULD BE THE GENERATED CODE, TESTED ELSEWHERE --"
				}
			};

			if(test.testInputs.Length == 3)
			{
				mockReplacementDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(test.testInputs[2]);
			}


			var interfaceStore = new Mock<IInterfaceStore>();
			interfaceStore
				.Setup(x => x.Exists(It.IsAny<string>()))
				.Returns((string name) => name == "IStringAnalyzer");

			var mockText = new Mock<IMockText>();

			Expression<Func<IMockText, IReadOnlyDictionary<string, string>>> getMockTextByNamespace = x => x.GetMockTextByNamespace(It.IsAny<string>(), It.IsAny<IndentationConfig>());
			mockText
				.Setup(getMockTextByNamespace)
				.Returns((string interfaceName, IndentationConfig indentationConfig) =>
				{
					return mockReplacementDictionary;
				});


			var mockIndentation = new Mock<IIndentation>();
			mockIndentation
				.Setup(x => x.GetIndentationConfig(It.IsAny<string>(), It.IsAny<uint>()))
				.Returns(new IndentationConfig(3, "\t", false));

			var diagnoser = new Diagnoser(
				interfaceStore.Object,
				mockText.Object,
				mockIndentation.Object,
				new LoggerDouble<Diagnoser>()
				);
			var textDoc = new TextDocumentItem(new TextDocumentIdentifier("somefile.cs", 0), Constants.LanguageId, input);
			var actual = diagnoser.GetDiagnostics(textDoc);

			try
			{
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