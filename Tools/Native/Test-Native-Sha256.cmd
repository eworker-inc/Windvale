@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Native-Sha256.cmd
    exit /b 64
)
set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
node "%RepositoryRoot%\Tools\Native\Test-Native-Sha256.mjs" windows "%RepositoryRoot%"
exit /b %ERRORLEVEL%
