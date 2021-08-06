# Contributing

## Building/launching from a fresh clone

At the root of the project, run both of these commands:
- npm ci
- npm run compile

then, switch to ./server:
- dotnet build

After that, you should be able to launch the extension using F5, and debug the server by using the "Attach to Server" launch configuration, if necessary.

I am currently developing this extension under:
- node 14.15.3
- npm 6.14.13

## Debugging the server

The goal of the extension is to load up interface definitions as quickly as possible. Therefore, when the TextDocumentHandler gets a notification that a doc is opened or modified, diagnostics are immediately triggered, and, loading up the definitions is immediately triggered. The downside of this approach for the developer is that by the time you get your debugger attached, the roslyn code is already well under way to building the semantic models, and you can't catch a break (literally, a breakpoint) before that happens. So, by default, when running in debug mode (the Extension Host) a value is passed in that will keep InterfaceStore from building definitions. Anywhere you need this "attach debugger, and THEN do work" functionality, just inject IWhoaCowboy and don't "do work" if GiddyUp == false (follow the model in InterfaceStore.)

So-- attach your debugger, set your breakpoint, and then, step INTO the implementation of IWhoaCowboy and set the single property to true at runtime so that you can continue stepping through interesting code.

If you don't need this functionality for a particular run of the extension, just change the "false" to "true" in the args array of extension.ts on the debug config as shown below:

```javascript
const serverOptions: ServerOptions = {
    run: { command: serverExe, args: [releaseServer, "true"], transport: TransportKind.pipe },
    debug: { command: serverExe, args: [debugServer, "false"], transport: TransportKind.pipe, runtime: "" }
};
```

## New models in code

I'm trying to keep the implementation of the language server more or less separate (or as separate as possible) from the LSP provider. I'm not saying there's a better choice out there for C#, but, I had a lot of trouble trying to get it up and running, due to an extreme lack of documentation. I hope not to have to replace the OmniLsp project, but, if I do, I want the transition as smooth as possible, so:

If you need a new model to communicate between OmniLsp <==> MoqGenerator, take the model from this spec, version 3.16:

https://microsoft.github.io/language-server-protocol/specification

The model should live in the MoqGenerator.Model.Lsp namespace

## Logging in the server

I spent more hours than I care to count trying to make logging work propertly. I have not yet found a way to suppress the logs from the language client itself, and I have ALSO not found a way for LogTrace() or LogDebug() to show up in logs (properly). LogDebug() will show up in logs, but without the header `[Debug - <time>]` like you will get from LogInfo() or above.

Whether or not it's possible to surface an `ILogger.Log<Trace|Debug>()` statement to the client, I'd like to, for now, keep log levels where they're at.

What this means to you as you're developing is that you may want to see some log statements that aren't normally surfaced to the client, so you may temporarily need to use level `Information` or higher to see the logs in the VSCode output window-- but before making a PR please adjust log levels appropriately.