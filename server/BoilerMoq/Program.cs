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
				.WithLoggerFactory(new LoggerFactory())
				.AddDefaultLoggingProvider()
				.WithServices(x => x.AddLogging(b => b.SetMinimumLevel(LogLevel.Trace)))
				.WithServices(ConfigureServices)
				);

			await server.WaitForExit;
		}

		static void ConfigureServices(IServiceCollection services)
		{
		}
	}
}
