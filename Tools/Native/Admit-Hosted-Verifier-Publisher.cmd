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
certutil -hashfile "%Admitter%" SHA256 | findstr /I /C:"7f58a5e321d1b4baa16ba673b3e0e1c21c9acd040cba92dae0f180d629c63e6b" >nul
if errorlevel 1 (
    >&2 echo The Windows native hosted-verifier publisher admitter digest is invalid.
    exit /b 1
)

"%Admitter%" "%~1" "%~f2"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Admit-Hosted-Verifier-Publisher.cmd ^<windows^|linux^> ^<publisher.exe^|publisher.elf^>
exit /b 64
