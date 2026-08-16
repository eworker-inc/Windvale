@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if not "%~3"=="" goto :usage
if /I not "%~x1"==".wvproj" (
    >&2 echo The current native build input must use the .wvproj extension.
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "CompilerRoot=%RepositoryRoot%\Artifacts\Native-Compiler-Reconstruction-Candidate"
set "BuildDriver=%CompilerRoot%\windows-x64\wvbuild.exe"

call :verify_file "%BuildDriver%" 30381568 b0d58f4d8d6d32e09d45358035ce4521410ec1280f11fa72b66245e621ba49a3 "current Windows native build driver"
if errorlevel 1 exit /b 1

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
        >&2 echo The current native build output must use the .wvb extension.
        exit /b 64
    )
    set "OutputPath=%~f2"
)
set "OutputResource=%OutputPath:\=/%"

"%BuildDriver%" --workspace "%WorkspaceResource%" --project "%ProjectResource%" "%OutputResource%"
exit /b %ERRORLEVEL%

:verify_file
if not exist "%~1" (
    >&2 echo Missing %~4: %~1
    exit /b 1
)
for %%F in ("%~1") do if not "%%~zF"=="%~2" (
    >&2 echo The %~4 byte length is invalid.
    exit /b 1
)
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~3" >nul
if errorlevel 1 (
    >&2 echo The %~4 digest is invalid.
    exit /b 1
)
exit /b 0

:usage
>&2 echo Usage: Tools\Native\Build-Current-Wvb.cmd ^<project.wvproj^> [output.wvb]
exit /b 64
