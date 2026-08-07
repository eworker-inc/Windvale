@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Hosted-Console-Container-Mutations.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "TemporaryDirectory=%TEMP%\windvale-hosted-console-mutations-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "CorpusDirectory=%TemporaryDirectory%\Corpus"
mkdir "%CorpusDirectory%" || exit /b 1

set "Archive=%TemporaryDirectory%\Corpus.tar.gz"
set "Manifest=%CorpusDirectory%\Manifest.txt"
set "Sentinel=%TemporaryDirectory%\Sentinel.bin"
set "RunOutput=%TemporaryDirectory%\Run.out"
set "RunError=%TemporaryDirectory%\Run.err"
set "DecodeOutput=%TemporaryDirectory%\Decode.out"
set "DecodeError=%TemporaryDirectory%\Decode.err"
set "ExtractOutput=%TemporaryDirectory%\Extract.out"
set "ExtractError=%TemporaryDirectory%\Extract.err"
set "ArchiveDigest=a8027a9d4238767ae9b7ab18e3d0114da4e4fdf3edcbbc044d4358f2ce1fd055"
set "ManifestDigest=208a309624bef868b657cc87e2e95d6c085da1528bc5bc471226dc4b22c764f9"
set "SentinelDigest=0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5"
set "RejectedReportDigest=39db034713225109f62c272db447d75cfe93ff0c259c8d9e5211f0df5c007e1f"
set "WindowsValidReportDigest=6eb507dd88b808f1a0b8fdc811da18bcfa2e6c5d18d56f8b1fb7a5cca33bff2d"
set "LinuxValidReportDigest=0e3fc5697dd9f6b882d0d4b7cc8c1d771a65789278a35f28ec7f3e729952f142"
set /a Total=0
set /a Passed=0
set /a WindowsCases=0
set /a LinuxCases=0
set /a ValidCases=0
set /a RejectedCases=0
set /a XorCases=0
set /a RehashCases=0
set /a TruncateCases=0
set /a AppendCases=0

call :decode_fixture "Tests\Native\Hosted-Console-Container-Mutations\Corpus.tar.gz.b64" "%Archive%" "%ArchiveDigest%" "hosted mutation archive"
if errorlevel 1 goto :failed
call :decode_fixture "Tests\Native\Wvo\Return-42.wvo.b64" "%Sentinel%" "%SentinelDigest%" "destination sentinel"
if errorlevel 1 goto :failed

tar -xzf "%Archive%" -C "%CorpusDirectory%" > "%ExtractOutput%" 2> "%ExtractError%"
if errorlevel 1 (
    >&2 echo FAIL  hosted-console-mutations: archive could not be extracted
    type "%ExtractError%" >&2
    goto :failed
)
for %%S in ("%ExtractOutput%" "%ExtractError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  hosted-console-mutations: extractor wrote output
    type "%%~fS" >&2
    goto :failed
)
call :check_hash "%Manifest%" "%ManifestDigest%" "manifest identity differs"
if errorlevel 1 goto :failed
set "Header="
for /f "usebackq delims=" %%H in ("%Manifest%") do if not defined Header set "Header=%%H"
if not "%Header%"=="windvale-hosted-console-container-mutations 1" (
    >&2 echo FAIL  hosted-console-mutations: manifest header differs
    goto :failed
)

for /f "usebackq skip=2 tokens=1-7 delims=|" %%A in ("%Manifest%") do (
    call :run_case "%%A" "%%B" "%%C" "%%D" "%%E" "%%F" "%%G"
    if errorlevel 1 goto :failed
)
if not "%Total%"=="15" call :fail_count "total case count differs"
if errorlevel 1 goto :failed
if not "%WindowsCases%"=="8" call :fail_count "Windows case count differs"
if errorlevel 1 goto :failed
if not "%LinuxCases%"=="7" call :fail_count "Linux case count differs"
if errorlevel 1 goto :failed
if not "%ValidCases%"=="2" call :fail_count "valid base count differs"
if errorlevel 1 goto :failed
if not "%RejectedCases%"=="13" call :fail_count "rejection count differs"
if errorlevel 1 goto :failed
if not "%XorCases%"=="9" call :fail_count "xor count differs"
if errorlevel 1 goto :failed
if not "%RehashCases%"=="2" call :fail_count "rehashed-leaf count differs"
if errorlevel 1 goto :failed
if not "%TruncateCases%"=="2" call :fail_count "truncation count differs"
if errorlevel 1 goto :failed
if not "%AppendCases%"=="2" call :fail_count "trailing-byte count differs"
if errorlevel 1 goto :failed

call :cleanup
echo Tests: %Total%, Passed: %Passed%, Failed: 0
exit /b 0

