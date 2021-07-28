using Features.Model.Lsp;

namespace Features.Interfaces.Lsp
{
	public interface IMockText
	{
		#warning send indentation level, indentation string into this interface method:
		string GetMockText(string interfaceName);
	}
}