@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "Mode=all"
if "%~1"=="" goto :arguments_ready
if "%~1"=="--positive-only" if "%~2"=="" (
    set "Mode=positive"
    goto :arguments_ready
)
>&2 echo Usage: Tools\Native\Test-Wva-Differential.cmd [--positive-only]
exit /b 64

:arguments_ready

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "TemporaryDirectory=%TEMP%\windvale-wva-differential-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "CorpusDirectory=%TemporaryDirectory%\Corpus"
mkdir "%CorpusDirectory%" || exit /b 1
set "PositiveDirectory=%TemporaryDirectory%\Positive"
mkdir "%PositiveDirectory%" || exit /b 1

set "Archive=%TemporaryDirectory%\Corpus.tar.gz"
set "Manifest=%CorpusDirectory%\Manifest.txt"
set "PositiveArchive=%TemporaryDirectory%\Positive-Corpus.tar.gz"
set "PositiveManifest=%PositiveDirectory%\Manifest.txt"
set "Destination=%TemporaryDirectory%\Destination.wvo"
set "Sentinel=%TemporaryDirectory%\Sentinel.wvo"
set "RunOutput=%TemporaryDirectory%\Run.out"
set "RunError=%TemporaryDirectory%\Run.err"
set "VerifyOutput=%TemporaryDirectory%\Verify.out"
set "VerifyError=%TemporaryDirectory%\Verify.err"
set "DecodeOutput=%TemporaryDirectory%\Decode.out"
set "DecodeError=%TemporaryDirectory%\Decode.err"
set "ExtractOutput=%TemporaryDirectory%\Extract.out"
set "ExtractError=%TemporaryDirectory%\Extract.err"
set "ArchiveDigest=b9a076cf9416488d733ed4c4887c052e61548acb45574256cd3c65d94da31970"
set "ManifestDigest=50153c0f7a6e9b596f3a7e0c4ce5bc1c6f240b01ce8657d99c5775a61d9391e4"
set "PositiveArchiveDigest=ebb9e8e4ae5d90ace39f828996ebab9b75fc66d78c62ac7c58e86cf05ba9ba00"
set "PositiveManifestDigest=81172a33451d422ccc1e6c2a418041d6fc6436ad801d15f1adda45afe685ce28"
set "SentinelDigest=0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5"
set "AssemblyReportDigest=4713cc6a74e88cab45421a8bed22b4c72de19fb330f77212a8193aa0e1224c73"
set "VerifyReportDigest=4a31e8a0ea20ff90039366745ec6df8ce8abe87361395c0643c95b72a054e4e7"
set /a Total=0
set /a Passed=0
set /a AcceptedCases=0
set /a RejectedCases=0
set /a Assignment1=0
set /a Assignment2=0
set /a Assignment3=0
set /a Assignment4=0

call :decode_fixture "Tests\Native\Wvo\Return-42.wvo.b64" "%Sentinel%" "%SentinelDigest%"
if errorlevel 1 goto :failed
if "%Mode%"=="positive" goto :positive_corpus

call :decode_fixture "Tests\Native\Wva-Differential\Corpus.tar.gz.b64" "%Archive%" "%ArchiveDigest%"
if errorlevel 1 goto :failed
tar -xzf "%Archive%" -C "%CorpusDirectory%" > "%ExtractOutput%" 2> "%ExtractError%"
if errorlevel 1 (
    >&2 echo FAIL  wva-differential: corpus archive could not be extracted
    type "%ExtractError%" >&2
    goto :failed
)
for %%S in ("%ExtractOutput%" "%ExtractError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  wva-differential: corpus extractor wrote output
    type "%%~fS" >&2
    goto :failed
)
call :check_hash "%Manifest%" "%ManifestDigest%" "manifest identity differs"
if errorlevel 1 goto :failed
set "Header="
for /f "usebackq delims=" %%H in ("%Manifest%") do if not defined Header set "Header=%%H"
if not "%Header%"=="windvale-wva-differential-corpus 1" (
    >&2 echo FAIL  wva-differential: manifest header differs
    goto :failed
)

set "ActiveCorpusDirectory=%CorpusDirectory%"
for /f "usebackq skip=2 tokens=1-15 delims=|" %%A in ("%Manifest%") do (
    call :record_assignment "%%C"
    if errorlevel 1 goto :failed
    call :run_case "%%A" "%%E" "%%F" "%%G" "%%H" "%%K" "%%L" "%AssemblyReportDigest%" "%VerifyReportDigest%"
    if errorlevel 1 goto :failed
)
if not "%Total%"=="200" (
    >&2 echo FAIL  wva-differential: total case count differs
    goto :failed
)
if not "%AcceptedCases%"=="1" (
    >&2 echo FAIL  wva-differential: accepted case count differs
    goto :failed
)
if not "%RejectedCases%"=="199" (
    >&2 echo FAIL  wva-differential: rejected case count differs
    goto :failed
)
if not "%Assignment1%"=="58" (
    >&2 echo FAIL  wva-differential: one-assignment case count differs
    goto :failed
)
if not "%Assignment2%"=="45" (
    >&2 echo FAIL  wva-differential: two-assignment case count differs
    goto :failed
)
if not "%Assignment3%"=="50" (
    >&2 echo FAIL  wva-differential: three-assignment case count differs
    goto :failed
)
if not "%Assignment4%"=="47" (
    >&2 echo FAIL  wva-differential: four-assignment case count differs
    goto :failed
)

