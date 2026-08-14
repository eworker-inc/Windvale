@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~5"=="" goto :usage
if not "%~6"=="" goto :usage
if /I not "%~x1"==".wvproj" goto :usage
if /I not "%~x4"==".wvb" goto :usage
if /I not "%~x5"==".wvo" goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Project=%~f1"
set "BuildDriver=%~f2"
set "Lowerer=%~f3"
set "OutputWvb=%~f4"
set "OutputWvo=%~f5"
set "Workspace=%RepositoryRoot%\Windvale.wvws"
set "WorkspaceResource=%Workspace:\=/%"
set "ProjectResource=%Project:\=/%"
set "KeyTool=%RepositoryRoot%\Tools\Native\Get-Native-Project-Cache-Key.mjs"
set "AdmissionTool=%RepositoryRoot%\Tools\Native\Check-Wvo.cmd"

if not exist "%Project%" exit /b 1
if not exist "%BuildDriver%" exit /b 1
if not exist "%Lowerer%" exit /b 1
if not exist "%Workspace%" exit /b 1
if not exist "%KeyTool%" exit /b 1
for %%D in ("%OutputWvb%") do if not exist "%%~dpD." exit /b 1
for %%D in ("%OutputWvo%") do if not exist "%%~dpD." exit /b 1

:allocate_key
set "KeyOutput=%TEMP%\windvale-project-cache-key-%RANDOM%-%RANDOM%-%RANDOM%.txt"
if exist "%KeyOutput%" goto :allocate_key
node "%KeyTool%" database-project-object-v1 "%Project%" "%BuildDriver%" "%Lowerer%" >"%KeyOutput%"
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
set "CheckpointProductRoot=%CheckpointRoot%\project-object-v1"
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
set "CheckpointWvo=%CheckpointDirectory%\Product.wvo"
set "CheckpointStatus=Hit"
if exist "%CheckpointDirectory%\." goto :validate_checkpoint

:allocate_checkpoint
set "CheckpointTemporary=%CheckpointFamily%\.new-%CheckpointKey%-%RANDOM%-%RANDOM%"
if exist "%CheckpointTemporary%\." goto :allocate_checkpoint
mkdir "%CheckpointTemporary%" || exit /b 1
set "CandidateWvb=%CheckpointTemporary%\Product.wvb"
set "CandidateWvo=%CheckpointTemporary%\Product.wvo"
set "CandidateWvbResource=%CandidateWvb:\=/%"

"%BuildDriver%" --workspace "%WorkspaceResource%" --project "%ProjectResource%" "%CandidateWvbResource%" >nul
if errorlevel 1 exit /b 1
"%Lowerer%" "%CandidateWvb%" "%CandidateWvo%" >nul
if errorlevel 1 exit /b 1
call "%AdmissionTool%" "%CandidateWvo%" >nul
if errorlevel 1 exit /b 1

call :measure_file "%CandidateWvb%" CandidateWvbBytes CandidateWvbSha256
if errorlevel 1 exit /b 1
call :measure_file "%CandidateWvo%" CandidateWvoBytes CandidateWvoSha256
if errorlevel 1 exit /b 1
>"%CheckpointTemporary%\Checkpoint.txt" echo windvale-native-project-object-checkpoint 1
>>"%CheckpointTemporary%\Checkpoint.txt" echo key %CheckpointKey%
>>"%CheckpointTemporary%\Checkpoint.txt" echo wvb-bytes %CandidateWvbBytes%
>>"%CheckpointTemporary%\Checkpoint.txt" echo wvb-sha256 %CandidateWvbSha256%
>>"%CheckpointTemporary%\Checkpoint.txt" echo wvo-bytes %CandidateWvoBytes%
>>"%CheckpointTemporary%\Checkpoint.txt" echo wvo-sha256 %CandidateWvoSha256%
move "%CheckpointTemporary%" "%CheckpointDirectory%" >nul
if errorlevel 1 exit /b 1
set "CheckpointStatus=Created"

:validate_checkpoint
fsutil reparsepoint query "%CheckpointDirectory%" >nul 2>nul
if not errorlevel 1 exit /b 1
if not exist "%CheckpointManifest%" exit /b 1
if not exist "%CheckpointWvb%" exit /b 1
if not exist "%CheckpointWvo%" exit /b 1
for %%F in ("%CheckpointManifest%" "%CheckpointWvb%" "%CheckpointWvo%") do (
    fsutil reparsepoint query "%%~fF" >nul 2>nul
    if not errorlevel 1 exit /b 1
)
for %%F in ("%CheckpointManifest%") do if %%~zF GTR 1024 exit /b 1
call :measure_file "%CheckpointWvb%" CheckpointWvbBytes CheckpointWvbSha256
if errorlevel 1 exit /b 1
call :measure_file "%CheckpointWvo%" CheckpointWvoBytes CheckpointWvoSha256
if errorlevel 1 exit /b 1

:allocate_expected
set "ExpectedManifest=%TEMP%\windvale-project-cache-expected-%RANDOM%-%RANDOM%-%RANDOM%.txt"
if exist "%ExpectedManifest%" goto :allocate_expected
>"%ExpectedManifest%" echo windvale-native-project-object-checkpoint 1
>>"%ExpectedManifest%" echo key %CheckpointKey%
>>"%ExpectedManifest%" echo wvb-bytes %CheckpointWvbBytes%
>>"%ExpectedManifest%" echo wvb-sha256 %CheckpointWvbSha256%
>>"%ExpectedManifest%" echo wvo-bytes %CheckpointWvoBytes%
>>"%ExpectedManifest%" echo wvo-sha256 %CheckpointWvoSha256%
fc /b "%ExpectedManifest%" "%CheckpointManifest%" >nul
set "ManifestResult=%ERRORLEVEL%"
del /f /q "%ExpectedManifest%" >nul 2>nul
if not "%ManifestResult%"=="0" exit /b 1

copy /y "%CheckpointWvb%" "%OutputWvb%" >nul || exit /b 1
copy /y "%CheckpointWvo%" "%OutputWvo%" >nul || exit /b 1
fc /b "%CheckpointWvb%" "%OutputWvb%" >nul || exit /b 1
fc /b "%CheckpointWvo%" "%OutputWvo%" >nul || exit /b 1
call "%AdmissionTool%" "%OutputWvo%" >nul || exit /b 1
echo native project object cache status=%CheckpointStatus% key=%CheckpointKey%
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
>&2 echo Usage: Tools\Native\Build-Cached-Project-Object.cmd ^<project.wvproj^> ^<build-driver.exe^> ^<lowerer.exe^> ^<output.wvb^> ^<output.wvo^>
exit /b 64
