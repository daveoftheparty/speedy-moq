using Moq;
using System.Threading.Tasks;
using NUnit.Framework;
using System.IO;
using MoqGenerator.Interfaces.Lsp;
using MoqGenerator.UnitTests.Utils;
using MoqGenerator.Services;
using MoqGenerator.Model.Lsp;
using System;

namespace MoqGenerator.IntegrationTests
{
	public class InterfaceStoreRunner
	{
		/*
			not very scientific, but you can see the runtimes here when using Buildalyzer.Build() to get project refs:
			
			hello from InterfaceStore:0c4d2d69-348e-45c9-b016-79fd49d87959 ctor...
			method GetCsProjFromCsFile is looking for a .csproj related to file ../../../../Demo/Demo.Lib.UnitTests/StringAnalyzerTests.cs
			method GetCsProjFromCsFile is searching for a .csproj in ..\..\..\..\Demo\Demo.Lib.UnitTests
			time to find csproj file: 0ms
			method GetCsProjFromCsFile found ..\..\..\..\Demo\Demo.Lib.UnitTests\Demo.Lib.UnitTests.csproj
			time to build projs to get refs: [ss.ms] 03.666
			time to add projs to workspace: [ss.ms] 03.822
			time to get semantic models: 926ms
			time to get interface definitions from semantic models: 20ms
			LogDefinitions was called by LoadCSProjAsync. CsProjs loaded: c:\src\daveoftheparty\speedy-moq\server\Demo\Demo.Lib.UnitTests\Demo.Lib.UnitTests.csproj|c:\src\daveoftheparty\speedy-moq\server\Demo\Demo.Lib\Demo.Lib.csproj
			LogDefinitions was called by LoadCSProjAsync. Interfaces loaded: ISomeMagicSauce|IStringAnalyzer
			(single file) time to get semantic models: 1ms
			(single file) time to get interface definitions from semantic models: 0ms

			and here's the runtimes for the new refactor, loading project references by reading the XML of the csproj themselves:

			InterfaceStore Logger entries:
			hello from InterfaceStore:eb50958c-1d0f-49c0-9106-c532e3ffe713 ctor...
			time to build projs to get refs: 7ms
			time to add projs to workspace: [ss.ms] 03.768
			time to get semantic models: 942ms
			time to get interface definitions from semantic models: 20ms
			LogDefinitions was called by LoadCSProjAsync. CsProjs loaded: c:\src\daveoftheparty\speedy-moq\server\Demo\Demo.Lib.UnitTests\Demo.Lib.UnitTests.csproj|c:\src\daveoftheparty\speedy-moq\server\Demo\Demo.Lib\Demo.Lib.csproj
			LogDefinitions was called by LoadCSProjAsync. Interfaces loaded: ISomeMagicSauce|IStringAnalyzer
			(single file) time to get semantic models: 1ms
			(single file) time to get interface definitions from semantic models: 0ms
			
			ProjectHandler Logger entries:
			method GetCsProjFromCsFile is looking for a .csproj related to file ../../../../Demo/Demo.Lib.UnitTests/StringAnalyzerTests.cs
			method GetCsProjFromCsFile is searching for a .csproj in ..\..\..\..\Demo\Demo.Lib.UnitTests
			time to find csproj file: 1ms
			method GetCsProjFromCsFile found ..\..\..\..\Demo\Demo.Lib.UnitTests\Demo.Lib.UnitTests.csproj
			Loading project references for project c:\src\daveoftheparty\speedy-moq\server\Demo\Demo.Lib.UnitTests\Demo.Lib.UnitTests.csproj
			Loading project references for project c:\src\daveoftheparty\speedy-moq\server\Demo\Demo.Lib\Demo.Lib.csproj
		*/
		[Test]
		public async Task TestDemo()
		{
			var uri = "../../../../Demo/Demo.Lib.UnitTests/StringAnalyzerTests.cs";
			// C:\src\daveoftheparty\speedy-moq\server\Demo\Demo.Lib.UnitTests\Demo.Lib.UnitTests.csproj
			var text = File.ReadAllText(uri);

			var whoaCowboy = new Mock<IWhoaCowboy>();
			whoaCowboy.SetupGet(x => x.GiddyUp).Returns(true);

			var storeLogger = new LoggerDouble<InterfaceStore>();
			var projectHandlerLogger = new LoggerDouble<ProjectHandler>();

			var store = new InterfaceStore(storeLogger, whoaCowboy.Object, new ProjectHandler(projectHandlerLogger));
			await store.LoadDefinitionsIfNecessaryAsync(
				new TextDocumentItem
				(
					new TextDocumentIdentifier(uri, 0),
					Constants.LanguageId,
					text
				)
			);

			Assert.IsNotNull(store.GetInterfaceDefinitionByNamespace("IStringAnalyzer"));
			Assert.IsNotNull(store.GetInterfaceDefinitionByNamespace("ISomeMagicSauce"));

			Console.WriteLine("InterfaceStore Logger entries:");
			storeLogger.LogEntries.ForEach(e => Console.WriteLine(e.ToString()));

			Console.WriteLine("ProjectHandler Logger entries:");
			projectHandlerLogger.LogEntries.ForEach(e => Console.WriteLine(e.ToString()));
		}
	}
}