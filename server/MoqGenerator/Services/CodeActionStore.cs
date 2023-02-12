using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

using MoqGenerator.Interfaces.Lsp;
using Payload=System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IReadOnlyList<MoqGenerator.Model.Lsp.TextEdit>>;

namespace MoqGenerator.Services;

// this class ONLY exists because Visual Studio as a client doesn't seem to persist Diagnostic.Data? between TextDocument and CodeAction
public class CodeActionStore : ICodeActionStore
{
	private readonly Dictionary<string, Payload> _store = new();

	private readonly ILogger<CodeActionStore> _logger;
	private readonly DateTime? _utcNow;

	private class CacheInfo
	{
		public DateTime CachedOn { get; init; }
		public DateTime? RetrievedOn { get; set; }
	}

	private readonly Dictionary<string, CacheInfo> _cache = new();

	public CodeActionStore(ILogger<CodeActionStore> logger)
	{
		_logger = logger;
	}

	public CodeActionStore(ILogger<CodeActionStore> logger, DateTime utcNow)
	{
		_logger = logger;
		_utcNow = utcNow;
	}

	public string StoreAction(Payload action)
	{
		var key = $"{Constants.DiagnosticSource}({Guid.NewGuid().ToString()})";
		_store.Add(key, action);

		_cache.Add(key, new CacheInfo { CachedOn = DateTime.UtcNow });
		TrimCache();
		return key;
	}


	public Payload GetAction(string key)
	{
		var result = _store[key];
		_cache[key].RetrievedOn = DateTime.UtcNow;
		TrimCache();
		return result;
	} 

	private void TrimCache()
	{
		var now = _utcNow ?? DateTime.UtcNow;
		_logger.LogInformation($"store depth, pre-trim: {_store.Count}");


		// if it's been retrieved and is more than 3 minutes old, kill it
		// if it has not been retrieved and is more than 15 minutes old, kill it
		foreach(var pair in _cache)
		{
			var oldAndUsed = pair.Value.RetrievedOn.HasValue && pair.Value.RetrievedOn.Value < now.AddMinutes(-3);
			var oldUnused = !pair.Value.RetrievedOn.HasValue && pair.Value.CachedOn < now.AddMinutes(-15);

			if(oldAndUsed || oldUnused)
			{
				_logger.LogInformation($"removing {pair.Key} which was cached on {pair.Value.CachedOn} and retrieved on:{pair.Value.RetrievedOn}");
				_cache.Remove(pair.Key);
				_store.Remove(pair.Key);
			}
		}
		
		_logger.LogInformation($"store depth, post-trim: {_store.Count}");
	}
}