@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if not "%~2"=="" goto :usage
if /I not "%~x1"==".wvb" (
    >&2 echo The native runner input must use the .wvb extension.
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Runner=%RepositoryRoot%\Artifacts\Native-Wvb-Runner-Candidate\windows-x64-wvrun.exe"

for %%F in ("%Runner%") do if not "%%~zF"=="778240" (
    >&2 echo The Windows native WVB runner artifact size is invalid.
    exit /b 1
)
certutil -hashfile "%Runner%" SHA256 | findstr /I /C:"578ddd302da5fbd8d8e14c9410787f5aa05378429a1aca738ee2057e2f9ac1a5" >nul
if errorlevel 1 (
    >&2 echo The Windows native WVB runner artifact digest is invalid.
    exit /b 1
)

"%Runner%" "%~f1"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Run-Wvb.cmd ^<module.wvb^>
exit /b 64
