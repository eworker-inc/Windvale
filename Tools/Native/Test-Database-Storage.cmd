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
call :verify_target ProviderTable ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Capability-Provider-Table.wvproj"
if errorlevel 1 goto :cleanup
call :verify_target ProviderCall ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-X64-Provider-Call.wvproj"
if errorlevel 1 goto :cleanup
call :verify_target Context9 ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Execution-Context-9.wvproj"
if errorlevel 1 goto :cleanup
call :verify_storage_lowering ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-X64-Storage-Random-Access.wvproj"
if errorlevel 1 goto :cleanup
call :verify_host_storage ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Host-Storage.wvproj"
if errorlevel 1 goto :cleanup

set "Result=0"

:cleanup
for %%R in ("%TemporaryDirectory%") do set "ResolvedTemporaryDirectory=%%~fR"
echo(%ResolvedTemporaryDirectory%| findstr /b /i /c:"%TEMP%\windvale-database-storage-" >nul || exit /b 1
if exist "%ResolvedTemporaryDirectory%\." rmdir /s /q "%ResolvedTemporaryDirectory%"
if not "%Result%"=="0" exit /b %Result%
echo native database storage status=Passed cases=9 local-results=0 cross-host-images=Verified
exit /b 0

:verify_host_storage
setlocal EnableExtensions DisableDelayedExpansion
set "ProjectPath=%~f1"
set "ProjectResource=%ProjectPath:\=/%"
set "FirstWvb=%TemporaryDirectory%\HostStorage-First.wvb"
set "SecondWvb=%TemporaryDirectory%\HostStorage-Second.wvb"
set "FirstWvbResource=%FirstWvb:\=/%"
set "SecondWvbResource=%SecondWvb:\=/%"
set "FirstWvo=%TemporaryDirectory%\HostStorage-First.wvo"
set "SecondWvo=%TemporaryDirectory%\HostStorage-Second.wvo"
set "CommonFirst=%TemporaryDirectory%\HostStorage-Common-First.wvo"
set "CommonSecond=%TemporaryDirectory%\HostStorage-Common-Second.wvo"
set "WindowsPlatform=%TemporaryDirectory%\HostStorage-Windows.wvo"
set "LinuxPlatform=%TemporaryDirectory%\HostStorage-Linux.wvo"
set "WindowsImage=%TemporaryDirectory%\HostStorage-Windows.bin"
set "WindowsImagePrefix=%TemporaryDirectory%\HostStorage-Windows-Image"
set "WindowsMap=%TemporaryDirectory%\HostStorage-Windows.map"
set "WindowsApplication=%TemporaryDirectory%\HostStorage.exe"
set "LinuxImage=%TemporaryDirectory%\HostStorage-Linux.bin"
set "LinuxImagePrefix=%TemporaryDirectory%\HostStorage-Linux-Image"
set "LinuxMap=%TemporaryDirectory%\HostStorage-Linux.map"
set "LinuxApplication=%TemporaryDirectory%\HostStorage.elf"
set "RunDirectory=%TemporaryDirectory%\HostStorage-Run"
set "StorageFile=%RunDirectory%\Windvale-Database-Storage.bin"
set "InitialFile=%RunDirectory%\Windvale-Database-Storage.initial"

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

call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" ^
    "%RepositoryRoot%\Runtime\Native\X64-Random-Access-Storage-Host.wva" ^
    "%CommonFirst%" >nul
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" ^
    "%RepositoryRoot%\Runtime\Native\X64-Random-Access-Storage-Host.wva" ^
    "%CommonSecond%" >nul
if errorlevel 1 exit /b 1
fc /b "%CommonFirst%" "%CommonSecond%" >nul
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%CommonFirst%" >nul
if errorlevel 1 exit /b 1

call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" ^
    "%RepositoryRoot%\Runtime\Native\Windows-X64-Random-Access-Storage.wva" ^
    "%WindowsPlatform%" >nul
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%WindowsPlatform%" >nul
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" ^
    "%RepositoryRoot%\Runtime\Native\Linux-X64-Random-Access-Storage.wva" ^
    "%LinuxPlatform%" >nul
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%LinuxPlatform%" >nul
if errorlevel 1 exit /b 1

call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 ^
    Storage_host_entry "%WindowsImage%" "%FirstWvo%" ^
    "%CommonFirst%" "%WindowsPlatform%" >"%WindowsMap%"
if errorlevel 1 exit /b 1
set "WindowsEntry="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Storage_host_entry address=" "%WindowsMap%"') do set "WindowsEntry=%%E"
if not defined WindowsEntry exit /b 1
echo(%WindowsEntry%| findstr /r /x "[0-9][0-9]*" >nul || exit /b 1
copy /b "%WindowsImage%" "%WindowsImagePrefix%.chunk-0" >nul
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
    "%FirstWvb%" "%WindowsImagePrefix%" 1 %WindowsEntry% ^
    "%WindowsApplication%" windows >nul
if errorlevel 1 exit /b 1

mkdir "%RunDirectory%" || exit /b 1
pushd "%RunDirectory%" || exit /b 1
"%WindowsApplication%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="0" (
    >&2 echo The native host-storage create run returned %ApplicationResult%, expected 0.
    exit /b 1
)
for %%F in ("%StorageFile%") do if not "%%~zF"=="4608" exit /b 1
copy /b "%StorageFile%" "%InitialFile%" >nul
if errorlevel 1 exit /b 1

fsutil file seteof "%StorageFile%" 4625 >nul
if errorlevel 1 exit /b 1
for %%F in ("%StorageFile%") do if not "%%~zF"=="4625" exit /b 1
pushd "%RunDirectory%" || exit /b 1
"%WindowsApplication%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="0" (
    >&2 echo The native host-storage recovery run returned %ApplicationResult%, expected 0.
    exit /b 1
)
for %%F in ("%StorageFile%") do if not "%%~zF"=="4608" exit /b 1
fc /b "%InitialFile%" "%StorageFile%" >nul
if errorlevel 1 exit /b 1

