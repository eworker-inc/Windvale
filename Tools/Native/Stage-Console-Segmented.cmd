@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if "%~2"=="" goto :usage
if "%~3"=="" goto :usage
if "%~4"=="" goto :usage
if "%~5"=="" goto :usage
if not "%~6"=="" goto :usage
if /I not "%~1"=="windows-x64-console-v1" if /I not "%~1"=="linux-x64-console-v1" goto :usage
if /I not "%~x5"==".wvcs" goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Packager=%RepositoryRoot%\Artifacts\Native-Console-Segmented-Packager-Candidate\Console-Segmented-Packager.exe"

certutil -hashfile "%Packager%" SHA256 | findstr /I /C:"954c4b2aaba56149c21e16e19ca6f16434069513e1d1b3034423dab457635412" >nul
if errorlevel 1 (
    >&2 echo The Windows native segmented console-packager artifact digest is invalid.
    exit /b 1
)

"%Packager%" "%~1" "%~f2" "%~3" "%~f4" "%~f5"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Stage-Console-Segmented.cmd ^<windows-x64-console-v1^|linux-x64-console-v1^> ^<native-image.bin^> ^<entry-offset^> ^<chunk-prefix^> ^<manifest.wvcs^>
exit /b 64
