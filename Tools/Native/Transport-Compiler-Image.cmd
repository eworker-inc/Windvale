@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~4"=="" goto :usage
if not "%~5"=="" goto :usage
if /I not "%~x2"==".wvli" goto :usage
if /I not "%~x4"==".wvli" goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Transport=%RepositoryRoot%\Artifacts\Native-Segmented-Compiler-Toolset-Candidate\windows-x64-wvimagetransport.exe"

certutil -hashfile "%Transport%" SHA256 | findstr /I /C:"6c204b9b3ee90a4d73ecdaa1ae0f0c4d5f3056973f3ccd3a8489789c6b46ef6d" >nul
if errorlevel 1 (
    >&2 echo The Windows compiler-image transport artifact digest is invalid.
    exit /b 1
)

"%Transport%" "%~f1" "%~f2" "%~f3" "%~f4"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Transport-Compiler-Image.cmd ^<source-chunk-prefix^> ^<source.wvli^> ^<canonical-chunk-prefix^> ^<canonical.wvli^>
exit /b 64
