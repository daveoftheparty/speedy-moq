using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System.Collections.Generic;
using System;

namespace BoilerMoq
{
	public class TextDocumentHandler : TextDocumentSyncHandlerBase
	{

		

		private readonly ILogger<TextDocumentHandler> _logger;
		private readonly LogTestin _logTestin;

		private readonly ILanguageServerFacade _router;

		public TextDocumentHandler(ILogger<TextDocumentHandler> logger, LogTestin logTestin, ILanguageServerFacade router)
		{
			_logger = logger;
			_logTestin = logTestin;
			var logMessage = $"hello from {nameof(TextDocumentHandler)} ctor...";
			_router = router;
#warning why is SayFoo not working?
			_logger.LogInformation(logMessage);
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
			_logger.LogInformation($"hey got a callback in DidChangeTextDocumentParams!");

			FuckOmnisharp(request.TextDocument.Uri, "DidChangeTextDocumentParams");
			return Unit.Task;
		}

		public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
		{
			_logger.LogInformation($"hey got a callback in DidSaveTextDocumentParams!");
			FuckOmnisharp(request.TextDocument.Uri, "DidSaveTextDocumentParams");
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
				DocumentSelector = _documentSelector,
				Change = TextDocumentSyncKind.Full,
				Save = new SaveOptions()
			};
		}

		private void FuckOmnisharp(DocumentUri uri, string who)
		{
			// let's publish some diagnostics...
			_logger.LogInformation($"trying to publish, yo, from: {who}");
			var diagnostics = new List<Diagnostic>();
			try
			{
				diagnostics.Add(
					new Diagnostic
					{
						Code = "BoilerMoq_0xId",
						// CodeDescription = "I'm a diag!",
						Message = "I'm a diagnostic message",
						Severity = DiagnosticSeverity.Warning,
						Source = "boilerMoq",
						Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(new Position(2,3), new Position(2,9)),
						Data = "huh",
					}
					);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "whelp we threw one!");
			}
			
			try
			{
				if(_router == null)
					_logger.LogError("well no fuckin' wonder, router is null!");

				if(diagnostics == null)
					_logger.LogError("well no fuckin' wonder, diag is null!");

				if(_router.TextDocument == null)
					_logger.LogError("well no fuckin' wonder, TestDocument is null!");
				if(uri == null)	
					_logger.LogError("well no fuckin' wonder, uri is null!");
					
				_router.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams
				{
					Uri = uri,
					Diagnostics = diagnostics
				});
				
			}
			catch (Exception e)
			{
				_logger.LogError(e, "whelp we threw one trying to publish!");
			}
		}
	}
}