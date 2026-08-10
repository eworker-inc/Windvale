@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Publisher-Rejections.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "TemporaryDirectory=%TEMP%\windvale-publisher-rejections-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1

set "Invalid=%TemporaryDirectory%\Invalid.bin"
set "Sentinel=%TemporaryDirectory%\Sentinel.bin"
set "RunOutput=%TemporaryDirectory%\Run.out"
set "RunError=%TemporaryDirectory%\Run.err"
set "DecodeOutput=%TemporaryDirectory%\Decode.out"
set "DecodeError=%TemporaryDirectory%\Decode.err"
set /a Total=0
set /a Passed=0

call :decode_fixture "Tests\Native\Wvo\Bad-Magic.wvo.b64" "%Invalid%" "0369f8b34765adb08799e6b852e9d1e249c40d1049976b01ff59355dd111f288" "invalid candidate"
if errorlevel 1 goto :failed
call :decode_fixture "Tests\Native\Wvo\Return-42.wvo.b64" "%Sentinel%" "0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5" "destination sentinel"
if errorlevel 1 goto :failed

call :run_case "console-application" "Publish-Console.cmd" "Candidate.exe" "Destination.exe" "39db034713225109f62c272db447d75cfe93ff0c259c8d9e5211f0df5c007e1f"
if errorlevel 1 goto :failed
call :run_case "hosted-verifier-application" "Publish-Hosted-Verifier-Application.cmd" "Candidate.exe" "Destination.exe" "d56759e7c74de5f7c15f2940b87f5d89cd7c5d9dff647854560cdd8cd1749c24"
if errorlevel 1 goto :failed
call :run_case "hosted-verifier-publisher" "Install-Hosted-Verifier-Publisher.cmd" "Candidate.exe" "Destination.exe" "22e5d25049052ee2a38f1775cc0c4ba1d5a5bbb95397c0b38a62ed310effe053"
if errorlevel 1 goto :failed
call :run_case "wvo" "Publish-Wvo.cmd" "Candidate.wvo" "Destination.wvo" "e7a127a800310d9fbaf8b511b20c7b8184159521dec1be56b641793939a5c69f"
if errorlevel 1 goto :failed

call :cleanup
echo Tests: %Total%, Passed: %Passed%, Failed: 0
exit /b 0

:decode_fixture
certutil -f -decode "%RepositoryRoot%\%~1" "%~2" > "%DecodeOutput%" 2> "%DecodeError%"
if errorlevel 1 (
    >&2 echo The native publisher %~4 fixture could not be decoded.
    exit /b 1
)
for %%S in ("%DecodeError%") do if not "%%~zS"=="0" (
    >&2 echo The native publisher %~4 decoder wrote a diagnostic.
    type "%DecodeError%" >&2
    exit /b 1
)
certutil -hashfile "%~2" SHA256 | findstr /I /C:"%~3" >nul
if errorlevel 1 (
    >&2 echo The native publisher %~4 identity differs.
    exit /b 1
)
exit /b 0

:run_case
set /a Total+=1
set "Candidate=%TemporaryDirectory%\%~3"
set "Destination=%TemporaryDirectory%\%~4"
copy /y "%Invalid%" "%Candidate%" >nul
copy /y "%Sentinel%" "%Destination%" >nul
if errorlevel 1 (
    >&2 echo FAIL  %~1: candidate or destination could not be created
    exit /b 1
)
call "%RepositoryRoot%\Tools\Native\%~2" "%Candidate%" "%Destination%" > "%RunOutput%" 2> "%RunError%"
set "RunExit=%ERRORLEVEL%"
if not "%RunExit%"=="1" (
    >&2 echo FAIL  %~1: native publisher exit differs
    exit /b 1
)
for %%S in ("%RunOutput%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  %~1: rejected publication wrote standard output
    type "%RunOutput%" >&2
    exit /b 1
)
certutil -hashfile "%RunError%" SHA256 | findstr /I /C:"%~5" >nul
if errorlevel 1 (
    >&2 echo FAIL  %~1: native publisher report differs
    type "%RunError%" >&2
    exit /b 1
)
certutil -hashfile "%Destination%" SHA256 | findstr /I /C:"0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5" >nul
if errorlevel 1 (
    >&2 echo FAIL  %~1: rejected publication changed the destination
    exit /b 1
)
certutil -hashfile "%Candidate%" SHA256 | findstr /I /C:"0369f8b34765adb08799e6b852e9d1e249c40d1049976b01ff59355dd111f288" >nul
if errorlevel 1 (
    >&2 echo FAIL  %~1: rejected publication changed the candidate
    exit /b 1
)
for /f "usebackq delims=" %%S in (`dir /b /a "%TemporaryDirectory%\.wvpublish-*" 2^>nul`) do (
    >&2 echo FAIL  %~1: rejected publication left scratch
    exit /b 1
)
del /f /q "%Candidate%" "%Destination%" "%RunOutput%" "%RunError%" >nul 2>nul
set /a Passed+=1
echo PASS  %~1
exit /b 0

:failed
set /a Failed=Total-Passed
call :cleanup
>&2 echo Tests: %Total%, Passed: %Passed%, Failed: %Failed%
exit /b 1

:cleanup
for %%F in (Invalid.bin Sentinel.bin Candidate.exe Destination.exe Candidate.wvo Destination.wvo Run.out Run.err Decode.out Decode.err) do (
    if exist "%TemporaryDirectory%\%%F" del /f /q "%TemporaryDirectory%\%%F" >nul 2>nul
)
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0
