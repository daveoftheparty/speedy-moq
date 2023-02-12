using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OmniSharp.Extensions.LanguageServer.Server;

using MoqGenerator.Interfaces.Lsp;
using MoqGenerator.Services;

namespace OmniLsp
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var (giddyUp, canReceiveDiagnosticData) = ParseArgs(args);

			
			var server = await LanguageServer.From(options =>
				options
				.WithInput(Console.OpenStandardInput())
				.WithOutput(Console.OpenStandardOutput())
				.ConfigureLogging(x => x
					// plenty of rants & raves about logging in CONTRIBUTING.md at the root of this repo 😎
					.AddLanguageProtocolLogging() // seems to be required to get any of OUR logs
					.AddFilter("OmniSharp", LogLevel.Information)
					.AddFilter("MoqGenerator", LogLevel.Information)
					.AddFilter("OmniLsp", LogLevel.Information)
				)
				.WithServices(x => ConfigureServices(x, giddyUp, canReceiveDiagnosticData))
				.WithHandler<TextDocumentHandler>()
				.WithHandler<CodeActionHandler>()
				);

			await server.WaitForExit;
		}

		static void ConfigureServices(IServiceCollection services, bool giddyUp, bool canReceiveDiagnosticData)
		{
			services.AddSingleton(typeof(IWhoaCowboy), new WhoaCowboy { GiddyUp = giddyUp });
			services.AddSingleton(typeof(IClientAbilities), new ClientAbilities { CanReceiveDiagnosticData = canReceiveDiagnosticData });

			services.AddSingleton<IInterfaceStore, InterfaceStore>();
			services.AddSingleton<ICodeActionStore, CodeActionStore>();

			services.AddTransient<IDiagnoser, Diagnoser>();
			services.AddTransient<IMockText, MockText>();
			services.AddTransient<IIndentation, Indentation>();
			services.AddTransient<IProjectHandler, ProjectHandler>();
			services.AddTransient<IUriHandler, UriHandler>();
			services.AddTransient<IInterfaceGenericsBuilder, InterfaceGenericsBuilder>();
			services.AddTransient<ITestFileFilter, TestFileFilter>();
		}

		static (bool giddyUp, bool canReceiveDiagnosticData) ParseArgs(string[] args)
		{
			var usage = new ArgumentException(
					"Pass in true or false for the first parameter. True means GO!" +
					" False means halt until a debugger can be attached." +
					" Second param needs to be language client name and match switch statement in Program.cs" +
					$" Args passed (pipe delimited): [{string.Join('|', args)}]"
					);

			if(args.Length != 2)
				throw usage;

			var giddyUp = false;
			var canReceiveDiagnosticData = false;

			try
			{
				giddyUp = Convert.ToBoolean(args[0]);
			}
			catch
			{
				throw usage;
			}

			switch (args[1])
			{
				case "vscode":
					canReceiveDiagnosticData = true;
					break;
				case "visual-studio":
					canReceiveDiagnosticData = false;
					break;
				default:
					throw usage;
			}

			return (giddyUp, canReceiveDiagnosticData);
		}
	}
}
