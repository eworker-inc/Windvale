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

certutil -hashfile "%Publisher%" SHA256 | findstr /I /C:"035a1baaada6be8d057b782804a8650d978da53dd008337ab00258f2ab597cb7" >nul
if errorlevel 1 (
    >&2 echo The Windows native WVO publisher artifact digest is invalid.
    exit /b 1
)

"%Publisher%" "%~f1" "%~f2"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Publish-Wvo.cmd ^<candidate.wvo^> ^<destination.wvo^>
exit /b 64
