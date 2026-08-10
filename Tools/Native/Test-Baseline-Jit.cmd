@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Baseline-Jit.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"

:allocate
set "TemporaryDirectory=%TEMP%\windvale-baseline-jit-suite-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "PlanOutput=%TemporaryDirectory%\Patch-Plan.out"
set "PublisherOutput=%TemporaryDirectory%\Publisher.out"

call "%RepositoryRoot%\Tools\Native\Test-Baseline-Jit-Patch-Plan.cmd" > "%PlanOutput%"
if errorlevel 1 goto :failed
set "PlanSummary="
for /f "usebackq delims=" %%L in ("%PlanOutput%") do set "PlanSummary=%%L"
if not "%PlanSummary%"=="native baseline jit patch plan status=Passed result=0 entry-offset=3808" goto :failed

call "%RepositoryRoot%\Tools\Native\Test-Baseline-Jit-Publisher.cmd" > "%PublisherOutput%"
if errorlevel 1 goto :failed
set "PublisherSummary="
for /f "usebackq delims=" %%L in ("%PublisherOutput%") do set "PublisherSummary=%%L"
if not "%PublisherSummary%"=="native baseline jit publisher status=Passed result=0 platform=windows-x64" goto :failed

type "%PlanOutput%"
type "%PublisherOutput%"
call :cleanup
echo Tests: 6, Passed: 6, Failed: 0
exit /b 0

:failed
if exist "%PlanOutput%" type "%PlanOutput%" >&2
if exist "%PublisherOutput%" type "%PublisherOutput%" >&2
call :cleanup
exit /b 1

:cleanup
if exist "%PlanOutput%" del /f /q "%PlanOutput%" >nul 2>nul
if exist "%PublisherOutput%" del /f /q "%PublisherOutput%" >nul 2>nul
if exist "%TemporaryDirectory%\." rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0
