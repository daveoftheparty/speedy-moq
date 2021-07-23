using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;

namespace BoilerMoq
{
	public class AnyCommandParams
	{

	}

	public class AllCommandsHandler : ExecuteTypedCommandHandlerBase<AnyCommandParams>
	{
		
		private static string _command = "boilerMoq.go";
		private readonly ISerializer _serializer;
		private readonly ILogger<AllCommandsHandler> _logger;

		public AllCommandsHandler(ISerializer serializer, ILogger<AllCommandsHandler> logger)
			: base(_command, serializer)
		{
			_serializer = serializer;
			_logger = logger;
			_logger.LogInformation($"inside {nameof(AllCommandsHandler)} ctor");
		}

		public override Task<Unit> Handle(AnyCommandParams commandParams, CancellationToken cancellationToken)
		{
			_logger.LogInformation($"hey got a callback in {nameof(AllCommandsHandler)} to process {nameof(AnyCommandParams)}!");
			return Unit.Task;
		}
	}
}