:positive_corpus
call :decode_fixture "Tests\Native\Wva-Differential\Positive-Corpus.tar.gz.b64" "%PositiveArchive%" "%PositiveArchiveDigest%"
if errorlevel 1 goto :failed
tar -xzf "%PositiveArchive%" -C "%PositiveDirectory%" > "%ExtractOutput%" 2> "%ExtractError%"
if errorlevel 1 (
    >&2 echo FAIL  wva-differential: positive corpus archive could not be extracted
    type "%ExtractError%" >&2
    goto :failed
)
for %%S in ("%ExtractOutput%" "%ExtractError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  wva-differential: positive corpus extractor wrote output
    type "%%~fS" >&2
    goto :failed
)
call :check_hash "%PositiveManifest%" "%PositiveManifestDigest%" "positive manifest identity differs"
if errorlevel 1 goto :failed
set "PositiveHeader="
for /f "usebackq delims=" %%H in ("%PositiveManifest%") do if not defined PositiveHeader set "PositiveHeader=%%H"
if not "%PositiveHeader%"=="windvale-wva-positive-corpus 1" (
    >&2 echo FAIL  wva-differential: positive manifest header differs
    goto :failed
)

set "ActiveCorpusDirectory=%PositiveDirectory%"
for /f "usebackq skip=2 tokens=1-10 delims=|" %%A in ("%PositiveManifest%") do (
    call :run_case "%%A" "%%B" "%%C" "accepted" "-" "%%D" "%%E" "%%I" "%%J"
    if errorlevel 1 goto :failed
)
if "%Mode%"=="positive" (
    if not "%Total%"=="17" goto :positive_count_failed
    if not "%AcceptedCases%"=="17" goto :positive_count_failed
    if not "%RejectedCases%"=="0" goto :positive_count_failed
) else (
    if not "%Total%"=="217" goto :positive_count_failed
    if not "%AcceptedCases%"=="18" goto :positive_count_failed
    if not "%RejectedCases%"=="199" goto :positive_count_failed
)

call :cleanup
echo Tests: %Total%, Passed: %Passed%, Failed: 0
exit /b 0

:positive_count_failed
>&2 echo FAIL  wva-differential: positive case counts differ
goto :failed

:record_assignment
if "%~1"=="1" (
    set /a Assignment1+=1
    exit /b 0
)
if "%~1"=="2" (
    set /a Assignment2+=1
    exit /b 0
)
if "%~1"=="3" (
    set /a Assignment3+=1
    exit /b 0
)
if "%~1"=="4" (
    set /a Assignment4+=1
    exit /b 0
)
>&2 echo FAIL  wva-differential: assignment count differs
exit /b 1

:run_case
for %%N in ("%~1") do if not "%%~nxN"=="%~1" (
    >&2 echo FAIL  %~1: corpus input name is not a filename
    exit /b 1
)
set "Input=%ActiveCorpusDirectory%\%~1"
set /a Total+=1
for %%S in ("%Input%") do if not "%%~zS"=="%~2" (
    >&2 echo FAIL  %~1: input size differs
    exit /b 1
)
call :check_hash "%Input%" "%~3" "input identity differs"
if errorlevel 1 exit /b 1
copy /y "%Sentinel%" "%Destination%" >nul
if errorlevel 1 (
    >&2 echo FAIL  %~1: destination could not be created
    exit /b 1
)

