using System;
using System.Collections.Generic;
using System.Text.Json;
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

namespace OmniLsp
{
	public class TextDocumentHandler : TextDocumentSyncHandlerBase
	{
		private readonly ILogger<TextDocumentHandler> _logger;
		private readonly ILanguageServerFacade _router;

		public TextDocumentHandler(ILogger<TextDocumentHandler> logger, ILanguageServerFacade router)
		{
			_logger = logger;
			_router = router;
			_logger.LogInformation($"hello from {nameof(TextDocumentHandler)} ctor...");
		}

		#region magic strings

		private const string _magicLanguage = "csharp";
		private const string _magicFileGlob = "**/*.cs";
		
		#endregion magic strings

		#region TextDocumentSyncHandlerBase overrides

		private readonly DocumentSelector _documentSelector = new DocumentSelector(new DocumentFilter { Pattern = _magicFileGlob });

		public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri) => new TextDocumentAttributes(uri, _magicLanguage);
		
		protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions(SynchronizationCapability capability, ClientCapabilities clientCapabilities)
		{
			return new TextDocumentSyncRegistrationOptions
			{
				DocumentSelector = DocumentSelector.ForLanguage(_magicLanguage),
				Change = TextDocumentSyncKind.Full
			};
		}

		public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
		{
			return Unit.Task;
		}

		public override Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
		{
			_logger.LogInformation($"hey got a callback in DidChangeTextDocumentParams!");

			DumpRequest<DidChangeTextDocumentParams>(request);
			
			PublishDiagnostics(request.TextDocument.Uri, "DidChangeTextDocumentParams");
			return Unit.Task;
		}

		public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
		{
			_logger.LogInformation($"hey got a callback in DidSaveTextDocumentParams!");

			DumpRequest<DidSaveTextDocumentParams>(request);
			PublishDiagnostics(request.TextDocument.Uri, "DidSaveTextDocumentParams");
			return Unit.Task;
		}

		public override Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
		{
			return Unit.Task;
		}

		#endregion TextDocumentSyncHandlerBase overrides


		private void DumpRequest<T>(T request)
		{
			_logger.LogInformation($"request from {typeof(T).Name} event");
			_logger.LogInformation(JsonSerializer.Serialize(request));


			/*
			an example request:
				{
					"TextDocument": {
						"Version": 26,
						"Uri": {
						"Scheme": "file",
						"Authority": "",
						"Path": "/e:/temp/junk.cs",
						"Query": "",
						"Fragment": ""
						}
					},
					"ContentChanges": [
						{
						"Range": null,
						"RangeLength": 0,
						"Text": "namespace Wtf public class WtfChuck (full file here...)"
						}
					]
				}
			*/
		}


		private bool _alreadyDoneDidIt = false;
		private void PublishDiagnostics(DocumentUri uri, string who)
		{
			// let's publish some diagnostics...
			if(_alreadyDoneDidIt)
			{
				_logger.LogInformation($"we've already published this diagnostic before!: {who}");
				return;
			}

			_alreadyDoneDidIt = true;
			_logger.LogInformation($"trying to publish, yo, from: {who}");
			var diagnostics = new List<Diagnostic>();
			try
			{
				diagnostics.Add(
					new Diagnostic
					{
						Code = "BoilerMoq_0xId",
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