pushd "%RunDirectory%" || exit /b 1
"%WindowsApplication%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="0" (
    >&2 echo The native host-storage stable reopen returned %ApplicationResult%, expected 0.
    exit /b 1
)
fc /b "%InitialFile%" "%StorageFile%" >nul
if errorlevel 1 exit /b 1

call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 ^
    Storage_host_entry "%LinuxImage%" "%FirstWvo%" ^
    "%CommonFirst%" "%LinuxPlatform%" >"%LinuxMap%"
if errorlevel 1 exit /b 1
set "LinuxEntry="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Storage_host_entry address=" "%LinuxMap%"') do set "LinuxEntry=%%E"
if not defined LinuxEntry exit /b 1
echo(%LinuxEntry%| findstr /r /x "[0-9][0-9]*" >nul || exit /b 1
copy /b "%LinuxImage%" "%LinuxImagePrefix%.chunk-0" >nul
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
    "%FirstWvb%" "%LinuxImagePrefix%" 1 %LinuxEntry% ^
    "%LinuxApplication%" linux >nul
if errorlevel 1 exit /b 1
endlocal
exit /b 0

:verify_storage_lowering
setlocal EnableExtensions DisableDelayedExpansion
set "ProjectPath=%~f1"
set "ProjectResource=%ProjectPath:\=/%"
set "FirstWvb=%TemporaryDirectory%\StorageLowering-First.wvb"
set "SecondWvb=%TemporaryDirectory%\StorageLowering-Second.wvb"
set "FirstWvbResource=%FirstWvb:\=/%"
set "SecondWvbResource=%SecondWvb:\=/%"
set "FirstWvo=%TemporaryDirectory%\StorageLowering-First.wvo"
set "SecondWvo=%TemporaryDirectory%\StorageLowering-Second.wvo"
set "FirstReport=%TemporaryDirectory%\StorageLowering-First.txt"
set "SecondReport=%TemporaryDirectory%\StorageLowering-Second.txt"
set "FirstBridge=%TemporaryDirectory%\StorageLowering-Bridge-First.wvo"
set "SecondBridge=%TemporaryDirectory%\StorageLowering-Bridge-Second.wvo"
set "Image=%TemporaryDirectory%\StorageLowering.bin"
set "ImagePrefix=%TemporaryDirectory%\StorageLowering-Image"
set "Map=%TemporaryDirectory%\StorageLowering.map"
set "WindowsApplication=%TemporaryDirectory%\StorageLowering.exe"
set "LinuxApplication=%TemporaryDirectory%\StorageLowering.elf"

"%BuildDriver%" --workspace "%WorkspaceResource%" --project "%ProjectResource%" "%FirstWvbResource%" >nul
if errorlevel 1 exit /b 1
"%BuildDriver%" --workspace "%WorkspaceResource%" --project "%ProjectResource%" "%SecondWvbResource%" >nul
if errorlevel 1 exit /b 1
fc /b "%FirstWvb%" "%SecondWvb%" >nul
if errorlevel 1 exit /b 1

"%Lowerer%" "%FirstWvb%" "%FirstWvo%" >"%FirstReport%"
if errorlevel 1 exit /b 1
"%Lowerer%" "%SecondWvb%" "%SecondWvo%" >"%SecondReport%"
if errorlevel 1 exit /b 1
findstr /b /c:"native x64 status=Valid abi=23 " "%FirstReport%" >nul
if errorlevel 1 exit /b 1
fc /b "%FirstReport%" "%SecondReport%" >nul
if errorlevel 1 exit /b 1
fc /b "%FirstWvo%" "%SecondWvo%" >nul
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%FirstWvo%" >nul
if errorlevel 1 exit /b 1

call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" ^
    "%RepositoryRoot%\Runtime\Native\X64-Random-Access-Storage-Describe-Probe.wva" ^
    "%FirstBridge%" >nul
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" ^
    "%RepositoryRoot%\Runtime\Native\X64-Random-Access-Storage-Describe-Probe.wva" ^
    "%SecondBridge%" >nul
if errorlevel 1 exit /b 1
fc /b "%FirstBridge%" "%SecondBridge%" >nul
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%FirstBridge%" >nul
if errorlevel 1 exit /b 1

call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 ^
    Storage_describe_probe_entry "%Image%" "%FirstWvo%" "%FirstBridge%" >"%Map%"
if errorlevel 1 exit /b 1
set "EntryOffset="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Storage_describe_probe_entry address=" "%Map%"') do set "EntryOffset=%%E"
if not defined EntryOffset exit /b 1
echo(%EntryOffset%| findstr /r /x "[0-9][0-9]*" >nul || exit /b 1
copy /b "%Image%" "%ImagePrefix%.chunk-0" >nul
if errorlevel 1 exit /b 1

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
    "%FirstWvb%" "%ImagePrefix%" 1 %EntryOffset% "%WindowsApplication%" windows >nul
if errorlevel 1 exit /b 1
"%WindowsApplication%" >nul
if not "%ERRORLEVEL%"=="0" (
    >&2 echo The ABI-23 storage describe execution did not return 0.
    exit /b 1
)

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
    "%FirstWvb%" "%ImagePrefix%" 1 %EntryOffset% "%LinuxApplication%" linux >nul
if errorlevel 1 exit /b 1
endlocal
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
