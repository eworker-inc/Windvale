@echo off
setlocal EnableExtensions DisableDelayedExpansion
if "%~3"=="" goto :usage
if not "%~4"=="" goto :usage
node "%~dp0Build-Wvdb-Query-Package.mjs" "%~f1" "%~f2" "%~f3"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Build-Wvdb-Query-Package.cmd ^<manifest.wvpack^> ^<lock.wvlock^> ^<output.wvb^>
exit /b 64
