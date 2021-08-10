using System.Collections.Generic;
using System.Threading.Tasks;
using MoqGenerator.Model;
using MoqGenerator.Model.Lsp;

namespace MoqGenerator.Interfaces.Lsp
{
	public interface IInterfaceStore
	{
		Dictionary<string, InterfaceDefinition> GetInterfaceDefinitionByNamespace(string interfaceName);
		Task LoadDefinitionsIfNecessaryAsync(TextDocumentItem textDocItem);
		bool Exists(string interfaceName);
	}
}