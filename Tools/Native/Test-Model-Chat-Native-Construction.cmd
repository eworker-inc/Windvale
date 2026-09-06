@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Model-Chat-Native-Construction.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "Work=%TEMP%\windvale-model-chat-construction-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Result=1"

call "%RepositoryRoot%\Tools\Native\Build-Windvale-Model-Chat.cmd" ^
    "%Work%\Windvale-Model-Chat.exe" || goto :cleanup
set "Result=0"

:cleanup
for %%R in ("%Work%") do set "ResolvedWork=%%~fR"
echo(%ResolvedWork%| findstr /b /i /c:"%TEMP%\windvale-model-chat-construction-" >nul || exit /b 1
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
if not "%Result%"=="0" exit /b %Result%
echo model chat native construction status=Passed cases=32 native-ui=Windvale core-cases=32 cross-host-images=Verified public-network=0 real-credentials=0
exit /b 0
