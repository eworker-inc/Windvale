@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Offline-Package-Uninstall.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
node "%RepositoryRoot%\Tools\Package\Verify-Offline-Package-Uninstall.mjs"
exit /b %ERRORLEVEL%
