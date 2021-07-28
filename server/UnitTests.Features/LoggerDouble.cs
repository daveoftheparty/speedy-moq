using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace UnitTests.Features
{
	// thanks, Stack Overflow!!! https://stackoverflow.com/questions/52707702/how-do-you-mock-ilogger-loginformation

	public class LoggerDouble<T> : ILogger, ILogger<T>
	{
		public List<LogEntry> LogEntries { get; } = new List<LogEntry>();

		// Add more of these if they make life easier.
		public IEnumerable<LogEntry> InformationEntries =>
			LogEntries.Where(e => e.LogLevel == LogLevel.Information);

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			LogEntries.Add(new LogEntry(logLevel, eventId, state, exception));
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return true;
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			return new LoggingScope();
		}

		public class LoggingScope : IDisposable
		{
			public void Dispose()
			{
			}
		}
	}

	public class LogEntry
	{
		public LogEntry(LogLevel logLevel, EventId eventId, object state, Exception exception)
		{
			LogLevel = logLevel;
			EventId = eventId;
			State = state;
			Exception = exception;
		}

		public LogLevel LogLevel { get; }
		public EventId EventId { get; }
		public object State { get; }
		public Exception Exception { get; }

		public override string ToString()
		{
			var messages = State as IEnumerable<KeyValuePair<string, object>>;
			if(messages == null)
				throw new Exception("Logging is too hard!! This is NOT coming from YOU calling _logger.Log<T>!");

			var message = messages
				.FirstOrDefault(m => m.Key == "{OriginalFormat}")
			;

			if(message.Key == null)
				throw new Exception("Logging is too hard!! The dictionary of log messages isn't right, and it's not YOUR fault!");
			
			var sMessage = message.Value as string;
			if(sMessage == null)
				throw new Exception("Logging is too hard!! Why can't it be easier!?!");

			return sMessage;
		}
	}
}