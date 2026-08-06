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
set "ManySections=%TemporaryDirectory%\Many-Sections.wvo"
set "UnresolvedImport=%TemporaryDirectory%\Unresolved-Import.wvo"
set "WrongKindProvider=%TemporaryDirectory%\Wrong-Kind-Provider.wvo"
set "AbsoluteOverflow=%TemporaryDirectory%\Absolute-Overflow.wvo"
set "RelativeOverflow=%TemporaryDirectory%\Relative-Overflow.wvo"
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
call :decode_fixture "Tests\Native\Wvo-Linker-Rejections\Many-Sections.wvo.b64" "%ManySections%" "09cad03b9bf0543db2dec815f3f20deff044f5226e9347314b8c4d9a9e1020f8" "many-sections WVO"
if errorlevel 1 goto :failed
call :decode_fixture "Tests\Native\Wvo-Linker-Rejections\Unresolved-Import.wvo.b64" "%UnresolvedImport%" "569926307b578cd1bf90dfb2b3c70eeb4b5ec7eff8e638e83613e89463717617" "unresolved-import WVO"
if errorlevel 1 goto :failed
call :decode_fixture "Tests\Native\Wvo-Linker-Rejections\Wrong-Kind-Provider.wvo.b64" "%WrongKindProvider%" "1276a484c52d48996a7d781121f85cab93ecde729cb6ce18dd7c77b4bdb98ce6" "wrong-kind provider WVO"
if errorlevel 1 goto :failed
call :decode_fixture "Tests\Native\Wvo-Linker-Rejections\Absolute-Overflow.wvo.b64" "%AbsoluteOverflow%" "994bc31ed39548dbd9339e7b0d2ac9b58936250b3603f90e84bda51f74b8bb11" "absolute-overflow WVO"
if errorlevel 1 goto :failed
call :decode_fixture "Tests\Native\Wvo-Linker-Rejections\Relative-Overflow.wvo.b64" "%RelativeOverflow%" "4d6dcc8211e02399e8ba38fbbec94dcd11c15842efe09fd8af615e25b57d7a48" "relative-overflow WVO"
if errorlevel 1 goto :failed

call :run_case "invalid-base" "b5a687af92c9eca7eb5ba850bddf6dec932c94a6be304af35357655a915056b8" "invalid" "Main" "%Valid%"
if errorlevel 1 goto :failed
call :run_case "malformed-object" "18eeeeb5d84e82c54cf14480bc5c54e593f5cd429d68686ad64110d9780a5353" "1048576" "Main" "%Invalid%"
if errorlevel 1 goto :failed
call :run_case "aggregate-limit" "33ecb82d77ff1f307b60a18993edf46807a39bf66ab7091054fc9ee7ad04ef61" "1048576" "Main" "%ManySections%" "%ManySections%" "%ManySections%" "%ManySections%" "%ManySections%"
if errorlevel 1 goto :failed
call :run_case "duplicate-export" "cd8c0a1c80784f3d6db68984fe07f9bcbc0657c12e548bd923efad7f2666c324" "1048576" "Main" "%Valid%" "%Valid%"
if errorlevel 1 goto :failed
call :run_case "undefined-import" "448d3e4eb8053d1aca41ebcdcf61af3d8519f3fea033859f82eb95d63ac275e0" "1048576" "Main" "%UnresolvedImport%"
if errorlevel 1 goto :failed
call :run_case "kind-mismatch" "047bea593cba87e948ea03c3cee09c5b04879683a1eb5856b9d0d30f7f774441" "1048576" "Main" "%UnresolvedImport%" "%WrongKindProvider%"
if errorlevel 1 goto :failed
call :run_case "missing-entry" "883ad60b71d4c010d4a2ddf168199dfaae04d1e076313ee1cf4dac8bee67a517" "1048576" "Missing" "%Valid%"
if errorlevel 1 goto :failed
call :run_case "layout-overflow" "9c393cdbef3dc4a6dbe28ae5ba0c77fc56166a84b30c845bee78475f2679912d" "4294967295" "Main" "%Valid%"
if errorlevel 1 goto :failed
call :run_case "absolute-overflow" "1867b048e4c725d2ea76f0ed0dd28b80f360fe07395d17ff62b743d5bc974b74" "2147483649" "Main" "%AbsoluteOverflow%"
if errorlevel 1 goto :failed
call :run_case "relative-overflow" "d8a7ac5340b29066470b5656c840654221b508702cbc62ebfcecf7f36aa66e67" "0" "Main" "%RelativeOverflow%"
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
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" "%~3" "%~4" "%Output%" %5 %6 %7 %8 %9 > "%RunOutput%" 2> "%RunError%"
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
for %%F in (Return-42.wvo Bad-Magic.wvo Many-Sections.wvo Unresolved-Import.wvo Wrong-Kind-Provider.wvo Absolute-Overflow.wvo Relative-Overflow.wvo Output.bin Run.out Run.err Decode.out Decode.err) do (
    if exist "%TemporaryDirectory%\%%F" del /f /q "%TemporaryDirectory%\%%F" >nul 2>nul
)
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0
