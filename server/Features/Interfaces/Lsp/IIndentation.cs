using Features.Model.Lsp;

namespace Features.Interfaces.Lsp
{
	public interface IIndentation
	{
		#warning make async if possible...
		IndentationConfig GetIndentationConfig(string text, Range range);
	}
}