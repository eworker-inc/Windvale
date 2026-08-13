@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if not "%~3"=="" goto :usage
if /I not "%~x1"==".wva" (
    >&2 echo The native assembler input must use the .wva extension.
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Assembler=%RepositoryRoot%\Artifacts\Native-Front-Door\windows-x64\wvasm.exe"

certutil -hashfile "%Assembler%" SHA256 | findstr /I /C:"40a35687fb052dcd4f6d3a767436f4024d91bd5f03890b30fa4f0300184a35ed" >nul
if errorlevel 1 (
    >&2 echo The Windows native WVA assembler artifact digest is invalid.
    exit /b 1
)

if "%~2"=="" (
    set "OutputPath=%~dpn1.wvo"
) else (
    if /I not "%~x2"==".wvo" (
        >&2 echo The native assembler output must use the .wvo extension.
        exit /b 64
    )
    set "OutputPath=%~f2"
)

"%Assembler%" "%~f1" "%OutputPath%"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Assemble-Wva.cmd ^<source.wva^> [output.wvo]
exit /b 64
