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
set "Publisher=%RepositoryRoot%\Artifacts\Native-Hosted-Verifier-Application-Publisher-Candidate\windows-x64-wvhostverifierpublish.exe"

certutil -hashfile "%Publisher%" SHA256 | findstr /I /C:"17cb5c4228e8448693b17f1b73695fd0ecfd03d7ada922794a5bf3bd7594fc96" >nul
if errorlevel 1 (
    >&2 echo The Windows native hosted-verifier-application publisher artifact digest is invalid.
    exit /b 1
)

"%Publisher%" "%~f1" "%~f2"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Publish-Hosted-Verifier-Application.cmd ^<candidate.exe^|candidate.elf^> ^<destination.exe^|destination.elf^>
exit /b 64
