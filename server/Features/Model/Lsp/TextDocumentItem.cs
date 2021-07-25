namespace Features.Model.Lsp
{
	// received from textDocument/didOpen
	public record TextDocumentItem(TextDocumentIdentifier Identifier, string LanguageId, string Text);
}