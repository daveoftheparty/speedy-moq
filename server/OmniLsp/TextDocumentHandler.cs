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
using MoqGenerator.Interfaces.Lsp;
using OmniLsp.Adapters;

namespace OmniLsp
{
	public class TextDocumentHandler : TextDocumentSyncHandlerBase
	{
		private readonly ILogger<TextDocumentHandler> _logger;
		private readonly ILanguageServerFacade _router;
		private readonly IDiagnoser _diagnoser;
		private readonly IInterfaceStore _interfaceStore;
		private readonly string _thisInstance = Guid.NewGuid().ToString();
		
		public TextDocumentHandler(ILogger<TextDocumentHandler> logger, ILanguageServerFacade router, IDiagnoser diagnoser, IInterfaceStore interfaceStore)
		{
			_logger = logger;
			_router = router;
			_diagnoser = diagnoser;
			_interfaceStore = interfaceStore;

			_logger.LogInformation($"hello from {nameof(TextDocumentHandler)}:{_thisInstance} ctor...");
		}

		#region TextDocumentSyncHandlerBase overrides

		private readonly DocumentSelector _documentSelector = new DocumentSelector(new DocumentFilter { Pattern = MoqGenerator.Constants.FileGlob });

		public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri) => new TextDocumentAttributes(uri, MoqGenerator.Constants.LanguageId);
		
		protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions(SynchronizationCapability capability, ClientCapabilities clientCapabilities)
		{
			return new TextDocumentSyncRegistrationOptions
			{
				DocumentSelector = DocumentSelector.ForLanguage(MoqGenerator.Constants.LanguageId),
				Change = TextDocumentSyncKind.Full
			};
		}

		public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
		{
			var textDoc = TextDocAdapter.From(request.TextDocument);
			_ = _interfaceStore.LoadDefinitionsIfNecessaryAsync(textDoc);

			PublishDiagnostics(
				textDoc,
				request.TextDocument.Uri,
				"textDocument/didOpen");
			return Unit.Task;
		}

		public override Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
		{
			var textDoc = TextDocAdapter.From(request, MoqGenerator.Constants.LanguageId);
			_ = _interfaceStore.LoadDefinitionsIfNecessaryAsync(textDoc);

			PublishDiagnostics(
				textDoc,
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



		private void PublishDiagnostics(MoqGenerator.Model.Lsp.TextDocumentItem textDoc, DocumentUri uri, string who)
		{
			

			try
			{
				_logger.LogInformation($"requesting diagnostics, triggered by {who}");
				var diagnostics = _diagnoser.GetDiagnostics(textDoc);
					
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