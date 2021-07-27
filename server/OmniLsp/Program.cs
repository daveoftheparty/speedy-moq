using System;
using System.Threading.Tasks;
using Features.Interfaces.Lsp;
using Features.MoqGenerator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OmniSharp.Extensions.LanguageServer.Server;

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
				.WithServices(x => x.AddLogging(b => b.SetMinimumLevel(LogLevel.Trace)))
				.WithServices(ConfigureServices)
				.WithHandler<TextDocumentHandler>()
				.WithHandler<CodeActionHandler>()
				);

			await server.WaitForExit;
		}

		static void ConfigureServices(IServiceCollection services)
		{
			services.AddTransient<IDiagnoser, Diagnoser>();
		}
	}
}
