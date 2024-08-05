#!/bin/bash

cd $(dirname $0)/..

RIDS=("linux-x64" "linux-arm" "linux-arm64" "win-x64" "win-x86")

FREAMEWORKS=("net6.0" "net8.0")
FREAMEWORKS_LEGACY=("net481" "net472" "net462")

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
	for PF in "${FREAMEWORKS[@]}"; do
		echo Publishing portable $CFG for $PF
		dotnet publish "$PROJECT_FILE" -f "$PF" -c "$CFG" --no-self-contained -o "$OUTPUT_DIR/$CFG/portable-$PF"
		echo
	done
	for PFL in "${FREAMEWORKS_LEGACY[@]}"; do
		echo Publishing portable $CFG for $PFL
		dotnet publish "$PROJECT_FILE_LEGACY" -f "$PFL" -c "$CFG" --no-self-contained -o "$OUTPUT_DIR/$CFG/portable-$PFL"
		echo
	done
	for RID in "${RIDS[@]}"; do
		if [ "$RID" = "win-x64" ] || [ "$RID" = "win-x86" ]
		then
			for FL in "${FREAMEWORKS_LEGACY[@]}"; do
				__build "$PROJECT_FILE_LEGACY" "$FL" "$CFG" "$RID"
			done
		fi
		for F in "${FREAMEWORKS[@]}"; do
			__build "$PROJECT_FILE" "$F" "$CFG" "$RID"
		done
	done
done
