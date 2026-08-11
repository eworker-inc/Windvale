@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if "%~2"=="" goto :usage
if not "%~3"=="" goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Verifier=%RepositoryRoot%\Artifacts\Native-Console-Application-Verifier-Candidate\windows-x64-wvappverify.exe"

certutil -hashfile "%Verifier%" SHA256 | findstr /I /C:"a82027ab78ee5f4d7d9f34180392ee8b8364ea78616c11aeac1e684250fc3679" >nul
if errorlevel 1 (
    >&2 echo The Windows native console-application verifier artifact digest is invalid.
    exit /b 1
)

"%Verifier%" "%~f1" "%~f2"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Verify-Console-Segmented.cmd ^<first-application-chunk^> ^<second-application-chunk^>
exit /b 64
