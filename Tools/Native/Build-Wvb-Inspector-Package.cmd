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
set "ExpectedManifest=%RepositoryRoot%\Distribution\Applications\Wvb-Inspector\Windvale-Wvb-Inspector.wvpack"
if /I not "%Manifest%"=="%ExpectedManifest%" (
    >&2 echo package status=Invalid_invocation reason=manifest-identity
    exit /b 64
)

call :verify_file "%Lock%" 1021 eef8bd6d8ab5c535d263fb914fa3fae6f82ee9ae16b0854de497749475f76ad1
if errorlevel 1 goto :lock_rejected
call :verify_file "%Manifest%" 412 a58441a48b0e11c4062e77b0176934952c1de238c78d04ba88ca9ca61e0a41b6
if errorlevel 1 goto :lock_rejected
call :verify_file "%RepositoryRoot%\Windvale.wvws" 21 5cb4f5f771ffd5a9f443ca993fd66f53109cd5862f7c268f1f3958a36b8f4199
if errorlevel 1 goto :lock_rejected
call :verify_file "%RepositoryRoot%\Artifacts\Native-Compiler-Seed\Wvb\Windvale-Compiler.wvb" 914746 48ff781359d9bab96ec3e19e4edba19a26ba82552d5bfd1c1a72d64b75f224a6
if errorlevel 1 goto :lock_rejected
call :verify_file "%RepositoryRoot%\Artifacts\Native-Front-Door\Wvb\Wvb-Inspector.wvb" 76527 293be3267ff95f9272e96684e036a5647abc060f2bc87a9e654beac7140af753
if errorlevel 1 goto :lock_rejected

rem The distribution lock intentionally retains the pre-metadata source product.
"%RepositoryRoot%\Artifacts\Native-Front-Door\windows-x64\wvpublish.exe" ^
    "%RepositoryRoot%\Artifacts\Native-Front-Door\Wvb\Wvb-Inspector.wvb" ^
    "%Output%" >nul || exit /b 1
echo package status=Published root=windvale.wvb-inspector target=hosted-wvb-v1 bytes=76527 sha256=293be3267ff95f9272e96684e036a5647abc060f2bc87a9e654beac7140af753 source=frozen-historical-input
exit /b 0

:lock_rejected
>&2 echo package status=Lock_rejected reason=identity-or-resource
exit /b 1

:verify_file
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /i /c:"%~3" >nul
if errorlevel 1 exit /b 1
exit /b 0

:usage
>&2 echo Usage: Tools\Native\Build-Wvb-Inspector-Package.cmd ^<manifest.wvpack^> ^<lock.wvlock^> ^<output.wvb^>
exit /b 64
