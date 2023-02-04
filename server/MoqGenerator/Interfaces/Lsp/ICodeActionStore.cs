using System;
using Payload=System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IReadOnlyList<MoqGenerator.Model.Lsp.TextEdit>>;

namespace MoqGenerator.Interfaces.Lsp;

// this interface ONLY exists because Visual Studio sucks and I can't get Diagnostic data to/from TextDocument <== > CodeAction
public interface ICodeActionStore
{
	Guid StoreAction(Payload action);
	Payload GetAction(Guid key);
}