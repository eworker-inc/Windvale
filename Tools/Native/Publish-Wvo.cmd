@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if "%~2"=="" goto :usage
if not "%~3"=="" goto :usage
if /I not "%~x1"==".wvo" goto :usage
if /I not "%~x2"==".wvo" goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Publisher=%RepositoryRoot%\Artifacts\Native-Wvo-Publisher-Candidate\windows-x64-wvopublish.exe"

certutil -hashfile "%Publisher%" SHA256 | findstr /I /C:"76f632ffa7998a6cce0386456fee98f02cbb5ec424d0d914a7e1f06ff3853910" >nul
if errorlevel 1 (
    >&2 echo The Windows native WVO publisher artifact digest is invalid.
    exit /b 1
)

"%Publisher%" "%~f1" "%~f2"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Publish-Wvo.cmd ^<candidate.wvo^> ^<destination.wvo^>
exit /b 64
