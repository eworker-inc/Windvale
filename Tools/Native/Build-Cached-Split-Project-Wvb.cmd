@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~6"=="" goto :usage
if not "%~7"=="" goto :usage

node "%~dp0Build-Cached-Split-Project-Wvb.mjs" ^
    "%~f1" "%~f2" "%~f3" "%~f4" "%~f5" "%~f6"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Build-Cached-Split-Project-Wvb.cmd ^<project.wvproj^> ^<output.wvb^> ^<analyzer.exe^> ^<analyzer.identity^> ^<emitter.exe^> ^<emitter.identity^>
exit /b 64
