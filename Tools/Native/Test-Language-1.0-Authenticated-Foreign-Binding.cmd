@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Language-1.0-Authenticated-Foreign-Binding.cmd
    exit /b 64
)

node "%~dp0Test-Language-1.0-Authenticated-Foreign-Binding.mjs"
exit /b %ERRORLEVEL%
