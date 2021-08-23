using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace MoqGenerator.Lsp
{
	public class DebugUtils
	{
		public static void DumpData<T>(ILogger logger, T request)
		{
			logger.LogInformation($"serializing {typeof(T).Name} object:");
			logger.LogInformation(JsonSerializer.Serialize(request));
		}
	}
}