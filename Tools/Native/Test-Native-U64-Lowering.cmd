@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Native-U64-Lowering.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"

:allocate
set "TemporaryDirectory=%TEMP%\windvale-native-u64-lowering-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1

set "LowererWvb=%TemporaryDirectory%\Lowerer.wvb"
set "Lowerer=%TemporaryDirectory%\Lowerer.exe"
set "TargetWvb=%TemporaryDirectory%\Target.wvb"
set "TargetWvo=%TemporaryDirectory%\Target.wvo"
set "Image=%TemporaryDirectory%\Target.bin"
set "WindowsApplication=%TemporaryDirectory%\Target.exe"
set "LinuxApplication=%TemporaryDirectory%\Target.elf"
set "PageWvb=%TemporaryDirectory%\Page.wvb"
set "PageWvo=%TemporaryDirectory%\Page.wvo"
set "PageImage=%TemporaryDirectory%\Page.bin"
set "PageImagePrefix=%TemporaryDirectory%\Page-Image"
set "PageWindowsApplication=%TemporaryDirectory%\Page.exe"
set "PageLinuxApplication=%TemporaryDirectory%\Page.elf"
set "PageFixture=%RepositoryRoot%\Tests\Fixtures\Database\Native-Hosted-Snapshot-Page.txt"
set "Result=1"

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Compiler\Windvale-Native-X64-Lowering-Tool.wvproj" ^
    "%LowererWvb%" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Segmented-Compiler-Wvb.cmd" ^
    6 "%LowererWvb%" "%Lowerer%" >nul
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Wvb-To-Wvo-U64.wvproj" ^
    "%TargetWvb%" >nul
if errorlevel 1 goto :cleanup
call :verify "%TargetWvb%" 1024 ced899022e388413714456621f029974fa6e4db5361ff6348b6f127f97316bf5 "u64 WVB"
if errorlevel 1 goto :cleanup

"%Lowerer%" "%TargetWvb%" "%TargetWvo%" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%TargetWvo%" >nul
if errorlevel 1 goto :cleanup
call :verify "%TargetWvo%" 8032 0807b47f84505a256cdb1a542df2ccab1bce87b07424cdeaeae895e2b86d2459 "u64 WVO"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Image%" "%TargetWvo%" >nul
if errorlevel 1 goto :cleanup
call :verify "%Image%" 7848 114143636e072c92a20673ca23176fc4584d2ae84b5f46258bb816a98e49e77e "u64 flat image"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 ^
    "%Image%" 0 "%WindowsApplication%" >nul
if errorlevel 1 goto :cleanup
call :verify "%WindowsApplication%" 9728 184ed47cc4c77bb86925c62920a6d4e884a46dee4d62e8b079cda0bd3b01c799 "u64 Windows application"
if errorlevel 1 goto :cleanup
"%WindowsApplication%" >nul
if not "%ERRORLEVEL%"=="42" goto :cleanup

call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 ^
    "%Image%" 0 "%LinuxApplication%" >nul
if errorlevel 1 goto :cleanup
call :verify "%LinuxApplication%" 12400 573171b3564ec1fa201470f5d0dcbdf19ff10856fee8bf6374e32bada24a8453 "u64 Linux application"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Native-Hosted-Snapshot-Page.wvproj" ^
    "%PageWvb%" >nul
if errorlevel 1 goto :cleanup
call :verify "%PageWvb%" 5386 22a8b4a44a73b1cfbfdf7ba19ded9e5c921e6870fd4afd2f76a982c555805c00 "native database-page WVB"
if errorlevel 1 goto :cleanup
call :verify "%PageFixture%" 17 4897fe28a3fa1ded2c3e9f79192b23671d1fe1e39c10f71ed94703d317886f73 "native database-page fixture"
if errorlevel 1 goto :cleanup

"%Lowerer%" "%PageWvb%" "%PageWvo%" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%PageWvo%" >nul
if errorlevel 1 goto :cleanup
call :verify "%PageWvo%" 74228 1f652d116e9cd59f1e033831fc6b8c227d23c91a19a4b3e027e1fabb35880558 "native database-page WVO"
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%PageImage%" "%PageWvo%" >nul
if errorlevel 1 goto :cleanup
call :verify "%PageImage%" 73888 2792d693240b36122c0f9d2c706a80985a366bf61316bb50751ebd997f9b7d15 "native database-page image"
if errorlevel 1 goto :cleanup
copy /b "%PageImage%" "%PageImagePrefix%.chunk-0" >nul
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
    "%PageWvb%" "%PageImagePrefix%" 1 0 "%PageWindowsApplication%" windows >nul
if errorlevel 1 goto :cleanup
call :verify "%PageWindowsApplication%" 92160 4b51c69313be614d7cae3534cc6fad2a78848814838758914edb64986fb6ecb6 "native database-page Windows application"
if errorlevel 1 goto :cleanup
"%PageWindowsApplication%" "%PageFixture%" >nul
if not "%ERRORLEVEL%"=="42" goto :cleanup

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
    "%PageWvb%" "%PageImagePrefix%" 1 0 "%PageLinuxApplication%" linux >nul
if errorlevel 1 goto :cleanup
call :verify "%PageLinuxApplication%" 94208 36ed10422a46c6eb43a1435472d89476a5c0aea5029079321392e4917993067b "native database-page Linux application"
if errorlevel 1 goto :cleanup

set "Result=0"

:cleanup
for %%R in ("%TemporaryDirectory%") do set "ResolvedTemporaryDirectory=%%~fR"
echo(%ResolvedTemporaryDirectory%| findstr /b /i /c:"%TEMP%\windvale-native-u64-lowering-" >nul || exit /b 1
if exist "%ResolvedTemporaryDirectory%\." rmdir /s /q "%ResolvedTemporaryDirectory%"
if not "%Result%"=="0" exit /b %Result%
echo native u64 lowering status=Passed local-result=42 database-page=42 cross-host-images=Verified
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
