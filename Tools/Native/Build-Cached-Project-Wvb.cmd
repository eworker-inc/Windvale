@echo off
setlocal EnableExtensions DisableDelayedExpansion
node "%~dp0Build-Cached-Project-Wvb.mjs" %*
exit /b %ERRORLEVEL%
