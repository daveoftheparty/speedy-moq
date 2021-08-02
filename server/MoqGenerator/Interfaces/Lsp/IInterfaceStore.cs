using System.Threading.Tasks;
using MoqGenerator.Model;
using MoqGenerator.Model.Lsp;

namespace MoqGenerator.Interfaces.Lsp
{
	public interface IInterfaceStore
	{
		InterfaceDefinition GetInterfaceDefinition(string interfaceName);

		Task LoadDefinitionsIfNecessaryAsync(TextDocumentItem textDocItem);
		bool Exists(string interfaceName);
	}
}