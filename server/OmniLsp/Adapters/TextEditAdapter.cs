using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace OmniLsp.Adapters;

public class TextEditAdapter
{
	public static OmniSharp.Extensions.LanguageServer.Protocol.Models.TextEdit From(MoqGenerator.Model.Lsp.TextEdit edit)
	{
		// have to convert our TextEdit to Omni's version:
		return new TextEdit
		{
			Range = new Range(
				(int)edit.Range.start.line,
				(int)edit.Range.start.character,
				(int)edit.Range.end.line,
				(int)edit.Range.end.character),
			NewText = edit.NewText
		};
	}
}
