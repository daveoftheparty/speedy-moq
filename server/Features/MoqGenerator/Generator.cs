using System.Threading.Tasks;
using Features.Interfaces;

namespace Features.MoqGenerator
{
	public class Generator : IFullTextReplace
	{
		public async Task<string> GetReplacementAsync(string text)
		{
			/*
				if ! bool IHasFileChanged(uri fileUri, string text) // store a dictionary of file URI and MD5?
					return orig text / text + IsChanged bool sentinel?

				if bool IMockText.AlreadyMocked(uri fileUri, string interfaceName) // store a dictionary of file URI and list of mocked interfaces?


				mockText = string IMockText.IGetMock(uri fileUri, string text) // add the "new" interface[s] to the IAlreadyMocked dict
				
				insert mockText into original input text and return...
			*/
			return await Task.FromResult(text);
		}
	}
}