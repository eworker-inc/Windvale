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

certutil -hashfile "%BuildDriver%" SHA256 | findstr /I /C:"ee338c635aa817a26081c4327da4b36b78557f10518268162b8039d1f82316f4" >nul
if errorlevel 1 (
    >&2 echo The Windows native build-driver artifact digest is invalid.
    exit /b 1
)
certutil -hashfile "%Publisher%" SHA256 | findstr /I /C:"f2502ecf9143cfa1343c5f5cb1de066bdf1f82f0e4782afae178f11c41afd735" >nul
if errorlevel 1 (
    >&2 echo The Windows native publisher artifact digest is invalid.
    exit /b 1
)

set "ProjectPath=%~f1"
set "ProjectResource=%ProjectPath:\=/%"
if "%~2"=="" (
    set "OutputPath=%~dpn1.wvb"
) else (
    if /I not "%~x2"==".wvb" (
        >&2 echo The native build output must use the .wvb extension.
        exit /b 64
    )
    set "OutputPath=%~f2"
)

:allocate
set "TemporaryDirectory=%TEMP%\windvale-native-build-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "CandidatePath=%TemporaryDirectory%\Candidate.wvb"
set "CandidateResource=%CandidatePath:\=/%"

"%BuildDriver%" --project "%ProjectResource%" "%CandidateResource%"
set "Result=%ERRORLEVEL%"
if "%Result%"=="0" (
    "%Publisher%" "%CandidatePath%" "%OutputPath%"
    set "Result=%ERRORLEVEL%"
)
if exist "%CandidatePath%" del /f /q "%CandidatePath%" >nul 2>nul
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b %Result%

:usage
>&2 echo Usage: Tools\Native\Build-Wvb.cmd ^<project.wvproj^> [output.wvb]
exit /b 64
