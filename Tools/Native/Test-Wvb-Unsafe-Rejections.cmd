@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Wvb-Unsafe-Rejections.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "TemporaryDirectory=%TEMP%\windvale-wvb-unsafe-rejections-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1

set "Input=%TemporaryDirectory%\Input.wvb"
set "RunOutput=%TemporaryDirectory%\Run.out"
set "RunError=%TemporaryDirectory%\Run.err"
set "DecodeOutput=%TemporaryDirectory%\Decode.out"
set "DecodeError=%TemporaryDirectory%\Decode.err"
set /a Total=0
set /a Passed=0

call :run_case "unknown-opcode" "Tests\Native\Wvb-Unsafe-Rejections\Unknown-Opcode.wvb.b64" "f84528a577647a8d9c988f2cf082ea642dc7b8f61220bb5d23d57e8d3238c0aa" "4938c6906dfb318e36b11a2795699638f4cdef1bef12c477b5b726137cd3d9b5"
if errorlevel 1 goto :failed
call :run_case "truncated-operand" "Tests\Native\Wvb-Unsafe-Rejections\Truncated-Operand.wvb.b64" "eac2a31112958af23f89941be6e9591e870438439ea037e2b12a6c23216f74d9" "4938c6906dfb318e36b11a2795699638f4cdef1bef12c477b5b726137cd3d9b5"
if errorlevel 1 goto :failed
call :run_case "local-index" "Tests\Native\Wvb-Unsafe-Rejections\Local-Index.wvb.b64" "857f94ae40c95dd2f2e3f27ba07892c0ae351f1875fc16c91695e5a3872f56a3" "4938c6906dfb318e36b11a2795699638f4cdef1bef12c477b5b726137cd3d9b5"
if errorlevel 1 goto :failed
call :run_case "jump-target" "Tests\Native\Wvb-Unsafe-Rejections\Jump-Target.wvb.b64" "b56e962d4e4d24d6366354e1f4798c4352de236dcad421829d4b8714db3eb2a3" "4938c6906dfb318e36b11a2795699638f4cdef1bef12c477b5b726137cd3d9b5"
if errorlevel 1 goto :failed
call :run_case "after-return" "Tests\Native\Wvb-Unsafe-Rejections\After-Return.wvb.b64" "ece563bb06b953ef1587004c3517c21098702b644511cdda989e49d89d9061e7" "c083d8e4a7dbe48f3c72248285d6f4ace645202ca8f2013df1d51ab328db7930"
if errorlevel 1 goto :failed
call :run_case "record-parameter-type" "Tests\Native\Wvb-Unsafe-Rejections\Record-Parameter-Type.wvb.b64" "8e89cf9b526e1ea93d81d62425f95986daff4469dc7f113f5e38b580ccf163aa" "4938c6906dfb318e36b11a2795699638f4cdef1bef12c477b5b726137cd3d9b5"
if errorlevel 1 goto :failed
call :run_case "record-field-index" "Tests\Native\Wvb-Unsafe-Rejections\Record-Field-Index.wvb.b64" "1d5ed90586e2327af309cb9fe6ba1110da879ee461f7fd56d7c5414d1c637999" "c083d8e4a7dbe48f3c72248285d6f4ace645202ca8f2013df1d51ab328db7930"
if errorlevel 1 goto :failed
call :run_case "duplicate-record-field" "Tests\Native\Wvb-Unsafe-Rejections\Duplicate-Record-Field.wvb.b64" "73867dcf74f30f4b9237091aa59ea981200f4139636b67eb730bdb71752571b6" "4938c6906dfb318e36b11a2795699638f4cdef1bef12c477b5b726137cd3d9b5"
if errorlevel 1 goto :failed
call :run_case "mismatched-enum-comparison" "Tests\Native\Wvb-Unsafe-Rejections\Mismatched-Enum-Comparison.wvb.b64" "6ae2e65a43f68f0aa4b46b7ca306ad1dd06b72b1328e02e611f98e9f7abc869e" "c083d8e4a7dbe48f3c72248285d6f4ace645202ca8f2013df1d51ab328db7930"
if errorlevel 1 goto :failed
call :run_case "duplicate-nominal-name" "Tests\Native\Wvb-Unsafe-Rejections\Duplicate-Nominal-Name.wvb.b64" "60d12d56015678f3197a1413cfb058bff64188a8e2256d09f504280fad805f9c" "4938c6906dfb318e36b11a2795699638f4cdef1bef12c477b5b726137cd3d9b5"
if errorlevel 1 goto :failed

call :cleanup
echo Tests: %Total%, Passed: %Passed%, Failed: 0
exit /b 0

:run_case
set /a Total+=1
certutil -f -decode "%RepositoryRoot%\%~2" "%Input%" > "%DecodeOutput%" 2> "%DecodeError%"
if errorlevel 1 (
    >&2 echo FAIL  %~1: WVB fixture could not be decoded
    exit /b 1
)
for %%S in ("%DecodeError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  %~1: WVB decoder wrote a diagnostic
    type "%DecodeError%" >&2
    exit /b 1
)
certutil -hashfile "%Input%" SHA256 | findstr /I /C:"%~3" >nul
if errorlevel 1 (
    >&2 echo FAIL  %~1: WVB input identity differs
    exit /b 1
)
call :run_launcher "%~1" "Verify-Wvb.cmd" "%~3" "%~4"
if errorlevel 1 exit /b 1
call :run_launcher "%~1" "Inspect-Wvb.cmd" "%~3" "%~4"
if errorlevel 1 exit /b 1
del /f /q "%Input%" "%DecodeOutput%" "%DecodeError%" >nul 2>nul
set /a Passed+=1
echo PASS  %~1
exit /b 0

:run_launcher
call "%RepositoryRoot%\Tools\Native\%~2" "%Input%" > "%RunOutput%" 2> "%RunError%"
set "RunExit=%ERRORLEVEL%"
if not "%RunExit%"=="1" (
    >&2 echo FAIL  %~1: native WVB read-only exit differs
    exit /b 1
)
for %%S in ("%RunOutput%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  %~1: rejected WVB wrote standard output
    type "%RunOutput%" >&2
    exit /b 1
)
certutil -hashfile "%RunError%" SHA256 | findstr /I /C:"%~4" >nul
if errorlevel 1 (
    >&2 echo FAIL  %~1: native WVB report differs
    type "%RunError%" >&2
    exit /b 1
)
certutil -hashfile "%Input%" SHA256 | findstr /I /C:"%~3" >nul
if errorlevel 1 (
    >&2 echo FAIL  %~1: native WVB read-only command changed its input
    exit /b 1
)
del /f /q "%RunOutput%" "%RunError%" >nul 2>nul
exit /b 0

:failed
set /a Failed=Total-Passed
call :cleanup
>&2 echo Tests: %Total%, Passed: %Passed%, Failed: %Failed%
exit /b 1

:cleanup
for %%F in (Input.wvb Run.out Run.err Decode.out Decode.err) do (
    if exist "%TemporaryDirectory%\%%F" del /f /q "%TemporaryDirectory%\%%F" >nul 2>nul
)
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0
