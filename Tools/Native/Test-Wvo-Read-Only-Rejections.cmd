@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Wvo-Read-Only-Rejections.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "TemporaryDirectory=%TEMP%\windvale-wvo-read-only-rejections-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1

set "Input=%TemporaryDirectory%\Input.wvo"
set "RunOutput=%TemporaryDirectory%\Run.out"
set "RunError=%TemporaryDirectory%\Run.err"
set "DecodeOutput=%TemporaryDirectory%\Decode.out"
set "DecodeError=%TemporaryDirectory%\Decode.err"
set /a Total=0
set /a Passed=0

call :run_case "short-header" "Tests\Native\Wvo-Rejections\Short-Header.wvo.b64" "6e340b9cffb37a989ca544e6bb780a2c78901d3fb33738768511a30617afa01d" "97779c19c3b55c92f53faa567de292403493fbff7180cfb6e2bade8991ef63aa"
if errorlevel 1 goto :failed
call :run_case "bad-magic" "Tests\Native\Wvo\Bad-Magic.wvo.b64" "0369f8b34765adb08799e6b852e9d1e249c40d1049976b01ff59355dd111f288" "2e53f573d1e94159c58368c4d9ebcba284d6c13f63a286bd75264bc837a162e4"
if errorlevel 1 goto :failed
call :run_case "bad-version" "Tests\Native\Wvo-Rejections\Bad-Version.wvo.b64" "3c724339c2a6fe6d41c07a461907e5bbee7abc95cf899b0605e77f744f0c6081" "bce421b96f8ee4ce19c322eba64a71bcefa3539640b41ecca2a5cd70bab4055e"
if errorlevel 1 goto :failed
call :run_case "bad-architecture" "Tests\Native\Wvo-Rejections\Bad-Architecture.wvo.b64" "7ff46081c9b5f3d50d0a499f74d665bb9b474e308432ddcf484079a6f434db3d" "8f6a586a1323284e6aeb9522fc292b266e4368f44d5e022b87fab28632a2da97"
if errorlevel 1 goto :failed
call :run_case "unsupported-flags" "Tests\Native\Wvo-Rejections\Unsupported-Flags.wvo.b64" "b1b581c75901f1bba0dfeb37fb888342d32c6f6eff565165277d051d7ae0f4c7" "3eab07bbffa763acfd259b4e3b0b09206098c61f625bf23e202dc16fb19cc11c"
if errorlevel 1 goto :failed
call :run_case "limit-exceeded" "Tests\Native\Wvo-Rejections\Limit-Exceeded.wvo.b64" "6e191db4e2ce6107493baed610e9d116018ae887972d94d3df5969d3d405c0a8" "d502b71111e5f7557fff108bc740b558d6d15acdc5eb22ada9f8cfe2dca0a46e"
if errorlevel 1 goto :failed
call :run_case "out-of-bounds" "Tests\Native\Wvo\Truncated.wvo.b64" "6f120ce6b833f781ab014844af535b25fe28eb2d565afa2b2f4360c7a0c99371" "9b45f12022ab0ba549e6c2ffa49cb15673d96c8f58efd5d6d9c2def87097aedb"
if errorlevel 1 goto :failed
call :run_case "invalid-name" "Tests\Native\Wvo-Rejections\Invalid-Name.wvo.b64" "2cf0c91c9e6df189f2a79214bc5b5a3690e3b0140e41eae2683efd817bf9d067" "bf35958972ccf812961fd52b92b1ebeb6f5e9b7e87a77c7083064de590c548cb"
if errorlevel 1 goto :failed
call :run_case "invalid-section" "Tests\Native\Wvo-Rejections\Invalid-Section.wvo.b64" "d0a93c19fceb58070797c893f3ba5eb3ebae60e380a85d5fd84cf037995702e8" "430a541121485335be6635ec6277141489dafb4b73ec47dcfb1ddc72a32e649d"
if errorlevel 1 goto :failed
call :run_case "invalid-symbol" "Tests\Native\Wvo-Rejections\Invalid-Symbol.wvo.b64" "9ba10fcccc2e6d4b9a9fef8343dacb1743a2c2e1f0c1795ef0b97a3b50f655a5" "b3dd9e318a471bf1f8f5e589d1c119f4b89b02f69d3956f42a51bde5afc1875e"
if errorlevel 1 goto :failed
call :run_case "invalid-relocation" "Tests\Native\Wvo-Rejections\Invalid-Relocation.wvo.b64" "b36011ba5615c228dcf6c4d389c7c50f24b25934b47d01bbbc701c9bf02b2736" "b6b147e8141a3de78ab59b3af4d04081c37d77b8124ccdbefffc94645ab18995"
if errorlevel 1 goto :failed
call :run_case "noncanonical-order" "Tests\Native\Wvo-Rejections\Noncanonical-Order.wvo.b64" "443499e89326160f6172be9dd0be918935373e1c862d2192570cc922471026a7" "2012a1501f7861708c992f61dfe308bc8ef217781b5e92bd2ca67fc56d6e31d8"
if errorlevel 1 goto :failed
call :run_case "trailing-bytes" "Tests\Native\Wvo\Trailing.wvo.b64" "3ca5e84240e8f12be84fdb957df37f8162e74415417cd7009f92698e683ee981" "3cdcb2fa62f4fc698e9624e68dc10dbf95e7363cf0332b280066083cc1783711"
if errorlevel 1 goto :failed

