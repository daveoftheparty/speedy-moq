namespace Features.Interfaces
{
	public interface IFileHandler
	{
		bool HasFileChanged(string filePath, string text);
	}
}