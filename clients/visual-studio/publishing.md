# Publishing Visual Studio

- [Publishing Visual Studio](#publishing-visual-studio)
	- [getting lang server in client](#getting-lang-server-in-client)
	- [Manual publishing](#manual-publishing)


is this the command: 

`VsixPublisher.exe publish -payload "{path to vsix}" -publishManifest "{path to vs-publish.json}" -ignoreWarnings "VSIXValidatorWarning01,VSIXValidatorWarning02"`


Maybe some fuckin help with the awful publisher: https://github.com/jcansdale/DogfoodVsix/tree/master
here's a another sample extension repo that's on the marketplace, and it seems to have an msbuild command line at the root in appveyor.yml: https://github.com/madskristensen/ImageOptimizer/tree/master

## getting lang server in client
some links:
https://www.google.com/search?q=VSIXSourceItem
https://stackoverflow.com/questions/53089844/vsix-how-to-include-xml-documentation-files-in-package
https://stackoverflow.com/questions/43726044/how-to-include-assemblies-from-nuget-packages-in-a-vsix-installer
https://learn.microsoft.com/en-us/answers/questions/84987/how-to-auto-include-dependencies-in-a-vsix-project

and if all that fails, do a post-build command line to put the release version of server in a subfolder?

## Manual publishing

see screenshots in this directory

I typed in "speedy-moq-visual-studio" as internal name.
I copypasta'd the readme markdown from root of this repo (the one that has always been the overview/readme for vscode ext)
then i had to upoad the demo.gif and put it in the right place in the markdown
manually added "coding, testing" categories from my publishManifest.json
manually had to copy and paste the tags, one by one from source.extension.vsixmanifest -- using semi colon or comma delimiter tried to make it a single tag cuz their interface sucks
manually copied in repo url
