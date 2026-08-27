@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~3"=="" goto :usage
if not "%~4"=="" goto :usage
if /I not "%~x1"==".wvb" goto :usage
if /I not "%~x3"==".wvop" goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Producer=%RepositoryRoot%\Artifacts\Native-Segmented-Compiler-Toolset-Candidate\windows-x64-wvstage.exe"

certutil -hashfile "%Producer%" SHA256 | findstr /I /C:"fa939fb8f0d45182dcffe1a4a4e9e2eedb264bcba5e71544e81e7c5c0013628e" >nul
if errorlevel 1 (
    >&2 echo The Windows segmented WVO producer artifact digest is invalid.
    exit /b 1
)

"%Producer%" "%~f1" "%~f2" "%~f3"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Stage-Compiler-Wvb.cmd ^<input.wvb^> ^<wvo-chunk-prefix^> ^<manifest.wvop^>
exit /b 64
