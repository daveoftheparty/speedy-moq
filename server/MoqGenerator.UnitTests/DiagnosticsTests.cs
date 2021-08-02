using System.Collections.Generic;
using System.Text.Json;

using NUnit.Framework;
using Moq;

using MoqGenerator;
using MoqGenerator.Model.Lsp;
using MoqGenerator.Services;
using MoqGenerator.Interfaces.Lsp;
using ourRange = MoqGenerator.Model.Lsp.Range;
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

			var interfaceStore = new Mock<IInterfaceStore>();
			interfaceStore
				.Setup(x => x.Exists(It.IsAny<string>()))
				.Returns((string name) => name == "IStringAnalyzer");

			var mockText = new Mock<IMockText>();
			mockText
				.Setup(x => x.GetMockText(It.IsAny<string>(), It.IsAny<IndentationConfig>()))
				.Returns("-- THIS WOULD BE THE GENERATED CODE, TESTED ELSEWHERE --")
				;

			var mockIndentation = new Mock<IIndentation>();
			mockIndentation
				.Setup(x => x.GetIndentationConfig(It.IsAny<string>(), It.IsAny<ourRange>()))
				.Returns(new IndentationConfig(3, "\t", false));

			var diagnoser = new Diagnoser(interfaceStore.Object, mockText.Object, mockIndentation.Object);
			var textDoc = new TextDocumentItem(new TextDocumentIdentifier("somefile.cs", 0), Constants.LanguageId, input);
			var actual = diagnoser.GetDiagnostics(textDoc);

			try
			{
				CollectionAssert.AreEquivalent(expected, actual, test.testIdMessage);
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