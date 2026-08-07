@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Console-Application-Segmented-Size-Boundaries.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "TemporaryDirectory=%TEMP%\windvale-console-segmented-size-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "CorpusDirectory=%TemporaryDirectory%\Corpus"
mkdir "%CorpusDirectory%" || exit /b 1

set "Archive=%TemporaryDirectory%\Corpus.tar.gz"
set "Manifest=%CorpusDirectory%\Manifest.txt"
set "RunOutput=%TemporaryDirectory%\Run.out"
set "RunError=%TemporaryDirectory%\Run.err"
set "DecodeOutput=%TemporaryDirectory%\Decode.out"
set "DecodeError=%TemporaryDirectory%\Decode.err"
set "ExtractOutput=%TemporaryDirectory%\Extract.out"
set "ExtractError=%TemporaryDirectory%\Extract.err"
set "ArchiveDigest=d0e9aa4f6e31d3bd28fb0468606f43b275c320adb470e4d3b78034d440573200"
set "ManifestDigest=50c1c87ac9dcaaccbd5036c2d67677dde044a6b24f11fe78149784741c72ca29"
set /a Total=0
set /a Passed=0

certutil -f -decode "%RepositoryRoot%\Tests\Native\Console-Application-Segmented-Size-Boundaries\Corpus.tar.gz.b64" "%Archive%" > "%DecodeOutput%" 2> "%DecodeError%"
if errorlevel 1 goto :decode_failed
for %%S in ("%DecodeError%") do if not "%%~zS"=="0" goto :decode_diagnostic
call :check_hash "%Archive%" "%ArchiveDigest%" "archive identity differs"
if errorlevel 1 goto :failed

tar -xzf "%Archive%" -C "%CorpusDirectory%" > "%ExtractOutput%" 2> "%ExtractError%"
if errorlevel 1 goto :extract_failed
for %%S in ("%ExtractOutput%" "%ExtractError%") do if not "%%~zS"=="0" goto :extract_diagnostic
call :check_hash "%Manifest%" "%ManifestDigest%" "manifest identity differs"
if errorlevel 1 goto :failed
set "Header="
for /f "usebackq delims=" %%H in ("%Manifest%") do if not defined Header set "Header=%%H"
if not "%Header%"=="windvale-console-application-segmented-size-boundaries 1" goto :manifest_failed

for /f "usebackq skip=2 tokens=1-9 delims=|" %%A in ("%Manifest%") do (
    call :run_case "%%A" "%%B" "%%C" "%%D" "%%E" "%%F" "%%G" "%%H" "%%I"
    if errorlevel 1 goto :failed
)
if not "%Total%"=="2" goto :count_failed

call :cleanup
echo Tests: %Total%, Passed: %Passed%, Failed: 0
exit /b 0

:run_case
set "Case=%~1"
set "Platform=%~2"
set "Stage0=%~3"
set "FirstName=%~4"
set "FirstBytes=%~5"
set "FirstDigest=%~6"
set "SecondName=%~7"
set "SecondBytes=%~8"
set "SecondDigest=%~9"
set "ReportDigest="
if "%Case%"=="windows-max-plus-one" (
    if not "%Platform%"=="windows" exit /b 1
    if not "%Stage0%"=="WVW2001" exit /b 1
    if not "%FirstName%"=="Windows-First.bin" exit /b 1
    if not "%FirstBytes%"=="4194304" exit /b 1
    if not "%SecondName%"=="Windows-Second.bin" exit /b 1
    if not "%SecondBytes%"=="2049" exit /b 1
    set "ReportDigest=d0b1304c62778d71c7df11b2c9d3759139810b0acca3115e77bb44aae1b052ba"
) else if "%Case%"=="linux-max-plus-one" (
    if not "%Platform%"=="linux" exit /b 1
    if not "%Stage0%"=="WVL2001" exit /b 1
    if not "%FirstName%"=="Linux-First.bin" exit /b 1
    if not "%FirstBytes%"=="4194304" exit /b 1
    if not "%SecondName%"=="Linux-Second.bin" exit /b 1
    if not "%SecondBytes%"=="8305" exit /b 1
    set "ReportDigest=9b8b2d84bdb475db94d5a0e1be47a73f12d9663e966c2c8708ce4f556aacb1d2"
) else (
    exit /b 1
)
set "First=%CorpusDirectory%\%FirstName%"
set "Second=%CorpusDirectory%\%SecondName%"
for %%S in ("%First%") do if not "%%~zS"=="%FirstBytes%" exit /b 1
for %%S in ("%Second%") do if not "%%~zS"=="%SecondBytes%" exit /b 1
call :check_hash "%First%" "%FirstDigest%" "%Case% first-chunk identity differs"
if errorlevel 1 exit /b 1
call :check_hash "%Second%" "%SecondDigest%" "%Case% second-chunk identity differs"
if errorlevel 1 exit /b 1

set /a Total+=1
call "%RepositoryRoot%\Tools\Native\Verify-Console-Segmented.cmd" "%First%" "%Second%" > "%RunOutput%" 2> "%RunError%"
if not "%ERRORLEVEL%"=="1" exit /b 1
for %%S in ("%RunOutput%") do if not "%%~zS"=="0" exit /b 1
call :check_hash "%RunError%" "%ReportDigest%" "%Case% rejection report differs"
if errorlevel 1 exit /b 1
call :check_hash "%First%" "%FirstDigest%" "%Case% changed the first chunk"
if errorlevel 1 exit /b 1
call :check_hash "%Second%" "%SecondDigest%" "%Case% changed the second chunk"
if errorlevel 1 exit /b 1
set /a Passed+=1
echo PASS  %Case% oracle=%Stage0%
exit /b 0

:check_hash
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~2" >nul
if errorlevel 1 (
    >&2 echo FAIL  console-segmented-size: %~3
    >&2 echo Expected SHA-256: %~2
    certutil -hashfile "%~1" SHA256 >&2
    exit /b 1
)
exit /b 0

:decode_failed
>&2 echo FAIL  console-segmented-size: corpus could not be decoded
goto :failed
:decode_diagnostic
>&2 echo FAIL  console-segmented-size: decoder wrote a diagnostic
type "%DecodeError%" >&2
goto :failed
:extract_failed
>&2 echo FAIL  console-segmented-size: archive could not be extracted
type "%ExtractError%" >&2
goto :failed
:extract_diagnostic
>&2 echo FAIL  console-segmented-size: extractor wrote output
type "%ExtractOutput%" >&2
type "%ExtractError%" >&2
goto :failed
:manifest_failed
>&2 echo FAIL  console-segmented-size: manifest header differs
goto :failed
:count_failed
>&2 echo FAIL  console-segmented-size: total case count differs
goto :failed
:failed
set /a Failed=Total-Passed
call :cleanup
>&2 echo Tests: %Total%, Passed: %Passed%, Failed: %Failed%
exit /b 1

:cleanup
for %%F in (Manifest.txt Windows-First.bin Windows-Second.bin Linux-First.bin Linux-Second.bin) do if exist "%CorpusDirectory%\%%F" del /f /q "%CorpusDirectory%\%%F" >nul 2>nul
rmdir "%CorpusDirectory%" >nul 2>nul
for %%F in (Corpus.tar.gz Run.out Run.err Decode.out Decode.err Extract.out Extract.err) do if exist "%TemporaryDirectory%\%%F" del /f /q "%TemporaryDirectory%\%%F" >nul 2>nul
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0
