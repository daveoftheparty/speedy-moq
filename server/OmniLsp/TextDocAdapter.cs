using System.Linq;

using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace OmniLsp
{
	public class TextDocAdapter
	{
		public static Features.Model.Lsp.TextDocumentItem From(OmniSharp.Extensions.LanguageServer.Protocol.Models.TextDocumentItem doc)
		{
			return new Features.Model.Lsp.TextDocumentItem
			(
				new Features.Model.Lsp.TextDocumentIdentifier(doc.Uri.ToString(), doc.Version ?? 0),
				doc.LanguageId,
				doc.Text
			);
		}

		public static Features.Model.Lsp.TextDocumentItem From(DidChangeTextDocumentParams doc, string LanguageId)
		{
			return new Features.Model.Lsp.TextDocumentItem
			(
				new Features.Model.Lsp.TextDocumentIdentifier(doc.TextDocument.Uri.ToString(), doc.TextDocument.Version ?? 0),
				LanguageId,
				doc.ContentChanges.First().Text
			);
		}
	}
}