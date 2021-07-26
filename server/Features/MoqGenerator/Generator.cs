using System.Threading.Tasks;
using Features.Interfaces.Lsp;
using Features.Model;

namespace Features.MoqGenerator
{
	public class Generator : IFullTextReplace
	{
		public async Task<FullTextReplaceResponse> GetReplacementAsync(string filePath, string text)
		{
			/*
				main behavior of this class outside of services it calls is to perform the text replacement
				it shouldn't need to know anything about calculating changes or compilation syntax, just, if there
				are changes to make to the incoming text, make the changes and return the new text..


				decompose:
					IMockText.GetChanges();

					then perform text replaces...

				older thoughts:
				if ! bool IHasFileChanged(uri fileUri, string text) // store a dictionary of file URI and MD5?
					return orig text / text + IsChanged bool sentinel?

				interfacename = IMockText.GetInterfaceName(text); // roslyn syntax or semantic model here to find the raw interface in the test text

				if bool IMockText.AlreadyMocked(uri fileUri, string interfaceName) // store a dictionary of file URI and list of mocked interfaces?


				mockText =   IMockText.IGetMock(uri fileUri, string text) // add the "new" interface[s] to the IAlreadyMocked dict
				
				insert mockText into original input text and return...
			*/

			return await Task.FromResult(new FullTextReplaceResponse() { IsChanged = true, Text = text });
		}
	}
}