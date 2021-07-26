using System.Threading.Tasks;
using Features.Model;

namespace Features.Interfaces.Lsp
{
	public interface IFullTextReplace
	{
		Task<FullTextReplaceResponse> GetReplacementAsync(string filePath, string text);
	}
}