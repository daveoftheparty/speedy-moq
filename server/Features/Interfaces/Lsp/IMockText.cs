using Features.Model.Lsp;

namespace Features.Interfaces.Lsp
{
	public interface IMockText
	{
		string GetMockText(TextDocumentIdentifier textDocumentIdentifier, string interfaceName);
	}
}