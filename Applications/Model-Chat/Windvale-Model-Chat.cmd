@echo off
setlocal EnableExtensions DisableDelayedExpansion
if /I not "%~1"=="chat" goto :launch
if defined WINDVALE_MODEL_CHAT_APPLICATION goto :launch
set "WINDVALE_MODEL_CHAT_APPLICATION=%~dp0..\..\Artifacts\Applications\Model-Chat\Windvale-Model-Chat.exe"
if not exist "%~dp0..\..\Artifacts\Applications\Model-Chat\." mkdir "%~dp0..\..\Artifacts\Applications\Model-Chat" || exit /b 1
if not exist "%WINDVALE_MODEL_CHAT_APPLICATION%" call "%~dp0..\..\Tools\Native\Build-Windvale-Model-Chat.cmd" "%WINDVALE_MODEL_CHAT_APPLICATION%" || exit /b %ERRORLEVEL%
:launch
node "%~dp0Windvale-Model-Chat.mjs" %*
exit /b %ERRORLEVEL%
