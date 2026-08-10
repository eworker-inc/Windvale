@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~2"=="" goto :usage
if not "%~3"=="" goto :usage
call "%~dp0Construct-Hosted-Verifier-Publisher.cmd" promoter "%~1" "%~2"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Construct-Hosted-Verifier-Publisher-Promoter.cmd ^<windows^|linux^> ^<output.exe^|output.elf^>
exit /b 64
