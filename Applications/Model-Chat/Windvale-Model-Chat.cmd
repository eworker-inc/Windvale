@echo off
setlocal EnableExtensions DisableDelayedExpansion
if /I not "%~1"=="chat" goto :launch
if defined WINDVALE_MODEL_CHAT_APPLICATION goto :launch
if exist "%~dp0..\..\Artifacts\Applications\Model-Chat\Windvale-Model-Chat.exe" set "WINDVALE_MODEL_CHAT_APPLICATION=%~dp0..\..\Artifacts\Applications\Model-Chat\Windvale-Model-Chat.exe"
:launch
node "%~dp0Windvale-Model-Chat.mjs" %*
exit /b %ERRORLEVEL%