:run_case
for %%N in ("%~1") do if not "%%~nxN"=="%~1" (
    >&2 echo FAIL  %~1: input name is not a filename
    exit /b 1
)
set "Input=%CorpusDirectory%\%~1"
set "Destination=%TemporaryDirectory%\Destination%~x1"
set "Operation=%~5"
set "ValidReportDigest="
if /I "%~2"=="windows" (
    if /I not "%~x1"==".exe" (
        >&2 echo FAIL  %~1: Windows suffix differs
        exit /b 1
    )
    set /a WindowsCases+=1
    set "ValidReportDigest=%WindowsValidReportDigest%"
) else if /I "%~2"=="linux" (
    if /I not "%~x1"==".elf" (
        >&2 echo FAIL  %~1: Linux suffix differs
        exit /b 1
    )
    set /a LinuxCases+=1
    set "ValidReportDigest=%LinuxValidReportDigest%"
) else (
    >&2 echo FAIL  %~1: platform differs
    exit /b 1
)
if /I "%~3"=="valid" (
    if /I not "%~4"=="Valid" (
        >&2 echo FAIL  %~1: valid Stage 0 result differs
        exit /b 1
    )
    if /I not "%Operation%"=="base" (
        >&2 echo FAIL  %~1: valid operation differs
        exit /b 1
    )
    set /a ValidCases+=1
) else if /I "%~3"=="reject" (
    if /I "%~2"=="windows" if /I not "%~4"=="WVW2100" (
        >&2 echo FAIL  %~1: Windows Stage 0 code differs
        exit /b 1
    )
    if /I "%~2"=="linux" if /I not "%~4"=="WVL2100" (
        >&2 echo FAIL  %~1: Linux Stage 0 code differs
        exit /b 1
    )
    set /a RejectedCases+=1
) else (
    >&2 echo FAIL  %~1: expectation differs
    exit /b 1
)
if /I "%Operation:~0,5%"=="xor1:" set /a XorCases+=1
if /I not "%Operation:rehash=%"=="%Operation%" set /a RehashCases+=1
if /I "%Operation%"=="truncate:500" set /a TruncateCases+=1
if /I "%Operation%"=="append:00" set /a AppendCases+=1

set /a Total+=1
for %%S in ("%Input%") do if not "%%~zS"=="%~6" (
    >&2 echo FAIL  %~1: input size differs
    exit /b 1
)
call :check_hash "%Input%" "%~7" "input identity differs"
if errorlevel 1 exit /b 1
copy /y "%Sentinel%" "%Destination%" >nul
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Publish-Console.cmd" "%Input%" "%Destination%" > "%RunOutput%" 2> "%RunError%"
set "RunExit=%ERRORLEVEL%"
if /I "%~3"=="valid" (
    if not "%RunExit%"=="0" (
        >&2 echo FAIL  %~1: valid publication exit differs
        exit /b 1
    )
    call :check_hash "%RunOutput%" "%ValidReportDigest%" "valid publication report differs"
    if errorlevel 1 exit /b 1
    call :check_empty "%RunError%" "valid publication wrote a diagnostic"
    if errorlevel 1 exit /b 1
    call :check_hash "%Destination%" "%~7" "valid publication destination differs"
    if errorlevel 1 exit /b 1
) else (
    if not "%RunExit%"=="1" (
        >&2 echo FAIL  %~1: rejected publication exit differs
        exit /b 1
    )
    call :check_empty "%RunOutput%" "rejected publication wrote standard output"
    if errorlevel 1 exit /b 1
    call :check_hash "%RunError%" "%RejectedReportDigest%" "rejection report differs"
    if errorlevel 1 exit /b 1
    call :check_hash "%Destination%" "%SentinelDigest%" "rejection changed the destination"
    if errorlevel 1 exit /b 1
)
call :check_hash "%Input%" "%~7" "publisher changed the input"
if errorlevel 1 exit /b 1
for /f "usebackq delims=" %%S in (`dir /b /a "%TemporaryDirectory%\.wvpublish-*" 2^>nul`) do (
    >&2 echo FAIL  %~1: publication left scratch
    exit /b 1
)
del /f /q "%Destination%" "%RunOutput%" "%RunError%" >nul 2>nul
set /a Passed+=1
echo PASS  %~1 expectation=%~3 oracle=%~4 operation=%~5
exit /b 0

:check_empty
for %%S in ("%~1") do if not "%%~zS"=="0" (
    >&2 echo FAIL  hosted-console-mutations: %~2
    type "%~1" >&2
    exit /b 1
)
exit /b 0

:fail_count
>&2 echo FAIL  hosted-console-mutations: %~1
exit /b 1

:decode_fixture
certutil -f -decode "%RepositoryRoot%\%~1" "%~2" > "%DecodeOutput%" 2> "%DecodeError%"
if errorlevel 1 (
    >&2 echo FAIL  hosted-console-mutations: %~4 could not be decoded
    exit /b 1
)
for %%S in ("%DecodeError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  hosted-console-mutations: %~4 decoder wrote a diagnostic
    type "%DecodeError%" >&2
    exit /b 1
)
call :check_hash "%~2" "%~3" "%~4 identity differs"
exit /b %ERRORLEVEL%

:check_hash
for %%S in ("%~1") do if "%%~zS"=="0" (
    if /I "%~2"=="e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855" exit /b 0
    >&2 echo FAIL  hosted-console-mutations: %~3
    exit /b 1
)
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~2" >nul
if errorlevel 1 (
    >&2 echo FAIL  hosted-console-mutations: %~3
    exit /b 1
)
exit /b 0

:failed
set /a Failed=Total-Passed
call :cleanup
>&2 echo Tests: %Total%, Passed: %Passed%, Failed: %Failed%
exit /b 1

:cleanup
if exist "%CorpusDirectory%\*.exe" del /f /q "%CorpusDirectory%\*.exe" >nul 2>nul
if exist "%CorpusDirectory%\*.elf" del /f /q "%CorpusDirectory%\*.elf" >nul 2>nul
if exist "%Manifest%" del /f /q "%Manifest%" >nul 2>nul
rmdir "%CorpusDirectory%" >nul 2>nul
for %%F in (Corpus.tar.gz Sentinel.bin Destination.exe Destination.elf Run.out Run.err Decode.out Decode.err Extract.out Extract.err) do (
    if exist "%TemporaryDirectory%\%%F" del /f /q "%TemporaryDirectory%\%%F" >nul 2>nul
)
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0
