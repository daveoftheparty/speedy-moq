using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

using NUnit.Framework;

using Microsoft.Extensions.Logging;
using Moq;

using Features.Interfaces.Lsp;
using Features.Model;
using Features.MoqGenerator;

namespace UnitTests.Features.MoqGenerator
{
	public class MockTextTests
	{

		[Test]
		public void NoInterfaceDefinitionShouldLog()
		{
			var logMock = new LoggerDouble<MockText>();
			var storeMock = GetInterfaceStoreMock(new Dictionary<string, InterfaceDefinition>
			{
			});

			var mockText = new MockText(logMock, storeMock.mock.Object);

			var interfaceName = "asdf";
			var actual = mockText.GetMockText(interfaceName);
			
			Assert.IsNull(actual);
			Assert.IsTrue(
				logMock
					.LogEntries
					.Any(entry =>
					{
						var message = entry.ToString();
						return
							entry.LogLevel == LogLevel.Error
							&& message.Contains(interfaceName);
					})
			);
		}

		[TestCaseSource(typeof(TestDataReader), nameof(TestDataReader.GetTestInputs), new object[] {"TestData/MockTests/"})]
		public void Go((string testIdMessage, string[] testInputs) test)
		{
			var interfaceName = test.testInputs[0];
			var expected = test.testInputs[1];

			var logMock = new LoggerDouble<MockText>();
			var storeMock = GetInterfaceStoreMock(GetInterfaceDefinitions());

			var mockText = new MockText(logMock, storeMock.mock.Object);

			var actual = mockText.GetMockText(interfaceName);
			
			if(expected != actual)
			{
				Console.WriteLine("Here's the actual output we got from MockText:");
				Console.WriteLine(actual);
			}
			Assert.AreEqual(expected, actual, test.testIdMessage);
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



		private Dictionary<string, InterfaceDefinition> GetInterfaceDefinitions()
		{
			return new Dictionary<string, InterfaceDefinition>
			{
				{
					/*
						public interface IStringAnalyzer
						{
							int HowManyItems(string patient, char charToCount);
						}
					*/
					"IStringAnalyzer",
					new InterfaceDefinition
					(
						"IStringAnalyzer",
						"IStringAnalyzer.cs",
						new List<InterfaceMethod>
						{
							{
								new InterfaceMethod
								(
									"HowManyItems",
									"int",
									new List<InterfaceMethodParameter>
									{
										new InterfaceMethodParameter("string", "patient", "string patient"),
										new InterfaceMethodParameter("char", "charToCount", "char charToCount"),
									}
								)
							}
						}
					)
				}
			};
		}
	}
}