@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Console-Container-Mutations.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "TemporaryDirectory=%TEMP%\windvale-console-container-mutations-%RANDOM%-%RANDOM%-%RANDOM%"
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
set "ArchiveDigest=63b7d5187aa0f5407aa5a68be851c03fb0b64991c418f8c2407548f0ad6c89c9"
set "ManifestDigest=35794ce75d80a06b099f705a8c0fce91295a5d627cee2a76803617f372e13669"
set "SentinelDigest=0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5"
set "ReportDigest=39db034713225109f62c272db447d75cfe93ff0c259c8d9e5211f0df5c007e1f"
set /a Total=0
set /a Passed=0
set /a WindowsCases=0
set /a LinuxCases=0
set /a TruncateCases=0
set /a XorCases=0
set /a AppendCases=0
for %%C in (Wvw2001 Wvw2002 Wvw2003 Wvw2004 Wvw2005 Wvw2006 Wvw2007 Wvw2008 Wvw2009 Wvl2001 Wvl2002 Wvl2003 Wvl2004 Wvl2005 Wvl2006 Wvl2007 Wvl2008) do set /a %%C=0

call :decode_fixture "Tests\Native\Console-Container-Mutations\Corpus.tar.gz.b64" "%Archive%" "%ArchiveDigest%" "mutation archive"
if errorlevel 1 goto :failed
call :decode_fixture "Tests\Native\Wvo\Return-42.wvo.b64" "%Sentinel%" "%SentinelDigest%" "destination sentinel"
if errorlevel 1 goto :failed

tar -xzf "%Archive%" -C "%CorpusDirectory%" > "%ExtractOutput%" 2> "%ExtractError%"
if errorlevel 1 (
    >&2 echo FAIL  console-container-mutations: mutation archive could not be extracted
    type "%ExtractError%" >&2
    goto :failed
)
for %%S in ("%ExtractOutput%" "%ExtractError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  console-container-mutations: mutation extractor wrote output
    type "%%~fS" >&2
    goto :failed
)
call :check_hash "%Manifest%" "%ManifestDigest%" "mutation manifest identity differs"
if errorlevel 1 goto :failed
set "Header="
for /f "usebackq delims=" %%H in ("%Manifest%") do if not defined Header set "Header=%%H"
if not "%Header%"=="windvale-console-container-mutation-corpus 1" (
    >&2 echo FAIL  console-container-mutations: mutation manifest header differs
    goto :failed
)

for /f "usebackq skip=1 tokens=1-7 delims=|" %%A in ("%Manifest%") do (
    call :run_case "%%A" "%%B" "%%C" "%%D" "%%E" "%%F" "%%G"
    if errorlevel 1 goto :failed
)
if not "%Total%"=="19" call :fail_count "total case count differs"
if errorlevel 1 goto :failed
if not "%WindowsCases%"=="10" call :fail_count "Windows case count differs"
if errorlevel 1 goto :failed
if not "%LinuxCases%"=="9" call :fail_count "Linux case count differs"
if errorlevel 1 goto :failed
if not "%TruncateCases%"=="2" call :fail_count "truncation case count differs"
if errorlevel 1 goto :failed
if not "%XorCases%"=="15" call :fail_count "one-byte mutation case count differs"
if errorlevel 1 goto :failed
if not "%AppendCases%"=="2" call :fail_count "trailing-byte case count differs"
if errorlevel 1 goto :failed
if not "%Wvw2001%"=="2" call :fail_count "WVW2001 case count differs"
if errorlevel 1 goto :failed
for %%C in (Wvw2002 Wvw2003 Wvw2004 Wvw2005 Wvw2006 Wvw2007 Wvw2008 Wvw2009) do (
    call :require_one %%C
    if errorlevel 1 goto :failed
)
if not "%Wvl2001%"=="2" call :fail_count "WVL2001 case count differs"
if errorlevel 1 goto :failed
for %%C in (Wvl2002 Wvl2003 Wvl2004 Wvl2005 Wvl2006 Wvl2007 Wvl2008) do (
    call :require_one %%C
    if errorlevel 1 goto :failed
)

call :cleanup
echo Tests: %Total%, Passed: %Passed%, Failed: 0
exit /b 0

:run_case
for %%N in ("%~1") do if not "%%~nxN"=="%~1" (
    >&2 echo FAIL  %~1: mutation input name is not a filename
    exit /b 1
)
if /I "%~2"=="windows-x64-console-v1" (
    if /I not "%~x1"==".exe" (
        >&2 echo FAIL  %~1: Windows mutation suffix differs
        exit /b 1
    )
    set /a WindowsCases+=1
    set /a BaseBytes=5120
) else if /I "%~2"=="linux-x64-console-v1" (
    if /I not "%~x1"==".elf" (
        >&2 echo FAIL  %~1: Linux mutation suffix differs
        exit /b 1
    )
    set /a LinuxCases+=1
    set /a BaseBytes=8304
) else (
    >&2 echo FAIL  %~1: mutation target differs
    exit /b 1
)
if /I "%~3"=="truncate-last" (
    set /a TruncateCases+=1
    set /a ExpectedBytes=BaseBytes-1
) else if /I "%~3"=="xor-one" (
    set /a XorCases+=1
    set /a ExpectedBytes=BaseBytes
    if %~4 LSS 0 (
        >&2 echo FAIL  %~1: mutation offset differs
        exit /b 1
    )
    if %~4 GEQ %BaseBytes% (
        >&2 echo FAIL  %~1: mutation offset differs
        exit /b 1
    )
) else if /I "%~3"=="append-zero" (
    set /a AppendCases+=1
    set /a ExpectedBytes=BaseBytes+1
) else (
    >&2 echo FAIL  %~1: mutation operation differs
    exit /b 1
)
if not "%~6"=="%ExpectedBytes%" (
    >&2 echo FAIL  %~1: operation and size disagree
    exit /b 1
)
if /I not "%~3"=="xor-one" if not "%~4"=="%BaseBytes%" if not "%~4"=="%ExpectedBytes%" (
    >&2 echo FAIL  %~1: boundary operation offset differs
    exit /b 1
)
call :record_code "%~2" "%~5"
if errorlevel 1 exit /b 1

