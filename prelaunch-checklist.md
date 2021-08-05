package.json:
- trace settings, should this name be:
	- speedy-moq.trace.server
	- speedymoq.trace.server
	- speedyMoq.trace.server
	- SpeedyMoq.trace.server
version: use semver to go from 0.0.0 to 0.0.1?
- i think i want to cd to main directory (speedy-moq) and run:
	- vsce login daveoftheparty
	- vsce publish patch -m "hey beta, it's your birthday"


IT'S... ALIVE???

```
Publishing daveoftheparty.speedy-moq@0.0.3...
 INFO  Extension URL (might take a few minutes): https://marketplace.visualstudio.com/items?itemName=daveoftheparty.speedy-moq        
 INFO  Hub URL: https://marketplace.visualstudio.com/manage/publishers/daveoftheparty/extensions/speedy-moq/hub
 DONE  Published daveoftheparty.speedy-moq@0.0.3.
```

whelp... learned a couple things.
1) using the versioning when publishing will increment the version even on a failed publish, it took me 3 tries, I ended up going from version 0.0.0 to actually succeeding to publish 0.0.3, and yes, git was tagged each time
2) the demo.gif is broken in VS code and on the marketplace for some reason, but may be because the repo isn't public yet
3) server failed to start, error is: `* You intended to execute a .NET program, but dotnet-c:\Users\dave\.vscode\extensions\daveoftheparty.speedy-moq-0.0.3\server\OmniLsp.dll does not exist.`
	- maybe I need to explicitly mention the "out" folder in extension.ts when building the path?
	- actual path to dll is: C:\Users\dave\.vscode\extensions\daveoftheparty.speedy-moq-0.0.3\client\out\server