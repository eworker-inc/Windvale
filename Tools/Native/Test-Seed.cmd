@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Plan=%RepositoryRoot%\Tests\Native\Plan.txt"

certutil -hashfile "%Plan%" SHA256 | findstr /I /C:"9e36b8a6a1c272ee6335dee5fdd6c42aaa3d742437e8fd57af979c56e6943cff" >nul
if errorlevel 1 (
    >&2 echo The native test plan artifact digest is invalid.
    exit /b 1
)
:allocate
set "TemporaryDirectory=%TEMP%\windvale-native-tests-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1

set /a Total=0
set /a Passed=0
for /f "usebackq skip=1 tokens=1-6 delims=|" %%A in ("%Plan%") do (
    call :run_case "%%A" "%%B" "%%C" "%%D" "%%E" "%%F"
    if errorlevel 1 goto :failed
)

call :cleanup
echo Tests: %Total%, Passed: %Passed%, Failed: 0
exit /b 0

:run_case
set /a Total+=1
set "Name=%~1"
set "InputKind=%~2"
set "Input=%~3"
set "ExpectedHash=%~4"
set "ExpectedKind=%~5"
set "ExpectedValue=%~6"
set "Output=%TemporaryDirectory%\Current.wvb"
set "BuildOutput=%TemporaryDirectory%\Build.out"
set "BuildError=%TemporaryDirectory%\Build.err"
set "DecodeOutput=%TemporaryDirectory%\Decode.out"
set "DecodeError=%TemporaryDirectory%\Decode.err"
set "RunOutput=%TemporaryDirectory%\Run.out"
set "RunError=%TemporaryDirectory%\Run.err"

if "%InputKind%"=="project" goto :build_project
if "%InputKind%"=="fixture-base64" goto :decode_fixture
if "%InputKind%"=="wvo-fixture-base64" goto :decode_wvo_fixture
>&2 echo FAIL  %Name%: test input kind is invalid
exit /b 1

:build_project
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\%Input%" "%Output%" > "%BuildOutput%" 2> "%BuildError%"
if errorlevel 1 (
    >&2 echo FAIL  %Name%: native build failed
    type "%BuildError%" >&2
    exit /b 1
)
for %%S in ("%BuildError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  %Name%: native build wrote a diagnostic
    type "%BuildError%" >&2
    exit /b 1
)
goto :input_ready

:decode_fixture
set "Output=%TemporaryDirectory%\Current.wvb"
goto :decode_base64

:decode_wvo_fixture
set "Output=%TemporaryDirectory%\Current.wvo"

:decode_base64
certutil -f -decode "%RepositoryRoot%\%Input%" "%Output%" > "%DecodeOutput%" 2> "%DecodeError%"
if errorlevel 1 (
    >&2 echo FAIL  %Name%: malformed fixture decoding failed
    type "%DecodeError%" >&2
    exit /b 1
)
for %%S in ("%DecodeError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  %Name%: malformed fixture decoding wrote a diagnostic
    type "%DecodeError%" >&2
    exit /b 1
)

:input_ready
certutil -hashfile "%Output%" SHA256 | findstr /I /C:"%ExpectedHash%" >nul
if errorlevel 1 (
    >&2 echo FAIL  %Name%: input identity differs
    exit /b 1
)

if "%ExpectedKind%"=="wvo-valid" goto :wvo_case
if "%ExpectedKind%"=="wvo-invalid" goto :wvo_case
if "%ExpectedKind%"=="verify-failure" goto :verify_case
call "%RepositoryRoot%\Tools\Native\Run-Wvb.cmd" "%Output%" > "%RunOutput%" 2> "%RunError%"
set "RunExit=%ERRORLEVEL%"
if "%ExpectedKind%"=="result" (
    call :check_result
    if errorlevel 1 exit /b 1
    goto :case_passed
)
if "%ExpectedKind%"=="failure" (
    call :check_failure
    if errorlevel 1 exit /b 1
    goto :case_passed
)
>&2 echo FAIL  %Name%: test expectation kind is invalid
exit /b 1

:verify_case
call "%RepositoryRoot%\Tools\Native\Verify-Wvb.cmd" "%Output%" > "%RunOutput%" 2> "%RunError%"
set "RunExit=%ERRORLEVEL%"
call :check_verify_failure
if errorlevel 1 exit /b 1
goto :case_passed

:wvo_case
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%Output%" > "%RunOutput%" 2> "%RunError%"
set "RunExit=%ERRORLEVEL%"
if "%ExpectedKind%"=="wvo-valid" (
    call :check_wvo_valid
    if errorlevel 1 exit /b 1
    goto :case_passed
)
call :check_wvo_invalid
if errorlevel 1 exit /b 1
goto :case_passed

