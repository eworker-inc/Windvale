@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Database-Superblock.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"

:allocate
set "TemporaryDirectory=%TEMP%\windvale-database-superblock-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1

set "LowererWvb=%TemporaryDirectory%\Lowerer.wvb"
set "Lowerer=%TemporaryDirectory%\Lowerer.exe"
set "FirstWvb=%TemporaryDirectory%\Superblock-First.wvb"
set "SecondWvb=%TemporaryDirectory%\Superblock-Second.wvb"
set "FirstWvo=%TemporaryDirectory%\Superblock-First.wvo"
set "SecondWvo=%TemporaryDirectory%\Superblock-Second.wvo"
set "Image=%TemporaryDirectory%\Superblock.bin"
set "ImagePrefix=%TemporaryDirectory%\Superblock-Image"
set "Map=%TemporaryDirectory%\Superblock.map"
set "WindowsApplication=%TemporaryDirectory%\Superblock.exe"
set "LinuxApplication=%TemporaryDirectory%\Superblock.elf"
set "Result=1"

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Compiler\Windvale-Native-X64-Lowering-Tool.wvproj" ^
    "%LowererWvb%" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Segmented-Compiler-Wvb.cmd" ^
    6 "%LowererWvb%" "%Lowerer%" >nul
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Durable-Superblock.wvproj" ^
    "%FirstWvb%" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Durable-Superblock.wvproj" ^
    "%SecondWvb%" >nul
if errorlevel 1 goto :cleanup
fc /b "%FirstWvb%" "%SecondWvb%" >nul
if errorlevel 1 goto :cleanup
call :verify "%FirstWvb%" 58784 c5934333b5254b767dbbccd630ca9f0320860ae0fc5b0ed4c73f41c8a5fced63 "database-superblock WVB"
if errorlevel 1 goto :cleanup

"%Lowerer%" "%FirstWvb%" "%FirstWvo%" >nul
if errorlevel 1 goto :cleanup
"%Lowerer%" "%SecondWvb%" "%SecondWvo%" >nul
if errorlevel 1 goto :cleanup
fc /b "%FirstWvo%" "%SecondWvo%" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Check-Wvo.cmd" "%FirstWvo%" >nul
if errorlevel 1 goto :cleanup
call :verify "%FirstWvo%" 1098332 c126573f46f5f7a85422fcc6b37a6751b05d58b43f75380206641612a6aee352 "database-superblock WVO"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main ^
    "%Image%" "%FirstWvo%" >"%Map%"
if errorlevel 1 goto :cleanup
set "EntryOffset="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Main address=" "%Map%"') do set "EntryOffset=%%E"
if not "%EntryOffset%"=="171555" goto :cleanup
call :verify "%Image%" 1095856 50cc4d33e1b0a47b75c3c089cb000d36c76b0f4e09ee7962704f4c23e1b73956 "database-superblock image"
if errorlevel 1 goto :cleanup
copy /b "%Image%" "%ImagePrefix%.chunk-0" >nul
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
    "%FirstWvb%" "%ImagePrefix%" 1 %EntryOffset% "%WindowsApplication%" windows >nul
if errorlevel 1 goto :cleanup
call :verify "%WindowsApplication%" 1114624 ae83fdfbfca118e033cc8c7716805f62c9595c6fb2d7407ce805a0e0c8a5f3f3 "database-superblock Windows application"
if errorlevel 1 goto :cleanup
for %%C in (A B C D E F G H I J K L M) do (
    call :run_case %%C
    if errorlevel 1 goto :cleanup
)

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
    "%FirstWvb%" "%ImagePrefix%" 1 %EntryOffset% "%LinuxApplication%" linux >nul
if errorlevel 1 goto :cleanup
call :verify "%LinuxApplication%" 1114112 a1c62f7075d85c20da3e7e5b1fb50c05c654ddc209a0f7a312e7b916616661ec "database-superblock Linux application"
if errorlevel 1 goto :cleanup

set "Result=0"

:cleanup
for %%R in ("%TemporaryDirectory%") do set "ResolvedTemporaryDirectory=%%~fR"
echo(%ResolvedTemporaryDirectory%| findstr /b /i /c:"%TEMP%\windvale-database-superblock-" >nul || exit /b 1
if exist "%ResolvedTemporaryDirectory%\." rmdir /s /q "%ResolvedTemporaryDirectory%"
if not "%Result%"=="0" exit /b %Result%
echo native database superblock status=Passed cases=13 local-result=42 cross-host-images=Verified
exit /b 0

:run_case
"%WindowsApplication%" "%~1" >nul
if not "%ERRORLEVEL%"=="42" (
    >&2 echo The database-superblock case %~1 did not return 42.
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
