using System.Collections.Generic;
using MoqGenerator.Model.Lsp;

namespace MoqGenerator.Interfaces.Lsp
{
	public interface IMockText
	{
		#warning deprecate this method:
		string GetMockText(string interfaceName, IndentationConfig indentationConfig);
		IReadOnlyDictionary<string, string> GetMockTextByNamespace(string interfaceName, IndentationConfig indentationConfig);
	}
}