using System.Linq;

using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace OmniLsp
{
	public class TextDocAdapter
	{
		public static MoqGenerator.Model.Lsp.TextDocumentItem From(OmniSharp.Extensions.LanguageServer.Protocol.Models.TextDocumentItem doc)
		{
			return new MoqGenerator.Model.Lsp.TextDocumentItem
			(
				new MoqGenerator.Model.Lsp.TextDocumentIdentifier(doc.Uri.ToString(), doc.Version ?? 0),
				doc.LanguageId,
				doc.Text
			);
		}

		public static MoqGenerator.Model.Lsp.TextDocumentItem From(DidChangeTextDocumentParams doc, string LanguageId)
		{
			return new MoqGenerator.Model.Lsp.TextDocumentItem
			(
				new MoqGenerator.Model.Lsp.TextDocumentIdentifier(doc.TextDocument.Uri.ToString(), doc.TextDocument.Version ?? 0),
				LanguageId,
				doc.ContentChanges.First().Text
			);
		}

		public static MoqGenerator.Model.Lsp.TextDocumentIdentifier From(OmniSharp.Extensions.LanguageServer.Protocol.Models.TextDocumentIdentifier documentIdentifier)
		{
			return new MoqGenerator.Model.Lsp.TextDocumentIdentifier
			(
				documentIdentifier.Uri.ToString(),
				0 // hopefully we never need this version!!! 
			);
		}
	}
}