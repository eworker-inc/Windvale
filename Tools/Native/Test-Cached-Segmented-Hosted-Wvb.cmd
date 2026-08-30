@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Cached-Segmented-Hosted-Wvb.cmd
    exit /b 64
)

node "%~dp0Test-Cached-Segmented-Hosted-Wvb.mjs"
exit /b %ERRORLEVEL%
