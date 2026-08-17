@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~5"=="" goto :usage
if not "%~6"=="" goto :usage
if /I not "%~x1"==".wvproj" goto :usage
if /I not "%~x4"==".wvb" goto :usage
if /I not "%~x5"==".wvo" goto :usage

node "%~dp0Build-Cached-Project-Object.mjs" ^
    "%~f1" "%~f2" "%~f3" "%~f4" "%~f5"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Build-Cached-Project-Object.cmd ^<project.wvproj^> ^<build-driver.exe^> ^<lowerer.exe^> ^<output.wvb^> ^<output.wvo^>
exit /b 64
