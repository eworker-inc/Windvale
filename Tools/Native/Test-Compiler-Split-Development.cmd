@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Compiler-Split-Development.cmd
    exit /b 64
)

node "%~dp0Test-Compiler-Split-Development.mjs"
exit /b %ERRORLEVEL%
