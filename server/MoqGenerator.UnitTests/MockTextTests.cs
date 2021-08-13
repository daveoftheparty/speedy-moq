using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

using NUnit.Framework;

using Microsoft.Extensions.Logging;
using Moq;

using MoqGenerator.Interfaces.Lsp;
using MoqGenerator.Model;
using MoqGenerator.Model.Lsp;
using MoqGenerator.Services;
using MoqGenerator.UnitTests.Utils;
using System.Text.Json;

namespace MoqGenerator.UnitTests
{
	public class MockTextTests
	{

		[Test]
		public void NoInterfaceDefinitionShouldLog()
		{
			var logMock = new LoggerDouble<MockText>();
			var storeMock = GetInterfaceStoreMock(new Dictionary<string, Dictionary<string, InterfaceDefinition>>
			{
			});

			var mockText = new MockText(logMock, storeMock.mock.Object);

			var interfaceName = "asdf";
			var actual = mockText.GetMockTextByNamespace(interfaceName, DefaultIndent());
			
			Assert.AreEqual(0, actual.Count);
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
			// if(test.testIdMessage != "TestId: 001")
			// 	return;

			var interfaceName = test.testInputs[0];
			var hereDict = new HereDict();
			var expected = hereDict.GetDictionary(test.testInputs[1]);

			var logMock = new LoggerDouble<MockText>();
			var storeMock = GetInterfaceStoreMock(GetInterfaceDefinitions());

			var mockText = new MockText(logMock, storeMock.mock.Object);

			var actual = mockText.GetMockTextByNamespace(interfaceName, DefaultIndent());
			
			try
			{
				CollectionAssert.AreEquivalent(expected, actual, test.testIdMessage);
			}
			catch
			{
				DictionaryDumper.DumpDictionaries(expected, actual);
			}
		}


		private	IndentationConfig DefaultIndent() => new IndentationConfig(3, "\t", false);
		
		private
		(
			Mock<IInterfaceStore> mock,
			Expression<Func<IInterfaceStore, Dictionary<string, InterfaceDefinition>>> getDefinition
		) GetInterfaceStoreMock(Dictionary<string, Dictionary<string, InterfaceDefinition>> defsByName)
		{
			var logged = new List<string>();
			var mock = new Mock<IInterfaceStore>();
			Expression<Func<IInterfaceStore, Dictionary<string, InterfaceDefinition>>> getDefinition = x => x.GetInterfaceDefinitionByNamespace(It.IsAny<string>());
			mock
				.Setup(getDefinition)
				.Returns((string name) => 
				{

					defsByName.TryGetValue(name, out var definition);
					return definition;
				});

			return (mock, getDefinition);
		}



		private Dictionary<string, Dictionary<string, InterfaceDefinition>> GetInterfaceDefinitions()
		{
			return new Dictionary<string, Dictionary<string, InterfaceDefinition>>
			{
				{
					/*
					namespace Hello
					{
						public interface IShowUpInTwoPlaces
						{
							string SomeConfigValue { get; }
						}
					}

					namespace World
					{
						public interface IShowUpInTwoPlaces
						{
							string AnotherConfigValue { get; }
						}
					}
					*/
					"IShowUpInTwoPlaces",
					new Dictionary<string, InterfaceDefinition>
					{
						{
							"Hello",
							new InterfaceDefinition
							(
								"IShowUpInTwoPlaces",
								"",
								new List<InterfaceMethod>(),
								new List<string>
								{
									"SomeConfigValue"
								}
							)
						},

						{
							"World",
							new InterfaceDefinition
							(
								"IShowUpInTwoPlaces",
								"",
								new List<InterfaceMethod>(),
								new List<string>
								{
									"AnotherConfigValue"
								}
							)
						},
					}
				},



				{
					/*
						public interface IStringAnalyzer
						{
							int HowManyItems(string patient, char charToCount);
						}
					*/
					"IStringAnalyzer",
					new Dictionary<string, InterfaceDefinition>
					{
						{
							"FooNamespace",
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
								},
								new List<string>()
							)
						}
					}
				},



