#!/bin/bash

RIDS=("linux-x64" "linux-arm" "linux-arm64" "win-x64" "win-x86")

FREAMEWORKS=("net6.0" "net8.0")

if [ "$1" = "-d" ]
then
	CONFIGURATIONS="Debug"
else
	CONFIGURATIONS=("Debug" "Release")
fi

PROJECT_FILE="cli/Kryptor.Cli/Kryptor.Cli.csproj"
PROJECT_FILE_LEGACY="cli/Kryptor.Cli.Legacy/Kryptor.Cli.Legacy.csproj"

OUTPUT_DIR="bin/Publish/Cli"

__build(){
	echo Publishing $3 for $2 on $4
	dotnet publish "$1" -f "$2" -c "$3" -r "$4" --no-self-contained -o "$OUTPUT_DIR/$3/$4-$2"
	echo
	
	if [ "$3" = "Release" ] && [ "$2" = "net6.0" ]
	then
		echo Publishing bundle $3 for $2 on $4
		dotnet publish "$1" -f "$2" -c "$3" -r "$4" --self-contained -o "$OUTPUT_DIR/$3/$4-$2-bundle"
		echo
	fi
}

for CFG in "${CONFIGURATIONS[@]}"; do
	for RID in "${RIDS[@]}"; do
		if [ "$RID" = "win-x64" ] || [ "$RID" = "win-x86" ]
		then
			__build "$PROJECT_FILE_LEGACY" "net481" "$CFG" "$RID"
		fi
		for F in "${FREAMEWORKS[@]}"; do
			__build "$PROJECT_FILE" "$F" "$CFG" "$RID"
		done
	done
done
