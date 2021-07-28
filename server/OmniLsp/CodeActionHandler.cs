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
				DocumentSelector = new DocumentSelector(new DocumentFilter { Pattern = "**/*.cs" } ),
				CodeActionKinds = new Container<CodeActionKind>(CodeActionKind.QuickFix),
				ResolveProvider = false
			};
		}

		public Task<CommandOrCodeActionContainer> Handle(CodeActionParams request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Code action firing...");

			var actions = request.Context.Diagnostics?
				.Where(diagnostic => diagnostic.Source.Equals(Features.Constants.DiagnosticSource, StringComparison.OrdinalIgnoreCase))
				.Select(diagnostic => {
					return new CommandOrCodeAction(new CodeAction {
						Title = "Pimp This Code!",
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
											NewText = "var x = \"Hello, world!\";",
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
				.ForEach(e => _logger.LogInformation($"Start ({e.Range.Start.Line}, {e.Range.Start.Character}), End ({e.Range.End.Line}, {e.Range.End.Character})"))
				;

			return Task.FromResult(new CommandOrCodeActionContainer(actions));
		}
		#endregion ICodeActionHandler
	}
}