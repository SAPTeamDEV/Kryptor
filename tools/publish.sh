#!/bin/bash

# List of known RIDs (full list available in the RID catalog)
RIDS=("linux-x64" "linux-arm64" "linux-arm" "win-x64" "win-x86" "osx-x64" "osx-arm64")

FREAMEWORKS=("net6.0" "net8.0" "net481")

CONFIGURATIONS=("Release" "Debug")

PROJECT_FILE="src/Kryptor.Cli/Kryptor.Cli.csproj"

OUTPUT_DIR="bin/Publish/Kryptor.Cli"

# Loop through each RID and publish
for F in "${FREAMEWORKS[@]}"; do
	for CFG in "${CONFIGURATIONS[@]}"; do
		for RID in "${RIDS[@]}"; do
			dotnet publish "$PROJECT_FILE" -f "$F" -c "$CFG" -r "$RID" --no-self-contained -o "$OUTPUT_DIR/$CFG/$F/$RID"
		done
	done
done
