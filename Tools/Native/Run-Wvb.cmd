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

for %%F in ("%Runner%") do if not "%%~zF"=="1349632" (
    >&2 echo The Windows native WVB runner artifact size is invalid.
    exit /b 1
)
certutil -hashfile "%Runner%" SHA256 | findstr /I /C:"51ba52c7509584cbf5c2adc8ae42ac0939b2b311eb9366c812f7f19367fc4e7e" >nul
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
