@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :qualification
if /I not "%~1"=="--development" goto :usage
if not "%~2"=="" goto :usage
node "%~dp0Test-Generic-Nominal-Development-Bundle.mjs" type-layout
exit /b %ERRORLEVEL%

:qualification
node "%~dp0Test-Generic-Nominal-Type-Layout.mjs"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Test-Generic-Nominal-Type-Layout.cmd [--development]
exit /b 64
