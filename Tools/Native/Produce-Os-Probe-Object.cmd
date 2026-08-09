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
) else if "%~1"=="memory" (
    set "ExpectedBytes=1529"
    set "ExpectedDigest=2668e17c3181e168415fb7bdee530873e2ddc8fa2d100af94bcc7b74909df3ed"
) else if "%~1"=="loader" (
    set "ExpectedBytes=6336"
    set "ExpectedDigest=b310bc0e9aebc7b14c0892bb3dd4b833d42539c2194427a8f333b511d6af3804"
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
set "CodeFixture="
if "%~1"=="loader" (
    set "Producer=%RepositoryRoot%\Artifacts\Native-Os-Probe-Loader-Object-Producer-Candidate\windows-x64-os-probe-loader-object.exe"
    set "ProducerBytes=387072"
    set "ProducerDigest=1ce2a2e3dd84d5af9a614b06382226c105e6051ba07d205a66c6d47e8d0e373c"
    set "CodeFixture=%RepositoryRoot%\Artifacts\Native-Os-Probe-Loader-Object-Producer-Candidate\normal-x64-loader.bin"
) else if "%~1"=="memory" (
    set "Producer=%RepositoryRoot%\Artifacts\Native-Os-Probe-Memory-Object-Producer-Candidate\windows-x64-os-probe-memory-object.exe"
    set "ProducerBytes=399872"
    set "ProducerDigest=79461480b72cc1865278ea6f06170b8f4e9f4e849898d7b3c06aa3d36ff70032"
) else (
    set "Producer=%RepositoryRoot%\Artifacts\Native-Os-Probe-Object-Producer-Candidate\windows-x64-os-probe-object.exe"
    set "ProducerBytes=461312"
    set "ProducerDigest=fcd22c975ed04534d30733c5ddabb7811a9b9578effd0d27839d171bdac76d0c"
)
if not exist "%Producer%" (
    >&2 echo The Windows native OS Probe object producer is missing.
    exit /b 1
)
for %%F in ("%Producer%") do if not "%%~zF"=="%ProducerBytes%" (
    >&2 echo The Windows native OS Probe object producer length is invalid.
    exit /b 1
)
certutil -hashfile "%Producer%" SHA256 | findstr /i /x /c:"%ProducerDigest%" >nul
if errorlevel 1 (
    >&2 echo The Windows native OS Probe object producer digest is invalid.
    exit /b 1
)
if defined CodeFixture (
    if not exist "%CodeFixture%" goto :invalid_fixture
    for %%F in ("%CodeFixture%") do if not "%%~zF"=="6115" goto :invalid_fixture
    certutil -hashfile "%CodeFixture%" SHA256 | findstr /i /x /c:"19008f698db52c206dae920cf57ca4461eb009d47d8ecba258d6b021b05a2eed" >nul
    if errorlevel 1 goto :invalid_fixture
)

if defined CodeFixture (
    "%Producer%" "%~1" "%CodeFixture%" "%Output%"
) else (
    "%Producer%" "%~1" "%Output%"
)
if errorlevel 1 goto :failure
for %%F in ("%Output%") do if not "%%~zF"=="%ExpectedBytes%" goto :failure
certutil -hashfile "%Output%" SHA256 | findstr /i /x /c:"%ExpectedDigest%" >nul
if errorlevel 1 goto :failure
exit /b 0

:failure
if exist "%Output%" del /f /q "%Output%" >nul 2>nul
>&2 echo The native OS Probe object producer failed.
exit /b 1

:invalid_fixture
>&2 echo The native OS Probe loader code fixture identity is invalid.
exit /b 1

:usage
>&2 echo Usage: Tools\Native\Produce-Os-Probe-Object.cmd ^<exceptions^|wvb-admission-bridge^|native-bridge-and-support^|paging^|memory^|loader^> ^<output.wvo^>
exit /b 64
