using MoqGenerator.Model.Lsp;

namespace MoqGenerator.Interfaces.Lsp
{
	public interface IIndentation
	{
		IndentationConfig GetIndentationConfig(string text, uint currentLine);
	}
}