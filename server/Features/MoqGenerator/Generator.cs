using System.Threading.Tasks;
using Features.Interfaces;

namespace Features.MoqGenerator
{
	public class Generator : IFullTextReplace
	{
		public async Task<string> GetReplacementAsync(string text)
		{
			return await Task.FromResult(text);
		}
	}
}