using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Features.Model.Lsp;
using NUnit.Framework;
using Features.MoqGenerator;
using Microsoft.Extensions.Logging;
using Moq;
using Features.Interfaces;
using Features.Model;

namespace UnitTests.Features.MoqGenerator
{
	public class MockTextTests
	{

		[Test]
		public async Task NoInterfaceDefinitionShouldLog()
		{
			var logMock = new LoggerDouble<MockText>();
			var storeMock = GetInterfaceStoreMock(new Dictionary<string, InterfaceDefinition>
			{
			});

			var mockText = new MockText(logMock, storeMock.mock.Object);

			var uri = "uri:somefile.txt";
			var interfaceName = "asdf";
			var docId = new TextDocumentIdentifier(uri, 13);
			var diagnosticData = new DiagnosticData(interfaceName);
			
			var actual = await mockText.GetMockTextAsync(docId, diagnosticData);
			
			Assert.IsNull(actual);
			Assert.IsTrue(
				logMock
					.LogEntries
					.Any(entry =>
					{
						var message = entry.ToString();
						return
							entry.LogLevel == LogLevel.Error
							&& message.Contains(uri)
							&& message.Contains(interfaceName);
					})
			);
		}

		[TestCaseSource(nameof(MockTestFiles))]
		public async Task GoMocks((string testId, string textDocJson, string diagnosticDataJson, string expected) test)
		{
			var logMock = new LoggerDouble<MockText>();
			var storeMock = GetInterfaceStoreMock(new Dictionary<string, InterfaceDefinition>
			{
			});

			var mockText = new MockText(logMock, storeMock.mock.Object);

			var docId = JsonSerializer.Deserialize<TextDocumentIdentifier>(test.textDocJson);
			var diagnosticData = JsonSerializer.Deserialize<DiagnosticData>(test.diagnosticDataJson);
			
			var actual = await mockText.GetMockTextAsync(docId, diagnosticData);
			
			Assert.AreEqual(test.expected, actual, test.testId);
		}

		private
		(
			Mock<IInterfaceStore> mock,
			Expression<Func<IInterfaceStore, InterfaceDefinition>> getDefinition
		) GetInterfaceStoreMock(Dictionary<string, InterfaceDefinition> defsByName)
		{
			var logged = new List<string>();
			var mock = new Mock<IInterfaceStore>();
			Expression<Func<IInterfaceStore, InterfaceDefinition>> getDefinition = x => x.GetInterfaceDefinition(It.IsAny<string>());
			mock
				.Setup(getDefinition)
				.Returns((string name) => 
				{

					defsByName.TryGetValue(name, out var definition);
					return definition;
				});

			return (mock, getDefinition);
		}


		public static IEnumerable<(string testId, string textDocJson, string textDocText, string expected)> MockTestFiles
		{
			get
			{
				const string path = "TestData/MockTests/";
				var data = TestDataReader.GetTests(path);

				foreach(var test in data)
				{
					yield return (test.testId, test.tests[0], test.tests[1], test.tests[2]);
				}
			}
		}
	}
}