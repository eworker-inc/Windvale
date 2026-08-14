@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "Development=0"
set "PrepareOnly=0"
if "%~1"=="" goto :arguments_ready
if not "%~2"=="" goto :usage
if /I "%~1"=="--development" set "Development=1"
if /I "%~1"=="--prepare-development-tools" (
    set "Development=1"
    set "PrepareOnly=1"
)
if "%Development%"=="0" goto :usage

:arguments_ready

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

if "%Development%"=="1" (
    call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
        "%RepositoryRoot%\Projects\Tools\Windvale-Compiler-Build-Driver.wvproj" ^
        "%BuildDriverWvb%" >nul
    if errorlevel 1 goto :cleanup
    call :prepare_cached_build_driver "%BuildDriverWvb%"
    if errorlevel 1 goto :cleanup
    set "Lowerer=%RepositoryRoot%\Artifacts\Native-Wvb-To-Wvo-Candidate\Wvb-To-Wvo.exe"
    call :verify_file "%RepositoryRoot%\Artifacts\Native-Wvb-To-Wvo-Candidate\Wvb-To-Wvo.exe" 6499840 a8041f1053fa04598a762998d7820ffc0b704b92494d3ae87ebb8d95ac94450e
    if errorlevel 1 goto :cleanup
) else (
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
)

if "%PrepareOnly%"=="1" (
    set "Result=0"
    goto :cleanup
)

if "%Development%"=="1" (
    call :verify_host_storage ^
        "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Host-Storage.wvproj"
    if errorlevel 1 (
        >&2 echo The native database storage development host-storage stage failed.
        goto :cleanup
    )
    call :verify_host_tree_reader ^
        "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Host-Tree-Reader.wvproj"
    if errorlevel 1 (
        >&2 echo The native database storage development tree-update stage failed.
        goto :cleanup
    )
    set "Result=0"
    goto :cleanup
)

call :verify_target Nested ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Nested-Record-Fields.wvproj"
if errorlevel 1 goto :cleanup
call :verify_target Publication ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Storage-Publication.wvproj"
if errorlevel 1 goto :cleanup
call :verify_target Recovery ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Storage-Recovery.wvproj"
if errorlevel 1 goto :cleanup
call :verify_target SingleWriter ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Single-Writer-Commit.wvproj"
if errorlevel 1 goto :cleanup
call :verify_target TreeNode ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Tree-Node.wvproj"
if errorlevel 1 goto :cleanup
call :verify_target RootSplit ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Root-Split.wvproj"
if errorlevel 1 goto :cleanup
call :verify_target DepthTwo ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Depth-Two-Upsert.wvproj"
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
call :verify_host_tree_reader ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Host-Tree-Reader.wvproj"
if errorlevel 1 goto :cleanup

set "Result=0"

