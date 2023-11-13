# Publishing Visual Studio

- [Publishing Visual Studio](#publishing-visual-studio)
	- [Manual publishing](#manual-publishing)


is this the command: 

`VsixPublisher.exe publish -payload "{path to vsix}" -publishManifest "{path to vs-publish.json}" -ignoreWarnings "VSIXValidatorWarning01,VSIXValidatorWarning02"`


Maybe some fuckin help with the awful publisher: https://github.com/jcansdale/DogfoodVsix/tree/master
here's a another sample extension repo that's on the marketplace, and it seems to have an msbuild command line at the root in appveyor.yml: https://github.com/madskristensen/ImageOptimizer/tree/master



## Manual publishing

see screenshots in this directory

I typed in "speedy-moq-visual-studio" as internal name.
I copypasta'd the readme markdown from root of this repo (the one that has always been the overview/readme for vscode ext)
then i had to upoad the demo.gif and put it in the right place in the markdown
manually added "coding, testing" categories from my publishManifest.json
manually had to copy and paste the tags, one by one from source.extension.vsixmanifest -- using semi colon or comma delimiter tried to make it a single tag cuz their interface sucks
manually copied in repo url
