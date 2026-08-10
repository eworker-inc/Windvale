@echo off
setlocal EnableExtensions DisableDelayedExpansion

call "%~dp0Test-Segmented-Compiler-Toolset-Reconstruction.cmd" %*
exit /b %ERRORLEVEL%
