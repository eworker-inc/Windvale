@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Linker-Map-Limit.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "TemporaryDirectory=%TEMP%\windvale-linker-map-limit-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1

set "Archive=%TemporaryDirectory%\Map-Objects.tar.gz"
set "Entry=%TemporaryDirectory%\Entry.wvo"
set "Locals4096=%TemporaryDirectory%\Map-Locals-4096.wvo"
set "Locals4095=%TemporaryDirectory%\Map-Locals-4095.wvo"
set "Output=%TemporaryDirectory%\Output.bin"
set "RunOutput=%TemporaryDirectory%\Run.out"
set "RunError=%TemporaryDirectory%\Run.err"
set "DecodeOutput=%TemporaryDirectory%\Decode.out"
set "DecodeError=%TemporaryDirectory%\Decode.err"
set "ExtractOutput=%TemporaryDirectory%\Extract.out"
set "ExtractError=%TemporaryDirectory%\Extract.err"
set "ArchiveDigest=1c6227931496f54c93677b4dfecfbfa256214a5da72ecfd05d441e49c809e27d"
set "EntryDigest=0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5"
set "Locals4096Digest=a05c4f51be960c7fc900d8cc9fc39dbc525ccd0b2b1a4c55b12ca8396107ee75"
set "Locals4095Digest=398737cfd465fb976e6319ce7ddc4dbefb9e082d39432d09474cf75f8aafffdc"
set "ReportDigest=097ad88fa0e4fd48504da8d69516e47ff7f6b5979fccf186e0307b814b5af86e"

call :decode_fixture "Tests\Native\Linker-Map-Limit\Map-Objects.tar.gz.b64" "%Archive%" "%ArchiveDigest%" "map-object archive"
if errorlevel 1 goto :failed
call :decode_fixture "Tests\Native\Wvo\Return-42.wvo.b64" "%Entry%" "%EntryDigest%" "entry WVO"
if errorlevel 1 goto :failed

tar -xzf "%Archive%" -C "%TemporaryDirectory%" > "%ExtractOutput%" 2> "%ExtractError%"
if errorlevel 1 (
    >&2 echo FAIL  canonical-map-limit: map-object archive could not be extracted
    type "%ExtractError%" >&2
    goto :failed
)
for %%S in ("%ExtractOutput%" "%ExtractError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  canonical-map-limit: map-object extractor wrote output
    type "%%~fS" >&2
    goto :failed
)
call :check_hash "%Locals4096%" "%Locals4096Digest%" "4,096-local WVO identity differs"
if errorlevel 1 goto :failed
call :check_hash "%Locals4095%" "%Locals4095Digest%" "4,095-local WVO identity differs"
if errorlevel 1 goto :failed

copy /y "%Entry%" "%Output%" >nul
if errorlevel 1 (
    >&2 echo FAIL  canonical-map-limit: output sentinel could not be created
    goto :failed
)

call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" "0" "Main" "%Output%" "%Entry%" "%Locals4096%" "%Locals4096%" "%Locals4096%" "%Locals4095%" > "%RunOutput%" 2> "%RunError%"
set "RunExit=%ERRORLEVEL%"
if not "%RunExit%"=="2" (
    >&2 echo FAIL  canonical-map-limit: native linker exit differs
    goto :failed
)
for %%S in ("%RunOutput%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  canonical-map-limit: rejected link wrote standard output
    type "%RunOutput%" >&2
    goto :failed
)
call :check_hash "%RunError%" "%ReportDigest%" "native linker report differs"
if errorlevel 1 (
    type "%RunError%" >&2
    goto :failed
)
call :check_hash "%Output%" "%EntryDigest%" "rejected link changed the output"
if errorlevel 1 goto :failed
call :check_hash "%Entry%" "%EntryDigest%" "entry WVO changed during linking"
if errorlevel 1 goto :failed
call :check_hash "%Locals4096%" "%Locals4096Digest%" "4,096-local WVO changed during linking"
if errorlevel 1 goto :failed
call :check_hash "%Locals4095%" "%Locals4095Digest%" "4,095-local WVO changed during linking"
if errorlevel 1 goto :failed

call :cleanup
echo PASS  canonical-map-limit
echo Tests: 1, Passed: 1, Failed: 0
exit /b 0

:decode_fixture
certutil -f -decode "%RepositoryRoot%\%~1" "%~2" > "%DecodeOutput%" 2> "%DecodeError%"
if errorlevel 1 (
    >&2 echo FAIL  canonical-map-limit: %~4 could not be decoded
    exit /b 1
)
for %%S in ("%DecodeError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  canonical-map-limit: %~4 decoder wrote a diagnostic
    type "%DecodeError%" >&2
    exit /b 1
)
call :check_hash "%~2" "%~3" "%~4 identity differs"
exit /b %ERRORLEVEL%

:check_hash
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~2" >nul
if errorlevel 1 (
    >&2 echo FAIL  canonical-map-limit: %~3
    certutil -hashfile "%~1" SHA256 >&2
    exit /b 1
)
exit /b 0

:failed
call :cleanup
>&2 echo Tests: 1, Passed: 0, Failed: 1
exit /b 1

:cleanup
for %%F in (Map-Objects.tar.gz Entry.wvo Map-Locals-4096.wvo Map-Locals-4095.wvo Output.bin Run.out Run.err Decode.out Decode.err Extract.out Extract.err) do (
    if exist "%TemporaryDirectory%\%%F" del /f /q "%TemporaryDirectory%\%%F" >nul 2>nul
)
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0
