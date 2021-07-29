using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

using NUnit.Framework;
using Moq;

using Features;
using Features.Model.Lsp;
using Features.MoqGenerator;
using Features.Interfaces.Lsp;

namespace UnitTests.Features.MoqGenerator
{
	public class DiagnosticsTests
	{
		[TestCaseSource(typeof(TestDataReader), nameof(TestDataReader.GetTestInputs), new object[] {"TestData/Diagnostics/"})]
		public async Task Go((string testIdMessage, string[] testInputs) test)
		{
			var input = test.testInputs[0];
			var expected = JsonSerializer.Deserialize<IEnumerable<Diagnostic>>(test.testInputs[1]);

			var interfaceStore = new Mock<IInterfaceStore>();
			interfaceStore
				.Setup(x => x.Exists(It.IsAny<string>()))
				.Returns(true);

			var mockText = new Mock<IMockText>();
			mockText
				.Setup(x => x.GetMockText(It.IsAny<string>()))
				.Returns("-- THIS WOULD BE THE GENERATED CODE, TESTED ELSEWHERE --")
				;

			var diagnoser = new Diagnoser(interfaceStore.Object, mockText.Object);
			var textDoc = new TextDocumentItem(new TextDocumentIdentifier("somefile.cs", 0), Constants.LanguageId, input);
			var actual = await diagnoser.GetDiagnosticsAsync(textDoc);

			CollectionAssert.AreEquivalent(expected, actual, test.testIdMessage);
		}
	}
}