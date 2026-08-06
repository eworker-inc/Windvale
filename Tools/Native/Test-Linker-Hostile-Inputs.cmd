@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Linker-Hostile-Inputs.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "TemporaryDirectory=%TEMP%\windvale-linker-hostile-inputs-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "CorpusDirectory=%TemporaryDirectory%\Corpus"
mkdir "%CorpusDirectory%" || exit /b 1

set "Archive=%TemporaryDirectory%\Corpus.tar.gz"
set "Manifest=%CorpusDirectory%\Manifest.txt"
set "Sentinel=%TemporaryDirectory%\Sentinel.wvo"
set "Output=%TemporaryDirectory%\Output.bin"
set "RunOutput=%TemporaryDirectory%\Run.out"
set "RunError=%TemporaryDirectory%\Run.err"
set "DecodeOutput=%TemporaryDirectory%\Decode.out"
set "DecodeError=%TemporaryDirectory%\Decode.err"
set "ExtractOutput=%TemporaryDirectory%\Extract.out"
set "ExtractError=%TemporaryDirectory%\Extract.err"
set "ArchiveDigest=3648bc4a00bb822096ad669d0f24828f034df5b69023f1bdb2c3b3ab2a034160"
set "ManifestDigest=b3ab716d55e8c2693dbf0610b8638b23780867082bec7e768635a16e8e1fbfef"
set "SentinelDigest=0369f8b34765adb08799e6b852e9d1e249c40d1049976b01ff59355dd111f288"
set "ReportDigest=18eeeeb5d84e82c54cf14480bc5c54e593f5cd429d68686ad64110d9780a5353"
set /a Total=0
set /a Passed=0

call :decode_fixture "Tests\Native\Linker-Hostile-Inputs\Corpus.tar.gz.b64" "%Archive%" "%ArchiveDigest%" "hostile-input archive"
if errorlevel 1 goto :failed
call :decode_fixture "Tests\Native\Wvo\Bad-Magic.wvo.b64" "%Sentinel%" "%SentinelDigest%" "output sentinel"
if errorlevel 1 goto :failed

tar -xzf "%Archive%" -C "%CorpusDirectory%" > "%ExtractOutput%" 2> "%ExtractError%"
if errorlevel 1 (
    >&2 echo FAIL  linker-hostile: hostile-input archive could not be extracted
    type "%ExtractError%" >&2
    goto :failed
)
for %%S in ("%ExtractOutput%" "%ExtractError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  linker-hostile: hostile-input extractor wrote output
    type "%%~fS" >&2
    goto :failed
)
call :check_hash "%Manifest%" "%ManifestDigest%" "hostile-input manifest identity differs"
if errorlevel 1 goto :failed

for /f "usebackq skip=1 tokens=1-3 delims=|" %%A in ("%Manifest%") do (
    call :run_case "%%A" "%%B" "%%C"
    if errorlevel 1 goto :failed
)
if not "%Total%"=="200" (
    >&2 echo FAIL  linker-hostile: hostile-input case count differs
    goto :failed
)

call :cleanup
echo Tests: %Total%, Passed: %Passed%, Failed: 0
exit /b 0

:run_case
set "Input=%CorpusDirectory%\%~1"
set /a Total+=1
for %%S in ("%Input%") do if not "%%~zS"=="%~2" (
    >&2 echo FAIL  %~1: hostile input size differs
    exit /b 1
)
call :check_hash "%Input%" "%~3" "hostile input identity differs"
if errorlevel 1 exit /b 1
copy /y "%Sentinel%" "%Output%" >nul
if errorlevel 1 (
    >&2 echo FAIL  %~1: output sentinel could not be created
    exit /b 1
)
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" "1048576" "Main" "%Output%" "%Input%" > "%RunOutput%" 2> "%RunError%"
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
call :check_hash "%RunError%" "%ReportDigest%" "native linker report differs"
if errorlevel 1 (
    type "%RunError%" >&2
    exit /b 1
)
call :check_hash "%Output%" "%SentinelDigest%" "rejected link changed the output"
if errorlevel 1 exit /b 1
call :check_hash "%Input%" "%~3" "native linker changed the hostile input"
if errorlevel 1 exit /b 1
del /f /q "%Output%" "%RunOutput%" "%RunError%" >nul 2>nul
set /a Passed+=1
echo PASS  %~1
exit /b 0

:decode_fixture
certutil -f -decode "%RepositoryRoot%\%~1" "%~2" > "%DecodeOutput%" 2> "%DecodeError%"
if errorlevel 1 (
    >&2 echo FAIL  linker-hostile: %~4 could not be decoded
    exit /b 1
)
for %%S in ("%DecodeError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  linker-hostile: %~4 decoder wrote a diagnostic
    type "%DecodeError%" >&2
    exit /b 1
)
call :check_hash "%~2" "%~3" "%~4 identity differs"
exit /b %ERRORLEVEL%

:check_hash
for %%S in ("%~1") do if "%%~zS"=="0" (
    if /I "%~2"=="e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855" exit /b 0
    >&2 echo FAIL  linker-hostile: %~3
    exit /b 1
)
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~2" >nul
if errorlevel 1 (
    >&2 echo FAIL  linker-hostile: %~3
    exit /b 1
)
exit /b 0

:failed
set /a Failed=Total-Passed
call :cleanup
>&2 echo Tests: %Total%, Passed: %Passed%, Failed: %Failed%
exit /b 1

:cleanup
for /l %%I in (0,1,9) do if exist "%CorpusDirectory%\Case-00%%I.wvo" del /f /q "%CorpusDirectory%\Case-00%%I.wvo" >nul 2>nul
for /l %%I in (10,1,99) do if exist "%CorpusDirectory%\Case-0%%I.wvo" del /f /q "%CorpusDirectory%\Case-0%%I.wvo" >nul 2>nul
for /l %%I in (100,1,199) do if exist "%CorpusDirectory%\Case-%%I.wvo" del /f /q "%CorpusDirectory%\Case-%%I.wvo" >nul 2>nul
if exist "%Manifest%" del /f /q "%Manifest%" >nul 2>nul
rmdir "%CorpusDirectory%" >nul 2>nul
for %%F in (Corpus.tar.gz Sentinel.wvo Output.bin Run.out Run.err Decode.out Decode.err Extract.out Extract.err) do (
    if exist "%TemporaryDirectory%\%%F" del /f /q "%TemporaryDirectory%\%%F" >nul 2>nul
)
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0
