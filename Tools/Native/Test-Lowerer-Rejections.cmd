@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Lowerer-Rejections.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "TemporaryDirectory=%TEMP%\windvale-lowerer-rejections-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "WorkDirectory=%TemporaryDirectory%\Work"
mkdir "%WorkDirectory%" || goto :failed

set "Invalid=%TemporaryDirectory%\Bad-Magic.wvb"
set "Unsupported=%TemporaryDirectory%\Unsupported-Function.wvb"
set "Sentinel=%TemporaryDirectory%\Sentinel.wvo"
set "Destination=%TemporaryDirectory%\Destination.wvo"
set "RunOutput=%TemporaryDirectory%\Run.out"
set "RunError=%TemporaryDirectory%\Run.err"
set "DecodeOutput=%TemporaryDirectory%\Decode.out"
set "DecodeError=%TemporaryDirectory%\Decode.err"
set /a Total=0
set /a Passed=0

call :decode_fixture "Tests\Native\Malformed-Wvb\Bad-Magic.wvb.b64" "%Invalid%" "20618498d9df059d52fc0d660bf52f32df291c88b94d4b5ded224078f936108e" "bad-magic input"
if errorlevel 1 goto :failed
copy /y "%RepositoryRoot%\Artifacts\Decimal-Parsing.wvb" "%Unsupported%" >nul
if errorlevel 1 (
    >&2 echo The native lowerer unsupported-function fixture could not be copied.
    goto :failed
)
certutil -hashfile "%Unsupported%" SHA256 | findstr /I /C:"bb120d1098855b8b4adced6bcd1b1ab695f115e76bebdacb19a2b07b798cad37" >nul
if errorlevel 1 (
    >&2 echo The native lowerer unsupported-function fixture identity differs.
    goto :failed
)
call :decode_fixture "Tests\Native\Wvo\Return-42.wvo.b64" "%Sentinel%" "0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5" "destination sentinel"
if errorlevel 1 goto :failed

call :run_case "malformed" "%Invalid%" "6dc739ce9e8c752efe41fbede32d6c373ea33e1c22159faf86772a4cc94ff323"
if errorlevel 1 goto :failed
call :run_case "unsupported-function" "%Unsupported%" "fc854d5370fe6da10243d8e28663f932baa4d7c30402488f5193d0a3dad77ded"
if errorlevel 1 goto :failed

call :cleanup
echo Tests: %Total%, Passed: %Passed%, Failed: 0
exit /b 0

:decode_fixture
certutil -f -decode "%RepositoryRoot%\%~1" "%~2" > "%DecodeOutput%" 2> "%DecodeError%"
if errorlevel 1 (
    >&2 echo The native lowerer %~4 fixture could not be decoded.
    exit /b 1
)
for %%S in ("%DecodeError%") do if not "%%~zS"=="0" (
    >&2 echo The native lowerer %~4 decoder wrote a diagnostic.
    type "%DecodeError%" >&2
    exit /b 1
)
certutil -hashfile "%~2" SHA256 | findstr /I /C:"%~3" >nul
if errorlevel 1 (
    >&2 echo The native lowerer %~4 identity differs.
    exit /b 1
)
exit /b 0

:run_case
set /a Total+=1
copy /y "%Sentinel%" "%Destination%" >nul
if errorlevel 1 (
    >&2 echo FAIL  %~1: destination could not be created
    exit /b 1
)
set "TEMP=%WorkDirectory%"
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%~2" "%Destination%" > "%RunOutput%" 2> "%RunError%"
set "RunExit=%ERRORLEVEL%"
if not "%RunExit%"=="1" (
    >&2 echo FAIL  %~1: native lowerer exit differs
    exit /b 1
)
for %%S in ("%RunOutput%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  %~1: rejected lowering wrote standard output
    type "%RunOutput%" >&2
    exit /b 1
)
certutil -hashfile "%RunError%" SHA256 | findstr /I /C:"%~3" >nul
if errorlevel 1 (
    >&2 echo FAIL  %~1: native lowerer report differs
    type "%RunError%" >&2
    exit /b 1
)
certutil -hashfile "%Destination%" SHA256 | findstr /I /C:"0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5" >nul
if errorlevel 1 (
    >&2 echo FAIL  %~1: rejected lowering changed the destination
    exit /b 1
)
for /f "usebackq delims=" %%S in (`dir /b /a "%WorkDirectory%" 2^>nul`) do (
    >&2 echo FAIL  %~1: rejected lowering left private work
    exit /b 1
)
del /f /q "%Destination%" "%RunOutput%" "%RunError%" >nul 2>nul
set /a Passed+=1
echo PASS  %~1
exit /b 0

:failed
set /a Failed=Total-Passed
call :cleanup
>&2 echo Tests: %Total%, Passed: %Passed%, Failed: %Failed%
exit /b 1

:cleanup
for %%F in (Bad-Magic.wvb Unsupported-Function.wvb Sentinel.wvo Destination.wvo Run.out Run.err Decode.out Decode.err) do (
    if exist "%TemporaryDirectory%\%%F" del /f /q "%TemporaryDirectory%\%%F" >nul 2>nul
)
rmdir "%WorkDirectory%" >nul 2>nul
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0
