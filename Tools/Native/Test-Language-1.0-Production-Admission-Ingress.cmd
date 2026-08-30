@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Language-1.0-Production-Admission-Ingress.cmd
    exit /b 64
)

node "%~dp0Test-Language-1.0-Production-Admission-Ingress.mjs"
exit /b %ERRORLEVEL%
