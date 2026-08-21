@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Generic-Nominal-Type-Materialization.cmd
    exit /b 64
)

node "%~dp0Test-Generic-Nominal-Type-Materialization.mjs"
exit /b %ERRORLEVEL%
