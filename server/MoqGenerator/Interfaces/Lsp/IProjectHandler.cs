using System.Collections.Generic;

namespace MoqGenerator.Interfaces.Lsp
{
	public interface IProjectHandler
	{
		string GetCsProjFromCsFile(string uri);
		IEnumerable<string> GetProjectAndProjectReferences(string csProjPath);
	}
}