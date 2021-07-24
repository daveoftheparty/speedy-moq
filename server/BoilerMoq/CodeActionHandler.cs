using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
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
				DocumentSelector = new DocumentSelector(new DocumentFilter { Pattern = "**/*.cs" } )
			};
		}

		public Task<CommandOrCodeActionContainer> Handle(CodeActionParams request, CancellationToken cancellationToken)
		{
			_logger.LogWarning("Code action isn't setup yet!!!");
			
			#warning boilerMoq magic string...
			#warning also, just fuckin great, omnisharp is using newtonsoft...
			var container = request.Context.Diagnostics?
				.Where(diagnostic => diagnostic.Source.Equals("boilerMoq", StringComparison.OrdinalIgnoreCase))
				.Select(diagnostic => {
					var url = $"http://google.com";
					var title = $"Click for more information {url}";
					return new CommandOrCodeAction(new CodeAction {
						Title = title,
						Diagnostics = new [] { diagnostic },
						Kind = CodeActionKind.QuickFix,
						Command = new Command {
							Name = "chocolatey.open",
							Title = title,
							Arguments = new JArray(url)
						},
						// Edit = new WorkspaceEdit
						// {
						// 	Changes = 
						// }
					});
				}).ToArray() ?? Array.Empty<CommandOrCodeAction>();

			return Task.FromResult(new CommandOrCodeActionContainer(container));
		}
		#endregion ICodeActionHandler
	}
}