- [Picking this back up 2023-10-25](#picking-this-back-up-2023-10-25)
	- [testing in Visual Studio](#testing-in-visual-studio)
	- [next steps](#next-steps)
- [Porting to Visual Studio (or another client)](#porting-to-visual-studio-or-another-client)
	- [google searches](#google-searches)
	- [vscode extension resources](#vscode-extension-resources)
	- [multi-client resources](#multi-client-resources)
	- [Main visual studio extension resources](#main-visual-studio-extension-resources)
		- [VisualStudio.Exensibility](#visualstudioexensibility)

# Picking this back up 2023-10-25
boy did I let this sit long (from February)

## testing in Visual Studio
To launch/test extension in Visual Studio, you actually need to start in VS. Open solution `.\daveoftheparty\speedy-moq\clients\visual-studio\VisualStudioClient.sln` and in there, you can just press F5. I don't recall if you can then debug the server from VSCode, or you'd have to do an attach process from within either VSCode or Visual Studio... but this is how you launch and manually test

## next steps
lots of links and info below, but VSCode still works from F5, Visual Studio as well.
really the next thing is all about packaging/deploying the extension for both clients and any cleanup/reorg to do now.

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



### VisualStudio.Exensibility 
YouTube playlist on the new VisualStudio.Exensibility model
https://www.youtube.com/watch?v=L5zYUZvWnJE&list=PLReL099Y5nRc6m-CLanAhWGO3_7DD_1Nu&index=1

a repo that has samples:
https://github.com/microsoft/VSExtensibility

