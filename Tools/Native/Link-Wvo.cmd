@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~4"=="" goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Linker=%RepositoryRoot%\Artifacts\Native-Wv-Linker-Candidate\Wv-Linker.exe"

certutil -hashfile "%Linker%" SHA256 | findstr /I /C:"c42b75a033fc79c5a967330e83fc498704840d2cb45723471a8c752dadf0b6e3" >nul
if errorlevel 1 (
    >&2 echo The Windows native WVO linker artifact digest is invalid.
    exit /b 1
)

"%Linker%" %*
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Link-Wvo.cmd ^<base-address^> ^<entry^> ^<output.bin^> ^<input.wvo^>...
exit /b 64
