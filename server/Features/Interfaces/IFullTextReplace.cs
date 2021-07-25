using System.Threading.Tasks;

namespace Features.Interfaces
{
	public interface IFullTextReplace
	{
		Task<string> GetReplacementAsync(string text);
	}
}