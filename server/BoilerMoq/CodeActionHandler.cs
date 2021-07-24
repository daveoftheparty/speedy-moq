using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
			
			// var container = new CommandOrCodeActionContainer[1];
			// var action = new CommandOrCodeAction(new CodeAction 
			// {
			// 	// Title = "hi dave",
			// 	// Kind = CodeActionKind.RefactorInline,
			// 	Command = new Command
			// 	{
					
			// 	}
			// });

			throw new System.NotImplementedException();
		}
		#endregion ICodeActionHandler
	}
}