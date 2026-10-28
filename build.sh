#!/usr/bin/env zsh
set -euo pipefail

# run from the repo root regardless of where this was invoked from
cd "${0:A:h}"

echo "building typescript"

npm ci
npm run compile

# not sure what my intention was here when I wrote this, building the whole solution. Was this just experimental? Seems redundant to the build below. Also, tests aren't run.
echo "building dotnet"
(cd ./server && dotnet build)

echo "building for package/extension release"

rm -rf ./client/out/server

dotnet build \
	-c Release \
	server/OmniLsp/OmniLsp.csproj \
	/property:GenerateFullPaths=true \
	/consoleloggerparameters:NoSummary \
	--output ./client/out/server
