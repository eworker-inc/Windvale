@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Wvo-Differential.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "TemporaryDirectory=%TEMP%\windvale-wvo-differential-%RANDOM%-%RANDOM%-%RANDOM%"
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
set "ArchiveDigest=74d90d981ef3665eee2fb16a5abb57ae2e9d308a8e56b1aff56c49d97997d684"
set "ManifestDigest=ef6a187dfc5d0bbffcfb61df40146af54f74d76302dee1358b4a3fbefd7aa556"
set /a Total=0
set /a Passed=0
set /a MutationCases=0
set /a RandomCases=0
set /a AcceptedCases=0
set /a RejectedCases=0

call :decode_fixture "Tests\Native\Wvo-Differential\Corpus.tar.gz.b64" "%Archive%" "%ArchiveDigest%"
if errorlevel 1 goto :failed
tar -xzf "%Archive%" -C "%CorpusDirectory%" > "%ExtractOutput%" 2> "%ExtractError%"
if errorlevel 1 (
    >&2 echo FAIL  wvo-differential: corpus archive could not be extracted
    type "%ExtractError%" >&2
    goto :failed
)
for %%S in ("%ExtractOutput%" "%ExtractError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  wvo-differential: corpus extractor wrote output
    type "%%~fS" >&2
    goto :failed
)
call :check_hash "%Manifest%" "%ManifestDigest%" "manifest identity differs"
if errorlevel 1 goto :failed
set "Header="
for /f "usebackq delims=" %%H in ("%Manifest%") do if not defined Header set "Header=%%H"
if not "%Header%"=="windvale-wvo-differential-corpus 1" (
    >&2 echo FAIL  wvo-differential: manifest header differs
    goto :failed
)

for /f "usebackq skip=1 tokens=1-10 delims=|" %%A in ("%Manifest%") do (
    call :run_case "%%A" "%%B" "%%C" "%%D" "%%E" "%%F" "%%G" "%%J"
    if errorlevel 1 goto :failed
)
if not "%Total%"=="256" (
    >&2 echo FAIL  wvo-differential: total case count differs
    goto :failed
)
if not "%MutationCases%"=="128" (
    >&2 echo FAIL  wvo-differential: mutation case count differs
    goto :failed
)
if not "%RandomCases%"=="128" (
    >&2 echo FAIL  wvo-differential: random case count differs
    goto :failed
)
if not "%AcceptedCases%"=="32" (
    >&2 echo FAIL  wvo-differential: accepted case count differs
    goto :failed
)
if not "%RejectedCases%"=="224" (
    >&2 echo FAIL  wvo-differential: rejected case count differs
    goto :failed
)

call :cleanup
echo Tests: %Total%, Passed: %Passed%, Failed: 0
exit /b 0

:run_case
for %%N in ("%~1") do if not "%%~nxN"=="%~1" (
    >&2 echo FAIL  %~1: corpus input name is not a filename
    exit /b 1
)
if /I "%~2"=="mutation" (
    set /a MutationCases+=1
) else if /I "%~2"=="random" (
    set /a RandomCases+=1
) else (
    >&2 echo FAIL  %~1: corpus family differs
    exit /b 1
)
set "Input=%CorpusDirectory%\%~1"
set /a Total+=1
for %%S in ("%Input%") do if not "%%~zS"=="%~5" (
    >&2 echo FAIL  %~1: input size differs
    exit /b 1
)
call :check_hash "%Input%" "%~6" "input identity differs"
if errorlevel 1 exit /b 1

