using System.Threading.Tasks;
using Features.Interfaces;
using Features.Model.Lsp;
using Microsoft.Extensions.Logging;

namespace Features.MoqGenerator
{
	public class MockText : IMockText
	{
		private readonly ILogger<MockText> _logger;
		private readonly IInterfaceStore _interfaceStore;

		public MockText(ILogger<MockText> logger, IInterfaceStore interfaceStore)
		{
			_logger = logger;
			_interfaceStore = interfaceStore;
		}

		public async Task<string> GetMockTextAsync(TextDocumentIdentifier textDocumentIdentifier, DiagnosticData data)
		{
			/*
				if our dictionary of implementations is empty, go compile the whole project...
				whoops, can't do that... cuz we don't get the right info coming into this interface, and this interface 
				IS RIGHT for what we need from CodeAction.... hmmm...

				I guess we do the full project compilation on Language Server startup, and force this to have it DI injected?
				might be some weirdness for the user if they open a doc that needs the replacement right away, but, the injected
				info isn't ready yet. not much we can do about that...
			*/
			var definition = _interfaceStore.GetInterfaceDefinition(data.InterfaceName);
			if(definition == null)
			{
				_logger.LogError($"Unable to retrieve interface definition for '{data.InterfaceName}'. Text document we are working with is: {textDocumentIdentifier.Uri}");
				return await Task.FromResult((string)null);	
			}
				

			return await Task.FromResult("hey, how YOU doin??");
		}
	}
}