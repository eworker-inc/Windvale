@echo off
setlocal EnableExtensions DisableDelayedExpansion

node "%~dp0Test-Language-1.0-Authenticated-Foreign-Binding.mjs" %*
exit /b %ERRORLEVEL%
