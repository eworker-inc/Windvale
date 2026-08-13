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
set "ExpectedManifest=%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvpack"
if /I not "%Manifest%"=="%ExpectedManifest%" (
    >&2 echo package status=Invalid_invocation reason=manifest-identity
    exit /b 64
)

call :verify_file "%Lock%" 1750 51d918b47e0dfb05f4d4aed0aff9f322a247e544b46c48a220fb9567be04612e "package lock"
if errorlevel 1 goto :lock_rejected
call :verify_file "%Manifest%" 866 835f573302377fdd38e4c3d51fa9106397beba0b9813f99bfc3143d08a156406 "package manifest"
if errorlevel 1 goto :lock_rejected
call :verify_file "%RepositoryRoot%\Windvale.wvws" 21 5cb4f5f771ffd5a9f443ca993fd66f53109cd5862f7c268f1f3958a36b8f4199 "workspace"
if errorlevel 1 goto :lock_rejected
call :verify_file "%RepositoryRoot%\Artifacts\Native-Compiler-Seed\Wvb\Windvale-Compiler.wvb" 914746 48ff781359d9bab96ec3e19e4edba19a26ba82552d5bfd1c1a72d64b75f224a6 "compiler"
if errorlevel 1 goto :lock_rejected
call :verify_file "%RepositoryRoot%\Projects\Applications\Windvale-Wvdb-Query.wvproj" 270 86570daa0dac6410dc8a64947901a3fc955db24afe3589bc70986f96abb8f49a "project"
if errorlevel 1 goto :lock_rejected
call :verify_file "%RepositoryRoot%\Applications\Database\Wvdb-Query.wv" 2721 1159c10ae4b565e382c57cdf1dcc07311da961fede5e73bb950bca80c9c1a77b "application part"
if errorlevel 1 goto :lock_rejected
call :verify_file "%RepositoryRoot%\Foundation\Decimal-Parsing.wv" 1276 797eb31da7e7a8c93e0d082bf910bc6d8e7988bcfad757a87c979075912e668a "decimal part"
if errorlevel 1 goto :lock_rejected
call :verify_file "%RepositoryRoot%\Libraries\Platform\Filesystem\Read-Only-Directory.wv" 6565 4c6ecc745b0755b0242c7127c391d27408a7694f91d634c55eeb512746393c81 "directory part"
if errorlevel 1 goto :lock_rejected
call :verify_file "%RepositoryRoot%\Libraries\Platform\Database\Read-Only-Wvdb.wv" 9084 7b3bd45397878e5468d979a2fb437feb4d72d5d8bbad21c832bcf3f280c018cb "WVDB platform part"
if errorlevel 1 goto :lock_rejected
call :verify_file "%RepositoryRoot%\Libraries\Database\Wvdb-Reader.wv" 11213 ad6fd38dafdab57793aead612dd050817f65f22179d11b0f3dbab6654ac909c2 "WVDB reader part"
if errorlevel 1 goto :lock_rejected

:allocate
set "TemporaryDirectory=%TEMP%\windvale-wvdb-package-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "Candidate=%TemporaryDirectory%\Candidate.wvb"
set "Result=1"

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Applications\Windvale-Wvdb-Query.wvproj" ^
    "%Candidate%" >nul
if errorlevel 1 goto :cleanup
call :verify_file "%Candidate%" 25763 063a23023d2baedbf21503ae43d4c3eac13a0e77bf87d0b41182518bf88f53f8 "locked output"
if errorlevel 1 goto :cleanup
"%RepositoryRoot%\Artifacts\Native-Front-Door\windows-x64\wvpublish.exe" "%Candidate%" "%Output%" >nul
if errorlevel 1 goto :cleanup
echo package status=Published root=windvale.wvdb-query target=hosted-wvb-v1 bytes=25763 sha256=063a23023d2baedbf21503ae43d4c3eac13a0e77bf87d0b41182518bf88f53f8
set "Result=0"

:cleanup
if exist "%Candidate%" del /f /q "%Candidate%" >nul 2>nul
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b %Result%

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
>&2 echo Usage: Tools\Native\Build-Wvdb-Query-Package.cmd ^<manifest.wvpack^> ^<lock.wvlock^> ^<output.wvb^>
exit /b 64
