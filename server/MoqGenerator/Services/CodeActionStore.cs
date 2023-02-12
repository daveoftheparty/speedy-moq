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

	public CodeActionStore(ILogger<CodeActionStore> logger)
	{
		_logger = logger;
	}

	public string StoreAction(Payload action)
	{
		#warning trim the store using some sort of queue/cache
		var key = $"{Constants.DiagnosticSource}({Guid.NewGuid().ToString()})";
		_store.Add(key, action);
		_logger.LogInformation($"store depth: {_store.Count}");
		return key;
	}

	public Payload GetAction(string key) => _store[key];
}