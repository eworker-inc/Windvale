@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Verification-Owner-Stream.cmd
    exit /b 64
)

node "%~dp0Test-Verification-Owner-Stream.mjs"
exit /b %ERRORLEVEL%
