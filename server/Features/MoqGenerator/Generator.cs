using System.Threading.Tasks;
using Features.Interfaces;
using Features.Model;

namespace Features.MoqGenerator
{
	public class Generator : IFullTextReplace
	{
		private readonly IFileHandler _fileHandler;

		public Generator(IFileHandler fileHandler)
		{
			_fileHandler = fileHandler;
		}

		public async Task<FullTextReplaceResponse> GetReplacementAsync(string filePath, string text)
		{
			/*
				if ! bool IHasFileChanged(uri fileUri, string text) // store a dictionary of file URI and MD5?
					return orig text / text + IsChanged bool sentinel?

				if bool IMockText.AlreadyMocked(uri fileUri, string interfaceName) // store a dictionary of file URI and list of mocked interfaces?


				mockText = string IMockText.IGetMock(uri fileUri, string text) // add the "new" interface[s] to the IAlreadyMocked dict
				
				insert mockText into original input text and return...
			*/
			if(!await _fileHandler.HasFileChangedAsync(filePath, text))
			{
				return new FullTextReplaceResponse() { IsChanged = false, Text = text };
			}

			return new FullTextReplaceResponse() { IsChanged = true, Text = text };
		}
	}
}