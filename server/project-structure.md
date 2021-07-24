# Server Project Structure

C# projects:

- server
	- Features
	- LanguageServer
	- IntegrationTests.Features??
	- UnitTests.Features
	- UnitTests.LanguageServer??
	- GuineaPig

A few of these are self-explanatory, for those that arent:
- Features is where the work of the extension/code generation happens
- Language Server uses some existing library to communicate with the IDE
- GuineaPig is just a project to run manual UI tests inside the extension host during development/debugging

For now:
LanguageServer will use OmniSharp to talk to the IDE.

All communication between LanguageServer <==> Features will go through simple c# interfaces.

Therefore, the LanguageServer is responsible for adapting to the Features interfaces.