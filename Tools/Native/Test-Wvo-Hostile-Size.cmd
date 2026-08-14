@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Wvo-Hostile-Size.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "TemporaryDirectory=%TEMP%\windvale-wvo-hostile-size-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1

set "Archive=%TemporaryDirectory%\Corpus.tar.gz"
set "Input=%TemporaryDirectory%\Oversized.wvo"
set "Sentinel=%TemporaryDirectory%\Sentinel.wvo"
set "Linked=%TemporaryDirectory%\Linked.bin"
set "Published=%TemporaryDirectory%\Published.wvo"
set "RunOutput=%TemporaryDirectory%\Run.out"
set "RunError=%TemporaryDirectory%\Run.err"
set "DecodeOutput=%TemporaryDirectory%\Decode.out"
set "DecodeError=%TemporaryDirectory%\Decode.err"
set "ExtractOutput=%TemporaryDirectory%\Extract.out"
set "ExtractError=%TemporaryDirectory%\Extract.err"
set "ArchiveDigest=4c9e5ed9aa6a822c64e799378ede641d86c37a6cc639003286afd2277144ef89"
set "InputDigest=95e441ca65cd41fa01b2a71799e79fd60db59ed34f13af32a91e85f90378676c"
set "SentinelDigest=0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5"
set /a Total=0
set /a Passed=0

call :decode_fixture "Tests\Native\Wvo-Hostile-Size\Corpus.tar.gz.b64" "%Archive%" "%ArchiveDigest%" "hostile-size archive"
if errorlevel 1 goto :failed
call :decode_fixture "Tests\Native\Wvo\Return-42.wvo.b64" "%Sentinel%" "%SentinelDigest%" "destination sentinel"
if errorlevel 1 goto :failed
tar -xzf "%Archive%" -C "%TemporaryDirectory%" > "%ExtractOutput%" 2> "%ExtractError%"
if errorlevel 1 (
    >&2 echo FAIL  wvo-hostile-size: archive could not be extracted
    type "%ExtractError%" >&2
    goto :failed
)
for %%S in ("%ExtractOutput%" "%ExtractError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  wvo-hostile-size: extractor wrote output
    type "%%~fS" >&2
    goto :failed
)
for %%S in ("%Input%") do if not "%%~zS"=="4194305" (
    >&2 echo FAIL  wvo-hostile-size: input size differs
    goto :failed
)
call :check_hash "%Input%" "%InputDigest%" "input identity differs"
if errorlevel 1 goto :failed

call :run_read_only "verify" "Verify-Wvo.cmd"
if errorlevel 1 goto :failed
call :run_read_only "inspect" "Inspect-Wvo.cmd"
if errorlevel 1 goto :failed
call :run_read_only "check" "Check-Wvo.cmd"
if errorlevel 1 goto :failed
call :run_linker
if errorlevel 1 goto :failed
call :run_publisher
if errorlevel 1 goto :failed

if not "%Total%"=="5" (
    >&2 echo FAIL  wvo-hostile-size: case count differs
    goto :failed
)
call :cleanup
echo Tests: %Total%, Passed: %Passed%, Failed: 0
exit /b 0

:run_read_only
set /a Total+=1
call "%RepositoryRoot%\Tools\Native\%~2" "%Input%" > "%RunOutput%" 2> "%RunError%"
set "RunExit=%ERRORLEVEL%"
if not "%RunExit%"=="1" (
    >&2 echo FAIL  %~1: hostile-size read-only exit differs
    exit /b 1
)
call :require_empty_channels "%~1"
if errorlevel 1 exit /b 1
call :check_hash "%Input%" "%InputDigest%" "hostile-size input changed"
if errorlevel 1 exit /b 1
del /f /q "%RunOutput%" "%RunError%" >nul 2>nul
set /a Passed+=1
echo PASS  %~1 boundary=file-snapshot oracle=WVO1001
exit /b 0

:run_linker
set /a Total+=1
copy /y "%Sentinel%" "%Linked%" >nul
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" "1048576" "Main" "%Linked%" "%Input%" > "%RunOutput%" 2> "%RunError%"
set "RunExit=%ERRORLEVEL%"
if not "%RunExit%"=="1" (
    >&2 echo FAIL  link: hostile-size linker exit differs
    exit /b 1
)
call :require_empty_channels "link"
if errorlevel 1 exit /b 1
call :check_hash "%Input%" "%InputDigest%" "linker changed the hostile-size input"
if errorlevel 1 exit /b 1
call :check_hash "%Linked%" "%SentinelDigest%" "hostile-size linker changed the output"
if errorlevel 1 exit /b 1
del /f /q "%Linked%" "%RunOutput%" "%RunError%" >nul 2>nul
set /a Passed+=1
echo PASS  link boundary=file-snapshot oracle=WVL1002
exit /b 0

:run_publisher
set /a Total+=1
copy /y "%Sentinel%" "%Published%" >nul
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Publish-Wvo.cmd" "%Input%" "%Published%" > "%RunOutput%" 2> "%RunError%"
set "RunExit=%ERRORLEVEL%"
if not "%RunExit%"=="1" (
    >&2 echo FAIL  publish: hostile-size publisher exit differs
    exit /b 1
)
call :require_empty_channels "publish"
if errorlevel 1 exit /b 1
call :check_hash "%Input%" "%InputDigest%" "publisher changed the hostile-size input"
if errorlevel 1 exit /b 1
call :check_hash "%Published%" "%SentinelDigest%" "hostile-size publisher changed the destination"
if errorlevel 1 exit /b 1
for /f "usebackq delims=" %%S in (`dir /b /a "%TemporaryDirectory%\.wvpublish-*" 2^>nul`) do (
    >&2 echo FAIL  publish: hostile-size rejection left scratch
    exit /b 1
)
del /f /q "%Published%" "%RunOutput%" "%RunError%" >nul 2>nul
set /a Passed+=1
echo PASS  publish boundary=file-snapshot oracle=WVO1001
exit /b 0

:require_empty_channels
for %%S in ("%RunOutput%" "%RunError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  %~1: hostile-size rejection wrote output
    type "%%~fS" >&2
    exit /b 1
)
exit /b 0

:decode_fixture
certutil -f -decode "%RepositoryRoot%\%~1" "%~2" > "%DecodeOutput%" 2> "%DecodeError%"
if errorlevel 1 (
    >&2 echo FAIL  wvo-hostile-size: %~4 could not be decoded
    exit /b 1
)
for %%S in ("%DecodeError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  wvo-hostile-size: %~4 decoder wrote a diagnostic
    type "%DecodeError%" >&2
    exit /b 1
)
call :check_hash "%~2" "%~3" "%~4 identity differs"
exit /b %ERRORLEVEL%

:check_hash
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~2" >nul
if errorlevel 1 (
    >&2 echo FAIL  wvo-hostile-size: %~3
    exit /b 1
)
exit /b 0

:failed
set /a Failed=Total-Passed
call :cleanup
>&2 echo Tests: %Total%, Passed: %Passed%, Failed: %Failed%
exit /b 1

:cleanup
for %%F in (Corpus.tar.gz Oversized.wvo Sentinel.wvo Linked.bin Published.wvo Run.out Run.err Decode.out Decode.err Extract.out Extract.err) do (
    if exist "%TemporaryDirectory%\%%F" del /f /q "%TemporaryDirectory%\%%F" >nul 2>nul
)
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0
