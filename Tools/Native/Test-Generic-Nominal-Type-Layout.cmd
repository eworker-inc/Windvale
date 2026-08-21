@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Generic-Nominal-Type-Layout.cmd
    exit /b 64
)

node "%~dp0Test-Generic-Nominal-Type-Layout.mjs"
exit /b %ERRORLEVEL%
