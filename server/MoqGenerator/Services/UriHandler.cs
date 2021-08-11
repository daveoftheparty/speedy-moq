using System.IO;

using MoqGenerator.Interfaces.Lsp;
using MoqGenerator.Model.Lsp;

namespace MoqGenerator.Services
{
	public class UriHandler : IUriHandler
	{
		public string GetFilePath(TextDocumentIdentifier textDocId)
		{
			var nonStandardizedPath = textDocId.Uri.Replace("file:///", "");
			return Path.GetFullPath(nonStandardizedPath);
		}
	}
}