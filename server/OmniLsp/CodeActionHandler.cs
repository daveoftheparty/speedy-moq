using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

using MoqGenerator.Interfaces.Lsp;

namespace OmniLsp
{
	public class CodeActionHandler : ICodeActionHandler
	{
		private readonly ILogger<CodeActionHandler> _logger;

		public CodeActionHandler(ILogger<CodeActionHandler> logger)
		{
			_logger = logger;
		}

		#region ICodeActionHandler
		public CodeActionRegistrationOptions GetRegistrationOptions(CodeActionCapability capability, ClientCapabilities clientCapabilities)
		{
			return new CodeActionRegistrationOptions
			{
				DocumentSelector = new DocumentSelector(new DocumentFilter { Pattern = MoqGenerator.Constants.FileGlob} ),
				CodeActionKinds = new Container<CodeActionKind>(CodeActionKind.QuickFix),
				ResolveProvider = false
			};
		}

		public Task<CommandOrCodeActionContainer> Handle(CodeActionParams request, CancellationToken cancellationToken)
		{
			_logger.LogTrace("Code action firing...");
			
			var actions = request.Context.Diagnostics?
				.Where(diagnostic => diagnostic.Source.Equals(MoqGenerator.Constants.DiagnosticSource, StringComparison.OrdinalIgnoreCase))
				.Select(diagnostic =>
				{
					var ourDocId = TextDocAdapter.From(request.TextDocument);
					var interfaceName = diagnostic.Data.ToString();

					return new CommandOrCodeAction(new CodeAction
					{
						Title = MoqGenerator.Constants.CodeActionFixTitle,
						Diagnostics = new [] { diagnostic },
						Kind = CodeActionKind.QuickFix,
						Edit = new WorkspaceEdit
						{
							Changes = new Dictionary<DocumentUri, IEnumerable<TextEdit>>
							{
								{
									request.TextDocument.Uri,
									new List<TextEdit>
									{
										new TextEdit
										{
											NewText = diagnostic.Data.ToString(),
											Range = diagnostic.Range
										}
									}
								}
							}
						}
					});
				}).ToArray() ?? Array.Empty<CommandOrCodeAction>();


			actions
				.SelectMany(a => a.CodeAction.Edit.Changes.Values.SelectMany(v => v))
				.ToList()
				.ForEach(e => _logger.LogTrace($"Requesting changes to '{request.TextDocument.Uri.ToString()}' at Start ({e.Range.Start.Line}, {e.Range.Start.Character}), End ({e.Range.End.Line}, {e.Range.End.Character})"))
				;

			return Task.FromResult(new CommandOrCodeActionContainer(actions));
		}
		#endregion ICodeActionHandler
	}
}