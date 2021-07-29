using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OmniSharp.Extensions.LanguageServer.Server;

using Features.Interfaces.Lsp;
using Features.MoqGenerator;

namespace OmniLsp
{
	class Program
	{
		static async Task Main(string[] args)
		{
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
				.WithServices(ConfigureServices)
				.WithHandler<TextDocumentHandler>()
				.WithHandler<CodeActionHandler>()
				);

			await server.WaitForExit;
		}

		static void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton<IInterfaceStore, InterfaceStore>();


			services.AddTransient<IDiagnoser, Diagnoser>();
			services.AddTransient<IMockText, MockText>();
			services.AddTransient<IIndentation, Indentation>();
		}
	}
}
