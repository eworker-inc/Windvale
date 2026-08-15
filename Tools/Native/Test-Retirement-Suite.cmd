@echo off
rem Compatibility entry point. Current work uses Test-Verification-Owners.cmd.
call "%~dp0Test-Verification-Owners.cmd" %*
exit /b %ERRORLEVEL%
