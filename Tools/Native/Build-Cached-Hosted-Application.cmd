@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~7"=="" goto :usage
if not "%~8"=="" goto :usage
echo(%~1| findstr /r /x "[1-7]" >nul || goto :usage
if /I not "%~x2"==".wvb" goto :usage
echo(%~4| findstr /r /x "[1-8]" >nul || goto :usage
echo(%~5| findstr /r /x "[0-9][0-9]*" >nul || goto :usage

set "Profile=%~1"
set "Input=%~f2"
set "ChunkPrefix=%~f3"
set "FragmentCount=%~4"
set "NativeEntry=%~5"
set "Output=%~f6"
set "Target=%~7"
if /I "%Target%"=="windows" (
    if /I not "%~x6"==".exe" goto :usage
    set "Target=windows"
    set "ProductLeaf=Product.exe"
) else if /I "%Target%"=="linux" (
    if /I not "%~x6"==".elf" goto :usage
    set "Target=linux"
    set "ProductLeaf=Product.elf"
) else (
    goto :usage
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Packager=%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd"
set "KeyTool=%RepositoryRoot%\Tools\Native\Get-Native-Hosted-Application-Cache-Key.mjs"

if not exist "%Input%" exit /b 1
if not exist "%Packager%" exit /b 1
if not exist "%KeyTool%" exit /b 1
for %%D in ("%Output%") do if not exist "%%~dpD." exit /b 1

:allocate_key
set "KeyOutput=%TEMP%\windvale-hosted-application-cache-key-%RANDOM%-%RANDOM%-%RANDOM%.txt"
if exist "%KeyOutput%" goto :allocate_key
node "%KeyTool%" hosted-application-v1 "%Target%" "%Profile%" "%Input%" ^
    "%ChunkPrefix%" "%FragmentCount%" "%NativeEntry%" "%Packager%" >"%KeyOutput%"
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
set "CheckpointProductRoot=%CheckpointRoot%\hosted-application-v1"
if not exist "%CheckpointProductRoot%\." mkdir "%CheckpointProductRoot%" || exit /b 1
fsutil reparsepoint query "%CheckpointProductRoot%" >nul 2>nul
if not errorlevel 1 exit /b 1
set "CheckpointFamily=%CheckpointProductRoot%\windows-x64"
if not exist "%CheckpointFamily%\." mkdir "%CheckpointFamily%" || exit /b 1
fsutil reparsepoint query "%CheckpointFamily%" >nul 2>nul
if not errorlevel 1 exit /b 1

set "CheckpointDirectory=%CheckpointFamily%\%CheckpointKey%"
set "CheckpointManifest=%CheckpointDirectory%\Checkpoint.txt"
set "CheckpointProduct=%CheckpointDirectory%\%ProductLeaf%"
set "CheckpointStatus=Hit"
if exist "%CheckpointDirectory%\." goto :validate_checkpoint

:allocate_checkpoint
set "CheckpointTemporary=%CheckpointFamily%\.new-%CheckpointKey%-%RANDOM%-%RANDOM%"
if exist "%CheckpointTemporary%\." goto :allocate_checkpoint
mkdir "%CheckpointTemporary%" || exit /b 1
set "CandidateProduct=%CheckpointTemporary%\%ProductLeaf%"
set "PackageLog=%CheckpointTemporary%\Package.log"
call "%Packager%" image "%Profile%" "%Input%" "%ChunkPrefix%" ^
    "%FragmentCount%" "%NativeEntry%" "%CandidateProduct%" "%Target%" >"%PackageLog%" 2>&1
if errorlevel 1 (
    >&2 echo The hosted-application cache packager failed.
    if exist "%PackageLog%" type "%PackageLog%" >&2
    exit /b 1
)
del /f /q "%PackageLog%" >nul 2>nul
call :measure_file "%CandidateProduct%" CandidateBytes CandidateSha256
if errorlevel 1 exit /b 1
>"%CheckpointTemporary%\Checkpoint.txt" echo windvale-native-hosted-application-checkpoint 1
>>"%CheckpointTemporary%\Checkpoint.txt" echo key %CheckpointKey%
>>"%CheckpointTemporary%\Checkpoint.txt" echo target %Target%
>>"%CheckpointTemporary%\Checkpoint.txt" echo application-bytes %CandidateBytes%
>>"%CheckpointTemporary%\Checkpoint.txt" echo application-sha256 %CandidateSha256%
move "%CheckpointTemporary%" "%CheckpointDirectory%" >nul
if errorlevel 1 exit /b 1
set "CheckpointStatus=Created"

:validate_checkpoint
fsutil reparsepoint query "%CheckpointDirectory%" >nul 2>nul
if not errorlevel 1 exit /b 1
if not exist "%CheckpointManifest%" exit /b 1
if not exist "%CheckpointProduct%" exit /b 1
for %%F in ("%CheckpointManifest%" "%CheckpointProduct%") do (
    fsutil reparsepoint query "%%~fF" >nul 2>nul
    if not errorlevel 1 exit /b 1
)
for %%F in ("%CheckpointManifest%") do if %%~zF GTR 1024 exit /b 1
call :measure_file "%CheckpointProduct%" CheckpointBytes CheckpointSha256
if errorlevel 1 exit /b 1

:allocate_expected
set "ExpectedManifest=%TEMP%\windvale-hosted-application-cache-expected-%RANDOM%-%RANDOM%-%RANDOM%.txt"
if exist "%ExpectedManifest%" goto :allocate_expected
>"%ExpectedManifest%" echo windvale-native-hosted-application-checkpoint 1
>>"%ExpectedManifest%" echo key %CheckpointKey%
>>"%ExpectedManifest%" echo target %Target%
>>"%ExpectedManifest%" echo application-bytes %CheckpointBytes%
>>"%ExpectedManifest%" echo application-sha256 %CheckpointSha256%
fc /b "%ExpectedManifest%" "%CheckpointManifest%" >nul
set "ManifestResult=%ERRORLEVEL%"
del /f /q "%ExpectedManifest%" >nul 2>nul
if not "%ManifestResult%"=="0" exit /b 1

copy /b /y "%CheckpointProduct%" "%Output%" >nul || exit /b 1
fc /b "%CheckpointProduct%" "%Output%" >nul || exit /b 1
echo native hosted application cache status=%CheckpointStatus% key=%CheckpointKey% target=%Target%
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
>&2 echo Usage: Tools\Native\Build-Cached-Hosted-Application.cmd ^<profile-1-through-7^> ^<input.wvb^> ^<chunk-prefix^> ^<fragment-count-1-through-8^> ^<entry^> ^<output.exe^|output.elf^> ^<windows^|linux^>
exit /b 64
