@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~4"=="" goto :usage
if not "%~5"=="" goto :usage
if /I not "%~x2"==".wvop" goto :usage
if /I not "%~x4"==".wvli" goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Linker=%RepositoryRoot%\Artifacts\Native-Segmented-Compiler-Toolset-Candidate\windows-x64-wvlinkstage.exe"

certutil -hashfile "%Linker%" SHA256 | findstr /I /C:"7f4be5d6b1236b5f5171e52f3861540432c4781140d154e28d52f804aa8cbcde" >nul
if errorlevel 1 (
    >&2 echo The Windows segmented compiler-image linker artifact digest is invalid.
    exit /b 1
)

"%Linker%" "%~f1" "%~f2" "%~f3" "%~f4"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Link-Staged-Compiler-Wvo.cmd ^<wvo-chunk-prefix^> ^<manifest.wvop^> ^<image-chunk-prefix^> ^<manifest.wvli^>
exit /b 64
