@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if not "%~2"=="" goto :usage
if /I not "%~x1"==".wvb" (
    >&2 echo The native inspector input must use the .wvb extension.
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Verifier=%RepositoryRoot%\Artifacts\Native-Front-Door\windows-x64\wvverify.exe"
set "Inspector=%RepositoryRoot%\Artifacts\Native-Front-Door\windows-x64\wvdump.exe"

certutil -hashfile "%Verifier%" SHA256 | findstr /I /C:"f15422397ad890909f481f131f945e25651c858695ba5ce58b2a7305b34647f0" >nul
if errorlevel 1 (
    >&2 echo The Windows native WVB verifier artifact digest is invalid.
    exit /b 1
)
certutil -hashfile "%Inspector%" SHA256 | findstr /I /C:"30f8c6cbb1555665063dfb70fa35f08d90818107298c6ab5b91f845814d22daa" >nul
if errorlevel 1 (
    >&2 echo The Windows native WVB inspector artifact digest is invalid.
    exit /b 1
)

"%Verifier%" "%~f1" >nul
if errorlevel 1 exit /b %ERRORLEVEL%
"%Inspector%" "%~f1"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Inspect-Wvb.cmd ^<module.wvb^>
exit /b 64
