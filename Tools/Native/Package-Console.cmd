@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if "%~2"=="" goto :usage
if "%~3"=="" goto :usage
if "%~4"=="" goto :usage
if not "%~5"=="" goto :usage
if /I "%~1"=="windows-x64-console-v1" if /I not "%~x4"==".exe" goto :usage
if /I "%~1"=="linux-x64-console-v1" if /I not "%~x4"==".elf" goto :usage
if /I not "%~1"=="windows-x64-console-v1" if /I not "%~1"=="linux-x64-console-v1" goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Packager=%RepositoryRoot%\Artifacts\Native-Console-Packager-Candidate\Console-Packager.exe"
set "PublisherLauncher=%RepositoryRoot%\Tools\Native\Publish-Console.cmd"

certutil -hashfile "%Packager%" SHA256 | findstr /I /C:"a9cd6e222b869d838f563ffc46ae3acbde74ff8beb10c28373b6d5985c8f680f" >nul
if errorlevel 1 (
    >&2 echo The Windows native console packager artifact digest is invalid.
    exit /b 1
)

:allocate
set "TemporaryDirectory=%TEMP%\windvale-native-package-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "CandidatePath=%TemporaryDirectory%\Candidate%~x4"

"%Packager%" "%~1" "%~f2" "%~3" "%CandidatePath%"
set "Result=%ERRORLEVEL%"
if not "%Result%"=="0" goto :cleanup
call "%PublisherLauncher%" "%CandidatePath%" "%~f4"
set "Result=%ERRORLEVEL%"

:cleanup
if exist "%CandidatePath%" del /f /q "%CandidatePath%" >nul 2>nul
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b %Result%

:usage
>&2 echo Usage: Tools\Native\Package-Console.cmd ^<windows-x64-console-v1^|linux-x64-console-v1^> ^<native-image.bin^> ^<entry-offset^> ^<output^>
exit /b 64
