/* --------------------------------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for license information.
 * ------------------------------------------------------------------------------------------ */
// tslint:disable
'use strict';

import * as vscode from 'vscode';
import { LanguageClient, LanguageClientOptions, SettingMonitor, ServerOptions, TransportKind, InitializeParams } from 'vscode-languageclient/node';
import { Trace } from 'vscode-jsonrpc';
import { join } from 'path';
import { release } from 'os';

const vsCodeName = 'speedyMoq';
const friendlyName = 'Speedy Moq';
const CSharpPackageName = 'OmniLsp';

export function activate(context: vscode.ExtensionContext) {

    const serverExe = 'dotnet';
    
    const debugServerLocation = join("server", CSharpPackageName, "bin", "Debug", "net6.0", `${CSharpPackageName}.dll`);
    const releaseServerLocation = join("client", "out", "server", `${CSharpPackageName}.dll`);
    const debugServer = context.asAbsolutePath(debugServerLocation);
    const releaseServer = context.asAbsolutePath(releaseServerLocation);

    // If the extension is launched in debug mode then the debug server options are used
    // Otherwise the run options are used
    const serverOptions: ServerOptions = {
        run: { command: serverExe, args: [releaseServer, "true", "vscode"], transport: TransportKind.pipe },
        debug: { command: serverExe, args: [debugServer, "true", "vscode"], transport: TransportKind.pipe, runtime: "" }
        // comment above, uncomment below if you want to debug the server:
        // debug: { command: serverExe, args: [debugServer, "false", "vscode"], transport: TransportKind.pipe, runtime: "" }
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
    const client = new LanguageClient(vsCodeName, friendlyName, serverOptions, clientOptions);

    console.log("Speedy Moq is up and rollin...");
    const lsp = client.start();

    // Push the disposable to the context's subscriptions so that the
    // client can be deactivated on extension deactivation
    context.subscriptions.push(lsp);
}