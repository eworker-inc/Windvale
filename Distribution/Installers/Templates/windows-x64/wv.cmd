@echo off
setlocal EnableExtensions DisableDelayedExpansion

if /i "%~1"=="version" (
    type "%~dp0..\VERSION"
    exit /b 0
)
if /i "%~1"=="doctor" (
    powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0wv-verify-installation.ps1" -Root "%~dp0.."
    if errorlevel 1 exit /b 1
    exit /b 0
)
if /i "%~1"=="tools" (
    echo wvbuild wvasm wvlink wvrun wvdump wvverify wvpublish
    exit /b 0
)
if /i "%~1"=="run" goto :run
if /i "%~1"=="help" goto :usage
if "%~1"=="" goto :usage

>&2 echo Unknown wv command: %~1

:usage
>&2 echo Usage: wv ^<version^|doctor^|tools^|run^|help^>
>&2 echo        wv run ^<source.wv^> [argument ...]
>&2 echo Run the installed native tools directly for build, assembly, linking, execution, and inspection.
exit /b 64

:run
powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0wv-run.ps1" %*
exit /b %ERRORLEVEL%
