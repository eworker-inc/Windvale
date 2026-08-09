@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"

:allocate
set "TemporaryDirectory=%TEMP%\windvale-assembler-golden-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "First=%TemporaryDirectory%\First.wvo"
set "Second=%TemporaryDirectory%\Second.wvo"
set "CommandOutput=%TemporaryDirectory%\Command.out"
set "CommandError=%TemporaryDirectory%\Command.err"
set "VerifyOutput=%TemporaryDirectory%\Verify.out"
set "VerifyError=%TemporaryDirectory%\Verify.err"
set /a Total=0
set /a Passed=0

call :run_case "hello-object" "Examples\Assembler\Hello-Object.wva" "a88f748ba87df1a291752ee8bda896279edd8d9f8a7811692c2229bbaba8cea0" 218 "992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85" "assembly status=valid object-bytes=218 sections=2 symbols=3 relocations=2 offset=403 line=22 column=1"
if errorlevel 1 goto :failed
call :run_case "expanded-x64" "Examples\Assembler\Expanded-X64.wva" "27a324b5c26c1e6a982c6f02b0a157ccfdcbb7500521dd8c95a381aa2ed20646" 238 "678551e9936ca1c901e2dc5ec129d2add73427edb1ea3d086bb4badbf1b6e4ad" "assembly status=valid object-bytes=238 sections=2 symbols=2 relocations=1 offset=740 line=35 column=1"
if errorlevel 1 goto :failed
call :run_case "scalar-x64" "Examples\Assembler\Scalar-X64.wva" "e76cb94b82857e097e734f6bdf01b3383487fd8a69f05214d74a1b69e261ae0e" 199 "e1cce07329b6183ebae26ebe252be7d2e754c4aeea08ffe6452c74d60d6ea64a" "assembly status=valid object-bytes=199 sections=1 symbols=1 relocations=0 offset=639 line=29 column=1"
if errorlevel 1 goto :failed
call :run_case "typed-scalar-x64" "Examples\Assembler\Typed-Scalar-X64.wva" "a66a36a06ac6375da7ed5287fe6fdae55901f5b8b236c3098723e7a6f856a4ef" 396 "860680074517025c69a2a6edf1dd9ff196475e05f9c50f95b53480c848c650c5" "assembly status=valid object-bytes=396 sections=2 symbols=2 relocations=5 offset=942 line=52 column=1"
if errorlevel 1 goto :failed

call :cleanup
echo Tests: %Total%, Passed: %Passed%, Failed: 0
exit /b 0

:run_case
set /a Total+=1
set "Name=%~1"
set "Input=%RepositoryRoot%\%~2"
set "InputDigest=%~3"
set "OutputBytes=%~4"
set "OutputDigest=%~5"
set "ExpectedReport=%~6"

certutil -hashfile "%Input%" SHA256 | findstr /I /C:"%InputDigest%" >nul
if errorlevel 1 (
    >&2 echo FAIL  %Name%: WVA input identity differs
    exit /b 1
)

call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" "%Input%" "%First%" > "%CommandOutput%" 2> "%CommandError%"
if errorlevel 1 (
    >&2 echo FAIL  %Name%: first native assembly failed
    type "%CommandError%" >&2
    exit /b 1
)
call :check_command_report
if errorlevel 1 exit /b 1
call :check_object "%First%"
if errorlevel 1 exit /b 1

call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%First%" > "%VerifyOutput%" 2> "%VerifyError%"
if errorlevel 1 (
    >&2 echo FAIL  %Name%: native WVO verification failed
    type "%VerifyError%" >&2
    exit /b 1
)
for %%S in ("%VerifyError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  %Name%: valid WVO wrote a diagnostic
    type "%VerifyError%" >&2
    exit /b 1
)

call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" "%Input%" "%Second%" > "%CommandOutput%" 2> "%CommandError%"
if errorlevel 1 (
    >&2 echo FAIL  %Name%: repeated native assembly failed
    type "%CommandError%" >&2
    exit /b 1
)
call :check_command_report
if errorlevel 1 exit /b 1
call :check_object "%Second%"
if errorlevel 1 exit /b 1
fc /b "%First%" "%Second%" >nul
if errorlevel 1 (
    >&2 echo FAIL  %Name%: repeated native object differs
    exit /b 1
)

del /f /q "%First%" "%Second%" "%CommandOutput%" "%CommandError%" "%VerifyOutput%" "%VerifyError%" >nul 2>nul
set /a Passed+=1
echo PASS  %Name%
exit /b 0

:check_command_report
for %%S in ("%CommandError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  %Name%: successful assembly wrote a diagnostic
    type "%CommandError%" >&2
    exit /b 1
)
set "ReportLine1="
set "ReportLine2="
set /a ReportLines=0
for /f "usebackq delims=" %%L in ("%CommandOutput%") do call :capture_command_line "%%L"
if not "%ReportLines%"=="2" (
    >&2 echo FAIL  %Name%: assembly report differs
    type "%CommandOutput%" >&2
    exit /b 1
)
if not "%ReportLine1%"=="wvasm 1" (
    >&2 echo FAIL  %Name%: assembly report header differs
    exit /b 1
)
if not "%ReportLine2%"=="%ExpectedReport%" (
    >&2 echo FAIL  %Name%: assembly report summary differs
    exit /b 1
)
exit /b 0

:capture_command_line
set /a ReportLines+=1
if "%ReportLines%"=="1" set "ReportLine1=%~1"
if "%ReportLines%"=="2" set "ReportLine2=%~1"
exit /b 0

:check_object
for %%F in ("%~1") do if not "%%~zF"=="%OutputBytes%" (
    >&2 echo FAIL  %Name%: object byte length differs
    exit /b 1
)
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%OutputDigest%" >nul
if errorlevel 1 (
    >&2 echo FAIL  %Name%: object identity differs
    exit /b 1
)
exit /b 0

:failed
set /a Failed=Total-Passed
call :cleanup
>&2 echo Tests: %Total%, Passed: %Passed%, Failed: %Failed%
exit /b 1

:cleanup
for %%F in (First.wvo Second.wvo Command.out Command.err Verify.out Verify.err) do (
    if exist "%TemporaryDirectory%\%%F" del /f /q "%TemporaryDirectory%\%%F" >nul 2>nul
)
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0

:usage
>&2 echo Usage: Tools\Native\Test-Assembler-Golden.cmd
exit /b 64
