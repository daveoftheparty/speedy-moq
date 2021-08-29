using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Microsoft.Extensions.Logging;

using MoqGenerator.Interfaces.Lsp;
using MoqGenerator.Model.Lsp;
using MoqGenerator.Util;

namespace MoqGenerator.Services
{
	public class MockText : IMockText
	{
		private readonly ILogger<MockText> _logger;
		private readonly IInterfaceStore _interfaceStore;

		public MockText(ILogger<MockText> logger, IInterfaceStore interfaceStore)
		{
			_logger = logger;
			_interfaceStore = interfaceStore;
		}


		public IReadOnlyDictionary<string, string> GetMockTextByNamespace(string interfaceName, IndentationConfig indentationConfig)
		{
			var result = new Dictionary<string, string>();

			var namespaceDict = _interfaceStore.GetInterfaceDefinitionByNamespace(interfaceName);
			if(namespaceDict == null)
			{
				_logger.LogError($"Unable to retrieve interface definition for '{interfaceName}'.");
				return result;
			}

			return namespaceDict
				.Keys
				.Select(nsName => new
				{
					Namespace = nsName,
					Text = GetTextForDefinition(interfaceName, nsName, namespaceDict[nsName], indentationConfig)
				})
				.ToDictionary(pair => pair.Namespace, pair => pair.Text);
		}


