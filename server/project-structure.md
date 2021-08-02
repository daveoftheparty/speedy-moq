# Server Project Structure

C# projects:

- server
	- MoqGenerator
	- MoqGenerator.UnitTests
	- OmniLsp
	

- MoqGenerator is where the work of the extension/code generation happens
- OmniLsp communicates with the IDE


For now:

All communication between OmniLsp <==> MoqGenerator will go through simple c# interfaces.

Therefore, the OmniLsp is responsible for adapting to the MoqGenerator interfaces.