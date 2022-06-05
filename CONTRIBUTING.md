# Contributing

## Table of Contents
- [Contributing](#contributing)
  - [Architecture](#architecture)
  - [Building/launching from a fresh clone](#buildinglaunching-from-a-fresh-clone)
  - [Debugging the server](#debugging-the-server)
## Architecture

See [docs](./docs/readme.md)

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

There does not seem to be ANY way that I can tell to suppress the log messages (that are very verbose) that are of this type:

```
[Trace - 8:46:27 PM] Sending request 'initialize - (0)'.
[Trace - 8:46:28 PM] Received notification 'window/logMessage'.
[Trace - 8:46:32 PM] Received notification 'textDocument/publishDiagnostics'.
[Trace - 8:46:28 PM] Sending notification 'textDocument/didOpen'.
```

In addition, when _**I**_ try to log anything at Trace or Debug level, my log levels don't print out:

```
MoqGenerator.Services.InterfaceStore: time to build projs to get refs: 6ms | 
OmniLsp.TextDocumentHandler: requesting diagnostics, triggered by textDocument/didOpen | 
```

AND I don't know where the pipe is coming from at the end of every log message, maybe an OmniSharp "feature" maybe a VSCode "feature"-- not a huge deal.

Also, these two log lines are the result of a Log.LogError and Log.LogCritical, respectively:
```
[Error - 8:46:28 PM] OmniLsp.TextDocumentHandler: hello from TextDocumentHandler:0fb0c0b4-ef8f-406c-a054-a00bb3e6fcb9 ctor... | 
[Error - 8:46:28 PM] OmniLsp.TextDocumentHandler: hello from TextDocumentHandler:0fb0c0b4-ef8f-406c-a054-a00bb3e6fcb9 ctor... | 
```
From here on out, I'm going to elevate all my log messages to Information and above, skipping the broken Critical level (not that I need it)-- in other words:
- `ILogger<T>.LogInformation()`
- `ILogger<T>.LogWarning()`
- `ILogger<T>.LogError()`

I'm also going to break a cardinal rule of logging (all information related to the message on a single log line), because as a human that reads the logs, I need them readable. In VSCode, logging the interface definitions will result in this:

```
[Info  - 9:16:23 PM] MoqGenerator.Services.InterfaceStore: LogDefinitions was called by LoadCSProjAsync. Interfaces loaded: | 
[Trace - 9:16:23 PM] Received notification 'window/logMessage'.
[Info  - 9:16:23 PM] MoqGenerator.Services.InterfaceStore: 				Two.Lib.IDealio : e:\temp\TwoInterface\Two.Lib\IDealio.cs | 
[Trace - 9:16:23 PM] Received notification 'window/logMessage'.
[Info  - 9:16:23 PM] MoqGenerator.Services.InterfaceStore: 				Two.Lib.Version1.IDealio : e:\temp\TwoInterface\Two.Lib\Version1\IDealio.cs | 
[Trace - 9:16:23 PM] Received notification 'window/logMessage'.
[Info  - 9:16:23 PM] MoqGenerator.Services.InterfaceStore: 				Two.Lib.Version2.IDealio : e:\temp\TwoInterface\Two.Lib\Version2\IDealio.cs | 
```

Which, at least I can READ, and looks even better after run through a Notepad++ regex find/replace this value: `^(?!\[.+?\] MoqGenerator).*` with nothing and then remove blank lines:

```
[Info  - 9:16:23 PM] MoqGenerator.Services.InterfaceStore: LogDefinitions was called by LoadCSProjAsync. Interfaces loaded: | 
[Info  - 9:16:23 PM] MoqGenerator.Services.InterfaceStore: 				Two.Lib.IDealio : e:\temp\TwoInterface\Two.Lib\IDealio.cs | 
[Info  - 9:16:23 PM] MoqGenerator.Services.InterfaceStore: 				Two.Lib.Version1.IDealio : e:\temp\TwoInterface\Two.Lib\Version1\IDealio.cs | 
[Info  - 9:16:23 PM] MoqGenerator.Services.InterfaceStore: 				Two.Lib.Version2.IDealio : e:\temp\TwoInterface\Two.Lib\Version2\IDealio.cs | 
```

As opposed to trying to cram all the data into a json blob on the log line, which, a machine could read, but I can't, not at first glance:
```
[Info  - 9:25:28 PM] MoqGenerator.Services.InterfaceStore: LogDefinitions was called by foo. Interfaces loaded: [{"Namespace":"Two.Lib.IDealio","File":"e:\\temp\\TwoInterface\\Two.Lib\\IDealio.cs"},{"Namespace":"Two.Lib.Version1.IDealio","File":"e:\\temp\\TwoInterface\\Two.Lib\\Version1\\IDealio.cs"},{"Namespace":"Two.Lib.Version2.IDealio","File":"e:\\temp\\TwoInterface\\Two.Lib\\Version2\\IDealio.cs"}] | 
```