@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" if not "%~1"=="--foundation-borrow" if not "%~1"=="--foundation-borrow-plan" if not "%~1"=="--foundation-borrow-directories" if not "%~1"=="--foundation-borrow-owners" goto :usage
if not "%~2"=="" goto :usage

node "%~dp0Test-Language-1.0-Memory-Budget-Split-Execution.mjs" %*
exit /b %ERRORLEVEL%

:usage
    >&2 echo Usage: Tools\Native\Test-Language-1.0-Memory-Budget-Split-Execution.cmd [--foundation-borrow^|--foundation-borrow-plan^|--foundation-borrow-directories^|--foundation-borrow-owners]
    exit /b 64
