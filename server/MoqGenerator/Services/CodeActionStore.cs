using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

using MoqGenerator.Interfaces.Lsp;
using Payload=System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IReadOnlyList<MoqGenerator.Model.Lsp.TextEdit>>;

namespace MoqGenerator.Services;

// this class ONLY exists because Visual Studio sucks and I can't get Diagnostic data to/from TextDocument <== > CodeAction
public class CodeActionStore : ICodeActionStore
{
	private readonly Dictionary<Guid, Payload> _store = new();

	private readonly ILogger<CodeActionStore> _logger;

	public CodeActionStore(ILogger<CodeActionStore> logger)
	{
		_logger = logger;
	}

	public Guid StoreAction(Payload action)
	{
		var guid = Guid.NewGuid();
		_store.Add(guid, action);
		_logger.LogInformation($"store depth: {_store.Count}");
		return guid;
	}

	public Payload GetAction(Guid key) => _store[key];
}