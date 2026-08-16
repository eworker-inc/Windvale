@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~3"=="" goto :usage
if not "%~4"=="" goto :usage
if /I not "%~x1"==".wvpack" goto :usage
if /I not "%~x2"==".wvlock" goto :usage
if /I not "%~x3"==".wvb" goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Manifest=%~f1"
set "Lock=%~f2"
set "Output=%~f3"
set "ExpectedManifest=%RepositoryRoot%\Distribution\Applications\Echo\Windvale-Echo.wvpack"
if /I not "%Manifest%"=="%ExpectedManifest%" (
    >&2 echo package status=Invalid_invocation reason=manifest-identity
    exit /b 64
)

call :verify_file "%Lock%" 920 212e5c4ddf28fb347b482c73d5c38d6df8273be4bcf14ce1b581084d7be1652d || goto :lock_rejected
call :verify_file "%Manifest%" 333 27d32dc98d1c2d57792f0a37b173a77d5dab465e005bc9c47fd8fd086c8b6234 || goto :lock_rejected
call :verify_file "%RepositoryRoot%\Windvale.wvws" 21 5cb4f5f771ffd5a9f443ca993fd66f53109cd5862f7c268f1f3958a36b8f4199 || goto :lock_rejected
call :verify_file "%RepositoryRoot%\Artifacts\Native-Compiler-Seed\Wvb\Windvale-Compiler.wvb" 914746 48ff781359d9bab96ec3e19e4edba19a26ba82552d5bfd1c1a72d64b75f224a6 || goto :lock_rejected
call :verify_file "%RepositoryRoot%\Projects\Applications\Windvale-Echo.wvproj" 62 bf5b476f36512f48c0798fc1683708872500094e6a853ba6274d3ee7a8b3c6ef || goto :lock_rejected
call :verify_file "%RepositoryRoot%\Applications\Shell\Echo.wv" 755 0738f826901ac6b03121d7a534b2c07f79f89475bd2af33f5c45cba895dae91d || goto :lock_rejected

:allocate
set "Work=%TEMP%\windvale-echo-package-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Candidate=%Work%\Candidate.wvb"
set "Result=1"

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Applications\Windvale-Echo.wvproj" "%Candidate%" >nul || goto :cleanup
call :verify_file "%Candidate%" 813 5d827b98be518a07a8dea60d79e70073535f78f07cf875d750021fa795c13c64 || goto :cleanup
"%RepositoryRoot%\Artifacts\Native-Front-Door\windows-x64\wvpublish.exe" "%Candidate%" "%Output%" >nul || goto :cleanup
echo package status=Published root=windvale.echo target=hosted-wvb-v1 bytes=813 sha256=5d827b98be518a07a8dea60d79e70073535f78f07cf875d750021fa795c13c64
set "Result=0"

:cleanup
for %%R in ("%Work%") do set "ResolvedWork=%%~fR"
echo(%ResolvedWork%| findstr /b /i /c:"%TEMP%\windvale-echo-package-" >nul || exit /b 1
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
exit /b %Result%

:lock_rejected
>&2 echo package status=Lock_rejected reason=identity-or-resource
exit /b 1

:verify_file
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /i /c:"%~3" >nul
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Build-Echo-Package.cmd ^<manifest.wvpack^> ^<lock.wvlock^> ^<output.wvb^>
exit /b 64
