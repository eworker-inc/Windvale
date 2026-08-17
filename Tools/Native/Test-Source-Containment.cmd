@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "Mode="
if "%~1"=="" goto :arguments_ready
if not "%~2"=="" goto :usage
if /I not "%~1"=="--compiler-only" goto :usage
set "Mode=--compiler-only"

:arguments_ready
node --expose-gc "%~dp0Test-Random-Containment.mjs" source %Mode%
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Test-Source-Containment.cmd [--compiler-only]
exit /b 64
