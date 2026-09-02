@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Database-Durable-Commit.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"

:allocate
set "TemporaryDirectory=%TEMP%\windvale-database-durable-commit-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1

set "Lowerer=%RepositoryRoot%\Artifacts\Native-Wvb-To-Wvo-Candidate\Wvb-To-Wvo.exe"
set "FirstWvb=%TemporaryDirectory%\Commit-First.wvb"
set "SecondWvb=%TemporaryDirectory%\Commit-Second.wvb"
set "FirstWvo=%TemporaryDirectory%\Commit-First.wvo"
set "SecondWvo=%TemporaryDirectory%\Commit-Second.wvo"
set "Image=%TemporaryDirectory%\Commit.bin"
set "ImagePrefix=%TemporaryDirectory%\Commit-Image"
set "Map=%TemporaryDirectory%\Commit.map"
set "WindowsApplication=%TemporaryDirectory%\Commit.exe"
set "LinuxApplication=%TemporaryDirectory%\Commit.elf"
set "Result=1"

echo START native database durable commit phase=tools item=1/4 retained-tools=1
call :verify "%Lowerer%" 10075136 22826b9bb6f391e5ac0e7605fe3246cce16d977c6bed88a5bafec90262aea6ea "retained lowerer"
if errorlevel 1 goto :cleanup
echo PASS  native database durable commit phase=tools item=1/4

echo START native database durable commit phase=compile item=2/4
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Durable-Commit.wvproj" ^
    "%FirstWvb%" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Durable-Commit.wvproj" ^
    "%SecondWvb%" >nul
if errorlevel 1 goto :cleanup
fc /b "%FirstWvb%" "%SecondWvb%" >nul
if errorlevel 1 goto :cleanup
call :verify "%FirstWvb%" 107828 479e631466733ae421d3477f61cedf1f716aa993cfecd7da560818a9d6dc4b60 "database-durable-commit WVB"
if errorlevel 1 goto :cleanup

"%Lowerer%" "%FirstWvb%" "%FirstWvo%" >nul
if errorlevel 1 goto :cleanup
"%Lowerer%" "%SecondWvb%" "%SecondWvo%" >nul
if errorlevel 1 goto :cleanup
fc /b "%FirstWvo%" "%SecondWvo%" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Check-Wvo.cmd" "%FirstWvo%" >nul
if errorlevel 1 goto :cleanup
call :verify "%FirstWvo%" 2011950 39eaa1823df0e4dfabda085eb3894d47b940a06a4d44a4f0d637aa08a5a4a4a5 "database-durable-commit WVO"
if errorlevel 1 goto :cleanup
echo PASS  native database durable commit phase=compile item=2/4

echo START native database durable commit phase=link item=3/4
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main ^
    "%Image%" "%FirstWvo%" >"%Map%"
if errorlevel 1 goto :cleanup
set "EntryOffset="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Main address=" "%Map%"') do set "EntryOffset=%%E"
if not "%EntryOffset%"=="151017" (
    >&2 echo The database-durable-commit entry offset is %EntryOffset%, expected 151017.
    goto :cleanup
)
call :verify "%Image%" 2008436 2f1182f785ad22e1011b0c76e1202b3fc436548c76d70d2be8fb5aa1f175e929 "database-durable-commit image"
if errorlevel 1 goto :cleanup
copy /b "%Image%" "%ImagePrefix%.chunk-0" >nul
if errorlevel 1 goto :cleanup
echo PASS  native database durable commit phase=link item=3/4

echo START native database durable commit phase=package-and-execute item=4/4
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
    "%FirstWvb%" "%ImagePrefix%" 1 %EntryOffset% "%WindowsApplication%" windows >nul
if errorlevel 1 goto :cleanup
call :verify "%WindowsApplication%" 2029568 680d56c853b502b5bb76bffc3526752290da697eba707fa768ace644fb144b15 "database-durable-commit Windows application"
if errorlevel 1 goto :cleanup
for %%C in (A B C D E F G H I J K L) do (
    call :run_case %%C
    if errorlevel 1 goto :cleanup
)

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
    "%FirstWvb%" "%ImagePrefix%" 1 %EntryOffset% "%LinuxApplication%" linux >nul
if errorlevel 1 goto :cleanup
call :verify "%LinuxApplication%" 2031616 6969a296c7d0819175b9a5b1dd4c64c5245d056be9d674b947f08d92f3ab0a5e "database-durable-commit Linux application"
if errorlevel 1 goto :cleanup
echo PASS  native database durable commit phase=package-and-execute item=4/4

set "Result=0"

:cleanup
for %%R in ("%TemporaryDirectory%") do set "ResolvedTemporaryDirectory=%%~fR"
echo(%ResolvedTemporaryDirectory%| findstr /b /i /c:"%TEMP%\windvale-database-durable-commit-" >nul || exit /b 1
if exist "%ResolvedTemporaryDirectory%\." rmdir /s /q "%ResolvedTemporaryDirectory%"
if not "%Result%"=="0" exit /b %Result%
echo native database durable commit status=Passed cases=12 local-result=42 cross-host-images=Verified
exit /b 0

:run_case
"%WindowsApplication%" "%~1" >nul
if not "%ERRORLEVEL%"=="42" (
    >&2 echo The database-durable-commit case %~1 did not return 42.
    exit /b 1
)
exit /b 0

:verify
if not exist "%~1" (
    >&2 echo Missing %~4: %~1
    exit /b 1
)
set "ActualBytes="
for %%F in ("%~1") do set "ActualBytes=%%~zF"
set "ActualSha256="
for /f "skip=1 delims=" %%H in ('certutil -hashfile "%~1" SHA256') do if not defined ActualSha256 set "ActualSha256=%%H"
if not "%ActualBytes%"=="%~2" (
    >&2 echo The %~4 byte length differs: bytes=%ActualBytes% expected=%~2 sha256=%ActualSha256%.
    exit /b 1
)
if /i not "%ActualSha256%"=="%~3" (
    >&2 echo The %~4 digest differs: sha256=%ActualSha256% expected=%~3.
    exit /b 1
)
exit /b 0
