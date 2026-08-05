@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Plan=%RepositoryRoot%\Tests\Native\Plan.txt"

certutil -hashfile "%Plan%" SHA256 | findstr /I /C:"d04f77c41bbae2c98541b3a0e6dec0ee0c725106dae72e5bb128d52c4abf3fc5" >nul
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
for /f "usebackq skip=1 tokens=1-4 delims=|" %%A in ("%Plan%") do (
    call :run_case "%%A" "%%B" "%%C" "%%D"
    if errorlevel 1 goto :failed
)

call :cleanup
echo Tests: %Total%, Passed: %Passed%, Failed: 0
exit /b 0

:run_case
set /a Total+=1
set "Name=%~1"
set "Project=%~2"
set "ExpectedHash=%~3"
set "ExpectedResult=%~4"
set "Output=%TemporaryDirectory%\Current.wvb"
set "BuildOutput=%TemporaryDirectory%\Build.out"
set "BuildError=%TemporaryDirectory%\Build.err"
set "RunOutput=%TemporaryDirectory%\Run.out"
set "RunError=%TemporaryDirectory%\Run.err"

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\%Project%" "%Output%" > "%BuildOutput%" 2> "%BuildError%"
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
certutil -hashfile "%Output%" SHA256 | findstr /I /C:"%ExpectedHash%" >nul
if errorlevel 1 (
    >&2 echo FAIL  %Name%: WVB identity differs
    exit /b 1
)

call "%RepositoryRoot%\Tools\Native\Run-Wvb.cmd" "%Output%" > "%RunOutput%" 2> "%RunError%"
if errorlevel 1 (
    >&2 echo FAIL  %Name%: native execution failed
    type "%RunError%" >&2
    exit /b 1
)
for %%S in ("%RunError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  %Name%: native execution wrote a diagnostic
    type "%RunError%" >&2
    exit /b 1
)
set "ActualResult="
set /a OutputLines=0
for /f "usebackq delims=" %%L in ("%RunOutput%") do call :read_output "%%L"
if not "%OutputLines%"=="1" (
    >&2 echo FAIL  %Name%: result report has extra lines
    exit /b 1
)
if not "%ActualResult%"=="Result: %ExpectedResult%" (
    >&2 echo FAIL  %Name%: result differs
    exit /b 1
)

set /a Passed+=1
echo PASS  %Name%
exit /b 0

:read_output
set /a OutputLines+=1
set "ActualResult=%~1"
exit /b 0

:failed
set /a Failed=Total-Passed
call :cleanup
>&2 echo Tests: %Total%, Passed: %Passed%, Failed: %Failed%
exit /b 1

:cleanup
for %%F in (Current.wvb Build.out Build.err Run.out Run.err) do (
    if exist "%TemporaryDirectory%\%%F" del /f /q "%TemporaryDirectory%\%%F" >nul 2>nul
)
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0
