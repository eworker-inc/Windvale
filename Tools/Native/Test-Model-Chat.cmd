@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Model-Chat.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
node "%RepositoryRoot%\Tools\Models\Test-Model-Chat.mjs" || exit /b %ERRORLEVEL%
exit /b 0
