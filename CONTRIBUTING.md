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

## New models in code

I'm trying to keep the implementation of the language server more or less separate (or as separate as possible) from the LSP provider. I'm not saying there's a better choice out there for C#, but, I had a lot of trouble trying to get it up and running, due to an extreme lack of documentation. I hope not to have to replace the OmniLsp project, but, if I do, I want the transition as smooth as possible, so:

If you need a new model to communicate between OmniLsp <==> MoqGenerator, take the model from this spec, version 3.16:

https://microsoft.github.io/language-server-protocol/specification

The model should live in the MoqGenerator.Model.Lsp namespace

## Logging in the server

I spent more hours than I care to count trying to make logging work propertly. I have not yet found a way to suppress the logs from the language client itself, and I have ALSO not found a way for LogTrace() or LogDebug() to show up in logs (properly). LogDebug() will show up in logs, but without the header `[Debug - <time>]` like you will get from LogInfo() or above.

Whether or not it's possible to surface an `ILogger.Log<Trace|Debug>()` statement to the client, I'd like to, for now, keep log levels where they're at.

What this means to you as you're developing is that you may want to see some log statements that aren't normally surfaced to the client, so you may temporarily need to use level `Information` or higher to see the logs in the VSCode output window-- but before making a PR please adjust log levels appropriately.