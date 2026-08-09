@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~3"=="" goto :usage
if not "%~4"=="" goto :usage
if /I not "%~x1"==".bin" goto :usage
if /I not "%~x3"==".efi" goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Packager=%RepositoryRoot%\Artifacts\Native-Uefi-Packager-Candidate\Uefi-Packager.exe"

if not exist "%Packager%" (
    >&2 echo The Windows native UEFI packager artifact is missing.
    exit /b 1
)
for %%F in ("%Packager%") do if not "%%~zF"=="278528" (
    >&2 echo The Windows native UEFI packager artifact length is invalid.
    exit /b 1
)
certutil -hashfile "%Packager%" SHA256 | findstr /I /C:"326401d0e3d9e6b1c1e329a1fa0c7f5e550f4d48e073d0ab83f6f8657edff320" >nul
if errorlevel 1 (
    >&2 echo The Windows native UEFI packager artifact digest is invalid.
    exit /b 1
)

"%Packager%" "%~f1" "%~2" "%~f3"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Package-Uefi.cmd ^<native-image.bin^> ^<entry-offset^> ^<output.efi^>
exit /b 64
