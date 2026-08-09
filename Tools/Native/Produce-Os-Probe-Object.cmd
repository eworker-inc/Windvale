@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if "%~2"=="" goto :usage
if not "%~3"=="" goto :usage
if /I not "%~x2"==".wvo" goto :usage
if "%~1"=="exceptions" (
    set "ExpectedBytes=483"
    set "ExpectedDigest=9caeb7ce353bca33e3bbac729ecca0423d59f8ce6b65ccd6b54fa53c381d617c"
) else if "%~1"=="wvb-admission-bridge" (
    set "ExpectedBytes=484"
    set "ExpectedDigest=271c378b1f12bb4affa33474d865611cbf14e5b1b8996c703cb3d3cbe22eee7d"
) else if "%~1"=="native-bridge-and-support" (
    set "ExpectedBytes=461"
    set "ExpectedDigest=472a0fbe6497525e634a4785e92aa9ee62c3c7d70fff7510e45acbea644eea0b"
) else if "%~1"=="paging" (
    set "ExpectedBytes=1292"
    set "ExpectedDigest=a6bcad24e4752acc1fbab75d6667e965f2ab4d5613edd2c8e6cda244616fba2d"
) else goto :usage

set "Output=%~f2"
if exist "%Output%" (
    >&2 echo The native OS Probe object output already exists.
    exit /b 1
)
for %%F in ("%Output%") do set "OutputDirectory=%%~dpF"
if not exist "%OutputDirectory%" (
    >&2 echo The native OS Probe object output directory does not exist.
    exit /b 1
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Producer=%RepositoryRoot%\Artifacts\Native-Os-Probe-Object-Producer-Candidate\windows-x64-os-probe-object.exe"
if not exist "%Producer%" (
    >&2 echo The Windows native OS Probe object producer is missing.
    exit /b 1
)
for %%F in ("%Producer%") do if not "%%~zF"=="461312" (
    >&2 echo The Windows native OS Probe object producer length is invalid.
    exit /b 1
)
certutil -hashfile "%Producer%" SHA256 | findstr /i /x /c:"fcd22c975ed04534d30733c5ddabb7811a9b9578effd0d27839d171bdac76d0c" >nul
if errorlevel 1 (
    >&2 echo The Windows native OS Probe object producer digest is invalid.
    exit /b 1
)

"%Producer%" "%~1" "%Output%"
if errorlevel 1 goto :failure
for %%F in ("%Output%") do if not "%%~zF"=="%ExpectedBytes%" goto :failure
certutil -hashfile "%Output%" SHA256 | findstr /i /x /c:"%ExpectedDigest%" >nul
if errorlevel 1 goto :failure
exit /b 0

:failure
if exist "%Output%" del /f /q "%Output%" >nul 2>nul
>&2 echo The native OS Probe object producer failed.
exit /b 1

:usage
>&2 echo Usage: Tools\Native\Produce-Os-Probe-Object.cmd ^<exceptions^|wvb-admission-bridge^|native-bridge-and-support^|paging^> ^<output.wvo^>
exit /b 64
