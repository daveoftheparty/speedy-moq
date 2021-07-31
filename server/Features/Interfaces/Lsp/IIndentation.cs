using Features.Model.Lsp;

namespace Features.Interfaces.Lsp
{
	public interface IIndentation
	{
		IndentationConfig GetIndentationConfig(string text, Range range);
	}
}