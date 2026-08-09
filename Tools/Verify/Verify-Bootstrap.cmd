@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Artifacts=%RepositoryRoot%\artifacts"
set "Output=%Artifacts%\Bootstrap-Windvale-Compiler.wvb"

if not exist "%Artifacts%\." mkdir "%Artifacts%" || exit /b 1
call "%RepositoryRoot%\Tools\Native\Bootstrap-Compiler.cmd" ^
    "%RepositoryRoot%\Artifacts" "%RepositoryRoot%" "%Output%"
if errorlevel 1 exit /b %ERRORLEVEL%

echo Native compiler bootstrap verification passed.
echo Compiler: %Output%
exit /b 0

:usage
>&2 echo Usage: Tools\Verify\Verify-Bootstrap.cmd
exit /b 64
