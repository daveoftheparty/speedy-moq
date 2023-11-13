:: of COURSE I can't use the reliable dotnet build for a visual studio package 🤬
:: dotnet build ^
:: 	-c Release ^
:: 	VisualStudioClient.sln


:: so, before running this batch file, build the solution in release mode INSIDE visual studio until I can get a working release build command line.

:: also, login:
:: VsixPublisher.exe login -publisherName "daveoftheparty" -personalAccessToken "<put token here>"

set vsixPublishPath="c:\Program Files\Microsoft Visual Studio\2022\Community\VSSDK\VisualStudioIntegration\Tools\Bin\VsixPublisher.exe"
:: VsixPublisher.exe publish -payload "{path to vsix}" -publishManifest "{path to vs-publish.json}" -ignoreWarnings "VSIXValidatorWarning01,VSIXValidatorWarning02
%vsixPublishPath% publish -payload "VisualStudioClient/bin/Release/VisualStudioClient.vsix" -publishManifest "publishManifest.json" -ignoreWarnings "VSIXValidatorWarning01,VSIXValidatorWarning02"