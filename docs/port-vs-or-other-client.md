# Porting to Visual Studio (or another client)

plan:

- reorganize repo to successfully deploy to vscode without adding a new client yet
	- research
		- MAYBE diff this port branch against previous branch `reorg-for-clients`
		- mainly, research what a basic extension needs
	- tasks
		- move everything in folder client to `clients\vscode`
			- probably: change .vscode files to properly launch extension host in debug mode
			- probably: change client/src/extension.ts to find the DLL in debug mode
			- probably: come up with multi-target deploy plan, affecting prelaunch-checklist.md, build.bat
		- kill extra json files (pacakge, package-lock, tsconfig)
		- build as vsix (vsce) and install locally to make sure it still works
			- https://code.visualstudio.com/api/working-with-extensions/publishing-extension#packaging-extensions
- setup `clients\visual-studio`
- NICE TO HAVE: figure out a multi-deploy strategy (ie, update server, re-deploy to ALL clients)
	- research
		- github action to do this? or better off doing the same
		- should the versions across clients be tracked? probably


## google searches

this search got me the blog with the single extension serving three clients, there may be other decent things in the search results:
`example LSP with multiple clients`


## vscode extension resources

is this a bullshit indexer page? is this just duplicate of the official vscode dox, or is it more useful?
https://vscode-docs.readthedocs.io/en/stable/extensions/example-language-server/

blog (likely old)-- just covers putting together a vscode extension:
https://www.syncfusion.com/blogs/post/creating-extensions-for-visual-studio-code-a-complete-guide.aspx


this has a lot of examples, not just LSP, so it might help me find the minimum required for vscode, but it's from microsoft, and they love unnecessary resources, even in their examples.
https://github.com/microsoft/vscode-extension-samples


so about bundling an lsp client/server for vs code
https://stackoverflow.com/questions/67247206/how-to-properly-bundle-a-language-server-extension-for-vscode-with-client-and-se



## multi-client resources

bicep appears to have both a vscode and vs client
https://github.com/Azure/bicep


`ACTUAL BLOG about creating extension for THREE different clients!!!`
https://www.toptal.com/javascript/language-server-protocol-tutorial


## Main visual studio extension resources

ms dox:
https://learn.microsoft.com/en-us/visualstudio/extensibility/starting-to-develop-visual-studio-extensions?view=vs-2022


this just has a link to ms dox above, but also mentions a community and a repo where people might answer questions:
https://visualstudio.microsoft.com/vs/features/extend/

ms dox SPECIFICALLY about creating an LSP extension:
https://learn.microsoft.com/en-us/visualstudio/extensibility/adding-an-lsp-extension?view=vs-2022





