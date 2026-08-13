@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if not "%~2"=="" goto :usage
if /I not "%~x1"==".wvb" (
    >&2 echo The native verifier input must use the .wvb extension.
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Verifier=%RepositoryRoot%\Artifacts\Native-Front-Door\windows-x64\wvverify.exe"

certutil -hashfile "%Verifier%" SHA256 | findstr /I /C:"a1dc701cc8d5ace0a680a15e19435c48b3bccde3cf6197bfdd07ee04a4bf9871" >nul
if errorlevel 1 (
    >&2 echo The Windows native WVB verifier artifact digest is invalid.
    exit /b 1
)

"%Verifier%" "%~f1"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Verify-Wvb.cmd ^<module.wvb^>
exit /b 64
