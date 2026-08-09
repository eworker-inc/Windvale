@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "Filter="
if "%~1"=="" goto :arguments_ready
if not "%~1"=="--filter" goto :usage
if "%~2"=="" goto :usage
if not "%~3"=="" goto :usage
set "Filter=%~2"

:arguments_ready
set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Plan=%RepositoryRoot%\Tests\Native\Retirement-Suite.txt"
set "PlanDigest=521488bb63e001cccc673db3e41c6718b20313a11b28a9e9421d735c6b992f56"
certutil -hashfile "%Plan%" SHA256 | findstr /I /C:"%PlanDigest%" >nul
if errorlevel 1 (
    >&2 echo Native retirement suite plan identity differs
    exit /b 1
)

:allocate
set "TemporaryDirectory=%TEMP%\windvale-native-retirement-suite-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "SuiteOutput=%TemporaryDirectory%\Suite.out"
set "SuiteError=%TemporaryDirectory%\Suite.err"
set /a Selected=0
set /a TotalSuites=0
set /a PassedSuites=0
set /a TotalCases=0

for /f "usebackq skip=1 tokens=1-4 delims=|" %%A in ("%Plan%") do (
    call :consider_suite "%%A" "%%B" "%%C" "%%D"
    if errorlevel 1 goto :failed
)

if defined Filter if "%Selected%"=="0" (
    call :cleanup
    >&2 echo Unknown native retirement suite: %Filter%
    exit /b 64
)

call :cleanup
echo Suites: %TotalSuites%, Passed: %PassedSuites%, Failed: 0, Cases: %TotalCases%
exit /b 0

:consider_suite
if defined Filter if not "%Filter%"=="%~1" exit /b 0
set /a Selected+=1
set /a TotalSuites+=1
set /a TotalCases+=%~3
call "%RepositoryRoot%\Tools\Native\%~2.cmd" > "%SuiteOutput%" 2> "%SuiteError%"
set "SuiteExit=%ERRORLEVEL%"
if not "%SuiteExit%"=="0" (
    >&2 echo FAIL  suite %~1: native command exited %SuiteExit%
    if exist "%SuiteOutput%" type "%SuiteOutput%" >&2
    if exist "%SuiteError%" type "%SuiteError%" >&2
    exit /b 1
)
for %%S in ("%SuiteError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  suite %~1: native command wrote standard error
    type "%SuiteError%" >&2
    exit /b 1
)
set "ActualSummary="
for /f "usebackq delims=" %%L in ("%SuiteOutput%") do set "ActualSummary=%%L"
if not "%ActualSummary%"=="%~4" (
    >&2 echo FAIL  suite %~1: summary differs
    type "%SuiteOutput%" >&2
    exit /b 1
)
del /f /q "%SuiteOutput%" "%SuiteError%" >nul 2>nul
set /a PassedSuites+=1
echo PASS  suite %~1 cases=%~3
exit /b 0

:failed
set /a FailedSuites=TotalSuites-PassedSuites
call :cleanup
>&2 echo Suites: %TotalSuites%, Passed: %PassedSuites%, Failed: %FailedSuites%, Cases: %TotalCases%
exit /b 1

:cleanup
if exist "%SuiteOutput%" del /f /q "%SuiteOutput%" >nul 2>nul
if exist "%SuiteError%" del /f /q "%SuiteError%" >nul 2>nul
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0

:usage
>&2 echo Usage: Tools\Native\Test-Retirement-Suite.cmd [--filter ^<suite-name^>]
exit /b 64
