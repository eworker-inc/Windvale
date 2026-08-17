@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~5"=="" goto :usage
if not "%~6"=="" goto :usage
if /I not "%~x1"==".wvproj" goto :usage
if /I not "%~x3"==".wvb" goto :usage
if /I not "%~x5"==".wvli" goto :usage

node "%~dp0Build-Cached-Segmented-Project.mjs" ^
    "%~f1" "%~f2" "%~f3" "%~f4" "%~f5"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Build-Cached-Segmented-Project.cmd ^<project.wvproj^> ^<build-driver.exe^> ^<output.wvb^> ^<canonical-chunk-prefix^> ^<canonical.wvli^>
exit /b 64
