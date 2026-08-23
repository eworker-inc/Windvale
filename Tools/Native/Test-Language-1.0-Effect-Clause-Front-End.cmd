@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Language-1.0-Effect-Clause-Front-End.cmd
    exit /b 64
)

node "%~dp0Test-Language-1.0-Effect-Clause-Front-End.mjs"
exit /b %ERRORLEVEL%
