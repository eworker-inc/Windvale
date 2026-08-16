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
set "Compiler=%RepositoryRoot%\Artifacts\Native-Compiler-Reconstruction-Candidate\windows-x64\wvcompiler.exe"
set "Publisher=%RepositoryRoot%\Artifacts\Native-Wvb-Publisher-Candidate\windows-x64-wvpublish.exe"
set "ExpectedManifest=%RepositoryRoot%\Distribution\Applications\Echo\Windvale-Echo.wvpack"
if /I not "%Manifest%"=="%ExpectedManifest%" (
    >&2 echo package status=Invalid_invocation reason=manifest-identity
    exit /b 64
)

call :verify_file "%Lock%" 940 b2bd85d1f76d062c45a8f5ee8ce531b87aeaf499440f1fdf52a4a8a5aca317f3 || goto :lock_rejected
call :verify_file "%Manifest%" 333 27d32dc98d1c2d57792f0a37b173a77d5dab465e005bc9c47fd8fd086c8b6234 || goto :lock_rejected
call :verify_file "%RepositoryRoot%\Windvale.wvws" 21 5cb4f5f771ffd5a9f443ca993fd66f53109cd5862f7c268f1f3958a36b8f4199 || goto :lock_rejected
call :verify_file "%RepositoryRoot%\Artifacts\Native-Compiler-Reconstruction-Candidate\Wvb\Windvale-Compiler.wvb" 923818 49b5cbf040de4bcb22c071a5da9a4fbad47f4f0658ef910957a67b52c07607c2 || goto :lock_rejected
call :verify_file "%Compiler%" 27678720 6f266759e2d2524ad9ce2045cb21243538efc7bce35ab1f94a7da4009865eac8 || goto :lock_rejected
call :verify_file "%Publisher%" 1544192 0fdb432aa54cc7b9cc4a1d42a438d2b56a29695e06b2369540dac845989751c1 || goto :lock_rejected
call :verify_file "%RepositoryRoot%\Projects\Applications\Windvale-Echo.wvproj" 62 bf5b476f36512f48c0798fc1683708872500094e6a853ba6274d3ee7a8b3c6ef || goto :lock_rejected
call :verify_file "%RepositoryRoot%\Applications\Shell\Echo.wv" 845 f843e69b9549a890aa808331f6ef503941c0a1d5240ecd5859e46f6f8ae044c7 || goto :lock_rejected

:allocate
set "Work=%TEMP%\windvale-echo-package-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Candidate=%Work%\Candidate.wvb"
set "Result=1"

"%Compiler%" "%RepositoryRoot%\Applications\Shell\Echo.wv" "%Candidate%" >nul || goto :cleanup
call :verify_file "%Candidate%" 927 b83890661281e79b17d14c49e7b971e37701c8112310b7b5f1f3f05e035dc713 || goto :cleanup
"%Publisher%" "%Candidate%" "%Output%" >nul || goto :cleanup
echo package status=Published root=windvale.echo target=hosted-wvb-v1 bytes=927 sha256=b83890661281e79b17d14c49e7b971e37701c8112310b7b5f1f3f05e035dc713 metadata=Present
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
