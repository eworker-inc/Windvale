@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Console-Application-Segmented-Construction.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "TemporaryDirectory=%TEMP%\windvale-console-segmented-construction-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "CorpusDirectory=%TemporaryDirectory%\Corpus"
mkdir "%CorpusDirectory%" || exit /b 1

set "Archive=%TemporaryDirectory%\Corpus.tar.gz"
set "Manifest=%CorpusDirectory%\Manifest.txt"
set "NativeImage=%CorpusDirectory%\Maximum-Native.bin"
set "RunOutput=%TemporaryDirectory%\Run.out"
set "RunError=%TemporaryDirectory%\Run.err"
set "VerifyOutput=%TemporaryDirectory%\Verify.out"
set "VerifyError=%TemporaryDirectory%\Verify.err"
set "DecodeOutput=%TemporaryDirectory%\Decode.out"
set "DecodeError=%TemporaryDirectory%\Decode.err"
set "ExtractOutput=%TemporaryDirectory%\Extract.out"
set "ExtractError=%TemporaryDirectory%\Extract.err"
set "ArchiveDigest=3363b3edc5c05f6665566f236793761cf9f7dd03aacfb29334f1535bcfcba7c9"
set "ManifestDigest=27cd7d83d6c44a5b53c26c6b732523a46036a76e1be78f6b0ae590d6f873b005"
set "NativeDigest=25711ae262e606e61654606b563aa7cdc93bb5288558bba0b3e533ab6eab238c"
set /a Total=0
set /a Passed=0

certutil -f -decode "%RepositoryRoot%\Tests\Native\Console-Application-Segmented-Construction\Corpus.tar.gz.b64" "%Archive%" > "%DecodeOutput%" 2> "%DecodeError%"
if errorlevel 1 goto :decode_failed
for %%S in ("%DecodeError%") do if not "%%~zS"=="0" goto :decode_diagnostic
call :check_hash "%Archive%" "%ArchiveDigest%" "archive identity differs"
if errorlevel 1 goto :failed

tar -xzf "%Archive%" -C "%CorpusDirectory%" > "%ExtractOutput%" 2> "%ExtractError%"
if errorlevel 1 goto :extract_failed
for %%S in ("%ExtractOutput%" "%ExtractError%") do if not "%%~zS"=="0" goto :extract_diagnostic
call :check_hash "%Manifest%" "%ManifestDigest%" "manifest identity differs"
if errorlevel 1 goto :failed
call :check_hash "%NativeImage%" "%NativeDigest%" "maximum native-image identity differs"
if errorlevel 1 goto :failed
for %%S in ("%NativeImage%") do if not "%%~zS"=="4194304" goto :native_size_failed
set "Header="
for /f "usebackq delims=" %%H in ("%Manifest%") do if not defined Header set "Header=%%H"
if not "%Header%"=="windvale-console-application-segmented-construction 1" goto :manifest_failed

set "Case=windows-maximum"
set "Target=windows-x64-console-v1"
set "ApplicationBytes=4196352"
set "ApplicationDigest=9cf6ab6650778969c97fad9e149a58d19de8334b806a6375ccc7150c3ad7091c"
set "FirstBytes=4194304"
set "FirstDigest=355595cad76cd8bf27cb4e8a0435ff85dadf3aa6a7afd642a2a9ca992de5522c"
set "SecondBytes=2048"
set "SecondDigest=2a34c2aac9cafc66984ca2407a4ad46652dd0a123f3cc6e28b609e0ea05c56f3"
set "StagingDigest=18f9b4cab9be796da23c9b686e139f031a7ebc51a44ca299cbb0f7ec09c55a26"
set "PackageReportDigest=53f0150046c8049298d59c3929a9015607e9c001d93f574fa647aa608b22c421"
set "VerifyReportDigest=3e771b72b5431a75e3f13de2504b91d48e7280ded0e8bbe601a13b0746ef2dd1"
call :run_case
if errorlevel 1 goto :failed

set "Case=linux-maximum"
set "Target=linux-x64-console-v1"
set "ApplicationBytes=4202608"
set "ApplicationDigest=7b5eb125ce971b53071be80c3424a34436d082b806918fd06690b32e86e87d3a"
set "FirstBytes=4194304"
set "FirstDigest=ad83d04b438b4acfea880214a031a78490d0c06da67e72a64fea8105b03a3234"
set "SecondBytes=8304"
set "SecondDigest=df08c9de1b2c12007861f7cddc1e5d28a02b188c6cf41a15dab77f3b25dd780b"
set "StagingDigest=632f4cfcc240c19f5009385eaf1bfe8e66c1f648c2302c5ab25335f8331c0aeb"
set "PackageReportDigest=868efd57fde343176900f1de742f5f7de6da8d3690b5511ba815937fa4ab9532"
set "VerifyReportDigest=03a0fd7f95baf46d78590ce3888cc29e80787f77553e00e790ac77bf8dafdd15"
call :run_case
if errorlevel 1 goto :failed

if not "%Total%"=="2" goto :count_failed
call :cleanup
echo Tests: %Total%, Passed: %Passed%, Failed: 0
exit /b 0

