@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"

:allocate
set "OutputDirectory=%TEMP%\windvale-seed-native-front-door-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%OutputDirectory%" goto :allocate
mkdir "%OutputDirectory%" || exit /b 1

pwsh -NoProfile -File "%RepositoryRoot%\Tools\Verify\Verify-Seed-Native-Front-Door.ps1" -OutputDirectory "%OutputDirectory%"
set "Result=%ERRORLEVEL%"
rmdir /s /q "%OutputDirectory%" >nul 2>nul
if not "%Result%"=="0" exit /b %Result%

echo Tests: 1, Passed: 1, Failed: 0
exit /b 0
