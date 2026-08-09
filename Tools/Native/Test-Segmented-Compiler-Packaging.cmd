@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Candidate=%RepositoryRoot%\Artifacts\Native-Segmented-Compiler-Toolset-Candidate"

:allocate
set "TemporaryDirectory=%TEMP%\windvale-segmented-compiler-package-test-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "Output=%TemporaryDirectory%\Compiler-Image-Staging.exe"
set "Result=1"

call "%RepositoryRoot%\Tools\Native\Package-Segmented-Compiler-Wvb.cmd" 6 ^
    "%Candidate%\Compiler-Image-Staging.wvb" "%Output%" >nul
if errorlevel 1 goto :cleanup
for %%F in ("%Output%") do if not "%%~zF"=="851968" goto :cleanup
certutil -hashfile "%Output%" SHA256 | findstr /I /C:"967827e4592c23f30e2a70b9a60a43837c1dfec6112584596c09d382058e2752" >nul
if errorlevel 1 goto :cleanup
fc /b "%Output%" "%Candidate%\windows-x64-wvlinkstage.exe" >nul
if errorlevel 1 goto :cleanup

echo PASS  segmented compiler packaging reproduces exact Windows application
echo Tests: 1, Passed: 1, Failed: 0
set "Result=0"

:cleanup
del /f /q "%TemporaryDirectory%\*" >nul 2>nul
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b %Result%
