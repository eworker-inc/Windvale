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

certutil -hashfile "%Publisher%" SHA256 | findstr /I /C:"ad4c2a05115b2acdb074c0f53b6d7470c8bcacfdfea86583043bdd0ff511188a" >nul
if errorlevel 1 (
    >&2 echo The Windows native WVO publisher artifact digest is invalid.
    exit /b 1
)

"%Publisher%" "%~f1" "%~f2"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Publish-Wvo.cmd ^<candidate.wvo^> ^<destination.wvo^>
exit /b 64
