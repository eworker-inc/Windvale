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

certutil -hashfile "%Inspector%" SHA256 | findstr /I /C:"8c6f30b0b55898776d8dc394ea763313527650a361ceb6f478ffad48979084f1" >nul
if errorlevel 1 (
    >&2 echo The Windows native WVO inspector artifact digest is invalid.
    exit /b 1
)

"%Inspector%" verify "%~f1"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Verify-Wvo.cmd ^<object.wvo^>
exit /b 64
