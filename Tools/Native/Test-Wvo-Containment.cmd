@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Wvo-Containment.cmd
    exit /b 64
)

node --expose-gc "%~dp0Test-Random-Containment.mjs" wvo
exit /b %ERRORLEVEL%
