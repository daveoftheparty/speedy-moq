using System.Collections.Generic;
using System.Threading.Tasks;
using Features.Model.Lsp;

namespace Features.Interfaces.Lsp
{
	public interface IDiagnoser
	{
		Task<IEnumerable<Diagnostic>> GetDiagnosticsAsync(TextDocumentItem item);
	}
}