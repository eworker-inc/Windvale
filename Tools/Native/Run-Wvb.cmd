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
set "Runner=%RepositoryRoot%\Artifacts\Native-Front-Door\windows-x64\wvrun.exe"

certutil -hashfile "%Runner%" SHA256 | findstr /I /C:"bf4fa16b9072215fadab6f1097155d85d5b77924aac01b14330acb0496f0af4c" >nul
if errorlevel 1 (
    >&2 echo The Windows native WVB runner artifact digest is invalid.
    exit /b 1
)

"%Runner%" "%~f1"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Run-Wvb.cmd ^<module.wvb^>
exit /b 64
