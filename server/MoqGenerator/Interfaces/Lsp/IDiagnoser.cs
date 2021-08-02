using System.Collections.Generic;
using MoqGenerator.Model.Lsp;

namespace MoqGenerator.Interfaces.Lsp
{
	public interface IDiagnoser
	{
		IEnumerable<Diagnostic> GetDiagnostics(TextDocumentItem item);
	}
}