:cleanup
for %%R in ("%TemporaryDirectory%") do set "ResolvedTemporaryDirectory=%%~fR"
echo(%ResolvedTemporaryDirectory%| findstr /b /i /c:"%TEMP%\windvale-database-storage-" >nul || exit /b 1
if exist "%ResolvedTemporaryDirectory%\." rmdir /s /q "%ResolvedTemporaryDirectory%"
if not "%Result%"=="0" exit /b %Result%
if "%PrepareOnly%"=="1" (
    echo native database storage development tools status=Passed checkpoint=%ToolCheckpoint%
    exit /b 0
)
if "%Development%"=="1" (
    echo native database storage development status=Passed cases=2 local-results=0 tools=%ToolCheckpoint%
    exit /b 0
)
echo native database storage status=Passed cases=14 local-results=0 cross-host-images=Verified
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
if errorlevel 1 (
    >&2 echo The native host-storage source build failed.
    exit /b 1
)
if not "%Development%"=="1" (
    "%BuildDriver%" --workspace "%WorkspaceResource%" --project "%ProjectResource%" "%SecondWvbResource%" >nul
    if errorlevel 1 exit /b 1
    fc /b "%FirstWvb%" "%SecondWvb%" >nul
    if errorlevel 1 exit /b 1
)

"%Lowerer%" "%FirstWvb%" "%FirstWvo%" >nul
if errorlevel 1 (
    >&2 echo The native host-storage lowering failed.
    exit /b 1
)
if not "%Development%"=="1" (
    "%Lowerer%" "%SecondWvb%" "%SecondWvo%" >nul
    if errorlevel 1 exit /b 1
    fc /b "%FirstWvo%" "%SecondWvo%" >nul
    if errorlevel 1 exit /b 1
)
call "%RepositoryRoot%\Tools\Native\Check-Wvo.cmd" "%FirstWvo%" >nul
if errorlevel 1 exit /b 1

call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" ^
    "%RepositoryRoot%\Runtime\Native\X64-Random-Access-Storage-Host.wva" ^
    "%CommonFirst%" >nul
if errorlevel 1 exit /b 1
if not "%Development%"=="1" (
    call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" ^
        "%RepositoryRoot%\Runtime\Native\X64-Random-Access-Storage-Host.wva" ^
        "%CommonSecond%" >nul
    if errorlevel 1 exit /b 1
    fc /b "%CommonFirst%" "%CommonSecond%" >nul
    if errorlevel 1 exit /b 1
)
call "%RepositoryRoot%\Tools\Native\Check-Wvo.cmd" "%CommonFirst%" >nul
if errorlevel 1 exit /b 1

call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" ^
    "%RepositoryRoot%\Runtime\Native\Windows-X64-Random-Access-Storage.wva" ^
    "%WindowsPlatform%" >nul
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Check-Wvo.cmd" "%WindowsPlatform%" >nul
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" ^
    "%RepositoryRoot%\Runtime\Native\Linux-X64-Random-Access-Storage.wva" ^
    "%LinuxPlatform%" >nul
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Check-Wvo.cmd" "%LinuxPlatform%" >nul
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

for %%S in (0 1 2 3 4) do (
    call :verify_host_storage_interruption ^
        "%WindowsApplication%" "%InitialFile%" %%S
    if errorlevel 1 exit /b 1
)

if "%Development%"=="1" (
    endlocal
    exit /b 0
)

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

:verify_host_tree_reader
setlocal EnableExtensions DisableDelayedExpansion
set "ProjectPath=%~f1"
set "ProjectResource=%ProjectPath:\=/%"
set "FirstWvb=%TemporaryDirectory%\HostTreeReader-First.wvb"
set "SecondWvb=%TemporaryDirectory%\HostTreeReader-Second.wvb"
set "FirstWvbResource=%FirstWvb:\=/%"
set "SecondWvbResource=%SecondWvb:\=/%"
set "FirstWvo=%TemporaryDirectory%\HostTreeReader-First.wvo"
set "SecondWvo=%TemporaryDirectory%\HostTreeReader-Second.wvo"
set "Common=%TemporaryDirectory%\HostStorage-Common-First.wvo"
set "WindowsPlatform=%TemporaryDirectory%\HostStorage-Windows.wvo"
set "LinuxPlatform=%TemporaryDirectory%\HostStorage-Linux.wvo"
set "WindowsImage=%TemporaryDirectory%\HostTreeReader-Windows.bin"
set "WindowsImagePrefix=%TemporaryDirectory%\HostTreeReader-Windows-Image"
set "WindowsMap=%TemporaryDirectory%\HostTreeReader-Windows.map"
set "WindowsApplication=%TemporaryDirectory%\HostTreeReader.exe"
set "LinuxImage=%TemporaryDirectory%\HostTreeReader-Linux.bin"
set "LinuxImagePrefix=%TemporaryDirectory%\HostTreeReader-Linux-Image"
set "LinuxMap=%TemporaryDirectory%\HostTreeReader-Linux.map"
set "LinuxApplication=%TemporaryDirectory%\HostTreeReader.elf"
set "InitialFile=%TemporaryDirectory%\HostStorage-Run\Windvale-Database-Storage.initial"
set "RunDirectory=%TemporaryDirectory%\HostTreeReader-Run"
set "StorageFile=%RunDirectory%\Windvale-Database-Storage.bin"
set "DepthTwoCommittedFile=%RunDirectory%\Windvale-Database-Storage.depth-two"
set "CommittedFile=%RunDirectory%\Windvale-Database-Storage.committed"

"%BuildDriver%" --workspace "%WorkspaceResource%" --project "%ProjectResource%" "%FirstWvbResource%" >nul
if errorlevel 1 (
    >&2 echo The native host tree-reader source build failed.
    exit /b 1
)
if not "%Development%"=="1" (
    "%BuildDriver%" --workspace "%WorkspaceResource%" --project "%ProjectResource%" "%SecondWvbResource%" >nul
    if errorlevel 1 exit /b 1
    fc /b "%FirstWvb%" "%SecondWvb%" >nul
    if errorlevel 1 exit /b 1
)
"%Lowerer%" "%FirstWvb%" "%FirstWvo%" >nul
if errorlevel 1 (
    >&2 echo The native host tree-reader lowering failed.
    exit /b 1
)
if not "%Development%"=="1" (
    "%Lowerer%" "%SecondWvb%" "%SecondWvo%" >nul
    if errorlevel 1 exit /b 1
    fc /b "%FirstWvo%" "%SecondWvo%" >nul
    if errorlevel 1 exit /b 1
)
if not exist "%Common%" (
    >&2 echo The native host tree-reader common provider object is missing.
    exit /b 1
)
if not exist "%WindowsPlatform%" (
    >&2 echo The native host tree-reader Windows provider object is missing.
    exit /b 1
)

call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 ^
    Storage_host_entry "%WindowsImage%" "%FirstWvo%" ^
    "%Common%" "%WindowsPlatform%" >"%WindowsMap%"
if errorlevel 1 (
    >&2 echo The native host tree-reader Windows link failed.
    exit /b 1
)
set "WindowsEntry="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Storage_host_entry address=" "%WindowsMap%"') do set "WindowsEntry=%%E"
if not defined WindowsEntry (
    >&2 echo The native host tree-reader Windows entry was not reported by the linker.
    exit /b 1
)
echo(%WindowsEntry%| findstr /r /x "[0-9][0-9]*" >nul || exit /b 1
copy /b "%WindowsImage%" "%WindowsImagePrefix%.chunk-0" >nul
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
    "%FirstWvb%" "%WindowsImagePrefix%" 1 %WindowsEntry% ^
    "%WindowsApplication%" windows >nul
if errorlevel 1 (
    >&2 echo The native host tree-reader Windows packaging failed.
    exit /b 1
)

mkdir "%RunDirectory%" || exit /b 1
copy /b "%InitialFile%" "%StorageFile%" >nul || exit /b 1
pushd "%RunDirectory%" || exit /b 1
"%WindowsApplication%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="0" (
    >&2 echo The native host tree-reader publication returned %ApplicationResult%, expected 0.
    exit /b 1
)
for %%F in ("%StorageFile%") do if not "%%~zF"=="20992" (
    >&2 echo The native host tree-reader first generation length is %%~zF, expected 20992.
    exit /b 1
)
copy /b "%StorageFile%" "%DepthTwoCommittedFile%" >nul || exit /b 1
pushd "%RunDirectory%" || exit /b 1
"%WindowsApplication%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="0" (
    >&2 echo The native host tree-reader depth-two update returned %ApplicationResult%, expected 0.
    exit /b 1
)
for %%F in ("%StorageFile%") do if not "%%~zF"=="33280" (
    >&2 echo The native host tree-reader updated generation length is %%~zF, expected 33280.
    exit /b 1
)
copy /b "%StorageFile%" "%CommittedFile%" >nul || exit /b 1
pushd "%RunDirectory%" || exit /b 1
"%WindowsApplication%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="0" (
    >&2 echo The native host tree-reader stable reopen returned %ApplicationResult%, expected 0.
    exit /b 1
)
fc /b "%CommittedFile%" "%StorageFile%" >nul || exit /b 1
for %%S in (0 1 2 3 4) do (
    call :verify_host_tree_reader_interruption ^
        "%WindowsApplication%" "%InitialFile%" %%S
    if errorlevel 1 exit /b 1
    call :verify_host_tree_reader_update_interruption ^
        "%WindowsApplication%" "%DepthTwoCommittedFile%" %%S
    if errorlevel 1 exit /b 1
)