:run_case
set /a Total+=1
set "ChunkPrefix=%TemporaryDirectory%\%Case%"
set "First=%ChunkPrefix%.chunk-0"
set "Second=%ChunkPrefix%.chunk-1"
set "Staging=%TemporaryDirectory%\%Case%.wvcs"
set "Joined=%TemporaryDirectory%\%Case%.application"
call "%RepositoryRoot%\Tools\Native\Stage-Console-Segmented.cmd" "%Target%" "%NativeImage%" "4194303" "%ChunkPrefix%" "%Staging%" > "%RunOutput%" 2> "%RunError%"
if not "%ERRORLEVEL%"=="0" (
    >&2 echo FAIL  %Case%: segmented constructor exit differs
    type "%RunError%" >&2
    exit /b 1
)
for %%S in ("%RunError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  %Case%: segmented constructor wrote a diagnostic
    type "%RunError%" >&2
    exit /b 1
)
call :check_hash "%RunOutput%" "%PackageReportDigest%" "%Case% package report differs"
if errorlevel 1 exit /b 1
for %%S in ("%First%") do if not "%%~zS"=="%FirstBytes%" exit /b 1
for %%S in ("%Second%") do if not "%%~zS"=="%SecondBytes%" exit /b 1
for %%S in ("%Staging%") do if not "%%~zS"=="60" exit /b 1
call :check_hash "%First%" "%FirstDigest%" "%Case% first chunk differs"
if errorlevel 1 exit /b 1
call :check_hash "%Second%" "%SecondDigest%" "%Case% second chunk differs"
if errorlevel 1 exit /b 1
call :check_hash "%Staging%" "%StagingDigest%" "%Case% staging manifest differs"
if errorlevel 1 exit /b 1
copy /b "%First%"+"%Second%" "%Joined%" >nul
if errorlevel 1 exit /b 1
for %%S in ("%Joined%") do if not "%%~zS"=="%ApplicationBytes%" exit /b 1
call :check_hash "%Joined%" "%ApplicationDigest%" "%Case% Stage 0 application identity differs"
if errorlevel 1 exit /b 1

call "%RepositoryRoot%\Tools\Native\Verify-Console-Segmented.cmd" "%First%" "%Second%" > "%VerifyOutput%" 2> "%VerifyError%"
if not "%ERRORLEVEL%"=="0" (
    >&2 echo FAIL  %Case%: segmented verification exit differs
    type "%VerifyError%" >&2
    exit /b 1
)
for %%S in ("%VerifyError%") do if not "%%~zS"=="0" exit /b 1
call :check_hash "%VerifyOutput%" "%VerifyReportDigest%" "%Case% verification report differs"
if errorlevel 1 exit /b 1
call :check_hash "%NativeImage%" "%NativeDigest%" "%Case% changed the native image"
if errorlevel 1 exit /b 1
set /a Passed+=1
echo PASS  %Case%
del /f /q "%First%" "%Second%" "%Staging%" "%Joined%" "%RunOutput%" "%RunError%" "%VerifyOutput%" "%VerifyError%" >nul 2>nul
exit /b 0

:check_hash
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~2" >nul
if errorlevel 1 (
    >&2 echo FAIL  console-segmented-construction: %~3
    >&2 echo Expected SHA-256: %~2
    certutil -hashfile "%~1" SHA256 >&2
    exit /b 1
)
exit /b 0

:decode_failed
>&2 echo FAIL  console-segmented-construction: corpus could not be decoded
goto :failed
:decode_diagnostic
>&2 echo FAIL  console-segmented-construction: decoder wrote a diagnostic
type "%DecodeError%" >&2
goto :failed
:extract_failed
>&2 echo FAIL  console-segmented-construction: archive could not be extracted
type "%ExtractError%" >&2
goto :failed
:extract_diagnostic
>&2 echo FAIL  console-segmented-construction: extractor wrote output
type "%ExtractOutput%" >&2
type "%ExtractError%" >&2
goto :failed
:manifest_failed
>&2 echo FAIL  console-segmented-construction: manifest header differs
goto :failed
:native_size_failed
>&2 echo FAIL  console-segmented-construction: maximum native-image size differs
goto :failed
:count_failed
>&2 echo FAIL  console-segmented-construction: total case count differs
goto :failed
:failed
set /a Failed=Total-Passed
call :cleanup
>&2 echo Tests: %Total%, Passed: %Passed%, Failed: %Failed%
exit /b 1

:cleanup
for %%F in (Manifest.txt Maximum-Native.bin) do if exist "%CorpusDirectory%\%%F" del /f /q "%CorpusDirectory%\%%F" >nul 2>nul
rmdir "%CorpusDirectory%" >nul 2>nul
for %%F in (Corpus.tar.gz Run.out Run.err Verify.out Verify.err Decode.out Decode.err Extract.out Extract.err windows-maximum.chunk-0 windows-maximum.chunk-1 windows-maximum.wvcs windows-maximum.application linux-maximum.chunk-0 linux-maximum.chunk-1 linux-maximum.wvcs linux-maximum.application) do if exist "%TemporaryDirectory%\%%F" del /f /q "%TemporaryDirectory%\%%F" >nul 2>nul
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0
