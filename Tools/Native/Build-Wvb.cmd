@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if not "%~3"=="" goto :usage
if /I not "%~x1"==".wvproj" (
    >&2 echo The native build input must use the .wvproj extension.
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "ArtifactRoot=%RepositoryRoot%\Artifacts\Native-Front-Door"
set "BuildDriver=%ArtifactRoot%\windows-x64\wvbuild.exe"
set "Publisher=%ArtifactRoot%\windows-x64\wvpublish.exe"

certutil -hashfile "%BuildDriver%" SHA256 | findstr /I /C:"65602cd41bd929f9d698d9a4a74f683a8525b7dc2c903a5462e8b22fe1fe34ec" >nul
if errorlevel 1 (
    >&2 echo The Windows native build-driver artifact digest is invalid.
    exit /b 1
)
certutil -hashfile "%Publisher%" SHA256 | findstr /I /C:"b9fd1b11bc1e4a726e4a43b16830a9351fe573b30e547ba8d8f6660f688ed421" >nul
if errorlevel 1 (
    >&2 echo The Windows native publisher artifact digest is invalid.
    exit /b 1
)

set "ProjectPath=%~f1"
set "ProjectResource=%ProjectPath:\=/%"
set "WorkspacePath=%RepositoryRoot%\Windvale.wvws"
if not exist "%WorkspacePath%" (
    >&2 echo The native workspace marker is missing.
    exit /b 1
)
fsutil reparsepoint query "%RepositoryRoot%" >nul 2>nul
if not errorlevel 1 (
    >&2 echo The native workspace root must not be a reparse point.
    exit /b 1
)
for /f "delims=" %%L in ('dir /a:l /s /b "%RepositoryRoot%" 2^>nul') do (
    >&2 echo The native workspace must not contain a reparse point: %%L
    exit /b 1
)
set "WorkspaceResource=%WorkspacePath:\=/%"
if "%~2"=="" (
    set "OutputPath=%~dpn1.wvb"
) else (
    if /I not "%~x2"==".wvb" (
        >&2 echo The native build output must use the .wvb extension.
        exit /b 64
    )
    set "OutputPath=%~f2"
)

set /a AllocationAttempts=0
:allocate
set /a AllocationAttempts+=1
if %AllocationAttempts% GTR 32 (
    >&2 echo The native build could not allocate a private temporary directory.
    exit /b 1
)
set "TemporaryDirectory=%TEMP%\windvale-native-build-%RANDOM%-%RANDOM%-%RANDOM%"
mkdir "%TemporaryDirectory%" >nul 2>nul
if errorlevel 1 goto :allocate
set "CandidatePath=%TemporaryDirectory%\Candidate.wvb"
set "CandidateResource=%CandidatePath:\=/%"

"%BuildDriver%" --workspace "%WorkspaceResource%" --project "%ProjectResource%" "%CandidateResource%"
set "Result=%ERRORLEVEL%"
if not "%Result%"=="0" goto :cleanup
"%Publisher%" "%CandidatePath%" "%OutputPath%"
set "Result=%ERRORLEVEL%"

:cleanup
if exist "%CandidatePath%" del /f /q "%CandidatePath%" >nul 2>nul
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b %Result%

:usage
>&2 echo Usage: Tools\Native\Build-Wvb.cmd ^<project.wvproj^> [output.wvb]
exit /b 64