call :cleanup
echo Tests: %Total%, Passed: %Passed%, Failed: 0
exit /b 0

:run_case
set /a Total+=1
certutil -f -decode "%RepositoryRoot%\%~2" "%Input%" > "%DecodeOutput%" 2> "%DecodeError%"
if errorlevel 1 (
    >&2 echo FAIL  %~1: WVO fixture could not be decoded
    exit /b 1
)
for %%S in ("%DecodeError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  %~1: WVO decoder wrote a diagnostic
    type "%DecodeError%" >&2
    exit /b 1
)
certutil -hashfile "%Input%" SHA256 | findstr /I /C:"%~3" >nul
if errorlevel 1 (
    >&2 echo FAIL  %~1: WVO input identity differs
    exit /b 1
)
call :run_launcher "%~1" "Verify-Wvo.cmd" "%~3" "%~4"
if errorlevel 1 exit /b 1
call :run_launcher "%~1" "Inspect-Wvo.cmd" "%~3" "%~4"
if errorlevel 1 exit /b 1
del /f /q "%Input%" "%DecodeOutput%" "%DecodeError%" >nul 2>nul
set /a Passed+=1
echo PASS  %~1
exit /b 0

:run_launcher
call "%RepositoryRoot%\Tools\Native\%~2" "%Input%" > "%RunOutput%" 2> "%RunError%"
set "RunExit=%ERRORLEVEL%"
if not "%RunExit%"=="2" (
    >&2 echo FAIL  %~1: native WVO read-only exit differs
    exit /b 1
)
for %%S in ("%RunOutput%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  %~1: rejected WVO wrote standard output
    type "%RunOutput%" >&2
    exit /b 1
)
certutil -hashfile "%RunError%" SHA256 | findstr /I /C:"%~4" >nul
if errorlevel 1 (
    >&2 echo FAIL  %~1: native WVO report differs
    type "%RunError%" >&2
    exit /b 1
)
certutil -hashfile "%Input%" SHA256 | findstr /I /C:"%~3" >nul
if errorlevel 1 (
    >&2 echo FAIL  %~1: native WVO read-only command changed its input
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
for %%F in (Input.wvo Run.out Run.err Decode.out Decode.err) do (
    if exist "%TemporaryDirectory%\%%F" del /f /q "%TemporaryDirectory%\%%F" >nul 2>nul
)
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0
