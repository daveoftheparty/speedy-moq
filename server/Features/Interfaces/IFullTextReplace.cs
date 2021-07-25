using System.Threading.Tasks;
using Features.Model;

namespace Features.Interfaces
{
	public interface IFullTextReplace
	{
		Task<FullTextReplaceResponse> GetReplacementAsync(string filePath, string text);
	}
}