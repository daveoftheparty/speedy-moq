using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace BoilerMoq
{
	public class TextDocumentHandler : TextDocumentSyncHandlerBase
	{

		

		private readonly ILogger<TextDocumentHandler> _logger;
		private readonly LogTestin _logTestin;

		public TextDocumentHandler(ILogger<TextDocumentHandler> logger, LogTestin logTestin)
		{
			_logger = logger;
			_logTestin = logTestin;
			_logger.LogTrace($"hello from {nameof(TextDocumentHandler)} ctor...");
			_logTestin.SayFoo();
		}

		private readonly DocumentSelector _documentSelector = new DocumentSelector(new DocumentFilter { Pattern = "**/*.cs" });

		// absolutely zero clue what this method is for or why I'm forced to override it...
		public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri) => new TextDocumentAttributes(uri, "csharp");
		
		public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
		{
			return Unit.Task;
		}

		public override Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
		{
			_logger.LogTrace($"hey got a callback in DidChangeTextDocumentParams!");
			return Unit.Task;
		}

		public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
		{
			_logger.LogTrace($"hey got a callback in DidSaveTextDocumentParams!");
			return Unit.Task;
		}

		public override Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
		{
			return Unit.Task;
		}

		protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions(SynchronizationCapability capability, ClientCapabilities clientCapabilities)
		{
			return new TextDocumentSyncRegistrationOptions
			{
				DocumentSelector = _documentSelector
			};
		}
	}
}