@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
node "%RepositoryRoot%\Tools\Native\Verify-Current-Split-Compiler-Convergence.mjs" ^
    "%RepositoryRoot%"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Verify-Compiler-Convergence.cmd
exit /b 64
