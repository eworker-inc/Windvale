@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "Filter="
set "Shard="
if "%~1"=="" goto :arguments_ready
if not "%~3"=="" goto :usage
if "%~2"=="" goto :usage
if /I "%~1"=="--filter" goto :set_filter
if /I "%~1"=="--shard" goto :set_shard
goto :usage

:set_filter
set "Filter=%~2"
goto :arguments_ready

:set_shard
set "Shard=%~2"
if "%Shard%"=="1" goto :arguments_ready
if "%Shard%"=="2" goto :arguments_ready
if "%Shard%"=="3" goto :arguments_ready
if "%Shard%"=="4" goto :arguments_ready
goto :usage

:arguments_ready
set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Plan=%RepositoryRoot%\Tests\Native\Verification-Owners.txt"
set "PlanDigest=e7a8f82c2b38eb6ec2f6deabff631df3b9a54f4ac635db96124277b6f19a333f"
certutil -hashfile "%Plan%" SHA256 | findstr /I /C:"%PlanDigest%" >nul
if errorlevel 1 (
    >&2 echo Native verification owner plan identity differs
    exit /b 1
)
set "PlanHeader="
for /f "usebackq delims=" %%H in ("%Plan%") do if not defined PlanHeader set "PlanHeader=%%H"
if not "%PlanHeader%"=="windvale-native-verification-owners 1" (
    >&2 echo Native verification owner plan header differs
    exit /b 1
)

for /f "usebackq skip=1 tokens=1,2,4 delims=|" %%N in ("%Plan%") do (
    call :verify_plan_entry "%%N" "%%O" "%%P"
    if errorlevel 1 exit /b 1
)

:allocate
set "TemporaryDirectory=%TEMP%\windvale-native-verification-owners-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "SuiteOutput=%TemporaryDirectory%\Suite.out"
set "SuiteError=%TemporaryDirectory%\Suite.err"
set /a Selected=0
set /a Planned=0
set /a TotalSuites=0
set /a PassedSuites=0
set /a TotalCases=0
call :read_clock TotalStart

for /f "usebackq skip=1 tokens=1,4 delims=|" %%A in ("%Plan%") do call :count_selected "%%A" "%%B"

for /f "usebackq skip=1 tokens=1-5 delims=|" %%A in ("%Plan%") do (
    call :consider_suite "%%A" "%%B" "%%C" "%%D" "%%E"
    if errorlevel 1 goto :failed
)

if "%Selected%"=="0" (
    call :cleanup
    if defined Filter >&2 echo Unknown native verification owner: %Filter%
    if defined Shard >&2 echo Empty native qualification shard: %Shard%
    exit /b 64
)

call :read_clock TotalEnd
set /a TotalElapsed=TotalEnd-TotalStart
if %TotalElapsed% LSS 0 set /a TotalElapsed+=8640000
set /a TotalElapsedMs=TotalElapsed*10
call :cleanup
echo Timing: elapsed-ms=%TotalElapsedMs%
echo Suites: %TotalSuites%, Passed: %PassedSuites%, Failed: 0, Cases: %TotalCases%
exit /b 0

:consider_suite
if defined Filter if not "%Filter%"=="%~1" exit /b 0
if defined Shard if not "%Shard%"=="%~4" exit /b 0
set /a Selected+=1
set /a TotalSuites+=1
set /a TotalCases+=%~3
echo Progress: step=native-owner item=%Selected%/%Planned% owner=%~1
call :read_clock SuiteStart
call "%RepositoryRoot%\Tools\Native\%~2.cmd" > "%SuiteOutput%" 2> "%SuiteError%"
set "SuiteExit=%ERRORLEVEL%"
call :read_clock SuiteEnd
set /a SuiteElapsed=SuiteEnd-SuiteStart
if %SuiteElapsed% LSS 0 set /a SuiteElapsed+=8640000
set /a SuiteElapsedMs=SuiteElapsed*10
if not "%SuiteExit%"=="0" (
    >&2 echo FAIL  suite %~1: native command exited %SuiteExit% elapsed-ms=%SuiteElapsedMs%
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
if not "%ActualSummary%"=="%~5" (
    >&2 echo FAIL  suite %~1: summary differs
    type "%SuiteOutput%" >&2
    exit /b 1
)
del /f /q "%SuiteOutput%" "%SuiteError%" >nul 2>nul
set /a PassedSuites+=1
echo PASS  suite %~1 cases=%~3 elapsed-ms=%SuiteElapsedMs%
exit /b 0

:failed
set /a FailedSuites=TotalSuites-PassedSuites
call :read_clock TotalEnd
set /a TotalElapsed=TotalEnd-TotalStart
if %TotalElapsed% LSS 0 set /a TotalElapsed+=8640000
set /a TotalElapsedMs=TotalElapsed*10
call :cleanup
>&2 echo Timing: elapsed-ms=%TotalElapsedMs%
>&2 echo Suites: %TotalSuites%, Passed: %PassedSuites%, Failed: %FailedSuites%, Cases: %TotalCases%
exit /b 1

:read_clock
setlocal EnableExtensions DisableDelayedExpansion
set "Clock=%TIME: =0%"
set /a ClockHours=1%Clock:~0,2%-100
set /a ClockMinutes=1%Clock:~3,2%-100
set /a ClockSeconds=1%Clock:~6,2%-100
set /a ClockCentiseconds=1%Clock:~9,2%-100
set /a ClockTicks=ClockHours*360000+ClockMinutes*6000+ClockSeconds*100+ClockCentiseconds
endlocal & set "%~1=%ClockTicks%"
exit /b 0

:count_selected
if defined Filter if not "%Filter%"=="%~1" exit /b 0
if defined Shard if not "%Shard%"=="%~2" exit /b 0
set /a Planned+=1
exit /b 0

:verify_plan_entry
if not exist "%RepositoryRoot%\Tools\Native\%~2.cmd" (
    >&2 echo Native verification owner is missing: %RepositoryRoot%\Tools\Native\%~2.cmd
    exit /b 1
)
if "%~3"=="1" exit /b 0
if "%~3"=="2" exit /b 0
if "%~3"=="3" exit /b 0
if "%~3"=="4" exit /b 0
>&2 echo Native qualification shard is invalid: %~1=%~3
exit /b 1

:cleanup
if exist "%SuiteOutput%" del /f /q "%SuiteOutput%" >nul 2>nul
if exist "%SuiteError%" del /f /q "%SuiteError%" >nul 2>nul
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0

:usage
>&2 echo Usage: Tools\Native\Test-Verification-Owners.cmd [--filter ^<owner-name^>^|--shard ^<1-4^>]
exit /b 64
