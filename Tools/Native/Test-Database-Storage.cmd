@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Database-Storage.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"

:allocate
set "TemporaryDirectory=%TEMP%\windvale-database-storage-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1

set "BuildDriverWvb=%TemporaryDirectory%\Build-Driver.wvb"
set "BuildDriver=%TemporaryDirectory%\Build-Driver.exe"
set "LowererWvb=%TemporaryDirectory%\Lowerer.wvb"
set "Lowerer=%TemporaryDirectory%\Lowerer.exe"
set "WorkspacePath=%RepositoryRoot%\Windvale.wvws"
set "WorkspaceResource=%WorkspacePath:\=/%"
set "Result=1"

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tools\Windvale-Compiler-Build-Driver.wvproj" ^
    "%BuildDriverWvb%" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Segmented-Compiler-Wvb.cmd" ^
    2 "%BuildDriverWvb%" "%BuildDriver%" >nul
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Compiler\Windvale-Native-X64-Lowering-Tool.wvproj" ^
    "%LowererWvb%" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Segmented-Compiler-Wvb.cmd" ^
    6 "%LowererWvb%" "%Lowerer%" >nul
if errorlevel 1 goto :cleanup

call :verify_target Nested ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Nested-Record-Fields.wvproj"
if errorlevel 1 goto :cleanup
call :verify_target Publication ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Storage-Publication.wvproj"
if errorlevel 1 goto :cleanup
call :verify_target Recovery ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Storage-Recovery.wvproj"
if errorlevel 1 goto :cleanup

set "Result=0"

:cleanup
for %%R in ("%TemporaryDirectory%") do set "ResolvedTemporaryDirectory=%%~fR"
echo(%ResolvedTemporaryDirectory%| findstr /b /i /c:"%TEMP%\windvale-database-storage-" >nul || exit /b 1
if exist "%ResolvedTemporaryDirectory%\." rmdir /s /q "%ResolvedTemporaryDirectory%"
if not "%Result%"=="0" exit /b %Result%
echo native database storage status=Passed cases=3 local-results=0 cross-host-images=Verified
exit /b 0

:verify_target
setlocal EnableExtensions DisableDelayedExpansion
set "Label=%~1"
set "ProjectPath=%~f2"
set "ProjectResource=%ProjectPath:\=/%"
set "FirstWvb=%TemporaryDirectory%\%~1-First.wvb"
set "SecondWvb=%TemporaryDirectory%\%~1-Second.wvb"
set "FirstWvbResource=%FirstWvb:\=/%"
set "SecondWvbResource=%SecondWvb:\=/%"
set "FirstWvo=%TemporaryDirectory%\%~1-First.wvo"
set "SecondWvo=%TemporaryDirectory%\%~1-Second.wvo"
set "Image=%TemporaryDirectory%\%~1.bin"
set "ImagePrefix=%TemporaryDirectory%\%~1-Image"
set "Map=%TemporaryDirectory%\%~1.map"
set "WindowsApplication=%TemporaryDirectory%\%~1.exe"
set "LinuxApplication=%TemporaryDirectory%\%~1.elf"

"%BuildDriver%" --workspace "%WorkspaceResource%" --project "%ProjectResource%" "%FirstWvbResource%" >nul
if errorlevel 1 exit /b 1
"%BuildDriver%" --workspace "%WorkspaceResource%" --project "%ProjectResource%" "%SecondWvbResource%" >nul
if errorlevel 1 exit /b 1
fc /b "%FirstWvb%" "%SecondWvb%" >nul
if errorlevel 1 exit /b 1

"%Lowerer%" "%FirstWvb%" "%FirstWvo%" >nul
if errorlevel 1 exit /b 1
"%Lowerer%" "%SecondWvb%" "%SecondWvo%" >nul
if errorlevel 1 exit /b 1
fc /b "%FirstWvo%" "%SecondWvo%" >nul
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%FirstWvo%" >nul
if errorlevel 1 exit /b 1

call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main ^
    "%Image%" "%FirstWvo%" >"%Map%"
if errorlevel 1 exit /b 1
set "EntryOffset="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Main address=" "%Map%"') do set "EntryOffset=%%E"
if not defined EntryOffset exit /b 1
echo(%EntryOffset%| findstr /r /x "[0-9][0-9]*" >nul || exit /b 1
copy /b "%Image%" "%ImagePrefix%.chunk-0" >nul
if errorlevel 1 exit /b 1

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
    "%FirstWvb%" "%ImagePrefix%" 1 %EntryOffset% "%WindowsApplication%" windows >nul
if errorlevel 1 exit /b 1
"%WindowsApplication%" >nul
if not "%ERRORLEVEL%"=="0" (
    >&2 echo The %Label% database-storage case did not return 0.
    exit /b 1
)

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
    "%FirstWvb%" "%ImagePrefix%" 1 %EntryOffset% "%LinuxApplication%" linux >nul
if errorlevel 1 exit /b 1
endlocal
exit /b 0
