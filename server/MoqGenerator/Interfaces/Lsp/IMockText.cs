using MoqGenerator.Model.Lsp;

namespace MoqGenerator.Interfaces.Lsp
{
	public interface IMockText
	{
		string GetMockText(string interfaceName, IndentationConfig indentationConfig);
	}
}