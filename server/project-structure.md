# Server Project Structure

C# projects:

- server
	- Features
	- IntegrationTests.Features??
	- OmniLsp
	- UnitTests.Features
	- UnitTests.OmniLsp??
	- GuineaPig

A few of these are self-explanatory, for those that arent:
- Features is where the work of the extension/code generation happens
- OmniLsp communicates with the IDE
- GuineaPig is just a project to run manual UI tests inside the extension host during development/debugging

For now:

All communication between OmniLsp <==> Features will go through simple c# interfaces.

Therefore, the OmniLsp is responsible for adapting to the Features interfaces.

I feel like I've witnessed buggy behavior with OmniSharp's LSP implementation and I want to insulate from that.