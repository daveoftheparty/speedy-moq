using System.Threading.Tasks;
using Features.Model;
using Features.Model.Lsp;

namespace Features.Interfaces.Lsp
{
	public interface IInterfaceStore
	{
		InterfaceDefinition GetInterfaceDefinition(string interfaceName);

		Task LoadDefinitionsIfNecessaryAsync(TextDocumentItem textDocItem);
	}
}