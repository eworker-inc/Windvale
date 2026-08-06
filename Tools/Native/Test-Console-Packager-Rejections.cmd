@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Console-Packager-Rejections.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "TemporaryDirectory=%TEMP%\windvale-packager-rejections-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1

set "Image=%TemporaryDirectory%\Return-42.bin"
set "Empty=%TemporaryDirectory%\Empty.bin"
set "Sentinel=%TemporaryDirectory%\Sentinel.bin"
set "Output=%TemporaryDirectory%\Rejected.exe"
set "RunOutput=%TemporaryDirectory%\Run.out"
set "RunError=%TemporaryDirectory%\Run.err"
set "DecodeOutput=%TemporaryDirectory%\Decode.out"
set "DecodeError=%TemporaryDirectory%\Decode.err"
set /a Total=0
set /a Passed=0

call :decode_fixture "Tests\Native\Images\Return-42.bin.b64" "%Image%" "11db5348e275fb704be582e8005ee7d604f7f17b154d6cc644d240eef29d456a" "native image"
if errorlevel 1 goto :failed
call :decode_fixture "Tests\Native\Wvo\Bad-Magic.wvo.b64" "%Sentinel%" "0369f8b34765adb08799e6b852e9d1e249c40d1049976b01ff59355dd111f288" "output sentinel"
if errorlevel 1 goto :failed
type nul > "%Empty%"
for %%S in ("%Empty%") do if not "%%~zS"=="0" (
    >&2 echo The native console-packager empty image is not empty.
    goto :failed
)

call :run_case "entry-at-end" "a48244ecee195c2171cd3bdcf93261deed94b5d3522623f81557d146ec0f4071" "%Image%" "6"
if errorlevel 1 goto :failed
call :run_case "invalid-entry" "52264e728059fe229b20c14ad9e1febecc97da454ed2de58f34b85fdd99d4349" "%Image%" "invalid"
if errorlevel 1 goto :failed
call :run_case "empty-image" "52264e728059fe229b20c14ad9e1febecc97da454ed2de58f34b85fdd99d4349" "%Empty%" "0"
if errorlevel 1 goto :failed

call :cleanup
echo Tests: %Total%, Passed: %Passed%, Failed: 0
exit /b 0

:decode_fixture
certutil -f -decode "%RepositoryRoot%\%~1" "%~2" > "%DecodeOutput%" 2> "%DecodeError%"
if errorlevel 1 (
    >&2 echo The native console-packager %~4 fixture could not be decoded.
    exit /b 1
)
for %%S in ("%DecodeError%") do if not "%%~zS"=="0" (
    >&2 echo The native console-packager %~4 decoder wrote a diagnostic.
    type "%DecodeError%" >&2
    exit /b 1
)
certutil -hashfile "%~2" SHA256 | findstr /I /C:"%~3" >nul
if errorlevel 1 (
    >&2 echo The native console-packager %~4 identity differs.
    exit /b 1
)
exit /b 0

:run_case
set /a Total+=1
copy /y "%Sentinel%" "%Output%" >nul
if errorlevel 1 (
    >&2 echo FAIL  %~1: output sentinel could not be created
    exit /b 1
)
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" "windows-x64-console-v1" "%~3" "%~4" "%Output%" > "%RunOutput%" 2> "%RunError%"
set "RunExit=%ERRORLEVEL%"
if not "%RunExit%"=="2" (
    >&2 echo FAIL  %~1: native console-packager exit differs
    exit /b 1
)
for %%S in ("%RunOutput%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  %~1: rejected package wrote standard output
    type "%RunOutput%" >&2
    exit /b 1
)
certutil -hashfile "%RunError%" SHA256 | findstr /I /C:"%~2" >nul
if errorlevel 1 (
    >&2 echo FAIL  %~1: native console-packager report differs
    type "%RunError%" >&2
    exit /b 1
)
certutil -hashfile "%Output%" SHA256 | findstr /I /C:"0369f8b34765adb08799e6b852e9d1e249c40d1049976b01ff59355dd111f288" >nul
if errorlevel 1 (
    >&2 echo FAIL  %~1: rejected package changed the output
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
for %%F in (Return-42.bin Empty.bin Sentinel.bin Rejected.exe Run.out Run.err Decode.out Decode.err) do (
    if exist "%TemporaryDirectory%\%%F" del /f /q "%TemporaryDirectory%\%%F" >nul 2>nul
)
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0
