@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if not "%~3"=="" goto :usage
if not "%~2"=="" if not "%~2"=="--report-steps" goto :usage
if /I not "%~x1"==".wvb" (
    >&2 echo The native runner input must use the .wvb extension.
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Runner=%RepositoryRoot%\Artifacts\Native-Wvb-Runner-Candidate\windows-x64-wvrun.exe"

for %%F in ("%Runner%") do if not "%%~zF"=="5366784" (
    >&2 echo The Windows native WVB runner artifact size is invalid.
    exit /b 1
)
certutil -hashfile "%Runner%" SHA256 | findstr /I /C:"063de8f1fadcf9c37e9cef6526d628b410fa0cd21067fe6f3c795b97623cb519" >nul
if errorlevel 1 (
    >&2 echo The Windows native WVB runner artifact digest is invalid.
    exit /b 1
)

if "%~2"=="" (
    "%Runner%" "%~f1"
) else (
    "%Runner%" "%~f1" --report-steps
)
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Run-Wvb.cmd ^<module.wvb^> [--report-steps]
exit /b 64
