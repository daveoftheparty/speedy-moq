using System.Collections.Generic;
using MoqGenerator.Model.Lsp;

namespace MoqGenerator.Interfaces.Lsp
{
	public interface IMockText
	{
		IReadOnlyDictionary<string, string> GetMockTextByNamespace(string interfaceName, IndentationConfig indentationConfig);
	}
}