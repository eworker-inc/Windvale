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
call :verify "%FirstWvb%" 106620 a5ba2df3660676d3bcb1e2024c30849080ed2e641ff2264a240f2b8868dac693 "database-durable-commit WVB"
if errorlevel 1 goto :cleanup

"%Lowerer%" "%FirstWvb%" "%FirstWvo%" >nul
if errorlevel 1 goto :cleanup
"%Lowerer%" "%SecondWvb%" "%SecondWvo%" >nul
if errorlevel 1 goto :cleanup
fc /b "%FirstWvo%" "%SecondWvo%" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%FirstWvo%" >nul
if errorlevel 1 goto :cleanup
call :verify "%FirstWvo%" 1988856 7fd7e95e26cad8bf9bf94b33f841c4a178f776428e4b9974eb3fb47dc0c7a7b0 "database-durable-commit WVO"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main ^
    "%Image%" "%FirstWvo%" >"%Map%"
if errorlevel 1 goto :cleanup
set "EntryOffset="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Main address=" "%Map%"') do set "EntryOffset=%%E"
if not "%EntryOffset%"=="151017" goto :cleanup
call :verify "%Image%" 1985396 9dea673ea1ea54d1b4abe5c8bab6e1f68d40545086f4cb389949bcd02705eb00 "database-durable-commit image"
if errorlevel 1 goto :cleanup
copy /b "%Image%" "%ImagePrefix%.chunk-0" >nul
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
    "%FirstWvb%" "%ImagePrefix%" 1 %EntryOffset% "%WindowsApplication%" windows >nul
if errorlevel 1 goto :cleanup
call :verify "%WindowsApplication%" 2006528 eff2d4643ea070df6c5867a3301ab495d776b81cae976de6939a5bf3cbda895f "database-durable-commit Windows application"
if errorlevel 1 goto :cleanup
for %%C in (A B C D E F G H I J K L) do (
    call :run_case %%C
    if errorlevel 1 goto :cleanup
)

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
    "%FirstWvb%" "%ImagePrefix%" 1 %EntryOffset% "%LinuxApplication%" linux >nul
if errorlevel 1 goto :cleanup
call :verify "%LinuxApplication%" 2007040 db27ac7755bd29931c1a298e42ee288c9a061216213a4067a1fc688c788065ed "database-durable-commit Linux application"
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
