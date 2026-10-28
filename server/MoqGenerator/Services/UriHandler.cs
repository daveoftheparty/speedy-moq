using System;

using MoqGenerator.Interfaces.Lsp;
using MoqGenerator.Model.Lsp;

namespace MoqGenerator.Services
{
	public class UriHandler : IUriHandler
	{
		public string GetFilePath(TextDocumentIdentifier textDocId)
		{
			// Uri handles percent-decoding, and keeps the leading slash on unix paths
			return new Uri(textDocId.Uri).LocalPath;
		}
	}
}