if "%Development%"=="1" (
    endlocal
    exit /b 0
)
if not exist "%LinuxPlatform%" exit /b 1
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 ^
    Storage_host_entry "%LinuxImage%" "%FirstWvo%" ^
    "%Common%" "%LinuxPlatform%" >"%LinuxMap%"
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

:verify_host_tree_reader_interruption
setlocal EnableExtensions DisableDelayedExpansion
set "Application=%~f1"
set "Initial=%~f2"
set "Step=%~3"
set "ScenarioDirectory=%TemporaryDirectory%\HostTreeReader-Interruption-%Step%"
set "ScenarioStorage=%ScenarioDirectory%\Windvale-Database-Storage.bin"
mkdir "%ScenarioDirectory%" || exit /b 1
copy /b "%Initial%" "%ScenarioStorage%" >nul || exit /b 1
set /a MarkerLength=4609+Step
fsutil file seteof "%ScenarioStorage%" %MarkerLength% >nul || exit /b 1
pushd "%ScenarioDirectory%" || exit /b 1
"%Application%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
set /a ExpectedResult=100+Step
if not "%ApplicationResult%"=="%ExpectedResult%" (
    >&2 echo The native host tree-reader interruption %Step% returned %ApplicationResult%, expected %ExpectedResult%.
    exit /b 1
)
pushd "%ScenarioDirectory%" || exit /b 1
"%Application%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="0" (
    >&2 echo The native host tree-reader restart %Step% returned %ApplicationResult%, expected 0.
    exit /b 1
)
pushd "%ScenarioDirectory%" || exit /b 1
"%Application%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="0" (
    >&2 echo The native host tree-reader convergence %Step% returned %ApplicationResult%, expected 0.
    exit /b 1
)
for %%F in ("%ScenarioStorage%") do if not "%%~zF"=="33280" (
    >&2 echo The native host tree-reader convergence %Step% length is %%~zF, expected 33280.
    exit /b 1
)
endlocal
exit /b 0

