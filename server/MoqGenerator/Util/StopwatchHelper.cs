using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MoqGenerator.Util
{
	public static class StopwatchHelper
	{

		public static void StopAndLogInformation(this Stopwatch watch, ILogger logger, string messagePrefix) => StopAndLog(watch, logger, messagePrefix, LogLevel.Information);
		public static void StopAndLogWarning(this Stopwatch watch, ILogger logger, string messagePrefix) => StopAndLog(watch, logger, messagePrefix, LogLevel.Warning);
		public static void StopAndLogError(this Stopwatch watch, ILogger logger, string messagePrefix) => StopAndLog(watch, logger, messagePrefix, LogLevel.Error);

		private static void StopAndLog(Stopwatch watch, ILogger logger, string messagePrefix, LogLevel level)
		{
			watch.Stop();
			
			if(string.IsNullOrEmpty(messagePrefix))
				messagePrefix = "Elapsed Time:";

				
			var spacer = 
				char.IsWhiteSpace(messagePrefix[messagePrefix.Length - 1])
				? ""
				: " ";

			logger.Log(level, messagePrefix + spacer + FormatSpan(watch.Elapsed));
		}

		public static string FormatSpan(TimeSpan ts)
		{
			if(ts.Days > 0)
				return $"[days:hh:mm:ss.ms] {ts.Days}:{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
			else if (ts.Hours > 0)
				return $"[hh:mm:ss.ms] {ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
			else if (ts.Minutes > 0)
				return $"[mm:ss.ms] {ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
			else if (ts.Seconds > 0)
				return $"[ss.ms] {ts.Seconds:00}.{ts.Milliseconds:000}";
			else
				return $"{ts.Milliseconds}ms";
		}
	}
}