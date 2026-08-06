@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if "%~2"=="" goto :usage
if not "%~3"=="" goto :usage
if /I not "%~x1"==".wvb" (
    >&2 echo The native lowerer input must use the .wvb extension.
    exit /b 2
)
if /I not "%~x2"==".wvo" (
    >&2 echo The native lowerer output must use the .wvo extension.
    exit /b 2
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Lowerer=%RepositoryRoot%\Artifacts\Native-Wvb-To-Wvo-Candidate\Wvb-To-Wvo.exe"

certutil -hashfile "%Lowerer%" SHA256 | findstr /I /C:"0e0d0c87f82f6576b11f888cfa26469f86f157064ea605a4bb188bcee5e3b280" >nul
if errorlevel 1 (
    >&2 echo The Windows native WVB-to-WVO lowerer artifact digest is invalid.
    exit /b 1
)

"%Lowerer%" "%~f1" "%~f2"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Lower-Wvb-To-Wvo.cmd ^<input.wvb^> ^<output.wvo^>
exit /b 2