:verify_host_tree_reader_update_interruption
setlocal EnableExtensions DisableDelayedExpansion
set "Application=%~f1"
set "Committed=%~f2"
set "Step=%~3"
set "ScenarioDirectory=%TemporaryDirectory%\HostTreeReader-Update-Interruption-%Step%"
set "ScenarioStorage=%ScenarioDirectory%\Windvale-Database-Storage.bin"
mkdir "%ScenarioDirectory%" || exit /b 1
copy /b "%Committed%" "%ScenarioStorage%" >nul || exit /b 1
set /a MarkerLength=20993+Step
fsutil file seteof "%ScenarioStorage%" %MarkerLength% >nul || exit /b 1
pushd "%ScenarioDirectory%" || exit /b 1
"%Application%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
set /a ExpectedResult=110+Step
if not "%ApplicationResult%"=="%ExpectedResult%" (
    >&2 echo The native host tree-reader update interruption %Step% returned %ApplicationResult%, expected %ExpectedResult%.
    exit /b 1
)
pushd "%ScenarioDirectory%" || exit /b 1
"%Application%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="0" (
    >&2 echo The native host tree-reader update restart %Step% returned %ApplicationResult%, expected 0.
    exit /b 1
)
for %%F in ("%ScenarioStorage%") do if not "%%~zF"=="33280" (
    >&2 echo The native host tree-reader update restart %Step% length is %%~zF, expected 33280.
    exit /b 1
)
endlocal
exit /b 0

:verify_host_storage_interruption
setlocal EnableExtensions DisableDelayedExpansion
set "Application=%~f1"
set "Initial=%~f2"
set "Step=%~3"
set "ScenarioDirectory=%TemporaryDirectory%\HostStorage-Interruption-%Step%"
set "ScenarioStorage=%ScenarioDirectory%\Windvale-Database-Storage.bin"
mkdir "%ScenarioDirectory%" || exit /b 1
copy /b "%Initial%" "%ScenarioStorage%" >nul || exit /b 1
set /a MarkerLength=4609+Step
fsutil file seteof "%ScenarioStorage%" %MarkerLength% >nul || exit /b 1

pushd "%ScenarioDirectory%" || exit /b 1
"%Application%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
set /a ExpectedResult=90+Step
if not "%ApplicationResult%"=="%ExpectedResult%" (
    >&2 echo The native host-storage interruption %Step% returned %ApplicationResult%, expected %ExpectedResult%.
    exit /b 1
)

