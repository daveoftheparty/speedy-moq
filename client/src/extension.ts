/* --------------------------------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for license information.
 * ------------------------------------------------------------------------------------------ */
// tslint:disable
'use strict';

import { workspace, ExtensionContext, languages } from 'vscode';
import { LanguageClient, LanguageClientOptions, SettingMonitor, ServerOptions, TransportKind, InitializeParams } from 'vscode-languageclient';
import { Trace } from 'vscode-jsonrpc';
import { join } from 'path';
import { release } from 'os';

const vsCodeName = 'boilerMoq';
const friendlyName = 'Boiler Moq';

export function activate(context: ExtensionContext) {

    // The server is implemented in node
    const serverExe = 'dotnet';
    
    const debugServerLocation = join("server", "BoilerMoq", "bin", "Debug", "netcoreapp3.1", "BoilerMoq.exe");
    const releaseServerLocation = join("server", "BoilerMoq", "bin", "Release", "netcoreapp3.1", "publish", "BoilerMoq.dll");
    const debugServer = context.asAbsolutePath(debugServerLocation);
    const releaseServer = context.asAbsolutePath(releaseServerLocation);

    // If the extension is launched in debug mode then the debug server options are used
    // Otherwise the run options are used
    const serverOptions: ServerOptions = {
        run: { command: serverExe, args: [releaseServer] },
        debug: { command: serverExe, args: [debugServer] }
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
        synchronize: {
            // Synchronize the setting section for our extension to the server
            configurationSection: vsCodeName,
            fileEvents: workspace.createFileSystemWatcher('**/*.cs')
        },
    };

    // Create the language client and start the client.
    const client = new LanguageClient(vsCodeName, friendlyName, serverOptions, clientOptions);

    client.trace = Trace.Verbose;
    const disposable = client.start();

    // Push the disposable to the context's subscriptions so that the
    // client can be deactivated on extension deactivation
    context.subscriptions.push(disposable);
}