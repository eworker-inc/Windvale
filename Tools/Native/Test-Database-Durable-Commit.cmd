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

set "LowererWvb=%TemporaryDirectory%\Lowerer.wvb"
set "Lowerer=%TemporaryDirectory%\Lowerer.exe"
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

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Compiler\Windvale-Native-X64-Lowering-Tool.wvproj" ^
    "%LowererWvb%" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Segmented-Compiler-Wvb.cmd" ^
    6 "%LowererWvb%" "%Lowerer%" >nul
if errorlevel 1 goto :cleanup

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
call :verify "%FirstWvb%" 107155 1a026edee89222585e5c6b7a7367fca807846d5cfdd58010fc85d872f7f2973c "database-durable-commit WVB"
if errorlevel 1 goto :cleanup

"%Lowerer%" "%FirstWvb%" "%FirstWvo%" >nul
if errorlevel 1 goto :cleanup
"%Lowerer%" "%SecondWvb%" "%SecondWvo%" >nul
if errorlevel 1 goto :cleanup
fc /b "%FirstWvo%" "%SecondWvo%" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Check-Wvo.cmd" "%FirstWvo%" >nul
if errorlevel 1 goto :cleanup
call :verify "%FirstWvo%" 2001802 2abe19205e0f1e64afb7d49931697ab7f96646e315adf76531f52c50ddff14b5 "database-durable-commit WVO"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main ^
    "%Image%" "%FirstWvo%" >"%Map%"
if errorlevel 1 goto :cleanup
set "EntryOffset="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Main address=" "%Map%"') do set "EntryOffset=%%E"
if not "%EntryOffset%"=="151017" goto :cleanup
call :verify "%Image%" 1998308 60da45fe57c3d1614024588be4c22044f4057fa04512693d82df47f90aebfbe1 "database-durable-commit image"
if errorlevel 1 goto :cleanup
copy /b "%Image%" "%ImagePrefix%.chunk-0" >nul
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
    "%FirstWvb%" "%ImagePrefix%" 1 %EntryOffset% "%WindowsApplication%" windows >nul
if errorlevel 1 goto :cleanup
call :verify "%WindowsApplication%" 2019328 36d24784407890f07b1a279276d52c7f979e6cb06340cc5ae08baa4f37cd286f "database-durable-commit Windows application"
if errorlevel 1 goto :cleanup
for %%C in (A B C D E F G H I J K L) do (
    call :run_case %%C
    if errorlevel 1 goto :cleanup
)

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
    "%FirstWvb%" "%ImagePrefix%" 1 %EntryOffset% "%LinuxApplication%" linux >nul
if errorlevel 1 goto :cleanup
call :verify "%LinuxApplication%" 2019328 965b4a1fb73b6aaf33aec2478443329b6759a6b20c93e3e3f83067476b81125d "database-durable-commit Linux application"
if errorlevel 1 goto :cleanup

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
for %%F in ("%~1") do if not "%%~zF"=="%~2" (
    >&2 echo The %~4 byte length differs.
    exit /b 1
)
certutil -hashfile "%~1" SHA256 | findstr /i /c:"%~3" >nul
if errorlevel 1 (
    >&2 echo The %~4 digest differs.
    exit /b 1
)
exit /b 0