pushd "%ScenarioDirectory%" || exit /b 1
"%Application%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="0" (
    >&2 echo The native host-storage restart %Step% returned %ApplicationResult%, expected 0.
    exit /b 1
)
for %%F in ("%ScenarioStorage%") do set "ScenarioBytes=%%~zF"
if %Step% LEQ 2 if not "%ScenarioBytes%"=="4608" exit /b 1
if "%Step%"=="3" if not "%ScenarioBytes%"=="4608" if not "%ScenarioBytes%"=="12800" exit /b 1
if "%Step%"=="4" if not "%ScenarioBytes%"=="12800" exit /b 1
endlocal
exit /b 0

:verify_file
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~3" >nul
exit /b %ERRORLEVEL%

:prepare_cached_build_driver
setlocal EnableExtensions DisableDelayedExpansion
set "CheckpointInput=%~f1"
call :get_sha256 "%CheckpointInput%" CheckpointInputSha256
if errorlevel 1 exit /b 1
call :get_sha256 "%RepositoryRoot%\Tools\Native\Package-Segmented-Compiler-Wvb.cmd" CheckpointPackageSha256
if errorlevel 1 exit /b 1
call :get_sha256 "%RepositoryRoot%\Tools\Native\Stage-Compiler-Wvb.cmd" CheckpointStageSha256
if errorlevel 1 exit /b 1
call :get_sha256 "%RepositoryRoot%\Tools\Native\Link-Staged-Compiler-Wvo.cmd" CheckpointLinkSha256
if errorlevel 1 exit /b 1
call :get_sha256 "%RepositoryRoot%\Tools\Native\Transport-Compiler-Image.cmd" CheckpointTransportSha256
if errorlevel 1 exit /b 1
call :get_sha256 "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" CheckpointHostedSha256
if errorlevel 1 exit /b 1
call :get_sha256 "%RepositoryRoot%\Artifacts\Native-Hosted-Container-Toolset-Candidate\SHA256SUMS" CheckpointInventorySha256
if errorlevel 1 exit /b 1
set "CheckpointMaterial=build-driver-v1-windows-profile-2-%CheckpointInputSha256%-%CheckpointPackageSha256%-%CheckpointStageSha256%-%CheckpointLinkSha256%-%CheckpointTransportSha256%-%CheckpointHostedSha256%-%CheckpointInventorySha256%"
set "CheckpointKeyMaterial=%TemporaryDirectory%\Checkpoint-Key.txt"
>"%CheckpointKeyMaterial%" echo %CheckpointMaterial%
call :get_sha256 "%CheckpointKeyMaterial%" CheckpointKey
if errorlevel 1 exit /b 1

if defined WINDVALE_NATIVE_CACHE_ROOT (
    set "CheckpointRoot=%WINDVALE_NATIVE_CACHE_ROOT%"
) else (
    if not defined LOCALAPPDATA exit /b 1
    set "CheckpointRoot=%LOCALAPPDATA%\Windvale\Native-Tool-Cache"
)
if not exist "%CheckpointRoot%\." mkdir "%CheckpointRoot%" || exit /b 1
fsutil reparsepoint query "%CheckpointRoot%" >nul 2>nul
if not errorlevel 1 exit /b 1
set "CheckpointFamily=%CheckpointRoot%\build-driver-v1\windows-profile-2"
if not exist "%CheckpointFamily%\." mkdir "%CheckpointFamily%" || exit /b 1
fsutil reparsepoint query "%CheckpointFamily%" >nul 2>nul
if not errorlevel 1 exit /b 1
set "CheckpointDirectory=%CheckpointFamily%\%CheckpointKey%"
set "CheckpointManifest=%CheckpointDirectory%\Checkpoint.txt"
set "CheckpointApplication=%CheckpointDirectory%\Build-Driver.exe"
set "CheckpointWasCreated=0"

if exist "%CheckpointDirectory%\." (
    fsutil reparsepoint query "%CheckpointDirectory%" >nul 2>nul
    if not errorlevel 1 exit /b 1
    goto :validate_checkpoint
)

