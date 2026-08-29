@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
call "%RepositoryRoot%\Tools\Native\Verify-Compiler-Convergence.cmd"
if errorlevel 1 exit /b %ERRORLEVEL%

echo Native compiler bootstrap verification passed.
exit /b 0

:usage
>&2 echo Usage: Tools\Verify\Verify-Bootstrap.cmd
exit /b 64
