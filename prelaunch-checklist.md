## publish steps:

(In main directory, speedy-moq)
1) npm run compile
2) build.bat (because for some reason I can't run npm in the bat file yet)
3) publish:
	- vsce login daveoftheparty
	- vsce publish patch -m "publishing extension: is this thing on?"



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
