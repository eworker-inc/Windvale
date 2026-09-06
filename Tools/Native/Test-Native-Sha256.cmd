@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~2"=="" goto :usage
if "%~1"=="" goto :run
if /I not "%~1"=="--streaming" goto :usage
:run
set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
node "%RepositoryRoot%\Tools\Native\Test-Native-Sha256.mjs" windows "%RepositoryRoot%" %1
exit /b %ERRORLEVEL%
:usage
>&2 echo Usage: Tools\Native\Test-Native-Sha256.cmd [--streaming]
exit /b 64