set "Input=%CorpusDirectory%\%~1"
set "Destination=%TemporaryDirectory%\Destination%~x1"
set /a Total+=1
for %%S in ("%Input%") do if not "%%~zS"=="%~6" (
    >&2 echo FAIL  %~1: mutation input size differs
    exit /b 1
)
call :check_hash "%Input%" "%~7" "mutation input identity differs"
if errorlevel 1 exit /b 1
copy /y "%Sentinel%" "%Destination%" >nul
if errorlevel 1 (
    >&2 echo FAIL  %~1: destination sentinel could not be created
    exit /b 1
)
call "%RepositoryRoot%\Tools\Native\Publish-Console.cmd" "%Input%" "%Destination%" > "%RunOutput%" 2> "%RunError%"
set "RunExit=%ERRORLEVEL%"
if not "%RunExit%"=="1" (
    >&2 echo FAIL  %~1: native console publisher exit differs
    exit /b 1
)
for %%S in ("%RunOutput%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  %~1: rejected publication wrote standard output
    type "%RunOutput%" >&2
    exit /b 1
)
call :check_hash "%RunError%" "%ReportDigest%" "native console publisher report differs"
if errorlevel 1 (
    type "%RunError%" >&2
    exit /b 1
)
call :check_hash "%Destination%" "%SentinelDigest%" "rejected publication changed the destination"
if errorlevel 1 exit /b 1
call :check_hash "%Input%" "%~7" "native console publisher changed the mutation input"
if errorlevel 1 exit /b 1
for /f "usebackq delims=" %%S in (`dir /b /a "%TemporaryDirectory%\.wvpublish-*" 2^>nul`) do (
    >&2 echo FAIL  %~1: rejected publication left scratch
    exit /b 1
)
del /f /q "%Destination%" "%RunOutput%" "%RunError%" >nul 2>nul
set /a Passed+=1
echo PASS  %~1 operation=%~3 offset=%~4 oracle=%~5
exit /b 0

:record_code
if /I "%~1"=="windows-x64-console-v1" (
    if /I "%~2"=="WVW2001" (set /a Wvw2001+=1& exit /b 0)
    if /I "%~2"=="WVW2002" (set /a Wvw2002+=1& exit /b 0)
    if /I "%~2"=="WVW2003" (set /a Wvw2003+=1& exit /b 0)
    if /I "%~2"=="WVW2004" (set /a Wvw2004+=1& exit /b 0)
    if /I "%~2"=="WVW2005" (set /a Wvw2005+=1& exit /b 0)
    if /I "%~2"=="WVW2006" (set /a Wvw2006+=1& exit /b 0)
    if /I "%~2"=="WVW2007" (set /a Wvw2007+=1& exit /b 0)
    if /I "%~2"=="WVW2008" (set /a Wvw2008+=1& exit /b 0)
    if /I "%~2"=="WVW2009" (set /a Wvw2009+=1& exit /b 0)
) else (
    if /I "%~2"=="WVL2001" (set /a Wvl2001+=1& exit /b 0)
    if /I "%~2"=="WVL2002" (set /a Wvl2002+=1& exit /b 0)
    if /I "%~2"=="WVL2003" (set /a Wvl2003+=1& exit /b 0)
    if /I "%~2"=="WVL2004" (set /a Wvl2004+=1& exit /b 0)
    if /I "%~2"=="WVL2005" (set /a Wvl2005+=1& exit /b 0)
    if /I "%~2"=="WVL2006" (set /a Wvl2006+=1& exit /b 0)
    if /I "%~2"=="WVL2007" (set /a Wvl2007+=1& exit /b 0)
    if /I "%~2"=="WVL2008" (set /a Wvl2008+=1& exit /b 0)
)
>&2 echo FAIL  console-container-mutations: Stage 0 oracle code differs
exit /b 1

:require_one
call set "CodeCount=%%%~1%%"
if not "%CodeCount%"=="1" (
    >&2 echo FAIL  console-container-mutations: %~1 case count differs
    exit /b 1
)
exit /b 0

:fail_count
>&2 echo FAIL  console-container-mutations: %~1
exit /b 1

:decode_fixture
certutil -f -decode "%RepositoryRoot%\%~1" "%~2" > "%DecodeOutput%" 2> "%DecodeError%"
if errorlevel 1 (
    >&2 echo FAIL  console-container-mutations: %~4 could not be decoded
    exit /b 1
)
for %%S in ("%DecodeError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  console-container-mutations: %~4 decoder wrote a diagnostic
    type "%DecodeError%" >&2
    exit /b 1
)
call :check_hash "%~2" "%~3" "%~4 identity differs"
exit /b %ERRORLEVEL%

:check_hash
for %%S in ("%~1") do if "%%~zS"=="0" (
    if /I "%~2"=="e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855" exit /b 0
    >&2 echo FAIL  console-container-mutations: %~3
    exit /b 1
)
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~2" >nul
if errorlevel 1 (
    >&2 echo FAIL  console-container-mutations: %~3
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
