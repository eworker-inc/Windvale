@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if not "%~2"=="" goto :usage
if /I not "%~x1"==".wvo" goto :usage

set "Output=%~f1"
if exist "%Output%" (
    >&2 echo The native x64 exception-object output already exists.
    exit /b 1
)
for %%F in ("%Output%") do set "OutputDirectory=%%~dpF"
if not exist "%OutputDirectory%" (
    >&2 echo The native x64 exception-object output directory does not exist.
    exit /b 1
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Producer=%RepositoryRoot%\Artifacts\Native-X64-Exception-Object-Producer-Candidate\windows-x64-exception-object.exe"
if not exist "%Producer%" (
    >&2 echo The Windows native x64 exception-object producer is missing.
    exit /b 1
)
for %%F in ("%Producer%") do if not "%%~zF"=="387584" (
    >&2 echo The Windows native x64 exception-object producer length is invalid.
    exit /b 1
)
certutil -hashfile "%Producer%" SHA256 | findstr /i /x /c:"80dd0c525f4bf8cf97743852b4e874eddcea7799a5dc98cff4845b97b409580a" >nul
if errorlevel 1 (
    >&2 echo The Windows native x64 exception-object producer digest is invalid.
    exit /b 1
)

"%Producer%" "%Output%"
if errorlevel 1 goto :failure
for %%F in ("%Output%") do if not "%%~zF"=="483" goto :failure
certutil -hashfile "%Output%" SHA256 | findstr /i /x /c:"9caeb7ce353bca33e3bbac729ecca0423d59f8ce6b65ccd6b54fa53c381d617c" >nul
if errorlevel 1 goto :failure
exit /b 0

:failure
if exist "%Output%" del /f /q "%Output%" >nul 2>nul
>&2 echo The native x64 exception-object producer failed.
exit /b 1

:usage
>&2 echo Usage: Tools\Native\Produce-X64-Exception-Object.cmd ^<output.wvo^>
exit /b 64
