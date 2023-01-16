## git steps
- get your thing ready for the wild
  - run unit tests
  - do a dotnet clean / dotnet build cycle to check for new warnings
  - update changelog.md
- publish to marketplace
- merge your PR into main


## publish steps:

(In main directory, speedy-moq)
1) npm run compile
2) build.bat (because for some reason I can't run npm in the bat file yet)
3) publish:
	- vsce login daveoftheparty
	- ~~vsce publish patch -m "publishing extension: is this thing on?"~~
	- vsce publish patch -m "publishing extension: is this thing on?"


The publish command breaks down like so:
- `vsce` is just the tool, see [Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- `publish` sends it to the marketplace
- `patch` says increment the patch version, aka 1.2.3 -> 1.2.4
- `-m "yer comment"` is the commit message to git; publishing creates a new git commit

TLDR;
if ya want control over the version you can also type it in directly instead of using `<major | minor | patch>` like so: `vsce publish 3.2.1` The git commit *should* have the version as the comment message...


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
