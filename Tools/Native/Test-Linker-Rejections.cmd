@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Linker-Rejections.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "TemporaryDirectory=%TEMP%\windvale-linker-rejections-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1

set "Valid=%TemporaryDirectory%\Return-42.wvo"
set "Invalid=%TemporaryDirectory%\Bad-Magic.wvo"
set "Output=%TemporaryDirectory%\Output.bin"
set "RunOutput=%TemporaryDirectory%\Run.out"
set "RunError=%TemporaryDirectory%\Run.err"
set "DecodeOutput=%TemporaryDirectory%\Decode.out"
set "DecodeError=%TemporaryDirectory%\Decode.err"
set /a Total=0
set /a Passed=0

call :decode_fixture "Tests\Native\Wvo\Return-42.wvo.b64" "%Valid%" "0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5" "valid WVO"
if errorlevel 1 goto :failed
call :decode_fixture "Tests\Native\Wvo\Bad-Magic.wvo.b64" "%Invalid%" "0369f8b34765adb08799e6b852e9d1e249c40d1049976b01ff59355dd111f288" "malformed WVO"
if errorlevel 1 goto :failed

call :run_case "invalid-base" "b5a687af92c9eca7eb5ba850bddf6dec932c94a6be304af35357655a915056b8" "invalid" "Main" "%Valid%"
if errorlevel 1 goto :failed
call :run_case "missing-entry" "883ad60b71d4c010d4a2ddf168199dfaae04d1e076313ee1cf4dac8bee67a517" "1048576" "Missing" "%Valid%"
if errorlevel 1 goto :failed
call :run_case "malformed-object" "18eeeeb5d84e82c54cf14480bc5c54e593f5cd429d68686ad64110d9780a5353" "1048576" "Main" "%Invalid%"
if errorlevel 1 goto :failed

call :cleanup
echo Tests: %Total%, Passed: %Passed%, Failed: 0
exit /b 0

:decode_fixture
certutil -f -decode "%RepositoryRoot%\%~1" "%~2" > "%DecodeOutput%" 2> "%DecodeError%"
if errorlevel 1 (
    >&2 echo The native linker %~4 fixture could not be decoded.
    exit /b 1
)
for %%S in ("%DecodeError%") do if not "%%~zS"=="0" (
    >&2 echo The native linker %~4 fixture decoder wrote a diagnostic.
    type "%DecodeError%" >&2
    exit /b 1
)
certutil -hashfile "%~2" SHA256 | findstr /I /C:"%~3" >nul
if errorlevel 1 (
    >&2 echo The native linker %~4 fixture identity differs.
    exit /b 1
)
exit /b 0

:run_case
set /a Total+=1
copy /y "%Invalid%" "%Output%" >nul
if errorlevel 1 (
    >&2 echo FAIL  %~1: output sentinel could not be created
    exit /b 1
)
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" "%~3" "%~4" "%Output%" "%~5" > "%RunOutput%" 2> "%RunError%"
set "RunExit=%ERRORLEVEL%"
if not "%RunExit%"=="2" (
    >&2 echo FAIL  %~1: native linker exit differs
    exit /b 1
)
for %%S in ("%RunOutput%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  %~1: rejected link wrote standard output
    type "%RunOutput%" >&2
    exit /b 1
)
certutil -hashfile "%RunError%" SHA256 | findstr /I /C:"%~2" >nul
if errorlevel 1 (
    >&2 echo FAIL  %~1: native linker report differs
    type "%RunError%" >&2
    exit /b 1
)
certutil -hashfile "%Output%" SHA256 | findstr /I /C:"0369f8b34765adb08799e6b852e9d1e249c40d1049976b01ff59355dd111f288" >nul
if errorlevel 1 (
    >&2 echo FAIL  %~1: rejected link changed the output
    exit /b 1
)
del /f /q "%Output%" "%RunOutput%" "%RunError%" >nul 2>nul
set /a Passed+=1
echo PASS  %~1
exit /b 0

:failed
set /a Failed=Total-Passed
call :cleanup
>&2 echo Tests: %Total%, Passed: %Passed%, Failed: %Failed%
exit /b 1

:cleanup
for %%F in (Return-42.wvo Bad-Magic.wvo Output.bin Run.out Run.err Decode.out Decode.err) do (
    if exist "%TemporaryDirectory%\%%F" del /f /q "%TemporaryDirectory%\%%F" >nul 2>nul
)
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0
