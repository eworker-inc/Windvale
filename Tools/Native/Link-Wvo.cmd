@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~4"=="" goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Linker=%RepositoryRoot%\Artifacts\Native-Wv-Linker-Candidate\Wv-Linker.exe"

certutil -hashfile "%Linker%" SHA256 | findstr /I /C:"f47a952867203fbff53abb131ea155b4fe9e14a8be153cc61c0ca5fd8e4a74e0" >nul
if errorlevel 1 (
    >&2 echo The Windows native WVO linker artifact digest is invalid.
    exit /b 1
)

"%Linker%" %*
set "LinkerExit=%ERRORLEVEL%"

rem The hosted-compiler shell exposes WVR3025 as 64 + service detail 9.
rem Wv-Linker's public immutable-snapshot boundary normalizes that failure to
rem one before its Windvale Main can run; 73 is not a linker process result.
if "%LinkerExit%"=="73" set "LinkerExit=1"
exit /b %LinkerExit%

:usage
>&2 echo Usage: Tools\Native\Link-Wvo.cmd ^<base-address^> ^<entry^> ^<output.bin^> ^<input.wvo^>...
exit /b 64
