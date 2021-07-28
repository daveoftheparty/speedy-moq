# FIRST SUCCESSFUL MOQ GENERATION!!!

Note that it looks like it took about 3-4 seconds to load up the interfaces after our first textDocument/didOpen request.

We should pre-load from the Program.cs, but also, not bad!!

Empty lines represent messages I deleted from the log so I could read it better that looked like this (other than varying timestamps):

`[Trace - 10:56:44 PM] Received notification 'window/logMessage'.`

```
[Trace - 10:56:43 PM] Sending request 'initialize - (0)'.

[Error - 10:56:44 PM] Features.MoqGenerator.InterfaceStore: hello from InterfaceStore:b9f80ad4-361b-49d0-beee-fb9d9ac18564 ctor... | 

[Error - 10:56:44 PM] OmniLsp.TextDocumentHandler: hello from TextDocumentHandler:560ac392-fb8e-4778-8027-215f0a21c4ac ctor... | 
[Trace - 10:56:44 PM] Received response 'initialize - (0)' in 1438ms.
[Trace - 10:56:44 PM] Sending notification 'initialized'.
[Trace - 10:56:44 PM] Sending notification 'workspace/didChangeConfiguration'.

[Warn  - 10:56:45 PM] OmniSharp.Extensions.LanguageServer.Server.Configuration.DidChangeConfigurationProvider: No ConfigurationItems have been defined, configuration won't surface any configuration from the client! | 
[Trace - 10:56:45 PM] Received request 'client/registerCapability - (1)'.
[Trace - 10:56:45 PM] Sending notification 'textDocument/didOpen'.
[Trace - 10:56:45 PM] Sending response 'client/registerCapability - (1)'. Processing request took 1ms
[Trace - 10:56:45 PM] Sending request 'textDocument/codeAction - (1)'.

[Info  - 10:56:45 PM] Features.MoqGenerator.InterfaceStore: method GetCsProjFromCsFile is looking for a .csproj related to file e:/temp/BoilerMoqTestDatas/TestId_001.cs | 

[Info  - 10:56:45 PM] Features.MoqGenerator.InterfaceStore: method GetCsProjFromCsFile is searching for a .csproj in e:\temp\BoilerMoqTestDatas | 

[Info  - 10:56:45 PM] Features.MoqGenerator.InterfaceStore: method GetCsProjFromCsFile is searching for a .csproj in e:\temp | 

[Info  - 10:56:45 PM] Features.MoqGenerator.InterfaceStore: method GetCsProjFromCsFile found e:\temp\Server.csproj | 
[Trace - 10:56:45 PM] Received notification 'textDocument/publishDiagnostics'.
[Trace - 10:56:45 PM] Received response 'textDocument/codeAction - (1)' in 925ms.
[Trace - 10:56:47 PM] Sending notification 'workspace/didChangeWatchedFiles'.
[Trace - 10:56:48 PM] Sending notification 'workspace/didChangeWatchedFiles'.

[Info  - 10:56:48 PM] Features.MoqGenerator.InterfaceStore: LogDefinitions was called by LoadCSProjAsync. CsProjs loaded: e:\temp\Server.csproj | 

[Info  - 10:56:48 PM] Features.MoqGenerator.InterfaceStore: LogDefinitions was called by LoadCSProjAsync. Interfaces loaded: IStringAnalyzer|IBaseType|IUserType | 

[Info  - 10:56:48 PM] Features.MoqGenerator.InterfaceStore: InterfaceStore in method LoadCsInterfaceIfNecessaryAsync received/is loading for uri file:///e:/temp/BoilerMoqTestDatas/TestId_001.cs | 

[Info  - 10:56:48 PM] Features.MoqGenerator.InterfaceStore: LogDefinitions was called by LoadCsInterfaceIfNecessaryAsync. CsProjs loaded: e:\temp\Server.csproj | 

[Info  - 10:56:48 PM] Features.MoqGenerator.InterfaceStore: LogDefinitions was called by LoadCsInterfaceIfNecessaryAsync. Interfaces loaded: IStringAnalyzer|IBaseType|IUserType | 
[Trace - 10:57:00 PM] Sending notification 'textDocument/didClose'.
[Trace - 10:57:01 PM] Sending notification 'textDocument/didOpen'.
[Trace - 10:57:01 PM] Sending request 'textDocument/codeAction - (2)'.

[Info  - 10:57:01 PM] Features.MoqGenerator.InterfaceStore: method GetCsProjFromCsFile is looking for a .csproj related to file e:/temp/BoilerMoqTestDatas/TestId_001_NoInterface.cs | 

[Info  - 10:57:01 PM] Features.MoqGenerator.InterfaceStore: method GetCsProjFromCsFile is searching for a .csproj in e:\temp\BoilerMoqTestDatas | 

[Info  - 10:57:01 PM] Features.MoqGenerator.InterfaceStore: method GetCsProjFromCsFile is searching for a .csproj in e:\temp | 

[Info  - 10:57:01 PM] Features.MoqGenerator.InterfaceStore: method GetCsProjFromCsFile found e:\temp\Server.csproj | 

[Info  - 10:57:01 PM] Features.MoqGenerator.InterfaceStore: LogDefinitions was called by LoadCsInterfaceIfNecessaryAsync. CsProjs loaded: e:\temp\Server.csproj | 

[Info  - 10:57:01 PM] Features.MoqGenerator.InterfaceStore: LogDefinitions was called by LoadCsInterfaceIfNecessaryAsync. Interfaces loaded: IStringAnalyzer|IBaseType|IUserType | 
[Trace - 10:57:01 PM] Received notification 'textDocument/publishDiagnostics'.
[Trace - 10:57:01 PM] Received response 'textDocument/codeAction - (2)' in 37ms.
[Trace - 10:57:02 PM] Sending request 'textDocument/codeAction - (3)'.

[Info  - 10:57:02 PM] Features.MoqGenerator.InterfaceStore: GetInterfaceDefinition is looking for interfaceName IStringAnalyzer. Interfaces loaded: IStringAnalyzer|IBaseType|IUserType. Interface Source file: First Method Name per interface: HowManyItems|CountChars|ComposeUser | 
[Trace - 10:57:02 PM] Received response 'textDocument/codeAction - (3)' in 27ms.
```