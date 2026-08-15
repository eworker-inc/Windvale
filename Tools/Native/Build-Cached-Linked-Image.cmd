@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~5"=="" goto :usage
if not "%~6"=="" goto :usage
if /I not "%~x3"==".wvo" goto :usage
if /I not "%~x4"==".bin" goto :usage
if /I not "%~x5"==".map" goto :usage

set "BaseAddress=%~1"
set "Entry=%~2"
set "Input=%~f3"
set "OutputImage=%~f4"
set "OutputMap=%~f5"
set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "FrontDoor=%RepositoryRoot%\Tools\Native\Link-Wvo.cmd"
set "Linker=%RepositoryRoot%\Artifacts\Native-Wv-Linker-Candidate\Wv-Linker.exe"
set "KeyTool=%RepositoryRoot%\Tools\Native\Get-Native-Linked-Image-Cache-Key.mjs"

if not exist "%Input%" exit /b 1
if not exist "%FrontDoor%" exit /b 1
if not exist "%Linker%" exit /b 1
if not exist "%KeyTool%" exit /b 1
for %%D in ("%OutputImage%" "%OutputMap%") do if not exist "%%~dpD." exit /b 1

:allocate_key
set "KeyOutput=%TEMP%\windvale-linked-image-cache-key-%RANDOM%-%RANDOM%-%RANDOM%.txt"
if exist "%KeyOutput%" goto :allocate_key
node "%KeyTool%" linked-image-v1 "%BaseAddress%" "%Entry%" "%Input%" ^
    "%FrontDoor%" "%Linker%" >"%KeyOutput%"
