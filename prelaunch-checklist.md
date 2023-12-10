## git steps
- get your thing ready for the wild
  - run unit tests
  - do a dotnet clean / dotnet build cycle to check for new warnings
  - update changelog.md
- publish to marketplace (this will create a new commit on your branch that is tagged and update package.json for you, just need to push the commit after successful publich!)
- push commit/tags
- merge your PR into main


## publish steps:

(In main directory, speedy-moq)
1) build.bat
2) publish:
	- vsce login daveoftheparty
	- ~~vsce publish patch -m "publishing extension: is this thing on?"~~
	- vsce publish 0.1.1


The publish command breaks down like so:
- `vsce` is just the tool, see [Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- `publish` sends it to the marketplace
- `patch` says increment the patch version, aka 1.2.3 -> 1.2.4
- `-m "yer comment"` is the commit message to git; publishing creates a new git commit

TLDR;
if ya want control over the version you can also type it in directly instead of using `<major | minor | patch>` like so: `vsce publish 3.2.1` The git commit *should* have the version as the comment message...


## warnings I'm getting from publish that should probably be addressed:

```
This extension consists of 597 files, out of which 262 are JavaScript files. For performance reasons, you should bundle your extension: https://aka.ms/vscode-bundle-extension . You should also exclude unnecessary files by adding them to your .vscodeignore: https://aka.ms/vscode-vscodeignore
Publishing daveoftheparty.speedy-moq@0.1.0...
 INFO  Extension URL (might take a few minutes): https://marketplace.visualstudio.com/items?itemName=daveoftheparty.speedy-moq
 INFO  Hub URL: https://marketplace.visualstudio.com/manage/publishers/daveoftheparty/extensions/speedy-moq/hub
 DONE  Published daveoftheparty.speedy-moq@0.1.0.
 INFO 
The latest version of vsce is 2.15.0 and you have 1.96.1.
Update it now: npm install -g vsce
```

IT'S... ALIVE???

```
Publishing daveoftheparty.speedy-moq@0.0.3...
 INFO  Extension URL (might take a few minutes): https://marketplace.visualstudio.com/items?itemName=daveoftheparty.speedy-moq        
 INFO  Hub URL: https://marketplace.visualstudio.com/manage/publishers/daveoftheparty/extensions/speedy-moq/hub
 DONE  Published daveoftheparty.speedy-moq@0.0.3.
```

whelp... learned a couple things.
1) using the versioning when publishing will increment the version even on a failed publish, it took me 3 tries, I ended up going from version 0.0.0 to actually succeeding to publish 0.0.3, and yes, git was tagged each time

2) install/uninstall locally:
- code --install-extension C:\src\daveoftheparty\speedy-moq\speedy-moq-0.0.3.vsix
- code --uninstall-extension daveoftheparty.speedy-moq
