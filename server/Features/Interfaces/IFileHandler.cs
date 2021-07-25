using System.Threading.Tasks;

namespace Features.Interfaces
{
	public interface IFileHandler
	{
		Task<bool> HasFileChangedAsync(string filePath, string text);
	}
}