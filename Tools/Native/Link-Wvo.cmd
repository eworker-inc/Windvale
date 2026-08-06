@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~4"=="" goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Linker=%RepositoryRoot%\Artifacts\Native-Wv-Linker-Candidate\Wv-Linker.exe"

certutil -hashfile "%Linker%" SHA256 | findstr /I /C:"ca88735061d7e36e79813346621a867a9293d04d3c01ffb0336f4ee32cbe316d" >nul
if errorlevel 1 (
    >&2 echo The Windows native WVO linker artifact digest is invalid.
    exit /b 1
)

"%Linker%" %*
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Link-Wvo.cmd ^<base-address^> ^<entry^> ^<output.bin^> ^<input.wvo^>...
exit /b 64
