using Payload=System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IReadOnlyList<MoqGenerator.Model.Lsp.TextEdit>>;

namespace MoqGenerator.Interfaces.Lsp;

public interface ICodeActionStore
{
	string StoreAction(Payload action);
	Payload GetAction(string key);
}