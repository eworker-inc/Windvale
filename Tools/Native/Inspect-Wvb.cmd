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

certutil -hashfile "%Verifier%" SHA256 | findstr /I /C:"2b870ae276ee8c53e7b6f7277a19067ddb9466e5ac7e0b640745a5d40810efd1" >nul
if errorlevel 1 (
    >&2 echo The Windows native WVB verifier artifact digest is invalid.
    exit /b 1
)
certutil -hashfile "%Inspector%" SHA256 | findstr /I /C:"61512dae2941607b93da7d29dd59f973c690f0fec3ba24f772f2101c87ed5381" >nul
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
