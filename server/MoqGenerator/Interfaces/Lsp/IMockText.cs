using System.Collections.Generic;
using MoqGenerator.Model;
using MoqGenerator.Model.Lsp;

namespace MoqGenerator.Interfaces.Lsp
{
	public interface IMockText
	{
		IReadOnlyDictionary<string, string> GetMockTextByNamespace(string interfaceName, InterfaceGenerics userGenerics, IndentationConfig indentationConfig);
	}
}