using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using Features.Interfaces.Lsp;

namespace OmniLsp
{
	public class TextDocumentHandler : TextDocumentSyncHandlerBase
	{
		private readonly ILogger<TextDocumentHandler> _logger;
		private readonly ILanguageServerFacade _router;
		private readonly IDiagnoser _diagnoser;

		public TextDocumentHandler(ILogger<TextDocumentHandler> logger, ILanguageServerFacade router, IDiagnoser diagnoser)
		{
			_logger = logger;
			_router = router;
			_diagnoser = diagnoser;

			_logger.LogTrace($"hello from {nameof(TextDocumentHandler)} ctor...");
		}

		#region TextDocumentSyncHandlerBase overrides

		private readonly DocumentSelector _documentSelector = new DocumentSelector(new DocumentFilter { Pattern = Features.Constants.FileGlob });

		public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri) => new TextDocumentAttributes(uri, Features.Constants.LanguageId);
		
		protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions(SynchronizationCapability capability, ClientCapabilities clientCapabilities)
		{
			return new TextDocumentSyncRegistrationOptions
			{
				DocumentSelector = DocumentSelector.ForLanguage(Features.Constants.LanguageId),
				Change = TextDocumentSyncKind.Full
			};
		}

		public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
		{
			PublishDiagnostics(
				TextDocAdapter.From(request.TextDocument),
				request.TextDocument.Uri,
				"textDocument/didOpen");
			return Unit.Task;
		}

		public override Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
		{
			PublishDiagnostics(
				TextDocAdapter.From(request, Features.Constants.LanguageId),
				request.TextDocument.Uri,
				"textDocument/didChange");
			return Unit.Task;
		}

		public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
		{
			return Unit.Task;
		}

		public override Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
		{
			return Unit.Task;
		}

		#endregion TextDocumentSyncHandlerBase overrides



		private void PublishDiagnostics(Features.Model.Lsp.TextDocumentItem textDoc, DocumentUri uri, string who)
		{
			_logger.LogTrace($"trying to publish, yo, from: {who}");

			try
			{
				var diagnosticTask = _diagnoser.GetDiagnosticsAsync(textDoc);
				diagnosticTask.Wait();
				var diagnostics = diagnosticTask.Result;
					
				_router.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams
				{
					Uri = uri,
					Diagnostics = diagnostics.Select(d => DiagnosticAdapter.From(d)).ToList()
				});
				
			}
			catch (Exception e)
			{
				_logger.LogError(e, "whelp we threw one trying to publish!");
			}
		}
	}
}