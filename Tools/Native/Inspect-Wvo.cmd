@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if not "%~2"=="" goto :usage
if /I not "%~x1"==".wvo" (
    >&2 echo The native WVO inspector input must use the .wvo extension.
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Inspector=%RepositoryRoot%\Artifacts\Native-Wvo-Object-Candidate\Wvo-Object.exe"

certutil -hashfile "%Inspector%" SHA256 | findstr /I /C:"a534b1c7a5ff9112c221a9576141842c4bb50c28b1d43d0ab02a8679bba6f366" >nul
if errorlevel 1 (
    >&2 echo The Windows native WVO inspector artifact digest is invalid.
    exit /b 1
)

"%Inspector%" inspect "%~f1"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Inspect-Wvo.cmd ^<object.wvo^>
exit /b 64
