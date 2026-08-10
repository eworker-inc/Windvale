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
certutil -hashfile "%Admitter%" SHA256 | findstr /I /C:"4742ee299759728be1b72fed3d3b42620c21b10f77aed12cf150c1549b177b53" >nul
if errorlevel 1 (
    >&2 echo The Windows native hosted-verifier publisher admitter digest is invalid.
    exit /b 1
)

"%Admitter%" "%~1" "%~f2"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Admit-Hosted-Verifier-Publisher.cmd ^<windows^|linux^> ^<publisher.exe^|publisher.elf^>
exit /b 64
