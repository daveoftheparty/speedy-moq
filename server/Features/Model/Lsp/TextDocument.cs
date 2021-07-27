namespace Features.Model.Lsp
{
	public record TextDocumentIdentifier(string Uri, int Version);
	public record TextDocumentItem(TextDocumentIdentifier Identifier, string LanguageId, string Text);
}