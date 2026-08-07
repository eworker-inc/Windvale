@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Console-Container-Hostile-Inputs.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "TemporaryDirectory=%TEMP%\windvale-console-container-hostile-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "CorpusDirectory=%TemporaryDirectory%\Corpus"
mkdir "%CorpusDirectory%" || exit /b 1

set "Archive=%TemporaryDirectory%\Corpus.tar.gz"
set "Manifest=%CorpusDirectory%\Manifest.txt"
set "Sentinel=%TemporaryDirectory%\Sentinel.bin"
set "Destination=%TemporaryDirectory%\Destination.bin"
set "RunOutput=%TemporaryDirectory%\Run.out"
set "RunError=%TemporaryDirectory%\Run.err"
set "DecodeOutput=%TemporaryDirectory%\Decode.out"
set "DecodeError=%TemporaryDirectory%\Decode.err"
set "ExtractOutput=%TemporaryDirectory%\Extract.out"
set "ExtractError=%TemporaryDirectory%\Extract.err"
set "ArchiveDigest=2aa0a153aaf1c70fe650f99e302ebd2aaa9908228175e0f0bebdd9894a872112"
set "ManifestDigest=94f2fb533dabaa57a54c331458ac0f0b478476e2923263840eff85dbd19dd8db"
set "SentinelDigest=0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5"
set "ReportDigest=39db034713225109f62c272db447d75cfe93ff0c259c8d9e5211f0df5c007e1f"
set /a Total=0
set /a Passed=0
set /a WindowsCases=0
set /a LinuxCases=0

call :decode_fixture "Tests\Native\Console-Container-Hostile-Inputs\Corpus.tar.gz.b64" "%Archive%" "%ArchiveDigest%" "hostile-input archive"
if errorlevel 1 goto :failed
call :decode_fixture "Tests\Native\Wvo\Return-42.wvo.b64" "%Sentinel%" "%SentinelDigest%" "destination sentinel"
if errorlevel 1 goto :failed

tar -xzf "%Archive%" -C "%CorpusDirectory%" > "%ExtractOutput%" 2> "%ExtractError%"
if errorlevel 1 (
    >&2 echo FAIL  console-container-hostile: hostile-input archive could not be extracted
    type "%ExtractError%" >&2
    goto :failed
)
for %%S in ("%ExtractOutput%" "%ExtractError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  console-container-hostile: hostile-input extractor wrote output
    type "%%~fS" >&2
    goto :failed
)
call :check_hash "%Manifest%" "%ManifestDigest%" "hostile-input manifest identity differs"
if errorlevel 1 goto :failed
set "Header="
for /f "usebackq delims=" %%H in ("%Manifest%") do if not defined Header set "Header=%%H"
if not "%Header%"=="windvale-console-container-hostile-corpus 1" (
    >&2 echo FAIL  console-container-hostile: hostile-input manifest header differs
    goto :failed
)

for /f "usebackq skip=1 tokens=1-4 delims=|" %%A in ("%Manifest%") do (
    call :run_case "%%A" "%%B" "%%C" "%%D"
    if errorlevel 1 goto :failed
)
if not "%Total%"=="256" (
    >&2 echo FAIL  console-container-hostile: hostile-input case count differs
    goto :failed
)
if not "%WindowsCases%"=="128" (
    >&2 echo FAIL  console-container-hostile: Windows case count differs
    goto :failed
)
if not "%LinuxCases%"=="128" (
    >&2 echo FAIL  console-container-hostile: Linux case count differs
    goto :failed
)

call :cleanup
echo Tests: %Total%, Passed: %Passed%, Failed: 0
exit /b 0

:run_case
for %%N in ("%~1") do if not "%%~nxN"=="%~1" (
    >&2 echo FAIL  %~1: hostile input name is not a filename
    exit /b 1
)
if /I "%~2"=="windows-x64-console-v1" (
    if /I not "%~x1"==".exe" (
        >&2 echo FAIL  %~1: Windows hostile input suffix differs
        exit /b 1
    )
    set /a WindowsCases+=1
) else if /I "%~2"=="linux-x64-console-v1" (
    if /I not "%~x1"==".elf" (
        >&2 echo FAIL  %~1: Linux hostile input suffix differs
        exit /b 1
    )
    set /a LinuxCases+=1
) else (
    >&2 echo FAIL  %~1: hostile input target differs
    exit /b 1
)
set "Input=%CorpusDirectory%\%~1"
set "Destination=%TemporaryDirectory%\Destination%~x1"
set /a Total+=1
for %%S in ("%Input%") do if not "%%~zS"=="%~3" (
    >&2 echo FAIL  %~1: hostile input size differs
    exit /b 1
)
call :check_hash "%Input%" "%~4" "hostile input identity differs"
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
call :check_hash "%Input%" "%~4" "native console publisher changed the hostile input"
if errorlevel 1 exit /b 1
for /f "usebackq delims=" %%S in (`dir /b /a "%TemporaryDirectory%\.wvpublish-*" 2^>nul`) do (
    >&2 echo FAIL  %~1: rejected publication left scratch
    exit /b 1
)
del /f /q "%Destination%" "%RunOutput%" "%RunError%" >nul 2>nul
set /a Passed+=1
echo PASS  %~1
exit /b 0

:decode_fixture
certutil -f -decode "%RepositoryRoot%\%~1" "%~2" > "%DecodeOutput%" 2> "%DecodeError%"
if errorlevel 1 (
    >&2 echo FAIL  console-container-hostile: %~4 could not be decoded
    exit /b 1
)
for %%S in ("%DecodeError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  console-container-hostile: %~4 decoder wrote a diagnostic
    type "%DecodeError%" >&2
    exit /b 1
)
call :check_hash "%~2" "%~3" "%~4 identity differs"
exit /b %ERRORLEVEL%

:check_hash
for %%S in ("%~1") do if "%%~zS"=="0" (
    if /I "%~2"=="e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855" exit /b 0
    >&2 echo FAIL  console-container-hostile: %~3
    exit /b 1
)
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~2" >nul
if errorlevel 1 (
    >&2 echo FAIL  console-container-hostile: %~3
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
    if exist "%CorpusDirectory%\Windows-00%%I.exe" del /f /q "%CorpusDirectory%\Windows-00%%I.exe" >nul 2>nul
    if exist "%CorpusDirectory%\Linux-00%%I.elf" del /f /q "%CorpusDirectory%\Linux-00%%I.elf" >nul 2>nul
)
for /l %%I in (10,1,99) do (
    if exist "%CorpusDirectory%\Windows-0%%I.exe" del /f /q "%CorpusDirectory%\Windows-0%%I.exe" >nul 2>nul
    if exist "%CorpusDirectory%\Linux-0%%I.elf" del /f /q "%CorpusDirectory%\Linux-0%%I.elf" >nul 2>nul
)
for /l %%I in (100,1,127) do (
    if exist "%CorpusDirectory%\Windows-%%I.exe" del /f /q "%CorpusDirectory%\Windows-%%I.exe" >nul 2>nul
    if exist "%CorpusDirectory%\Linux-%%I.elf" del /f /q "%CorpusDirectory%\Linux-%%I.elf" >nul 2>nul
)
if exist "%Manifest%" del /f /q "%Manifest%" >nul 2>nul
rmdir "%CorpusDirectory%" >nul 2>nul
for %%F in (Corpus.tar.gz Sentinel.bin Destination.exe Destination.elf Run.out Run.err Decode.out Decode.err Extract.out Extract.err) do (
    if exist "%TemporaryDirectory%\%%F" del /f /q "%TemporaryDirectory%\%%F" >nul 2>nul
)
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0
