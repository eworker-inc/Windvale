@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if "%~2"=="" goto :usage
if "%~3"=="" goto :usage
if "%~4"=="" goto :usage
if not "%~5"=="" goto :usage
if /I not "%~x1"==".wvo" goto :usage
if /I not "%~x4"==".wvo" goto :usage

set "Input=%~f1"
set "Output=%~f4"
if exist "%Output%" (
    >&2 echo The native WVO export-renamer output already exists.
    exit /b 1
)
for %%F in ("%Output%") do set "OutputDirectory=%%~dpF"
if not exist "%OutputDirectory%" (
    >&2 echo The native WVO export-renamer output directory does not exist.
    exit /b 1
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Renamer=%RepositoryRoot%\Artifacts\Native-Wvo-Export-Renamer-Candidate\windows-x64-wvorename.exe"
if not exist "%Renamer%" (
    >&2 echo The Windows native WVO export renamer is missing.
    exit /b 1
)
for %%F in ("%Renamer%") do if not "%%~zF"=="391680" (
    >&2 echo The Windows native WVO export-renamer length is invalid.
    exit /b 1
)
certutil -hashfile "%Renamer%" SHA256 | findstr /i /c:"2cf43335af7782676e21ecdd5cb946cb3c9a7309572e21eadac5c7f5d33d2244" >nul
if errorlevel 1 (
    >&2 echo The Windows native WVO export-renamer digest is invalid.
    exit /b 1
)

"%Renamer%" "%Input%" "%~2" "%~3" "%Output%"
set "Status=%ERRORLEVEL%"
if not "%Status%"=="0" if exist "%Output%" del /f /q "%Output%" >nul 2>nul
exit /b %Status%

:usage
>&2 echo Usage: Tools\Native\Rename-Wvo-Export.cmd ^<input.wvo^> ^<old-export^> ^<new-export^> ^<output.wvo^>
exit /b 64
