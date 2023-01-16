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
			var giddyUp = false;
			try
			{
				giddyUp = Convert.ToBoolean(args[0]);
			}
			catch
			{
				throw new ArgumentException(
					"Pass in true or false for the first parameter. True means GO!" +
					" False means halt until a debugger can be attached." +
					$" Args passed (pipe delimited): [{string.Join('|', args)}]"
					);
			}

			var server = await LanguageServer.From(options =>
				options
				.WithInput(Console.OpenStandardInput())
				.WithOutput(Console.OpenStandardOutput())
				.ConfigureLogging(x => x
					// plenty of rants & raves about logging in CONTRIBUTING.md at the root of this repo 😎
					.AddLanguageProtocolLogging() // seems to be required to get any of OUR logs
					.AddFilter("OmniSharp", LogLevel.Warning)
					.AddFilter("MoqGenerator", LogLevel.Information)
				)
				.WithServices(x => ConfigureServices(x, giddyUp))
				.WithHandler<TextDocumentHandler>()
				.WithHandler<CodeActionHandler>()
				);

			await server.WaitForExit;
		}

		static void ConfigureServices(IServiceCollection services, bool giddyUp)
		{
			services.AddSingleton(typeof(IWhoaCowboy), new WhoaCowboy { GiddyUp = giddyUp });

			services.AddSingleton<IInterfaceStore, InterfaceStore>();


			services.AddTransient<IDiagnoser, Diagnoser>();
			services.AddTransient<IMockText, MockText>();
			services.AddTransient<IIndentation, Indentation>();
			services.AddTransient<IProjectHandler, ProjectHandler>();
			services.AddTransient<IUriHandler, UriHandler>();
			services.AddTransient<IInterfaceGenericsBuilder, InterfaceGenericsBuilder>();
			services.AddTransient<ITestFileFilter, TestFileFilter>();
		}
	}
}