if errorlevel 1 goto :key_failed
set "CheckpointKey="
set /p CheckpointKey=<"%KeyOutput%"
del /f /q "%KeyOutput%" >nul 2>nul
echo(%CheckpointKey%| findstr /r /x "[0-9a-f][0-9a-f]*" >nul || exit /b 1
if "%CheckpointKey:~63,1%"=="" exit /b 1
if not "%CheckpointKey:~64,1%"=="" exit /b 1

if defined WINDVALE_NATIVE_CACHE_ROOT (
    set "CheckpointRoot=%WINDVALE_NATIVE_CACHE_ROOT%"
) else (
    if not defined LOCALAPPDATA exit /b 1
    set "CheckpointRoot=%LOCALAPPDATA%\Windvale\Native-Tool-Cache"
)
for %%R in ("%CheckpointRoot%") do set "CheckpointRoot=%%~fR"
if not exist "%CheckpointRoot%\." mkdir "%CheckpointRoot%" || exit /b 1
fsutil reparsepoint query "%CheckpointRoot%" >nul 2>nul
if not errorlevel 1 exit /b 1
set "CheckpointProductRoot=%CheckpointRoot%\linked-image-v1"
if not exist "%CheckpointProductRoot%\." mkdir "%CheckpointProductRoot%" || exit /b 1
fsutil reparsepoint query "%CheckpointProductRoot%" >nul 2>nul
if not errorlevel 1 exit /b 1
set "CheckpointFamily=%CheckpointProductRoot%\windows-x64"
if not exist "%CheckpointFamily%\." mkdir "%CheckpointFamily%" || exit /b 1
fsutil reparsepoint query "%CheckpointFamily%" >nul 2>nul
if not errorlevel 1 exit /b 1

set "CheckpointDirectory=%CheckpointFamily%\%CheckpointKey%"
set "CheckpointManifest=%CheckpointDirectory%\Checkpoint.txt"
set "CheckpointImage=%CheckpointDirectory%\Product.bin"
set "CheckpointMap=%CheckpointDirectory%\Product.map"
set "CheckpointStatus=Hit"
if exist "%CheckpointDirectory%\." goto :validate_checkpoint

:allocate_checkpoint
set "CheckpointTemporary=%CheckpointFamily%\.new-%CheckpointKey%-%RANDOM%-%RANDOM%"
if exist "%CheckpointTemporary%\." goto :allocate_checkpoint
mkdir "%CheckpointTemporary%" || exit /b 1
set "CandidateImage=%CheckpointTemporary%\Product.bin"
set "CandidateMap=%CheckpointTemporary%\Product.map"
call "%FrontDoor%" "%BaseAddress%" "%Entry%" "%CandidateImage%" "%Input%" >"%CandidateMap%"
if errorlevel 1 (
    >&2 echo The linked-image cache linker failed.
    exit /b 1
)
call :read_entry "%CandidateMap%" CandidateEntryOffset
if errorlevel 1 exit /b 1
call :measure_file "%CandidateImage%" CandidateImageBytes CandidateImageSha256
if errorlevel 1 exit /b 1
call :measure_file "%CandidateMap%" CandidateMapBytes CandidateMapSha256
if errorlevel 1 exit /b 1
>"%CheckpointTemporary%\Checkpoint.txt" echo windvale-native-linked-image-checkpoint 1
>>"%CheckpointTemporary%\Checkpoint.txt" echo key %CheckpointKey%
>>"%CheckpointTemporary%\Checkpoint.txt" echo entry-offset %CandidateEntryOffset%
>>"%CheckpointTemporary%\Checkpoint.txt" echo image-bytes %CandidateImageBytes%
>>"%CheckpointTemporary%\Checkpoint.txt" echo image-sha256 %CandidateImageSha256%
>>"%CheckpointTemporary%\Checkpoint.txt" echo map-bytes %CandidateMapBytes%
>>"%CheckpointTemporary%\Checkpoint.txt" echo map-sha256 %CandidateMapSha256%
move "%CheckpointTemporary%" "%CheckpointDirectory%" >nul
if errorlevel 1 exit /b 1
set "CheckpointStatus=Created"

:validate_checkpoint
fsutil reparsepoint query "%CheckpointDirectory%" >nul 2>nul
if not errorlevel 1 exit /b 1
if not exist "%CheckpointManifest%" exit /b 1
if not exist "%CheckpointImage%" exit /b 1
if not exist "%CheckpointMap%" exit /b 1
for %%F in ("%CheckpointManifest%" "%CheckpointImage%" "%CheckpointMap%") do (
    fsutil reparsepoint query "%%~fF" >nul 2>nul
    if not errorlevel 1 exit /b 1
)
for %%F in ("%CheckpointManifest%") do if %%~zF GTR 1024 exit /b 1
call :read_entry "%CheckpointMap%" CheckpointEntryOffset
if errorlevel 1 exit /b 1
call :measure_file "%CheckpointImage%" CheckpointImageBytes CheckpointImageSha256
if errorlevel 1 exit /b 1
call :measure_file "%CheckpointMap%" CheckpointMapBytes CheckpointMapSha256
if errorlevel 1 exit /b 1

:allocate_expected
set "ExpectedManifest=%TEMP%\windvale-linked-image-cache-expected-%RANDOM%-%RANDOM%-%RANDOM%.txt"
if exist "%ExpectedManifest%" goto :allocate_expected
>"%ExpectedManifest%" echo windvale-native-linked-image-checkpoint 1
>>"%ExpectedManifest%" echo key %CheckpointKey%
>>"%ExpectedManifest%" echo entry-offset %CheckpointEntryOffset%
>>"%ExpectedManifest%" echo image-bytes %CheckpointImageBytes%
>>"%ExpectedManifest%" echo image-sha256 %CheckpointImageSha256%
>>"%ExpectedManifest%" echo map-bytes %CheckpointMapBytes%
>>"%ExpectedManifest%" echo map-sha256 %CheckpointMapSha256%
fc /b "%ExpectedManifest%" "%CheckpointManifest%" >nul
set "ManifestResult=%ERRORLEVEL%"
del /f /q "%ExpectedManifest%" >nul 2>nul
if not "%ManifestResult%"=="0" exit /b 1

copy /b /y "%CheckpointImage%" "%OutputImage%" >nul || exit /b 1
copy /b /y "%CheckpointMap%" "%OutputMap%" >nul || exit /b 1
fc /b "%CheckpointImage%" "%OutputImage%" >nul || exit /b 1
fc /b "%CheckpointMap%" "%OutputMap%" >nul || exit /b 1
echo native linked image cache status=%CheckpointStatus% key=%CheckpointKey% entry=%CheckpointEntryOffset%
exit /b 0

:read_entry
setlocal EnableExtensions DisableDelayedExpansion
set "LocalEntryOffset="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=%Entry% address=" "%~1"') do set "LocalEntryOffset=%%E"
if not defined LocalEntryOffset exit /b 1
echo(%LocalEntryOffset%| findstr /r /x "[0-9][0-9]*" >nul || exit /b 1
endlocal & set "%~2=%LocalEntryOffset%"
exit /b 0

:measure_file
setlocal EnableExtensions DisableDelayedExpansion
if not exist "%~1" exit /b 1
for %%F in ("%~1") do set "MeasuredBytes=%%~zF"
if %MeasuredBytes% LEQ 0 exit /b 1
if %MeasuredBytes% GTR 67108864 exit /b 1
call :get_sha256 "%~1" MeasuredSha256
if errorlevel 1 exit /b 1
endlocal & set "%~2=%MeasuredBytes%" & set "%~3=%MeasuredSha256%"
exit /b 0

:get_sha256
setlocal EnableExtensions DisableDelayedExpansion
set "LocalDigest="
for /f "skip=1 tokens=* delims=" %%H in ('certutil -hashfile "%~1" SHA256') do if not defined LocalDigest set "LocalDigest=%%H"
set "LocalDigest=%LocalDigest: =%"
set "LocalDigest=%LocalDigest:A=a%"
set "LocalDigest=%LocalDigest:B=b%"
set "LocalDigest=%LocalDigest:C=c%"
set "LocalDigest=%LocalDigest:D=d%"
set "LocalDigest=%LocalDigest:E=e%"
set "LocalDigest=%LocalDigest:F=f%"
echo(%LocalDigest%| findstr /r /i /x "[0-9a-f][0-9a-f]*" >nul || exit /b 1
if "%LocalDigest:~63,1%"=="" exit /b 1
if not "%LocalDigest:~64,1%"=="" exit /b 1
endlocal & set "%~2=%LocalDigest%"
exit /b 0

:key_failed
del /f /q "%KeyOutput%" >nul 2>nul
exit /b 1

:usage
>&2 echo Usage: Tools\Native\Build-Cached-Linked-Image.cmd ^<base-address^> ^<entry^> ^<input.wvo^> ^<output.bin^> ^<output.map^>
exit /b 64
