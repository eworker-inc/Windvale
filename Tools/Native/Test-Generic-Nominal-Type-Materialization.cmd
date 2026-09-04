@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :qualification
if /I not "%~1"=="--development" goto :usage
if not "%~2"=="" goto :usage
node "%~dp0Test-Generic-Nominal-Development-Bundle.mjs" type-materialization
exit /b %ERRORLEVEL%

:qualification
node "%~dp0Test-Generic-Nominal-Type-Materialization.mjs"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Test-Generic-Nominal-Type-Materialization.cmd [--development]
exit /b 64
