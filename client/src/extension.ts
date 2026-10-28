/* --------------------------------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for license information.
 * ------------------------------------------------------------------------------------------ */
'use strict';

import * as vscode from 'vscode';
import { LanguageClient, LanguageClientOptions, ServerOptions, TransportKind } from 'vscode-languageclient/node';
import { join } from 'path';

const vsCodeName = 'speedyMoq';
const friendlyName = 'Speedy Moq';
const CSharpPackageName = 'OmniLsp';

let client: LanguageClient | undefined;

export function activate(context: vscode.ExtensionContext) {

    const serverExe = 'dotnet';

    const debugServerLocation = join("server", CSharpPackageName, "bin", "Debug", "net10.0", `${CSharpPackageName}.dll`);
    const releaseServerLocation = join("client", "out", "server", `${CSharpPackageName}.dll`);
    const debugServer = context.asAbsolutePath(debugServerLocation);
    const releaseServer = context.asAbsolutePath(releaseServerLocation);

    // If the extension is launched in debug mode then the debug server options are used
    // Otherwise the run options are used
    const serverOptions: ServerOptions = {
        run: { command: serverExe, args: [releaseServer, "true"], transport: TransportKind.stdio },
        debug: { command: serverExe, args: [debugServer, "true"], transport: TransportKind.stdio, runtime: "" }
        // comment above, uncomment below if you want to debug the server:
        // debug: { command: serverExe, args: [debugServer, "false"], transport: TransportKind.stdio, runtime: "" }
    };

    // Options to control the language client
    const clientOptions: LanguageClientOptions = {
        // Register the server for plain text documents
        documentSelector: [
            {
                scheme: "file",
                language: 'csharp'
            }
        ],
        // synchronize: {
        //     // Synchronize the setting section for our extension to the server
        //     configurationSection: vsCodeName,
        //     fileEvents: vscode.workspace.createFileSystemWatcher('**/*.cs')
        // },
    };

    // Create the language client and start the client.
    client = new LanguageClient(vsCodeName, friendlyName, serverOptions, clientOptions);

    console.log("Speedy Moq is up and rollin...");
    client.start();
}

export function deactivate(): Thenable<void> | undefined {
    return client?.stop();
}