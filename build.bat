@REM npm ci
@REM npm run compile

echo Go Dot Net

if exist "./client/out/server" (
	rmdir /s /q "./client/out/server"
)

dotnet build ^
	-c Release ^
	server/OmniLsp/OmniLsp.csproj ^
	/property:GenerateFullPaths=true ^
	/consoleloggerparameters:NoSummary ^
	--output ./client/out/server