		private string GetTextForDefinition(
			string interfaceName,
			string namespaceName,
			Model.InterfaceDefinition definition,
			IndentationConfig indentationConfig
		)
		{
			/*

				given the interface method:

					public interface IStringAnalyzer
					{
						string this[string key] { get; set; }
						public string SomeUrl { get; }
						int HowManyItems(string patient, char charToCount);
					}
				
				we want to return the following:

					var stringAnalyzer = new Mock<IStringAnalyzer>();

					stringAnalyzer
						.Setup(x => x[It.IsAny<string>()])
						.Returns((string key) => return default);

					stringAnalyzer
						.SetupSet(x => x[It.IsAny<string>()] = It.IsAny<string>())
						.Callback((string key, string value) => return);

					stringAnalyzer.SetupGet(x => x.SomeUrl).Returns(/ fill me in /);

					Expression<Func<IStringAnalyzer, int>> howManyItems = x =>
						x.HowManyItems(It.IsAny<string>(), It.IsAny<char>());

					stringAnalyzer
						.Setup(howManyItems)
						.Returns((string patient, char charToCount) =>
						{
							return default;
						});

					stringAnalyzer.Verify(howManyItems, Times.Once);

				for interfaces with multiple methods, we'll inject an extra blank line between the new Mock<T> declaration and
				each given Expression/Setup pair

				we will just let the mock.Verify() methods be one line after another with no additional whitespace
			*/

			var watch = new Stopwatch();
			watch.Start();

			var results = new List<string>();
			var tab = indentationConfig.IndentString;

			var mockName = interfaceName;
			if(mockName.StartsWith('I'))
				mockName = mockName.Substring(1);

			mockName = Camelify(mockName);

			var interfacePlusArgs = definition.InterfaceNameWithGenerics ?? definition.InterfaceName;

			results.Add(
				// var stringAnalyzer = new Mock<IStringAnalyzer>();
				$"var {mockName} = new Mock<{interfacePlusArgs}>();"
				);


			if(
				definition.Methods.Count + 
				definition.Properties.Count +
				(definition.Indexer != null ? 1 : 0)
				> 1
			)
				results.Add("");

			if(definition.Indexer != null)
			{
				if(definition.Indexer.HasGet)
				{
					// stringAnalyzer
					//	.Setup(x => x[It.IsAny<string>()])
					//	.Returns((string key) => return default);

					results.Add(mockName);
					results.Add(
						$"{tab}.Setup(x => x[It.IsAny<{definition.Indexer.KeyType}>()])"
					);
					results.Add(
						$"{tab}.Returns(({definition.Indexer.KeyType} key) => default);"
					);
				}

				if(definition.Indexer.HasSet)
				{
					// stringAnalyzer
					// 	.SetupSet(x => x[It.IsAny<string>()] = It.IsAny<string>())
					// 	.Callback((string key, string value) => {});

					results.Add(mockName);
					results.Add(
						$"{tab}.SetupSet(x => x[It.IsAny<{definition.Indexer.KeyType}>()] = It.IsAny<{definition.Indexer.ReturnType}>())"
					);
					results.Add(
						$"{tab}.Callback(({definition.Indexer.KeyType} key, {definition.Indexer.ReturnType} value) => {{}});"
					);
				}
			}

			foreach(var property in definition.Properties)
			{
				results.Add(
					// stringAnalyzer.SetupGet(x => x.SomeUrl).Returns(/ fill me in /);
					$"{mockName}.SetupGet(x => x.{property}).Returns(/* fill me in */);"
				);
			}
			if(definition.Properties.Count > 0 && definition.Methods.Count > 0)
				results.Add("");

			var verifyQueue = new Queue<string>();
			foreach(var method in definition.Methods)
			{

				var methodName = method.MethodName;
				var camelName = Camelify(methodName);
				
				var parameterDeclaration = string.Join(", ", 
					method.Parameters.Select(p => $"It.IsAny<{p.ParameterType}>()"));


				if(method.ReturnType == "void")
				{
					results.AddRange(new []
					{
						// Expression<Action<IStringAnalyzer>> howManyItems = x =>
						//	x.HowManyItems(It.IsAny<string>(), It.IsAny<char>());
						$"Expression<Action<{interfacePlusArgs}>> {camelName} = x =>",
						$"{tab}x.{methodName}({parameterDeclaration});"
					});
				}
				else
				{
					results.AddRange(new []
					{
						// Expression<Func<IStringAnalyzer, int>> howManyItems = x =>
						//	x.HowManyItems(It.IsAny<string>(), It.IsAny<char>());
						$"Expression<Func<{interfacePlusArgs}, {method.ReturnType}>> {camelName} = x =>",
						$"{tab}x.{methodName}({parameterDeclaration});"
					});
				}

				results.Add("");

				// stringAnalyzer
				//	.Setup(howManyItems)
				results.Add(
					$"{mockName}"
				);
				results.Add(
					$"{tab}.Setup({camelName})"
				);

				// string patient, char charToCount
				var callbackDeclaration = string.Join(", ", 
					method.Parameters.Select(p => $"{p.ParameterDefinition}"
					));

				var setupType = 
					method.ReturnType == "void"
					? "Callback"
					: "Returns"
					;

				results.Add(
					//	.Returns((string patient, char charToCount) =>
					$"{tab}.{setupType}(({callbackDeclaration}) =>"
				);


				// and finally, this:

				//	{
				//		return;
				//	}
				//	);

				results.Add($"{tab}{{");

				if(method.ReturnType == "void")
					results.Add($"{tab}{tab}return;");
				else
					results.Add($"{tab}{tab}return default;");
				results.Add($"{tab}}});");
				results.Add("");


				// stringAnalyzer.Verify(howManyItems, Times.Once);
				verifyQueue.Enqueue($"{mockName}.Verify({camelName}, Times.Once);");
			}
			
			// add mock.Verify() asserts
			while(verifyQueue.Count > 0)
				results.Add(verifyQueue.Dequeue());


			
			var currentIndentString = string.Join("",
				Enumerable.Repeat(indentationConfig.IndentString, indentationConfig.CurrentIndentationLevel)
				);

			var lines =  results.Select(l => 
				l.Length > 0
				? currentIndentString + l
				: l
				);

			watch.StopAndLogInformation(_logger, $"time to generate moq for namespace {namespaceName} and interface {interfaceName}: ");
			return string.Join(Environment.NewLine, lines);
		}


		private string Camelify(string input)
		{
			if(input == null || input.Length == 0)
				return input;

			var orig = input.ToCharArray();
			orig[0] = char.ToLowerInvariant(orig[0]);
			return new string(orig);
		}
	}
}