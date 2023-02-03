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

