using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;

namespace BoilerMoq
{
	public class CommandHandler : ExecuteCommandHandlerBase
	{
		private readonly string _command;
		private readonly ISerializer _serializer;
		private readonly ILogger<CommandHandler> _logger;

		public CommandHandler(ISerializer serializer, ILogger<CommandHandler> logger)
		{
			_command = "boilerMoq.go";
			_serializer = serializer;
			_logger = logger;
		}

		public override Task<Unit> Handle(ExecuteCommandParams request, CancellationToken cancellationToken)
		{
			_logger.LogInformation($"hey got a callback in ExecuteCommandParams!");
			return Unit.Task;
		}

		// this doesn't have to be overridden if inheriting from one of the ExecuteTypedCommandHandlerBase<T, T, ...> classes when/if we need to send args with the command
		protected override ExecuteCommandRegistrationOptions CreateRegistrationOptions(ExecuteCommandCapability capability, ClientCapabilities clientCapabilities)
		{
			return new ExecuteCommandRegistrationOptions { Commands = new Container<string>(_command) };
		}
	}
}