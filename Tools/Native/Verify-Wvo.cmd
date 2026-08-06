@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if not "%~2"=="" goto :usage
if /I not "%~x1"==".wvo" (
    >&2 echo The native WVO verifier input must use the .wvo extension.
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Inspector=%RepositoryRoot%\Artifacts\Native-Wvo-Object-Candidate\Wvo-Object.exe"

certutil -hashfile "%Inspector%" SHA256 | findstr /I /C:"2a8f6f8ca8fc6054fff23441f7971c0b90900383d5bed0fecc54f9cac102a300" >nul
if errorlevel 1 (
    >&2 echo The Windows native WVO inspector artifact digest is invalid.
    exit /b 1
)

"%Inspector%" verify "%~f1"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Verify-Wvo.cmd ^<object.wvo^>
exit /b 64
