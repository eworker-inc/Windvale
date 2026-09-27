@echo off
setlocal EnableExtensions DisableDelayedExpansion
node "%~dp0Test-Assembler-Golden.mjs" %*
exit /b %ERRORLEVEL%
