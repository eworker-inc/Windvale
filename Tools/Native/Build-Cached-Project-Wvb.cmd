@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~2"=="" goto :usage
if not "%~3"=="" goto :usage
if /I not "%~x1"==".wvproj" goto :usage
if /I not "%~x2"==".wvb" goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Project=%~f1"
set "OutputWvb=%~f2"
set "KeyTool=%RepositoryRoot%\Tools\Native\Get-Native-Project-Cache-Key.mjs"
set "FrontDoor=%RepositoryRoot%\Artifacts\Native-Front-Door"
set "Inventory=%FrontDoor%\SHA256SUMS"
set "BuildDriver=%RepositoryRoot%\Artifacts\Native-Compiler-Reconstruction-Candidate\windows-x64\wvbuild.exe"
set "Workspace=%RepositoryRoot%\Windvale.wvws"
set "WorkspaceResource=%Workspace:\=/%"
set "ProjectResource=%Project:\=/%"

for %%F in ("%Project%" "%KeyTool%" "%Inventory%" "%BuildDriver%" "%Workspace%") do if not exist "%%~fF" exit /b 1
for %%D in ("%OutputWvb%") do if not exist "%%~dpD." exit /b 1

:allocate_key
set "KeyOutput=%TEMP%\windvale-project-wvb-cache-key-%RANDOM%-%RANDOM%-%RANDOM%.txt"
if exist "%KeyOutput%" goto :allocate_key
node "%KeyTool%" project-wvb-v2 "%Project%" "%Inventory%" ^
    "%BuildDriver%" >"%KeyOutput%"
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
set "CheckpointProductRoot=%CheckpointRoot%\project-wvb-v2"
if not exist "%CheckpointProductRoot%\." mkdir "%CheckpointProductRoot%" || exit /b 1
fsutil reparsepoint query "%CheckpointProductRoot%" >nul 2>nul
if not errorlevel 1 exit /b 1
set "CheckpointFamily=%CheckpointProductRoot%\windows-x64"
if not exist "%CheckpointFamily%\." mkdir "%CheckpointFamily%" || exit /b 1
fsutil reparsepoint query "%CheckpointFamily%" >nul 2>nul
if not errorlevel 1 exit /b 1

set "CheckpointDirectory=%CheckpointFamily%\%CheckpointKey%"
set "CheckpointManifest=%CheckpointDirectory%\Checkpoint.txt"
set "CheckpointWvb=%CheckpointDirectory%\Product.wvb"
set "CheckpointStatus=Hit"
if exist "%CheckpointDirectory%\." goto :validate_checkpoint

:allocate_checkpoint
set "CheckpointTemporary=%CheckpointFamily%\.new-%CheckpointKey%-%RANDOM%-%RANDOM%"
if exist "%CheckpointTemporary%\." goto :allocate_checkpoint
mkdir "%CheckpointTemporary%" || exit /b 1
set "CandidateWvb=%CheckpointTemporary%\Product.wvb"
set "CandidateResource=%CandidateWvb:\=/%"
set "BuildLog=%CheckpointTemporary%\Build.log"
"%BuildDriver%" --workspace "%WorkspaceResource%" --project ^
    "%ProjectResource%" "%CandidateResource%" >"%BuildLog%" 2>&1
if errorlevel 1 (
    >&2 echo The project-WVB cache build failed.
    if exist "%BuildLog%" type "%BuildLog%" >&2
    exit /b 1
)
del /f /q "%BuildLog%" >nul 2>nul
call :measure_file "%CandidateWvb%" CandidateBytes CandidateSha256
if errorlevel 1 exit /b 1
>"%CheckpointTemporary%\Checkpoint.txt" echo windvale-native-project-wvb-checkpoint 1
>>"%CheckpointTemporary%\Checkpoint.txt" echo key %CheckpointKey%
>>"%CheckpointTemporary%\Checkpoint.txt" echo wvb-bytes %CandidateBytes%
>>"%CheckpointTemporary%\Checkpoint.txt" echo wvb-sha256 %CandidateSha256%
move "%CheckpointTemporary%" "%CheckpointDirectory%" >nul
if errorlevel 1 exit /b 1
set "CheckpointStatus=Created"

:validate_checkpoint
fsutil reparsepoint query "%CheckpointDirectory%" >nul 2>nul
if not errorlevel 1 exit /b 1
if not exist "%CheckpointManifest%" exit /b 1
if not exist "%CheckpointWvb%" exit /b 1
for %%F in ("%CheckpointManifest%" "%CheckpointWvb%") do (
    fsutil reparsepoint query "%%~fF" >nul 2>nul
    if not errorlevel 1 exit /b 1
)
for %%F in ("%CheckpointManifest%") do if %%~zF GTR 1024 exit /b 1
call :measure_file "%CheckpointWvb%" CheckpointBytes CheckpointSha256
if errorlevel 1 exit /b 1

:allocate_expected
set "ExpectedManifest=%TEMP%\windvale-project-wvb-cache-expected-%RANDOM%-%RANDOM%-%RANDOM%.txt"
if exist "%ExpectedManifest%" goto :allocate_expected
>"%ExpectedManifest%" echo windvale-native-project-wvb-checkpoint 1
>>"%ExpectedManifest%" echo key %CheckpointKey%
>>"%ExpectedManifest%" echo wvb-bytes %CheckpointBytes%
>>"%ExpectedManifest%" echo wvb-sha256 %CheckpointSha256%
fc /b "%ExpectedManifest%" "%CheckpointManifest%" >nul
set "ManifestResult=%ERRORLEVEL%"
del /f /q "%ExpectedManifest%" >nul 2>nul
if not "%ManifestResult%"=="0" exit /b 1

copy /b /y "%CheckpointWvb%" "%OutputWvb%" >nul || exit /b 1
fc /b "%CheckpointWvb%" "%OutputWvb%" >nul || exit /b 1
echo native project wvb cache status=%CheckpointStatus% key=%CheckpointKey%
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
>&2 echo Usage: Tools\Native\Build-Cached-Project-Wvb.cmd ^<project.wvproj^> ^<output.wvb^>
exit /b 64
