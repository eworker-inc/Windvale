@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-External-Model-Gateway.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
node "%RepositoryRoot%\Tools\Models\Test-External-Model-Gateway-Core.mjs" || exit /b %ERRORLEVEL%
node "%RepositoryRoot%\Tools\Models\Test-Supervised-External-Model-Gateway.mjs" || exit /b %ERRORLEVEL%
echo external model gateway status=Passed providers=3 cases=30 child-process=Verified differential=Verified public-network=0 real-credentials=0
exit /b 0
