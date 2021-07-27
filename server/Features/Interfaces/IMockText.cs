using Features.Model.Lsp;

namespace Features.Interfaces
{
	public interface IMockText
	{
		string GetMockText(TextDocumentIdentifier textDocumentIdentifier, string interfaceName);
	}
}