call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" "%Input%" "%Destination%" > "%RunOutput%" 2> "%RunError%"
set "RunExit=%ERRORLEVEL%"
if /I "%~4"=="accepted" (
    if not "%RunExit%"=="0" (
        >&2 echo FAIL  %~1: native assembler rejected an oracle-accepted input
        exit /b 1
    )
    for %%S in ("%RunError%") do if not "%%~zS"=="0" (
        >&2 echo FAIL  %~1: accepted input wrote a diagnostic
        type "%RunError%" >&2
        exit /b 1
    )
    call :check_hash "%RunOutput%" "%~8" "accepted report differs"
    if errorlevel 1 exit /b 1
    for %%S in ("%Destination%") do if not "%%~zS"=="%~6" (
        >&2 echo FAIL  %~1: accepted object size differs
        exit /b 1
    )
    call :check_hash "%Destination%" "%~7" "accepted object identity differs"
    if errorlevel 1 exit /b 1
    call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%Destination%" > "%VerifyOutput%" 2> "%VerifyError%"
    if errorlevel 1 (
        >&2 echo FAIL  %~1: accepted object failed native verification
        type "%VerifyError%" >&2
        exit /b 1
    )
    for %%S in ("%VerifyError%") do if not "%%~zS"=="0" (
        >&2 echo FAIL  %~1: native object verification wrote a diagnostic
        type "%VerifyError%" >&2
        exit /b 1
    )
    call :check_hash "%VerifyOutput%" "%~9" "native object verification report differs"
    if errorlevel 1 exit /b 1
    set /a AcceptedCases+=1
) else if /I "%~4"=="rejected" (
    if not "%RunExit%"=="2" (
        >&2 echo FAIL  %~1: native assembler accepted an oracle-rejected input
        exit /b 1
    )
    for %%S in ("%RunOutput%") do if not "%%~zS"=="0" (
        >&2 echo FAIL  %~1: rejected input wrote standard output
        type "%RunOutput%" >&2
        exit /b 1
    )
    call :check_rejection_report "%RunError%" "%~5"
    if errorlevel 1 exit /b 1
    call :check_hash "%Destination%" "%SentinelDigest%" "rejected assembly changed the destination"
    if errorlevel 1 exit /b 1
    set /a RejectedCases+=1
) else (
    >&2 echo FAIL  %~1: oracle outcome differs
    exit /b 1
)
call :check_hash "%Input%" "%~3" "native assembler changed the input"
if errorlevel 1 exit /b 1
del /f /q "%Destination%" "%RunOutput%" "%RunError%" "%VerifyOutput%" "%VerifyError%" >nul 2>nul
set /a Passed+=1
echo PASS  %~1 oracle=%~4
exit /b 0

:check_rejection_report
set "ReportLine="
set /a ReportLines=0
for /f "usebackq delims=" %%L in ("%~1") do call :record_report_line "%%L"
if not "%ReportLines%"=="1" (
    >&2 echo FAIL  wva-differential: rejected report line count differs
    exit /b 1
)
echo(%ReportLine%| findstr /B /L /C:"assembly status=%~2 " >nul
if errorlevel 1 (
    >&2 echo FAIL  wva-differential: rejected report code differs
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
    >&2 echo FAIL  wva-differential: fixture could not be decoded
    exit /b 1
)
for %%S in ("%DecodeError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  wva-differential: decoder wrote a diagnostic
    type "%DecodeError%" >&2
    exit /b 1
)
call :check_hash "%~2" "%~3" "decoded fixture identity differs"
exit /b %ERRORLEVEL%

:check_hash
for %%S in ("%~1") do if "%%~zS"=="0" (
    if /I "%~2"=="e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855" exit /b 0
    >&2 echo FAIL  wva-differential: %~3
    exit /b 1
)
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~2" >nul
if errorlevel 1 (
    >&2 echo FAIL  wva-differential: %~3
    exit /b 1
)
exit /b 0

:failed
set /a Failed=Total-Passed
call :cleanup
>&2 echo Tests: %Total%, Passed: %Passed%, Failed: %Failed%
exit /b 1

:cleanup
for /l %%I in (0,1,9) do if exist "%CorpusDirectory%\Case-00%%I.wva" del /f /q "%CorpusDirectory%\Case-00%%I.wva" >nul 2>nul
for /l %%I in (10,1,99) do if exist "%CorpusDirectory%\Case-0%%I.wva" del /f /q "%CorpusDirectory%\Case-0%%I.wva" >nul 2>nul
for /l %%I in (100,1,199) do if exist "%CorpusDirectory%\Case-%%I.wva" del /f /q "%CorpusDirectory%\Case-%%I.wva" >nul 2>nul
if exist "%Manifest%" del /f /q "%Manifest%" >nul 2>nul
rmdir "%CorpusDirectory%" >nul 2>nul
if exist "%PositiveDirectory%\*.wva" del /f /q "%PositiveDirectory%\*.wva" >nul 2>nul
if exist "%PositiveManifest%" del /f /q "%PositiveManifest%" >nul 2>nul
rmdir "%PositiveDirectory%" >nul 2>nul
for %%F in (Corpus.tar.gz Positive-Corpus.tar.gz Destination.wvo Sentinel.wvo Run.out Run.err Verify.out Verify.err Decode.out Decode.err Extract.out Extract.err) do (
    if exist "%TemporaryDirectory%\%%F" del /f /q "%TemporaryDirectory%\%%F" >nul 2>nul
)
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0
