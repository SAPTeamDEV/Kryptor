@echo off
setlocal enabledelayedexpansion

set "RIDS=linux-x64 linux-arm64 linux-arm win-x64 win-x86 osx-x64 osx-arm64"

set "FRAMEWORKS=net6.0 net8.0 net481"

set "CONFIGURATIONS=Release Debug"

set "PROJECT_FILE=src\Kryptor.Cli\Kryptor.Cli.csproj"

set "OUTPUT_DIR=bin\Publish\Kryptor.Cli"

for %%F in (%FRAMEWORKS%) do (
    for %%C in (%CONFIGURATIONS%) do (
        for %%R in (%RIDS%) do (
            dotnet publish !PROJECT_FILE! -f %%F -c %%C -r %%R --no-self-contained -o !OUTPUT_DIR!\%%C\%%F\%%R
        )
    )
)
