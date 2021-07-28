using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;

using Features.Interfaces.Lsp;
using Features.Model.Lsp;

namespace Features.MoqGenerator
{
	public class MockText : IMockText
	{
		private const int _indentationLevel = 3;
		private readonly ILogger<MockText> _logger;
		private readonly IInterfaceStore _interfaceStore;

		public MockText(ILogger<MockText> logger, IInterfaceStore interfaceStore)
		{
			_logger = logger;
			_interfaceStore = interfaceStore;
		}

		public string GetMockText(string interfaceName)
		{
			/*
				if our dictionary of implementations is empty, go compile the whole project...
				whoops, can't do that... cuz we don't get the right info coming into this interface, and this interface 
				IS RIGHT for what we need from CodeAction.... hmmm...

				I guess we do the full project compilation on Language Server startup, and force this to have it DI injected?
				might be some weirdness for the user if they open a doc that needs the replacement right away, but, the injected
				info isn't ready yet. not much we can do about that...
			*/
			var definition = _interfaceStore.GetInterfaceDefinition(interfaceName);
			if(definition == null)
			{
				_logger.LogError($"Unable to retrieve interface definition for '{interfaceName}'.");
				#warning check this null on the other side!!!
				return (string)null;
			}
				


			/*

				given the interface method:

					public interface IStringAnalyzer
					{
						int HowManyItems(string patient, char charToCount);
					}
				
				we want to return the following:

					var stringAnalyzer = new Mock<IStringAnalyzer>();
					Expression<Func<IStringAnalyzer, int>> howManyItems = x => x.HowManyItems(It.IsAny<string>(), It.IsAny<char>());
					stringAnalyzer
						.Setup(howManyItems)
						.Returns((string patient, char charToCount) =>
						{
							return 0;
						});

					stringAnalyzer.Verify(howManyItems, Times.Once);

				for interfaces with multiple methods, we'll inject an extra blank line between the new Mock<T> declaration and
				each given Expression/Setup pair

				we will just let the mock.Verify() methods be one line after another with no additional whitespace
			*/
			var results = new List<string>();

			var mockName = interfaceName;
			if(mockName.StartsWith('I'))
				mockName = mockName.Substring(1);

			mockName = Camelify(mockName);

			results.Add(
				// var stringAnalyzer = new Mock<IStringAnalyzer>();
				$"var {mockName} = new Mock<{interfaceName}>();"
				);


			if(definition.Methods.Count > 1)
				results.Add("");

			var verifyQueue = new Queue<string>();
			foreach(var method in definition.Methods)
			{

				var methodName = method.MethodName;
				var camelName = Camelify(methodName);
				
				var parameterDeclaration = string.Join(", ", 
					method.Parameters.Select(p => $"It.IsAny<{p.ParameterType}>()"));


				results.Add(
					// Expression<Func<IStringAnalyzer, int>> howManyItems = x => x.HowManyItems(It.IsAny<string>(), It.IsAny<char>());
					$"Expression<Func<{interfaceName}, {method.ReturnType}>> {camelName} = x => x.{methodName}({parameterDeclaration});"
				);


				// stringAnalyzer
				//	.Setup(howManyItems)
				results.Add(
					$"{mockName}"
				);
				results.Add(
					$"	.Setup({camelName})"
				);

				// string patient, char charToCount
				var callbackDeclaration = string.Join(", ", 
					method.Parameters.Select(p => $"{p.ParameterDefinition}"
					));

				results.Add(
					//	.Returns((string patient, char charToCount) =>
					$"	.Returns(({callbackDeclaration}) =>"
				);

				
				//	{
				//		return;
				//	}
				//	);
				
				results.Add("	{");
				results.Add($"		return;");
				results.Add("	});");
				results.Add("");


				// stringAnalyzer.Verify(howManyItems, Times.Once);
				verifyQueue.Enqueue($"{mockName}.Verify({camelName}, Times.Once);");
			}
			
			// add mock.Verify() asserts
			while(verifyQueue.Count > 0)
				results.Add(verifyQueue.Dequeue());


			var lines =  results.Select(l => 
				l.Length > 0
				? new string('\t', _indentationLevel) + l
				: l
				);


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