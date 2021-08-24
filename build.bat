@REM npm ci
@REM npm run compile

echo Go Dot Net

if exist "./client/out/server" (
	rmdir /s /q "./client/out/server"
)

dotnet build ^
	-c Release ^
	server/MoqGenerator.Lsp/MoqGenerator.Lsp.csproj ^
	/property:GenerateFullPaths=true ^
	/consoleloggerparameters:NoSummary ^
	--output ./client/out/server