set "CheckpointTemporary=%CheckpointFamily%\.new-%CheckpointKey%-%RANDOM%-%RANDOM%"
if exist "%CheckpointTemporary%\." exit /b 1
mkdir "%CheckpointTemporary%" || exit /b 1
set "CheckpointCandidate=%CheckpointTemporary%\Build-Driver.exe"
call "%RepositoryRoot%\Tools\Native\Package-Segmented-Compiler-Wvb.cmd" ^
    2 "%CheckpointInput%" "%CheckpointCandidate%" >nul
if errorlevel 1 (
    if exist "%CheckpointCandidate%" del /f /q "%CheckpointCandidate%" >nul 2>nul
    rmdir "%CheckpointTemporary%" >nul 2>nul
    exit /b 1
)
call :get_sha256 "%CheckpointCandidate%" CheckpointOutputSha256
if errorlevel 1 exit /b 1
for %%F in ("%CheckpointCandidate%") do set "CheckpointOutputBytes=%%~zF"
if %CheckpointOutputBytes% LEQ 0 exit /b 1
if %CheckpointOutputBytes% GTR 67108864 exit /b 1
>"%CheckpointTemporary%\Checkpoint.txt" echo windvale-native-tool-checkpoint 1
>>"%CheckpointTemporary%\Checkpoint.txt" echo key %CheckpointKey%
>>"%CheckpointTemporary%\Checkpoint.txt" echo input-sha256 %CheckpointInputSha256%
>>"%CheckpointTemporary%\Checkpoint.txt" echo output-bytes %CheckpointOutputBytes%
>>"%CheckpointTemporary%\Checkpoint.txt" echo output-sha256 %CheckpointOutputSha256%
move "%CheckpointTemporary%" "%CheckpointDirectory%" >nul
if errorlevel 1 exit /b 1
set "CheckpointWasCreated=1"

:validate_checkpoint
if not exist "%CheckpointManifest%" exit /b 1
if not exist "%CheckpointApplication%" exit /b 1
fsutil reparsepoint query "%CheckpointManifest%" >nul 2>nul
if not errorlevel 1 exit /b 1
fsutil reparsepoint query "%CheckpointApplication%" >nul 2>nul
if not errorlevel 1 exit /b 1
for %%F in ("%CheckpointManifest%") do if %%~zF GTR 512 exit /b 1
for %%F in ("%CheckpointApplication%") do set "CheckpointActualBytes=%%~zF"
if %CheckpointActualBytes% LEQ 0 exit /b 1
if %CheckpointActualBytes% GTR 67108864 exit /b 1
call :get_sha256 "%CheckpointApplication%" CheckpointActualSha256
if errorlevel 1 exit /b 1
set "CheckpointExpected=%TemporaryDirectory%\Checkpoint-Expected.txt"
>"%CheckpointExpected%" echo windvale-native-tool-checkpoint 1
>>"%CheckpointExpected%" echo key %CheckpointKey%
>>"%CheckpointExpected%" echo input-sha256 %CheckpointInputSha256%
>>"%CheckpointExpected%" echo output-bytes %CheckpointActualBytes%
>>"%CheckpointExpected%" echo output-sha256 %CheckpointActualSha256%
fc /b "%CheckpointExpected%" "%CheckpointManifest%" >nul || exit /b 1
set "BuildDriver=%CheckpointApplication%"
if "%CheckpointWasCreated%"=="1" (
    set "ToolCheckpoint=Created"
) else (
    set "ToolCheckpoint=Hit"
)
if not defined ToolCheckpoint set "ToolCheckpoint=Hit"

:prepare_cache_ok
set "PreparedBuildDriver=%BuildDriver%"
set "PreparedToolCheckpoint=%ToolCheckpoint%"
endlocal & set "BuildDriver=%PreparedBuildDriver%" & set "ToolCheckpoint=%PreparedToolCheckpoint%"
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

:usage
>&2 echo Usage: Tools\Native\Test-Database-Storage.cmd [--development^|--prepare-development-tools]
exit /b 64

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
call "%RepositoryRoot%\Tools\Native\Check-Wvo.cmd" "%FirstWvo%" >nul
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
call "%RepositoryRoot%\Tools\Native\Check-Wvo.cmd" "%FirstBridge%" >nul
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
call "%RepositoryRoot%\Tools\Native\Check-Wvo.cmd" "%FirstWvo%" >nul
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
