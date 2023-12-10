echo building typescript

call npm ci
call npm run compile

@echo on
REM not sure what my intention was here when I wrote this, building the whole solution. Was this just experimental? Seems redundant to the build below. Also, tests aren't run.
echo building dotnet
cd ./server
dotnet build
cd ..

echo building for package/extension release

if exist "./clients/vscode/out/server" (
	rmdir /s /q "./clients/vscode/out/server"
)

dotnet build ^
	-c Release ^
	server/OmniLsp/OmniLsp.csproj ^
	/property:GenerateFullPaths=true ^
	/consoleloggerparameters:NoSummary ^
	--output ./clients/vscode/out/server

:: /E is empty dirs
:: /Q is do not list filenames
:: /H copies hidden and system files
:: /R overwrites read-only files
:: /Y disables prompting when a file will be overwritten
xcopy ^
	./clients/vscode/out/server ^
	./clients/visual-studio/VisualStudioClient/LanguageServer ^
	/E /Q /H /R /Y