:check_result
if not "%RunExit%"=="0" (
    >&2 echo FAIL  %Name%: native execution failed
    type "%RunError%" >&2
    exit /b 1
)
for %%S in ("%RunError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  %Name%: successful execution wrote a diagnostic
    type "%RunError%" >&2
    exit /b 1
)
call :read_report "%RunOutput%"
if not "%ReportLines%"=="1" (
    >&2 echo FAIL  %Name%: result report has extra lines
    exit /b 1
)
if not "%ActualReport%"=="Result: %ExpectedValue%" (
    >&2 echo FAIL  %Name%: result differs
    exit /b 1
)
exit /b 0

:check_failure
if not "%RunExit%"=="1" (
    >&2 echo FAIL  %Name%: native failure exit differs
    exit /b 1
)
for %%S in ("%RunOutput%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  %Name%: failed execution wrote standard output
    type "%RunOutput%" >&2
    exit /b 1
)
set "ExpectedCode="
set "ExpectedInstructions="
for /f "tokens=1-2 delims=:" %%S in ("%ExpectedValue%") do (
    set "ExpectedCode=%%S"
    set "ExpectedInstructions=%%T"
)
if not defined ExpectedInstructions (
    >&2 echo FAIL  %Name%: failure expectation is invalid
    exit /b 1
)
call :read_report "%RunError%"
if not "%ReportLines%"=="1" (
    >&2 echo FAIL  %Name%: failure report has extra lines
    exit /b 1
)
if not "%ActualReport%"=="wvb run status=Failed code=%ExpectedCode% instructions=%ExpectedInstructions%" (
    >&2 echo FAIL  %Name%: failure report differs
    type "%RunError%" >&2
    exit /b 1
)
exit /b 0

:check_verify_failure
if not "%RunExit%"=="1" (
    >&2 echo FAIL  %Name%: native verification exit differs
    exit /b 1
)
for %%S in ("%RunOutput%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  %Name%: rejected verification wrote standard output
    type "%RunOutput%" >&2
    exit /b 1
)
call :read_report "%RunError%"
if not "%ReportLines%"=="1" (
    >&2 echo FAIL  %Name%: verification report has extra lines
    exit /b 1
)
if not "%ActualReport%"=="wvb status=Invalid phase=%ExpectedValue%" (
    >&2 echo FAIL  %Name%: verification report differs
    type "%RunError%" >&2
    exit /b 1
)
exit /b 0

:check_wvo_valid
if not "%RunExit%"=="0" (
    >&2 echo FAIL  %Name%: native WVO verification failed
    type "%RunError%" >&2
    exit /b 1
)
for %%S in ("%RunError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  %Name%: valid WVO wrote a diagnostic
    type "%RunError%" >&2
    exit /b 1
)
certutil -hashfile "%RunOutput%" SHA256 | findstr /I /C:"%ExpectedValue%" >nul
if errorlevel 1 (
    >&2 echo FAIL  %Name%: valid WVO report differs
    exit /b 1
)
exit /b 0

:check_wvo_invalid
if not "%RunExit%"=="2" (
    >&2 echo FAIL  %Name%: invalid WVO exit differs
    exit /b 1
)
for %%S in ("%RunOutput%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  %Name%: invalid WVO wrote standard output
    type "%RunOutput%" >&2
    exit /b 1
)
certutil -hashfile "%RunError%" SHA256 | findstr /I /C:"%ExpectedValue%" >nul
if errorlevel 1 (
    >&2 echo FAIL  %Name%: invalid WVO report differs
    type "%RunError%" >&2
    exit /b 1
)
exit /b 0

:case_passed
set /a Passed+=1
echo PASS  %Name%
exit /b 0

:read_report
set "ActualReport="
set /a ReportLines=0
for /f "usebackq delims=" %%L in ("%~1") do call :capture_report "%%L"
exit /b 0

:capture_report
set /a ReportLines+=1
set "ActualReport=%~1"
exit /b 0

:failed
set /a Failed=Total-Passed
call :cleanup
>&2 echo Tests: %Total%, Passed: %Passed%, Failed: %Failed%
exit /b 1

:cleanup
for %%F in (Current.wvb Current.wvo Build.out Build.err Decode.out Decode.err Run.out Run.err) do (
    if exist "%TemporaryDirectory%\%%F" del /f /q "%TemporaryDirectory%\%%F" >nul 2>nul
)
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0
