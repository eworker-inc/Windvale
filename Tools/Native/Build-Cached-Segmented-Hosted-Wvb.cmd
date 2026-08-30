@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~3"=="" goto :usage
if not "%~4"=="" goto :usage
if /I not "%~x2"==".wvb" goto :usage
if /I not "%~x3"==".exe" goto :usage

set "ScriptDirectory=%~dp0"
node "%ScriptDirectory%Build-Cached-Segmented-Hosted-Wvb.mjs" ^
    "%~1" "%~f2" "%~f3"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Build-Cached-Segmented-Hosted-Wvb.cmd ^<profile-1-through-7^> ^<input.wvb^> ^<output.exe^>
exit /b 64
