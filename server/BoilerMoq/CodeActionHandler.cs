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

namespace BoilerMoq
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
			_logger.LogWarning("Code action isn't setup yet!!!");
			
			#warning boilerMoq magic string...
			
			var container = request.Context.Diagnostics?
				.Where(diagnostic => diagnostic.Source.Equals("boilerMoq", StringComparison.OrdinalIgnoreCase))
				.Select(diagnostic => {
					var url = $"http://google.com";
					var title = $"Click for more information {url}";
					return new CommandOrCodeAction(new CodeAction {
						Title = title,
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
											NewText = "fuckit!!!",
											Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(new Position(2,3), new Position(2,9))
										}
									}
								}
							}
						}
					});
				}).ToArray() ?? Array.Empty<CommandOrCodeAction>();

			return Task.FromResult(new CommandOrCodeActionContainer(container));
		}
		#endregion ICodeActionHandler
	}
}