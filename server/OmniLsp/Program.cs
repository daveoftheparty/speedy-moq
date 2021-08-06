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
				.ConfigureLogging(
					x => x
						.AddLanguageProtocolLogging()
						.SetMinimumLevel(LogLevel.Debug)
				)
				.WithServices(x => x.AddLogging(b => b.SetMinimumLevel(LogLevel.Error))) // I bumped this up to see if we could filter out Omnisharp, it filtered absolutely nothing....
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
		}
	}
}
