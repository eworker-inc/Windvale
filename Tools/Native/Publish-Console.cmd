@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if "%~2"=="" goto :usage
if not "%~3"=="" goto :usage
if /I "%~x1"==".exe" goto :candidate_extension_ok
if /I "%~x1"==".elf" goto :candidate_extension_ok
goto :usage

:candidate_extension_ok
if /I not "%~x1"=="%~x2" goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Publisher=%RepositoryRoot%\Artifacts\Native-Console-Application-Publisher-Candidate\windows-x64-wvappublish.exe"

certutil -hashfile "%Publisher%" SHA256 | findstr /I /C:"0bafe84096859f4b88dc14be92c6cdc5336d791b7c5b0a332dccb76b913dd24e" >nul
if errorlevel 1 (
    >&2 echo The Windows native console-application publisher artifact digest is invalid.
    exit /b 1
)

"%Publisher%" "%~f1" "%~f2"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Publish-Console.cmd ^<candidate.exe^|candidate.elf^> ^<destination.exe^|destination.elf^>
exit /b 64
