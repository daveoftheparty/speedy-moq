echo building typescript

call npm ci
call npm run compile

@echo on

echo building dotnet
cd ./server
dotnet build
cd ..

echo building for package/extension release

if exist "./client/out/server" (
	rmdir /s /q "./client/out/server"
)

dotnet build ^
	-c Release ^
	server/OmniLsp/OmniLsp.csproj ^
	/property:GenerateFullPaths=true ^
	/consoleloggerparameters:NoSummary ^
	--output ./client/out/server

