@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="--vector-borrow-integration" goto :vector_integration
if not "%~1"=="" if not "%~1"=="--foundation-borrow" if not "%~1"=="--foundation-borrow-plan" if not "%~1"=="--foundation-borrow-directories" if not "%~1"=="--foundation-borrow-owners" if not "%~1"=="--foundation-borrow-components" goto :usage
if not "%~2"=="" goto :usage

:run
node "%~dp0Test-Language-1.0-Memory-Budget-Split-Execution.mjs" %*
exit /b %ERRORLEVEL%

:vector_integration
if "%~2"=="" goto :run
if not "%~2"=="--maximum-seconds" goto :usage
if "%~3"=="" goto :usage
if not "%~4"=="" goto :usage
goto :run

:usage
    >&2 echo Usage: Tools\Native\Test-Language-1.0-Memory-Budget-Split-Execution.cmd [--foundation-borrow^|--foundation-borrow-plan^|--foundation-borrow-directories^|--foundation-borrow-owners^|--foundation-borrow-components^|--vector-borrow-integration [--maximum-seconds ^<seconds^>]]
    exit /b 64
