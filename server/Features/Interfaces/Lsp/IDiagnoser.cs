using System.Collections.Generic;
using Features.Model.Lsp;

namespace Features.Interfaces.Lsp
{
	public interface IDiagnoser
	{
		IEnumerable<Diagnostic> GetDiagnostics(TextDocumentItem item);
	}
}