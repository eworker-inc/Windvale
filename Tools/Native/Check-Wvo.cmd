@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if not "%~2"=="" goto :usage
if /I not "%~x1"==".wvo" (
    >&2 echo The native WVO checker input must use the .wvo extension.
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Inspector=%RepositoryRoot%\Artifacts\Native-Wvo-Object-Candidate\Wvo-Object.exe"

certutil -hashfile "%Inspector%" SHA256 | findstr /I /C:"5362372e826958470eee7d90eb01938de5b91dcb3e1b0f952722e00578a82d03" >nul
if errorlevel 1 (
    >&2 echo The Windows native WVO inspector artifact digest is invalid.
    exit /b 1
)

"%Inspector%" check "%~f1"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Check-Wvo.cmd ^<object.wvo^>
exit /b 64
