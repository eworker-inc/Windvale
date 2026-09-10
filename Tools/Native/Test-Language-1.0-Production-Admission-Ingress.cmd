@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "Mode="
if "%~1"=="" goto :arguments_ready
if /I "%~1"=="--project4-launcher" (
    if not "%~2"=="" goto :usage
    set "Mode=--project4-launcher"
    goto :arguments_ready
)
goto :usage

:usage
    >&2 echo Usage: Tools\Native\Test-Language-1.0-Production-Admission-Ingress.cmd [--project4-launcher]
    exit /b 64

:arguments_ready
if defined Mode (
    node "%~dp0Test-Language-1.0-Production-Admission-Ingress.mjs" "%Mode%"
    exit /b %ERRORLEVEL%
)

node "%~dp0Test-Language-1.0-Production-Admission-Ingress.mjs"
exit /b %ERRORLEVEL%
