using MoqGenerator.Model.Lsp;

namespace MoqGenerator.Interfaces.Lsp
{
	public interface IUriHandler
	{
		string GetFilePath(TextDocumentIdentifier textDocId);
	}
}