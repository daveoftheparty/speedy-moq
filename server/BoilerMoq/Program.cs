using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OmniSharp.Extensions.LanguageServer.Server;

namespace BoilerMoq
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
				);

			await server.WaitForExit;
		}

		static void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton<LogTestin>();
			// services.AddSingleton<TextDocumentHandler>();
		}
	}

#warning this ol' class here can go later...
	public class LogTestin
	{
		private readonly ILogger<LogTestin> _logger;

		public LogTestin(ILogger<LogTestin> logger)
		{
			logger.LogInformation($"inside {nameof(LogTestin)} ctor");
			_logger = logger;
		}

		public void SayFoo() => _logger.LogInformation($"{nameof(LogTestin)} says howdy, mate!");
	}
}
