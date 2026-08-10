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
set "Promoter=%RepositoryRoot%\Artifacts\Native-Hosted-Verifier-Publisher-Promoter-Candidate\windows-x64-wvhostverifierpublisherinstall.exe"

certutil -hashfile "%Promoter%" SHA256 | findstr /I /C:"9cb234a57c9ff71b6ee44a0d687521e6fd7ccf82784b369e5e65b8ed40666069" >nul
if errorlevel 1 (
    >&2 echo The Windows native hosted-verifier publisher promoter artifact digest is invalid.
    exit /b 1
)

"%Promoter%" "%~f1" "%~f2"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Install-Hosted-Verifier-Publisher.cmd ^<candidate.exe^|candidate.elf^> ^<destination.exe^|destination.elf^>
exit /b 64
