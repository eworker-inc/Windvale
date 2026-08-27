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

for %%F in ("%Runner%") do if not "%%~zF"=="5327872" (
    >&2 echo The Windows native WVB runner artifact size is invalid.
    exit /b 1
)
certutil -hashfile "%Runner%" SHA256 | findstr /I /C:"7a8b97c68c3463af858b47178978f30507af947d7cf0e86e5ec71829702157c0" >nul
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