call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%Input%" > "%RunOutput%" 2> "%RunError%"
set "RunExit=%ERRORLEVEL%"
if /I "%~7"=="accepted" (
    if not "%RunExit%"=="0" (
        >&2 echo FAIL  %~1: native verifier rejected an oracle-accepted input
        exit /b 1
    )
    for %%S in ("%RunError%") do if not "%%~zS"=="0" (
        >&2 echo FAIL  %~1: accepted input wrote a diagnostic
        type "%RunError%" >&2
        exit /b 1
    )
    call :check_hash "%RunOutput%" "%~8" "accepted report differs"
    if errorlevel 1 exit /b 1
    set /a AcceptedCases+=1
) else if /I "%~7"=="rejected" (
    if not "%RunExit%"=="2" (
        >&2 echo FAIL  %~1: native verifier accepted an oracle-rejected input
        exit /b 1
    )
    for %%S in ("%RunOutput%") do if not "%%~zS"=="0" (
        >&2 echo FAIL  %~1: rejected input wrote standard output
        type "%RunOutput%" >&2
        exit /b 1
    )
    call :check_rejection_report "%RunError%"
    if errorlevel 1 exit /b 1
    set /a RejectedCases+=1
) else (
    >&2 echo FAIL  %~1: oracle outcome differs
    exit /b 1
)
call :check_hash "%Input%" "%~6" "native verifier changed the input"
if errorlevel 1 exit /b 1
del /f /q "%RunOutput%" "%RunError%" >nul 2>nul
set /a Passed+=1
echo PASS  %~1 oracle=%~7
exit /b 0

:check_rejection_report
set "ReportLine="
set /a ReportLines=0
for /f "usebackq delims=" %%L in ("%~1") do call :record_report_line "%%L"
if not "%ReportLines%"=="1" (
    >&2 echo FAIL  wvo-differential: rejected report line count differs
    exit /b 1
)
if not "%ReportLine:~0,14%"=="object status=" (
    >&2 echo FAIL  wvo-differential: rejected report shape differs
    exit /b 1
)
exit /b 0

:record_report_line
set /a ReportLines+=1
set "ReportLine=%~1"
exit /b 0

:decode_fixture
certutil -f -decode "%RepositoryRoot%\%~1" "%~2" > "%DecodeOutput%" 2> "%DecodeError%"
if errorlevel 1 (
    >&2 echo FAIL  wvo-differential: corpus archive could not be decoded
    exit /b 1
)
for %%S in ("%DecodeError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  wvo-differential: decoder wrote a diagnostic
    type "%DecodeError%" >&2
    exit /b 1
)
call :check_hash "%~2" "%~3" "archive identity differs"
exit /b %ERRORLEVEL%

:check_hash
for %%S in ("%~1") do if "%%~zS"=="0" (
    if /I "%~2"=="e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855" exit /b 0
    >&2 echo FAIL  wvo-differential: %~3
    exit /b 1
)
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~2" >nul
if errorlevel 1 (
    >&2 echo FAIL  wvo-differential: %~3
    exit /b 1
)
exit /b 0

:failed
set /a Failed=Total-Passed
call :cleanup
>&2 echo Tests: %Total%, Passed: %Passed%, Failed: %Failed%
exit /b 1

:cleanup
for /l %%I in (0,1,9) do (
    if exist "%CorpusDirectory%\Mutation-00%%I.wvo" del /f /q "%CorpusDirectory%\Mutation-00%%I.wvo" >nul 2>nul
    if exist "%CorpusDirectory%\Random-00%%I.wvo" del /f /q "%CorpusDirectory%\Random-00%%I.wvo" >nul 2>nul
)
for /l %%I in (10,1,99) do (
    if exist "%CorpusDirectory%\Mutation-0%%I.wvo" del /f /q "%CorpusDirectory%\Mutation-0%%I.wvo" >nul 2>nul
    if exist "%CorpusDirectory%\Random-0%%I.wvo" del /f /q "%CorpusDirectory%\Random-0%%I.wvo" >nul 2>nul
)
for /l %%I in (100,1,127) do (
    if exist "%CorpusDirectory%\Mutation-%%I.wvo" del /f /q "%CorpusDirectory%\Mutation-%%I.wvo" >nul 2>nul
    if exist "%CorpusDirectory%\Random-%%I.wvo" del /f /q "%CorpusDirectory%\Random-%%I.wvo" >nul 2>nul
)
if exist "%Manifest%" del /f /q "%Manifest%" >nul 2>nul
rmdir "%CorpusDirectory%" >nul 2>nul
for %%F in (Corpus.tar.gz Run.out Run.err Decode.out Decode.err Extract.out Extract.err) do (
    if exist "%TemporaryDirectory%\%%F" del /f /q "%TemporaryDirectory%\%%F" >nul 2>nul
)
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0
