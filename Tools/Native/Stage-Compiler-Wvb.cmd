@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~3"=="" goto :usage
if not "%~4"=="" goto :usage
if /I not "%~x1"==".wvb" goto :usage
if /I not "%~x3"==".wvop" goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Producer=%RepositoryRoot%\Artifacts\Native-Segmented-Compiler-Toolset-Candidate\windows-x64-wvstage.exe"

certutil -hashfile "%Producer%" SHA256 | findstr /I /C:"a3bdc02c4f6afc52f3391098599593a25a535dc5fff5d0fb22d499bbb38e1eae" >nul
if errorlevel 1 (
    >&2 echo The Windows segmented WVO producer artifact digest is invalid.
    exit /b 1
)

"%Producer%" "%~f1" "%~f2" "%~f3"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Stage-Compiler-Wvb.cmd ^<input.wvb^> ^<wvo-chunk-prefix^> ^<manifest.wvop^>
exit /b 64
