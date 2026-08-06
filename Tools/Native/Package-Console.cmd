@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if "%~2"=="" goto :usage
if "%~3"=="" goto :usage
if "%~4"=="" goto :usage
if not "%~5"=="" goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Packager=%RepositoryRoot%\Artifacts\Native-Console-Packager-Candidate\Console-Packager.exe"

certutil -hashfile "%Packager%" SHA256 | findstr /I /C:"a9cd6e222b869d838f563ffc46ae3acbde74ff8beb10c28373b6d5985c8f680f" >nul
if errorlevel 1 (
    >&2 echo The Windows native console packager artifact digest is invalid.
    exit /b 1
)

"%Packager%" %*
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Package-Console.cmd ^<windows-x64-console-v1^|linux-x64-console-v1^> ^<native-image.bin^> ^<entry-offset^> ^<output^>
exit /b 64
