@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~2"=="" goto :usage
if not "%~3"=="" goto :usage
if /I not "%~1"=="windows" if /I not "%~1"=="linux" goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Admitter=%RepositoryRoot%\Artifacts\Native-Hosted-Verifier-Publisher-Admission-Candidate\windows-x64-wvhostverifierpublisheradmit.exe"

for %%F in ("%Admitter%") do if not "%%~zF"=="570368" (
    >&2 echo The Windows native hosted-verifier publisher admitter byte length is invalid.
    exit /b 1
)
certutil -hashfile "%Admitter%" SHA256 | findstr /I /C:"1407ed428387986e170b4d8394e9a0a6295408ef668d5d6e16d719102428dd4f" >nul
if errorlevel 1 (
    >&2 echo The Windows native hosted-verifier publisher admitter digest is invalid.
    exit /b 1
)

"%Admitter%" "%~1" "%~f2"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Admit-Hosted-Verifier-Publisher.cmd ^<windows^|linux^> ^<publisher.exe^|publisher.elf^>
exit /b 64
