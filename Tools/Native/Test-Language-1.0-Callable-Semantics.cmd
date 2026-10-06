@echo off
setlocal EnableExtensions DisableDelayedExpansion

node "%~dp0Test-Language-1.0-Callable-Semantics.mjs" %*
exit /b %ERRORLEVEL%