				{
					/*
					public interface INotSoSimple
					{
						Task<IEnumerable<SomeUserOutput>> GetStuffAsync(IReadOnlyDictionary<int, IEnumerable<SomeUserInput>> transformData);
					}
					*/
					"INotSoSimple",
					new Dictionary<string, InterfaceDefinition>
					{
						{
							"FooNamespace",
							new InterfaceDefinition
							(
								"INotSoSimple",
								"INotSoSimple.cs",
								new List<InterfaceMethod>
								{
									{
										new InterfaceMethod
										(
											"GetStuffAsync",
											"Task<IEnumerable<SomeUserOutput>>",
											new List<InterfaceMethodParameter>
											{
												new InterfaceMethodParameter("IReadOnlyDictionary<int, IEnumerable<SomeUserInput>>", "transformData", "IReadOnlyDictionary<int, IEnumerable<SomeUserInput>> transformData"),
											}
										)
									}
								},
								new List<string>()
							)
						}
					}
				},



				{
					/*
					public interface IWassupNull
					{
						void Boom(DateTime? mostFuLlStAcKeNgInEeRsTooStupidForNulls);
					}
					*/
					"IWassupNull",
					new Dictionary<string, InterfaceDefinition>
					{
						{
							"FooNamespace",
							new InterfaceDefinition
							(
								"IWassupNull",
								"IWassupNull.cs",
								new List<InterfaceMethod>
								{
									{
										new InterfaceMethod
										(
											"Boom",
											"void",
											new List<InterfaceMethodParameter>
											{
												new InterfaceMethodParameter("DateTime?", "mostFuLlStAcKeNgInEeRsTooStupidForNulls", "DateTime? mostFuLlStAcKeNgInEeRsTooStupidForNulls"),
											}
										)
									}
								},
								new List<string>()
							)
						}
					}
				},



				{
					/*
					public interface IMakeTupleYay
					{
						(string hello, bool valid) YouDontSay(char c);
					}
					*/
					"IMakeTupleYay",
					new Dictionary<string, InterfaceDefinition>
					{
						{
							"FooNamespace",
							new InterfaceDefinition
							(
								"IMakeTupleYay",
								"IMakeTupleYay.cs",
								new List<InterfaceMethod>
								{
									{
										new InterfaceMethod
										(
											"YouDontSay",
											"(string hello, bool valid)",
											new List<InterfaceMethodParameter>
											{
												new InterfaceMethodParameter("char", "c", "char c"),
											}
										)
									}
								},
								new List<string>()
							)
						}
					}
				},



				{
					/*
					public interface ISomeMagicSauce
					{
						public string SomeUrl { get; }
						public DateTime SomeDate { get; }

						void Boom(DateTime? someNullableTimeSlice);
						(string hello, bool valid) ReturnSomeTuple(char c);
						bool Exists(string interfaceName);
						Task<IEnumerable<double>> GetStuffAsync(IReadOnlyDictionary<int, IEnumerable<double>> transformData);
					}
					*/
					"ISomeMagicSauce",
					new Dictionary<string, InterfaceDefinition>
					{
						{
							"FooNamespace",
							new InterfaceDefinition
							(
								"ISomeMagicSauce",
								"",
								new List<InterfaceMethod>
								{
									new InterfaceMethod
									(
										"Boom",
										"void",
										new List<InterfaceMethodParameter>
										{
											new InterfaceMethodParameter("DateTime?", "someNullableTimeSlice", "DateTime? someNullableTimeSlice"),
										}
									),

									new InterfaceMethod
									(
										"ReturnSomeTuple",
										"(string hello, bool valid)",
										new List<InterfaceMethodParameter>
										{
											new InterfaceMethodParameter("char", "c", "char c"),
										}
									),
									
									new InterfaceMethod
									(
										"Exists",
										"bool",
										new List<InterfaceMethodParameter>
										{
											new InterfaceMethodParameter("string", "interfaceName", "string interfaceName"),
										}
									),

									new InterfaceMethod
									(
										"GetStuffAsync",
										"Task<IEnumerable<double>>",
										new List<InterfaceMethodParameter>
										{
											new InterfaceMethodParameter("IReadOnlyDictionary<int, IEnumerable<double>>", "transformData", "IReadOnlyDictionary<int, IEnumerable<double>> transformData"),
										}
									),
								},
								new List<string>
								{
									"SomeUrl",
									"SomeDate"
								}
							)
						}
					}
				},
			};
		}
	}
}