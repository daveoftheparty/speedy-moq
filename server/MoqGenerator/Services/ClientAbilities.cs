using MoqGenerator.Interfaces.Lsp;

namespace MoqGenerator.Services;

public class ClientAbilities : IClientAbilities
{
	public bool CanReceiveDiagnosticData { get; init; }
}
