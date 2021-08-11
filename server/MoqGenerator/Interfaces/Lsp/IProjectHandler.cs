using System.Collections.Generic;
using MoqGenerator.Model.Lsp;

namespace MoqGenerator.Interfaces.Lsp
{
	public interface IProjectHandler
	{
		string GetCsProjFromCsFile(TextDocumentIdentifier textDocId);
		IEnumerable<string> GetProjectAndProjectReferences(string csProjPath);
	}
}