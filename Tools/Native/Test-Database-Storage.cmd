@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "Development=0"
set "PrepareOnly=0"
set "DevelopmentTarget=all"
if "%~1"=="" goto :arguments_ready
if /I "%~1"=="--development" (
    if not "%~2"=="" goto :usage
    set "Development=1"
)
if /I "%~1"=="--development-target" (
    if "%~2"=="" goto :usage
    if not "%~3"=="" goto :usage
    set "Development=1"
    set "DevelopmentTarget=%~2"
)
if /I "%~1"=="--prepare-development-tools" (
    if not "%~2"=="" goto :usage
    set "Development=1"
    set "PrepareOnly=1"
)
if "%Development%"=="0" goto :usage

:arguments_ready

set "SelectedCases=23"
if /I not "%DevelopmentTarget%"=="all" set "SelectedCases="
for %%T in (
    tree-node logical-record typed-row query-ir sql-lowerer json-value json-protocol local-service collection-catalog bootstrap single-leaf
    branch-split root-split depth-two depth-three depth-three-upsert
    tree-path-upsert host-storage
) do if /I "%DevelopmentTarget%"=="%%T" set "SelectedCases=1"
if /I "%DevelopmentTarget%"=="host-tree-reader" set "SelectedCases=2"
if /I "%DevelopmentTarget%"=="json" set "SelectedCases=2"
if /I "%DevelopmentTarget%"=="typed-query" set "SelectedCases=2"
if /I "%DevelopmentTarget%"=="query-sql" set "SelectedCases=2"
if /I "%DevelopmentTarget%"=="typed-query-sql" set "SelectedCases=3"
if /I "%DevelopmentTarget%"=="host-root-writer" set "SelectedCases=2"
if /I "%DevelopmentTarget%"=="host-local-service" set "SelectedCases=2"
if /I "%DevelopmentTarget%"=="engine" set "SelectedCases=3"
if /I "%DevelopmentTarget%"=="host-tree-writer" set "SelectedCases=3"
if not defined SelectedCases goto :usage
if "%PrepareOnly%"=="1" set "SelectedCases=0"
set /a ProgressTotal=SelectedCases+1
set "ProgressCurrent=1"

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
set "ProjectCheckpointHostStorage=NotRun"
set "ProjectCheckpointHostRootWriter=NotRun"
set "ProjectCheckpointHostLocalService=NotRun"
set "ProjectCheckpointHostTreeReader=NotRun"
set "ProjectCheckpointEngine=NotRun"
set "ProjectCheckpointHostTreeWriter=NotRun"
set "ApplicationCheckpointHostStorage=NotRun"
set "ApplicationCheckpointHostRootWriter=NotRun"
set "ApplicationCheckpointHostLocalService=NotRun"
set "ApplicationCheckpointHostTreeReader=NotRun"
set "ApplicationCheckpointEngine=NotRun"
set "ApplicationCheckpointHostTreeWriter=NotRun"
set "ProjectWvbCheckpoint=NotRun"
set "PortableProjectCheckpoints="
set "PortableApplicationCheckpoints="
if "%Development%"=="1" call :read_clock DevelopmentStart
if "%Development%"=="1" call :read_clock ToolsStart
if "%Development%"=="1" echo START native database storage development step=tools item=%ProgressCurrent%/%ProgressTotal% target=%DevelopmentTarget%

if "%Development%"=="1" (
    call "%RepositoryRoot%\Tools\Native\Build-Cached-Project-Wvb.cmd" ^
        "%RepositoryRoot%\Projects\Tools\Windvale-Compiler-Build-Driver.wvproj" ^
        "%BuildDriverWvb%" >"%TemporaryDirectory%\Build-Driver-Wvb-Cache.txt"
    if errorlevel 1 goto :cleanup
    set "ProjectWvbCheckpoint="
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native project wvb cache status=" "%TemporaryDirectory%\Build-Driver-Wvb-Cache.txt"') do set "ProjectWvbCheckpoint=%%S"
    if not defined ProjectWvbCheckpoint goto :cleanup
    call :prepare_cached_build_driver "%BuildDriverWvb%"
    if errorlevel 1 goto :cleanup
    set "Lowerer=%RepositoryRoot%\Artifacts\Native-Wvb-To-Wvo-Candidate\Wvb-To-Wvo.exe"
    call :verify_file "%RepositoryRoot%\Artifacts\Native-Wvb-To-Wvo-Candidate\Wvb-To-Wvo.exe" 7483904 496d9e00c682af316b69c0e7639858fa0da171b4d467c4838b1ed774ac18b4cc
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

if "%Development%"=="1" call :read_clock ToolsEnd
if "%Development%"=="1" call :elapsed_milliseconds ToolsStart ToolsEnd ToolsElapsedMs
if "%Development%"=="1" echo PASS  native database storage development step=tools item=%ProgressCurrent%/%ProgressTotal% target=%DevelopmentTarget% elapsed-ms=%ToolsElapsedMs% tool=%ToolCheckpoint% project-wvb=%ProjectWvbCheckpoint%

if "%PrepareOnly%"=="1" (
    set "Result=0"
    goto :cleanup
)

if "%Development%"=="1" (
    call :read_clock PortableStart
    call :verify_development_target TreeNode tree-node "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Tree-Node.wvproj"
    if errorlevel 1 goto :cleanup
    call :verify_development_target LogicalRecord logical-record "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Logical-Record.wvproj"
    if errorlevel 1 goto :cleanup
    call :verify_development_target TypedRow typed-row "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Typed-Row.wvproj" typed-query typed-query-sql
    if errorlevel 1 goto :cleanup
    call :verify_development_target QueryIr query-ir "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Query-Ir.wvproj" typed-query query-sql typed-query-sql
    if errorlevel 1 goto :cleanup
    call :verify_development_target SqlLowerer sql-lowerer "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Sql-Lowerer.wvproj" query-sql typed-query-sql
    if errorlevel 1 goto :cleanup
    call :verify_development_target JsonValue json-value "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Json-Value.wvproj" json
    if errorlevel 1 goto :cleanup
    call :verify_development_target JsonProtocol json-protocol "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Json-Protocol.wvproj" json
    if errorlevel 1 goto :cleanup
    call :verify_development_target LocalService local-service "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Local-Database-Service.wvproj"
    if errorlevel 1 goto :cleanup
    call :verify_development_target CollectionCatalog collection-catalog "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Collection-Catalog.wvproj"
    if errorlevel 1 goto :cleanup
    call :verify_development_target Bootstrap bootstrap "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Bootstrap.wvproj"
    if errorlevel 1 goto :cleanup
    call :verify_development_target SingleLeaf single-leaf "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Single-Leaf-Upsert.wvproj"
    if errorlevel 1 goto :cleanup
    call :verify_development_target BranchSplit branch-split "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Branch-Split.wvproj"
    if errorlevel 1 goto :cleanup
    call :verify_development_target RootSplit root-split "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Root-Split.wvproj"
    if errorlevel 1 goto :cleanup
    call :verify_development_target DepthTwo depth-two "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Depth-Two-Upsert.wvproj"
    if errorlevel 1 goto :cleanup
    call :verify_development_target DepthThree depth-three "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Depth-Three-Root-Growth.wvproj"
    if errorlevel 1 goto :cleanup
    call :verify_development_target DepthThreeUpsert depth-three-upsert "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Depth-Three-Upsert.wvproj"
    if errorlevel 1 goto :cleanup
    call :verify_development_target TreePathUpsert tree-path-upsert "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Tree-Path-Upsert.wvproj"
    if errorlevel 1 goto :cleanup
    call :read_clock PortableEnd
    call :elapsed_milliseconds PortableStart PortableEnd PortableElapsedMs
    set "HostStorageElapsedMs=0"
    set "HostRootWriterElapsedMs=0"
    set "HostLocalServiceElapsedMs=0"
    set "HostTreeReaderElapsedMs=0"
    set "EngineElapsedMs=0"
    set "HostTreeWriterElapsedMs=0"
    call :verify_development_host_targets
    if errorlevel 1 goto :cleanup
    call :read_clock DevelopmentEnd
    call :elapsed_milliseconds DevelopmentStart DevelopmentEnd DevelopmentElapsedMs
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
call :verify_target LogicalRecord ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Logical-Record.wvproj"
if errorlevel 1 goto :cleanup
call :verify_target TypedRow ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Typed-Row.wvproj"
if errorlevel 1 goto :cleanup
call :verify_target QueryIr ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Query-Ir.wvproj"
if errorlevel 1 goto :cleanup
call :verify_target SqlLowerer ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Sql-Lowerer.wvproj"
if errorlevel 1 goto :cleanup
call :verify_target JsonValue ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Json-Value.wvproj"
if errorlevel 1 goto :cleanup
call :verify_target JsonProtocol ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Json-Protocol.wvproj"
if errorlevel 1 goto :cleanup
call :verify_target LocalService ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Local-Database-Service.wvproj"
if errorlevel 1 goto :cleanup
call :verify_target CollectionCatalog ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Collection-Catalog.wvproj"
if errorlevel 1 goto :cleanup
call :verify_target Bootstrap ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Bootstrap.wvproj"
if errorlevel 1 goto :cleanup
call :verify_target SingleLeaf ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Single-Leaf-Upsert.wvproj"
if errorlevel 1 goto :cleanup
call :verify_target BranchSplit ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Branch-Split.wvproj"
if errorlevel 1 goto :cleanup
call :verify_target RootSplit ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Root-Split.wvproj"
if errorlevel 1 goto :cleanup
call :verify_target DepthTwo ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Depth-Two-Upsert.wvproj"
if errorlevel 1 goto :cleanup
call :verify_target DepthThree ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Depth-Three-Root-Growth.wvproj"
if errorlevel 1 goto :cleanup
call :verify_target DepthThreeUpsert ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Depth-Three-Upsert.wvproj"
if errorlevel 1 goto :cleanup
call :verify_target TreePathUpsert ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Tree-Path-Upsert.wvproj"
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
call :verify_host_root_writer ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Host-Root-Writer.wvproj"
if errorlevel 1 goto :cleanup
call :verify_host_root_split_writer ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Host-Root-Fill.wvproj" ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Host-Root-Split-Writer.wvproj" ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Host-Logical-Tree-Get.wvproj"
if errorlevel 1 goto :cleanup
call :verify_host_local_service ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Host-Local-Put.wvproj" ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Host-Local-Get.wvproj"
if errorlevel 1 goto :cleanup
call :verify_host_tree_reader ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Host-Tree-Reader.wvproj"
if errorlevel 1 goto :cleanup
call :verify_host_engine ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Engine.wvproj"
if errorlevel 1 goto :cleanup
call :verify_host_tree_writer ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Host-Tree-Writer.wvproj"
if errorlevel 1 goto :cleanup
call :verify_host_logical_tree_writer ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Host-Logical-Tree-Writer.wvproj" ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Host-Logical-Tree-Get.wvproj"
if errorlevel 1 goto :cleanup

set "Result=0"

:cleanup
for %%R in ("%TemporaryDirectory%") do set "ResolvedTemporaryDirectory=%%~fR"
echo(%ResolvedTemporaryDirectory%| findstr /b /i /c:"%TEMP%\windvale-database-storage-" >nul || exit /b 1
if exist "%ResolvedTemporaryDirectory%\." rmdir /s /q "%ResolvedTemporaryDirectory%"
if not "%Result%"=="0" exit /b %Result%
if "%PrepareOnly%"=="1" (
    echo native database storage development tools status=Passed checkpoint=%ToolCheckpoint% project-wvb=%ProjectWvbCheckpoint%
    exit /b 0
)
if "%Development%"=="1" (
    echo native database storage development timing target=%DevelopmentTarget% tools-ms=%ToolsElapsedMs% portable-ms=%PortableElapsedMs% host-storage-ms=%HostStorageElapsedMs% host-root-writer-ms=%HostRootWriterElapsedMs% host-local-service-ms=%HostLocalServiceElapsedMs% host-tree-reader-ms=%HostTreeReaderElapsedMs% engine-ms=%EngineElapsedMs% host-tree-writer-ms=%HostTreeWriterElapsedMs% total-ms=%DevelopmentElapsedMs%
    echo native database storage development status=Passed target=%DevelopmentTarget% cases=%SelectedCases% local-results=0 tools=%ToolCheckpoint% project-wvb=%ProjectWvbCheckpoint% portable-projects=%PortableProjectCheckpoints% portable-applications=%PortableApplicationCheckpoints% projects=HostStorage:%ProjectCheckpointHostStorage%,HostRootWriter:%ProjectCheckpointHostRootWriter%,HostLocalService:%ProjectCheckpointHostLocalService%,HostTreeReader:%ProjectCheckpointHostTreeReader%,Engine:%ProjectCheckpointEngine%,HostTreeWriter:%ProjectCheckpointHostTreeWriter% applications=HostStorage:%ApplicationCheckpointHostStorage%,HostRootWriter:%ApplicationCheckpointHostRootWriter%,HostLocalService:%ApplicationCheckpointHostLocalService%,HostTreeReader:%ApplicationCheckpointHostTreeReader%,Engine:%ApplicationCheckpointEngine%,HostTreeWriter:%ApplicationCheckpointHostTreeWriter%
    exit /b 0
)
echo native database storage status=Passed cases=32 local-results=0 cross-host-images=Verified
exit /b 0

:verify_development_target
if /I not "%DevelopmentTarget%"=="all" if /I not "%DevelopmentTarget%"=="%~2" if /I not "%DevelopmentTarget%"=="%~4" if /I not "%DevelopmentTarget%"=="%~5" if /I not "%DevelopmentTarget%"=="%~6" exit /b 0
set /a ProgressCurrent+=1
call echo START native database storage development step=%~2 item=%%ProgressCurrent%%/%ProgressTotal% target=%DevelopmentTarget%
call :verify_target "%~1" "%~3"
if errorlevel 1 (
    >&2 echo The native database storage development %~2 stage failed.
    exit /b 1
)
exit /b 0

:verify_development_host_targets
if /I "%DevelopmentTarget%"=="all" goto :development_run_host_storage
if /I "%DevelopmentTarget%"=="host-storage" goto :development_run_host_storage
if /I "%DevelopmentTarget%"=="host-root-writer" goto :development_run_host_storage
if /I "%DevelopmentTarget%"=="host-local-service" goto :development_run_host_storage
if /I "%DevelopmentTarget%"=="host-tree-reader" goto :development_run_host_storage
if /I "%DevelopmentTarget%"=="engine" goto :development_run_host_storage
if /I "%DevelopmentTarget%"=="host-tree-writer" goto :development_run_host_storage
exit /b 0

:development_run_host_storage
set /a ProgressCurrent+=1
call :read_clock HostStorageStart
call echo START native database storage development step=host-storage item=%%ProgressCurrent%%/%ProgressTotal% target=%DevelopmentTarget%
call :verify_host_storage "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Host-Storage.wvproj"
if errorlevel 1 (
    >&2 echo The native database storage development host-storage stage failed.
    exit /b 1
)
call :read_clock HostStorageEnd
call :elapsed_milliseconds HostStorageStart HostStorageEnd HostStorageElapsedMs
call echo PASS  native database storage development step=host-storage item=%%ProgressCurrent%%/%ProgressTotal% target=%DevelopmentTarget% elapsed-ms=%%HostStorageElapsedMs%% project=%%ProjectCheckpointHostStorage%% application=%%ApplicationCheckpointHostStorage%%
if /I "%DevelopmentTarget%"=="host-storage" exit /b 0

if /I "%DevelopmentTarget%"=="all" goto :development_run_host_root_writer
if /I "%DevelopmentTarget%"=="host-root-writer" goto :development_run_host_root_writer
if /I "%DevelopmentTarget%"=="host-local-service" goto :development_run_host_local_service
goto :development_run_host_tree_reader

:development_run_host_root_writer
set /a ProgressCurrent+=1
call :read_clock HostRootWriterStart
call echo START native database storage development step=host-root-writer item=%%ProgressCurrent%%/%ProgressTotal% target=%DevelopmentTarget%
call :verify_host_root_writer "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Host-Root-Writer.wvproj"
if errorlevel 1 (
    >&2 echo The native database storage development host-root-writer stage failed.
    exit /b 1
)
call :verify_host_root_split_writer ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Host-Root-Fill.wvproj" ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Host-Root-Split-Writer.wvproj" ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Host-Logical-Tree-Get.wvproj"
if errorlevel 1 (
    >&2 echo The native database storage development root-split writer stage failed.
    exit /b 1
)
call :read_clock HostRootWriterEnd
call :elapsed_milliseconds HostRootWriterStart HostRootWriterEnd HostRootWriterElapsedMs
call echo PASS  native database storage development step=host-root-writer item=%%ProgressCurrent%%/%ProgressTotal% target=%DevelopmentTarget% elapsed-ms=%%HostRootWriterElapsedMs%% project=%%ProjectCheckpointHostRootWriter%% application=%%ApplicationCheckpointHostRootWriter%%
if /I "%DevelopmentTarget%"=="host-root-writer" exit /b 0

if /I "%DevelopmentTarget%"=="all" goto :development_run_host_local_service
goto :development_run_host_tree_reader

:development_run_host_local_service
set /a ProgressCurrent+=1
call :read_clock HostLocalServiceStart
call echo START native database storage development step=host-local-service item=%%ProgressCurrent%%/%ProgressTotal% target=%DevelopmentTarget%
call :verify_host_local_service ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Host-Local-Put.wvproj" ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Host-Local-Get.wvproj"
if errorlevel 1 (
    >&2 echo The native database storage development host-local-service stage failed.
    exit /b 1
)
call :read_clock HostLocalServiceEnd
call :elapsed_milliseconds HostLocalServiceStart HostLocalServiceEnd HostLocalServiceElapsedMs
call echo PASS  native database storage development step=host-local-service item=%%ProgressCurrent%%/%ProgressTotal% target=%DevelopmentTarget% elapsed-ms=%%HostLocalServiceElapsedMs%% project=%%ProjectCheckpointHostLocalService%% application=%%ApplicationCheckpointHostLocalService%%
if /I "%DevelopmentTarget%"=="host-local-service" exit /b 0

:development_run_host_tree_reader

set /a ProgressCurrent+=1
call :read_clock HostTreeReaderStart
call echo START native database storage development step=host-tree-reader item=%%ProgressCurrent%%/%ProgressTotal% target=%DevelopmentTarget%
call :verify_host_tree_reader "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Host-Tree-Reader.wvproj"
if errorlevel 1 (
    >&2 echo The native database storage development host-tree-reader stage failed.
    exit /b 1
)
call :read_clock HostTreeReaderEnd
call :elapsed_milliseconds HostTreeReaderStart HostTreeReaderEnd HostTreeReaderElapsedMs
call echo PASS  native database storage development step=host-tree-reader item=%%ProgressCurrent%%/%ProgressTotal% target=%DevelopmentTarget% elapsed-ms=%%HostTreeReaderElapsedMs%% project=%%ProjectCheckpointHostTreeReader%% application=%%ApplicationCheckpointHostTreeReader%%
if /I "%DevelopmentTarget%"=="host-tree-reader" exit /b 0
if /I "%DevelopmentTarget%"=="host-tree-writer" goto :development_run_host_tree_writer

set /a ProgressCurrent+=1
call :read_clock EngineStart
call echo START native database storage development step=engine item=%%ProgressCurrent%%/%ProgressTotal% target=%DevelopmentTarget%
call :verify_host_engine "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Engine.wvproj"
if errorlevel 1 (
    >&2 echo The native database storage development engine stage failed.
    exit /b 1
)
call :read_clock EngineEnd
call :elapsed_milliseconds EngineStart EngineEnd EngineElapsedMs
call echo PASS  native database storage development step=engine item=%%ProgressCurrent%%/%ProgressTotal% target=%DevelopmentTarget% elapsed-ms=%%EngineElapsedMs%% project=%%ProjectCheckpointEngine%% application=%%ApplicationCheckpointEngine%%
if /I "%DevelopmentTarget%"=="engine" exit /b 0

:development_run_host_tree_writer
set /a ProgressCurrent+=1
call :read_clock HostTreeWriterStart
call echo START native database storage development step=host-tree-writer item=%%ProgressCurrent%%/%ProgressTotal% target=%DevelopmentTarget%
call :verify_host_tree_writer "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Host-Tree-Writer.wvproj"
if errorlevel 1 (
    >&2 echo The native database storage development host-tree-writer stage failed.
    exit /b 1
)
call :verify_host_logical_tree_writer ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Host-Logical-Tree-Writer.wvproj" ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Database-Host-Logical-Tree-Get.wvproj"
if errorlevel 1 (
    >&2 echo The native database storage development logical tree-writer stage failed.
    exit /b 1
)
call :read_clock HostTreeWriterEnd
call :elapsed_milliseconds HostTreeWriterStart HostTreeWriterEnd HostTreeWriterElapsedMs
call echo PASS  native database storage development step=host-tree-writer item=%%ProgressCurrent%%/%ProgressTotal% target=%DevelopmentTarget% elapsed-ms=%%HostTreeWriterElapsedMs%% project=%%ProjectCheckpointHostTreeWriter%% application=%%ApplicationCheckpointHostTreeWriter%%
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
set "HostStorageCheckpoint=Rebuilt"
set "HostStorageApplicationCheckpoint=Rebuilt"

if "%Development%"=="1" (
    call "%RepositoryRoot%\Tools\Native\Build-Cached-Project-Object.cmd" ^
        "%ProjectPath%" "%BuildDriver%" "%Lowerer%" "%FirstWvb%" "%FirstWvo%" ^
        >"%TemporaryDirectory%\HostStorage-Cache.txt"
    if errorlevel 1 (
        >&2 echo The native host-storage project-object checkpoint failed.
        exit /b 1
    )
    set "HostStorageCheckpoint="
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native project object cache status=" "%TemporaryDirectory%\HostStorage-Cache.txt"') do set "HostStorageCheckpoint=%%S"
    if not defined HostStorageCheckpoint exit /b 1
) else (
    "%BuildDriver%" --workspace "%WorkspaceResource%" --project "%ProjectResource%" "%FirstWvbResource%" >nul
    if errorlevel 1 (
        >&2 echo The native host-storage source build failed.
        exit /b 1
    )
    "%BuildDriver%" --workspace "%WorkspaceResource%" --project "%ProjectResource%" "%SecondWvbResource%" >nul
    if errorlevel 1 exit /b 1
    fc /b "%FirstWvb%" "%SecondWvb%" >nul
    if errorlevel 1 exit /b 1
    "%Lowerer%" "%FirstWvb%" "%FirstWvo%" >nul
    if errorlevel 1 (
        >&2 echo The native host-storage lowering failed.
        exit /b 1
    )
    "%Lowerer%" "%SecondWvb%" "%SecondWvo%" >nul
    if errorlevel 1 exit /b 1
    fc /b "%FirstWvo%" "%SecondWvo%" >nul
    if errorlevel 1 exit /b 1
)
if not "%Development%"=="1" (
    call "%RepositoryRoot%\Tools\Native\Check-Wvo.cmd" "%FirstWvo%" >nul
    if errorlevel 1 exit /b 1
)

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
if "%Development%"=="1" (
    call "%RepositoryRoot%\Tools\Native\Build-Cached-Hosted-Application.cmd" 6 ^
        "%FirstWvb%" "%WindowsImagePrefix%" 1 %WindowsEntry% ^
        "%WindowsApplication%" windows >"%TemporaryDirectory%\HostStorage-Application-Cache.txt"
    if errorlevel 1 exit /b 1
    set "HostStorageApplicationCheckpoint="
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native hosted application cache status=" "%TemporaryDirectory%\HostStorage-Application-Cache.txt"') do set "HostStorageApplicationCheckpoint=%%S"
    if not defined HostStorageApplicationCheckpoint exit /b 1
) else (
    call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
        "%FirstWvb%" "%WindowsImagePrefix%" 1 %WindowsEntry% ^
        "%WindowsApplication%" windows >nul
    if errorlevel 1 exit /b 1
)

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
    endlocal & set "ProjectCheckpointHostStorage=%HostStorageCheckpoint%" & set "ApplicationCheckpointHostStorage=%HostStorageApplicationCheckpoint%"
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

:verify_host_root_writer
setlocal EnableExtensions DisableDelayedExpansion
set "ProjectPath=%~f1"
set "ProjectResource=%ProjectPath:\=/%"
set "FirstWvb=%TemporaryDirectory%\HostRootWriter-First.wvb"
set "SecondWvb=%TemporaryDirectory%\HostRootWriter-Second.wvb"
set "FirstWvbResource=%FirstWvb:\=/%"
set "SecondWvbResource=%SecondWvb:\=/%"
set "FirstWvo=%TemporaryDirectory%\HostRootWriter-First.wvo"
set "SecondWvo=%TemporaryDirectory%\HostRootWriter-Second.wvo"
set "Common=%TemporaryDirectory%\HostStorage-Common-First.wvo"
set "WindowsPlatform=%TemporaryDirectory%\HostStorage-Windows.wvo"
set "LinuxPlatform=%TemporaryDirectory%\HostStorage-Linux.wvo"
set "WindowsImage=%TemporaryDirectory%\HostRootWriter-Windows.bin"
set "WindowsImagePrefix=%TemporaryDirectory%\HostRootWriter-Windows-Image"
set "WindowsMap=%TemporaryDirectory%\HostRootWriter-Windows.map"
set "WindowsApplication=%TemporaryDirectory%\HostRootWriter.exe"
set "LinuxImage=%TemporaryDirectory%\HostRootWriter-Linux.bin"
set "LinuxImagePrefix=%TemporaryDirectory%\HostRootWriter-Linux-Image"
set "LinuxMap=%TemporaryDirectory%\HostRootWriter-Linux.map"
set "LinuxApplication=%TemporaryDirectory%\HostRootWriter.elf"
set "InitialFile=%TemporaryDirectory%\HostStorage-Run\Windvale-Database-Storage.initial"
set "RunDirectory=%TemporaryDirectory%\HostRootWriter-Run"
set "StorageFile=%RunDirectory%\Windvale-Database-Storage.bin"
set "CommittedFile=%RunDirectory%\Windvale-Database-Storage.committed"
set "HostRootWriterCheckpoint=Rebuilt"
set "HostRootWriterApplicationCheckpoint=Rebuilt"

if "%Development%"=="1" (
    call "%RepositoryRoot%\Tools\Native\Build-Cached-Project-Object.cmd" ^
        "%ProjectPath%" "%BuildDriver%" "%Lowerer%" "%FirstWvb%" "%FirstWvo%" ^
        >"%TemporaryDirectory%\HostRootWriter-Cache.txt"
    if errorlevel 1 (
        >&2 echo The native host root-writer project-object checkpoint failed.
        exit /b 1
    )
    set "HostRootWriterCheckpoint="
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native project object cache status=" "%TemporaryDirectory%\HostRootWriter-Cache.txt"') do set "HostRootWriterCheckpoint=%%S"
    if not defined HostRootWriterCheckpoint exit /b 1
) else (
    "%BuildDriver%" --workspace "%WorkspaceResource%" --project "%ProjectResource%" "%FirstWvbResource%" >nul
    if errorlevel 1 exit /b 1
    "%BuildDriver%" --workspace "%WorkspaceResource%" --project "%ProjectResource%" "%SecondWvbResource%" >nul
    if errorlevel 1 exit /b 1
    fc /b "%FirstWvb%" "%SecondWvb%" >nul || exit /b 1
    "%Lowerer%" "%FirstWvb%" "%FirstWvo%" >nul || exit /b 1
    "%Lowerer%" "%SecondWvb%" "%SecondWvo%" >nul || exit /b 1
    fc /b "%FirstWvo%" "%SecondWvo%" >nul || exit /b 1
)
if not exist "%Common%" exit /b 1
if not exist "%WindowsPlatform%" exit /b 1

call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 ^
    Storage_host_entry "%WindowsImage%" "%FirstWvo%" ^
    "%Common%" "%WindowsPlatform%" >"%WindowsMap%"
if errorlevel 1 exit /b 1
set "WindowsEntry="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Storage_host_entry address=" "%WindowsMap%"') do set "WindowsEntry=%%E"
if not defined WindowsEntry exit /b 1
echo(%WindowsEntry%| findstr /r /x "[0-9][0-9]*" >nul || exit /b 1
copy /b "%WindowsImage%" "%WindowsImagePrefix%.chunk-0" >nul || exit /b 1
if "%Development%"=="1" (
    call "%RepositoryRoot%\Tools\Native\Build-Cached-Hosted-Application.cmd" 6 ^
        "%FirstWvb%" "%WindowsImagePrefix%" 1 %WindowsEntry% ^
        "%WindowsApplication%" windows >"%TemporaryDirectory%\HostRootWriter-Application-Cache.txt"
    if errorlevel 1 exit /b 1
    set "HostRootWriterApplicationCheckpoint="
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native hosted application cache status=" "%TemporaryDirectory%\HostRootWriter-Application-Cache.txt"') do set "HostRootWriterApplicationCheckpoint=%%S"
    if not defined HostRootWriterApplicationCheckpoint exit /b 1
) else (
    call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
        "%FirstWvb%" "%WindowsImagePrefix%" 1 %WindowsEntry% ^
        "%WindowsApplication%" windows >nul
    if errorlevel 1 exit /b 1
)

if not exist "%InitialFile%" exit /b 1
mkdir "%RunDirectory%" || exit /b 1
copy /b "%InitialFile%" "%StorageFile%" >nul || exit /b 1
pushd "%RunDirectory%" || exit /b 1
"%WindowsApplication%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="0" (
    >&2 echo The native host root-writer publication returned %ApplicationResult%, expected 0.
    exit /b 1
)
for %%F in ("%StorageFile%") do if not "%%~zF"=="12800" exit /b 1
copy /b "%StorageFile%" "%CommittedFile%" >nul || exit /b 1
pushd "%RunDirectory%" || exit /b 1
"%WindowsApplication%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="0" exit /b 1
fc /b "%CommittedFile%" "%StorageFile%" >nul || exit /b 1
for %%S in (0 1 2 3 4) do (
    call :verify_host_root_writer_interruption ^
        "%WindowsApplication%" "%InitialFile%" "%CommittedFile%" %%S
    if errorlevel 1 exit /b 1
)

if "%Development%"=="1" (
    endlocal & set "ProjectCheckpointHostRootWriter=%HostRootWriterCheckpoint%" & set "ApplicationCheckpointHostRootWriter=%HostRootWriterApplicationCheckpoint%"
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
copy /b "%LinuxImage%" "%LinuxImagePrefix%.chunk-0" >nul || exit /b 1
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
    "%FirstWvb%" "%LinuxImagePrefix%" 1 %LinuxEntry% ^
    "%LinuxApplication%" linux >nul
if errorlevel 1 exit /b 1
endlocal
exit /b 0

:verify_host_root_writer_interruption
setlocal EnableExtensions DisableDelayedExpansion
set "Application=%~f1"
set "Initial=%~f2"
set "Committed=%~f3"
set "Step=%~4"
set "ScenarioDirectory=%TemporaryDirectory%\HostRootWriter-Interruption-%Step%"
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
    >&2 echo The native host root-writer interruption %Step% returned %ApplicationResult%, expected %ExpectedResult%.
    exit /b 1
)
pushd "%ScenarioDirectory%" || exit /b 1
"%Application%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="0" exit /b 1
for %%F in ("%ScenarioStorage%") do if not "%%~zF"=="12800" exit /b 1
fc /b "%Committed%" "%ScenarioStorage%" >nul || exit /b 1
endlocal
exit /b 0

:verify_host_local_service
setlocal EnableExtensions DisableDelayedExpansion
set "PutProject=%~f1"
set "GetProject=%~f2"
set "PutApplication=%TemporaryDirectory%\HostLocal-Put.exe"
set "GetApplication=%TemporaryDirectory%\HostLocal-Get.exe"
set "InitialFile=%TemporaryDirectory%\HostStorage-Run\Windvale-Database-Storage.initial"
set "RunDirectory=%TemporaryDirectory%\HostLocalService-Run"
set "StorageFile=%RunDirectory%\Windvale-Database-Storage.bin"
set "CommittedFile=%RunDirectory%\Windvale-Database-Storage.committed"
set "HostLocalCheckpoint=Rebuilt"
set "HostLocalApplicationCheckpoint=Rebuilt"

call :build_host_local_component "%PutProject%" Put "%PutApplication%"
if errorlevel 1 exit /b 1
call :build_host_local_component "%GetProject%" Get "%GetApplication%"
if errorlevel 1 exit /b 1

if "%Development%"=="1" (
    set "PutProjectCheckpoint="
    set "GetProjectCheckpoint="
    set "PutApplicationCheckpoint="
    set "GetApplicationCheckpoint="
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native project object cache status=" "%TemporaryDirectory%\HostLocal-Put-Cache.txt"') do set "PutProjectCheckpoint=%%S"
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native project object cache status=" "%TemporaryDirectory%\HostLocal-Get-Cache.txt"') do set "GetProjectCheckpoint=%%S"
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native hosted application cache status=" "%TemporaryDirectory%\HostLocal-Put-Application-Cache.txt"') do set "PutApplicationCheckpoint=%%S"
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native hosted application cache status=" "%TemporaryDirectory%\HostLocal-Get-Application-Cache.txt"') do set "GetApplicationCheckpoint=%%S"
    if not defined PutProjectCheckpoint exit /b 1
    if not defined GetProjectCheckpoint exit /b 1
    if not defined PutApplicationCheckpoint exit /b 1
    if not defined GetApplicationCheckpoint exit /b 1
    call set "HostLocalCheckpoint=Put:%%PutProjectCheckpoint%%,Get:%%GetProjectCheckpoint%%"
    call set "HostLocalApplicationCheckpoint=Put:%%PutApplicationCheckpoint%%,Get:%%GetApplicationCheckpoint%%"
)

if not exist "%InitialFile%" exit /b 1
mkdir "%RunDirectory%" || exit /b 1
copy /b "%InitialFile%" "%StorageFile%" >nul || exit /b 1
pushd "%RunDirectory%" || exit /b 1
"%PutApplication%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="0" (
    >&2 echo The native local-service put returned %ApplicationResult%, expected 0.
    exit /b 1
)
for %%F in ("%StorageFile%") do if not "%%~zF"=="12800" exit /b 1
copy /b "%StorageFile%" "%CommittedFile%" >nul || exit /b 1
pushd "%RunDirectory%" || exit /b 1
"%GetApplication%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="0" (
    >&2 echo The native local-service restart get returned %ApplicationResult%, expected 0.
    exit /b 1
)
fc /b "%CommittedFile%" "%StorageFile%" >nul || exit /b 1

if "%Development%"=="1" (
    endlocal & set "ProjectCheckpointHostLocalService=%HostLocalCheckpoint%" & set "ApplicationCheckpointHostLocalService=%HostLocalApplicationCheckpoint%"
    exit /b 0
)
endlocal
exit /b 0

:verify_host_root_split_writer
setlocal EnableExtensions DisableDelayedExpansion
set "FillProject=%~f1"
set "SplitProject=%~f2"
set "GetProject=%~f3"
set "FillApplication=%TemporaryDirectory%\HostRootSplit-Fill.exe"
set "SplitApplication=%TemporaryDirectory%\HostRootSplit-Write.exe"
set "GetApplication=%TemporaryDirectory%\HostRootSplit-Get.exe"
set "InitialFile=%TemporaryDirectory%\HostStorage-Run\Windvale-Database-Storage.initial"
set "RunDirectory=%TemporaryDirectory%\HostRootSplit-Run"
set "StorageFile=%RunDirectory%\Windvale-Database-Storage.bin"
set "CommittedFile=%RunDirectory%\Windvale-Database-Storage.committed"

call :build_host_local_component "%FillProject%" RootFill "%FillApplication%"
if errorlevel 1 exit /b 1
call :build_host_local_component "%SplitProject%" RootSplit "%SplitApplication%"
if errorlevel 1 exit /b 1
call :build_host_local_component "%GetProject%" RootSplitGet "%GetApplication%"
if errorlevel 1 exit /b 1

if "%Development%"=="1" (
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native project object cache status=" "%TemporaryDirectory%\HostLocal-RootFill-Cache.txt"') do set "FillProjectCheckpoint=%%S"
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native project object cache status=" "%TemporaryDirectory%\HostLocal-RootSplit-Cache.txt"') do set "SplitProjectCheckpoint=%%S"
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native project object cache status=" "%TemporaryDirectory%\HostLocal-RootSplitGet-Cache.txt"') do set "GetProjectCheckpoint=%%S"
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native hosted application cache status=" "%TemporaryDirectory%\HostLocal-RootFill-Application-Cache.txt"') do set "FillApplicationCheckpoint=%%S"
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native hosted application cache status=" "%TemporaryDirectory%\HostLocal-RootSplit-Application-Cache.txt"') do set "SplitApplicationCheckpoint=%%S"
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native hosted application cache status=" "%TemporaryDirectory%\HostLocal-RootSplitGet-Application-Cache.txt"') do set "GetApplicationCheckpoint=%%S"
    if not defined FillProjectCheckpoint exit /b 1
    if not defined SplitProjectCheckpoint exit /b 1
    if not defined GetProjectCheckpoint exit /b 1
    if not defined FillApplicationCheckpoint exit /b 1
    if not defined SplitApplicationCheckpoint exit /b 1
    if not defined GetApplicationCheckpoint exit /b 1
)

if not exist "%InitialFile%" exit /b 1
mkdir "%RunDirectory%" || exit /b 1
copy /b "%InitialFile%" "%StorageFile%" >nul || exit /b 1
pushd "%RunDirectory%" || exit /b 1
"%FillApplication%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="0" exit /b 1
for %%F in ("%StorageFile%") do if not "%%~zF"=="12800" exit /b 1
pushd "%RunDirectory%" || exit /b 1
"%SplitApplication%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="0" exit /b 1
for %%F in ("%StorageFile%") do if not "%%~zF"=="29184" exit /b 1
copy /b "%StorageFile%" "%CommittedFile%" >nul || exit /b 1
pushd "%RunDirectory%" || exit /b 1
"%GetApplication%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="0" exit /b 1
fc /b "%CommittedFile%" "%StorageFile%" >nul || exit /b 1

if "%Development%"=="1" (
    call set "CombinedProject=%%ProjectCheckpointHostRootWriter%%,Fill:%%FillProjectCheckpoint%%,Split:%%SplitProjectCheckpoint%%,Get:%%GetProjectCheckpoint%%"
    call set "CombinedApplication=%%ApplicationCheckpointHostRootWriter%%,Fill:%%FillApplicationCheckpoint%%,Split:%%SplitApplicationCheckpoint%%,Get:%%GetApplicationCheckpoint%%"
    endlocal & set "ProjectCheckpointHostRootWriter=%CombinedProject%" & set "ApplicationCheckpointHostRootWriter=%CombinedApplication%"
    exit /b 0
)
endlocal
exit /b 0

:build_host_local_component
setlocal EnableExtensions DisableDelayedExpansion
set "ProjectPath=%~f1"
set "Component=%~2"
set "WindowsApplication=%~f3"
set "ProjectResource=%ProjectPath:\=/%"
set "FirstWvb=%TemporaryDirectory%\HostLocal-%Component%-First.wvb"
set "SecondWvb=%TemporaryDirectory%\HostLocal-%Component%-Second.wvb"
set "FirstWvbResource=%FirstWvb:\=/%"
set "SecondWvbResource=%SecondWvb:\=/%"
set "FirstWvo=%TemporaryDirectory%\HostLocal-%Component%-First.wvo"
set "SecondWvo=%TemporaryDirectory%\HostLocal-%Component%-Second.wvo"
set "Common=%TemporaryDirectory%\HostStorage-Common-First.wvo"
set "WindowsPlatform=%TemporaryDirectory%\HostStorage-Windows.wvo"
set "LinuxPlatform=%TemporaryDirectory%\HostStorage-Linux.wvo"
set "WindowsImage=%TemporaryDirectory%\HostLocal-%Component%-Windows.bin"
set "WindowsImagePrefix=%TemporaryDirectory%\HostLocal-%Component%-Windows-Image"
set "WindowsMap=%TemporaryDirectory%\HostLocal-%Component%-Windows.map"
set "LinuxImage=%TemporaryDirectory%\HostLocal-%Component%-Linux.bin"
set "LinuxImagePrefix=%TemporaryDirectory%\HostLocal-%Component%-Linux-Image"
set "LinuxMap=%TemporaryDirectory%\HostLocal-%Component%-Linux.map"
set "LinuxApplication=%TemporaryDirectory%\HostLocal-%Component%.elf"

if "%Development%"=="1" (
    call "%RepositoryRoot%\Tools\Native\Build-Cached-Project-Object.cmd" ^
        "%ProjectPath%" "%BuildDriver%" "%Lowerer%" "%FirstWvb%" "%FirstWvo%" ^
        >"%TemporaryDirectory%\HostLocal-%Component%-Cache.txt"
    if errorlevel 1 exit /b 1
) else (
    "%BuildDriver%" --workspace "%WorkspaceResource%" --project "%ProjectResource%" "%FirstWvbResource%" >nul
    if errorlevel 1 exit /b 1
    "%BuildDriver%" --workspace "%WorkspaceResource%" --project "%ProjectResource%" "%SecondWvbResource%" >nul
    if errorlevel 1 exit /b 1
    fc /b "%FirstWvb%" "%SecondWvb%" >nul || exit /b 1
    "%Lowerer%" "%FirstWvb%" "%FirstWvo%" >nul || exit /b 1
    "%Lowerer%" "%SecondWvb%" "%SecondWvo%" >nul || exit /b 1
    fc /b "%FirstWvo%" "%SecondWvo%" >nul || exit /b 1
)
if not exist "%Common%" exit /b 1
if not exist "%WindowsPlatform%" exit /b 1
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 ^
    Storage_host_entry "%WindowsImage%" "%FirstWvo%" ^
    "%Common%" "%WindowsPlatform%" >"%WindowsMap%"
if errorlevel 1 exit /b 1
set "WindowsEntry="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Storage_host_entry address=" "%WindowsMap%"') do set "WindowsEntry=%%E"
if not defined WindowsEntry exit /b 1
echo(%WindowsEntry%| findstr /r /x "[0-9][0-9]*" >nul || exit /b 1
copy /b "%WindowsImage%" "%WindowsImagePrefix%.chunk-0" >nul || exit /b 1
if "%Development%"=="1" (
    call "%RepositoryRoot%\Tools\Native\Build-Cached-Hosted-Application.cmd" 6 ^
        "%FirstWvb%" "%WindowsImagePrefix%" 1 %WindowsEntry% ^
        "%WindowsApplication%" windows >"%TemporaryDirectory%\HostLocal-%Component%-Application-Cache.txt"
    if errorlevel 1 exit /b 1
) else (
    call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
        "%FirstWvb%" "%WindowsImagePrefix%" 1 %WindowsEntry% ^
        "%WindowsApplication%" windows >nul
    if errorlevel 1 exit /b 1
    if not exist "%LinuxPlatform%" exit /b 1
    call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 ^
        Storage_host_entry "%LinuxImage%" "%FirstWvo%" ^
        "%Common%" "%LinuxPlatform%" >"%LinuxMap%"
    if errorlevel 1 exit /b 1
    set "LinuxEntry="
    for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Storage_host_entry address=" "%LinuxMap%"') do set "LinuxEntry=%%E"
    if not defined LinuxEntry exit /b 1
    call echo(%%LinuxEntry%%^| findstr /r /x "[0-9][0-9]*" ^>nul || exit /b 1
    copy /b "%LinuxImage%" "%LinuxImagePrefix%.chunk-0" >nul || exit /b 1
    call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
        "%FirstWvb%" "%LinuxImagePrefix%" 1 %%LinuxEntry%% ^
        "%LinuxApplication%" linux >nul
    if errorlevel 1 exit /b 1
)
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
set "HostTreeReaderCheckpoint=Rebuilt"
set "HostTreeReaderApplicationCheckpoint=Rebuilt"

if "%Development%"=="1" (
    call "%RepositoryRoot%\Tools\Native\Build-Cached-Project-Object.cmd" ^
        "%ProjectPath%" "%BuildDriver%" "%Lowerer%" "%FirstWvb%" "%FirstWvo%" ^
        >"%TemporaryDirectory%\HostTreeReader-Cache.txt"
    if errorlevel 1 (
        >&2 echo The native host tree-reader project-object checkpoint failed.
        exit /b 1
    )
    set "HostTreeReaderCheckpoint="
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native project object cache status=" "%TemporaryDirectory%\HostTreeReader-Cache.txt"') do set "HostTreeReaderCheckpoint=%%S"
    if not defined HostTreeReaderCheckpoint exit /b 1
) else (
    "%BuildDriver%" --workspace "%WorkspaceResource%" --project "%ProjectResource%" "%FirstWvbResource%" >nul
    if errorlevel 1 (
        >&2 echo The native host tree-reader source build failed.
        exit /b 1
    )
    "%BuildDriver%" --workspace "%WorkspaceResource%" --project "%ProjectResource%" "%SecondWvbResource%" >nul
    if errorlevel 1 exit /b 1
    fc /b "%FirstWvb%" "%SecondWvb%" >nul
    if errorlevel 1 exit /b 1
    "%Lowerer%" "%FirstWvb%" "%FirstWvo%" >nul
    if errorlevel 1 (
        >&2 echo The native host tree-reader lowering failed.
        exit /b 1
    )
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
if "%Development%"=="1" (
    call "%RepositoryRoot%\Tools\Native\Build-Cached-Hosted-Application.cmd" 6 ^
        "%FirstWvb%" "%WindowsImagePrefix%" 1 %WindowsEntry% ^
        "%WindowsApplication%" windows >"%TemporaryDirectory%\HostTreeReader-Application-Cache.txt"
    if errorlevel 1 (
        >&2 echo The native host tree-reader Windows application checkpoint failed.
        exit /b 1
    )
    set "HostTreeReaderApplicationCheckpoint="
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native hosted application cache status=" "%TemporaryDirectory%\HostTreeReader-Application-Cache.txt"') do set "HostTreeReaderApplicationCheckpoint=%%S"
    if not defined HostTreeReaderApplicationCheckpoint exit /b 1
) else (
    call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
        "%FirstWvb%" "%WindowsImagePrefix%" 1 %WindowsEntry% ^
        "%WindowsApplication%" windows >nul
    if errorlevel 1 (
        >&2 echo The native host tree-reader Windows packaging failed.
        exit /b 1
    )
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
    endlocal & set "ProjectCheckpointHostTreeReader=%HostTreeReaderCheckpoint%" & set "ApplicationCheckpointHostTreeReader=%HostTreeReaderApplicationCheckpoint%"
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

:verify_host_engine
setlocal EnableExtensions DisableDelayedExpansion
set "ProjectPath=%~f1"
set "ProjectResource=%ProjectPath:\=/%"
set "FirstWvb=%TemporaryDirectory%\Engine-First.wvb"
set "SecondWvb=%TemporaryDirectory%\Engine-Second.wvb"
set "FirstWvbResource=%FirstWvb:\=/%"
set "SecondWvbResource=%SecondWvb:\=/%"
set "FirstWvo=%TemporaryDirectory%\Engine-First.wvo"
set "SecondWvo=%TemporaryDirectory%\Engine-Second.wvo"
set "Common=%TemporaryDirectory%\HostStorage-Common-First.wvo"
set "WindowsPlatform=%TemporaryDirectory%\HostStorage-Windows.wvo"
set "LinuxPlatform=%TemporaryDirectory%\HostStorage-Linux.wvo"
set "WindowsImage=%TemporaryDirectory%\Engine-Windows.bin"
set "WindowsImagePrefix=%TemporaryDirectory%\Engine-Windows-Image"
set "WindowsMap=%TemporaryDirectory%\Engine-Windows.map"
set "WindowsApplication=%TemporaryDirectory%\Engine.exe"
set "LinuxImage=%TemporaryDirectory%\Engine-Linux.bin"
set "LinuxImagePrefix=%TemporaryDirectory%\Engine-Linux-Image"
set "LinuxMap=%TemporaryDirectory%\Engine-Linux.map"
set "LinuxApplication=%TemporaryDirectory%\Engine.elf"
set "DepthTwoCommittedFile=%TemporaryDirectory%\HostTreeReader-Run\Windvale-Database-Storage.depth-two"
set "RunDirectory=%TemporaryDirectory%\Engine-Run"
set "StorageFile=%RunDirectory%\Windvale-Database-Storage.bin"
set "CreateDirectory=%TemporaryDirectory%\Engine-Create"
set "CreateStorage=%CreateDirectory%\Windvale-Database-Storage.bin"
set "CreateSnapshot=%CreateDirectory%\Windvale-Database-Storage.created"
set "EngineCheckpoint=Rebuilt"
set "EngineApplicationCheckpoint=Rebuilt"

if "%Development%"=="1" (
    call "%RepositoryRoot%\Tools\Native\Build-Cached-Project-Object.cmd" ^
        "%ProjectPath%" "%BuildDriver%" "%Lowerer%" "%FirstWvb%" "%FirstWvo%" ^
        >"%TemporaryDirectory%\Engine-Cache.txt"
    if errorlevel 1 exit /b 1
    set "EngineCheckpoint="
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native project object cache status=" "%TemporaryDirectory%\Engine-Cache.txt"') do set "EngineCheckpoint=%%S"
    if not defined EngineCheckpoint exit /b 1
) else (
    "%BuildDriver%" --workspace "%WorkspaceResource%" --project "%ProjectResource%" "%FirstWvbResource%" >nul
    if errorlevel 1 exit /b 1
    "%BuildDriver%" --workspace "%WorkspaceResource%" --project "%ProjectResource%" "%SecondWvbResource%" >nul
    if errorlevel 1 exit /b 1
    fc /b "%FirstWvb%" "%SecondWvb%" >nul || exit /b 1
    "%Lowerer%" "%FirstWvb%" "%FirstWvo%" >nul || exit /b 1
    "%Lowerer%" "%SecondWvb%" "%SecondWvo%" >nul || exit /b 1
    fc /b "%FirstWvo%" "%SecondWvo%" >nul || exit /b 1
)
if not exist "%Common%" exit /b 1
if not exist "%WindowsPlatform%" exit /b 1
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 ^
    Storage_host_entry "%WindowsImage%" "%FirstWvo%" ^
    "%Common%" "%WindowsPlatform%" >"%WindowsMap%"
if errorlevel 1 exit /b 1
set "WindowsEntry="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Storage_host_entry address=" "%WindowsMap%"') do set "WindowsEntry=%%E"
if not defined WindowsEntry exit /b 1
echo(%WindowsEntry%| findstr /r /x "[0-9][0-9]*" >nul || exit /b 1
copy /b "%WindowsImage%" "%WindowsImagePrefix%.chunk-0" >nul || exit /b 1
if "%Development%"=="1" (
    call "%RepositoryRoot%\Tools\Native\Build-Cached-Hosted-Application.cmd" 6 ^
        "%FirstWvb%" "%WindowsImagePrefix%" 1 %WindowsEntry% ^
        "%WindowsApplication%" windows >"%TemporaryDirectory%\Engine-Application-Cache.txt"
    if errorlevel 1 exit /b 1
    set "EngineApplicationCheckpoint="
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native hosted application cache status=" "%TemporaryDirectory%\Engine-Application-Cache.txt"') do set "EngineApplicationCheckpoint=%%S"
    if not defined EngineApplicationCheckpoint exit /b 1
) else (
    call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
        "%FirstWvb%" "%WindowsImagePrefix%" 1 %WindowsEntry% ^
        "%WindowsApplication%" windows >nul
    if errorlevel 1 exit /b 1
)

mkdir "%CreateDirectory%" || exit /b 1
pushd "%CreateDirectory%" || exit /b 1
"%WindowsApplication%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="70" (
    >&2 echo The native engine fresh lifecycle returned %ApplicationResult%, expected 70.
    exit /b 1
)
for %%F in ("%CreateStorage%") do if not "%%~zF"=="4608" exit /b 1
copy /b "%CreateStorage%" "%CreateSnapshot%" >nul || exit /b 1
pushd "%CreateDirectory%" || exit /b 1
"%WindowsApplication%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="71" (
    >&2 echo The native engine initial reopen returned %ApplicationResult%, expected 71.
    exit /b 1
)
fc /b "%CreateSnapshot%" "%CreateStorage%" >nul || exit /b 1

if not exist "%DepthTwoCommittedFile%" exit /b 1
mkdir "%RunDirectory%" || exit /b 1
copy /b "%DepthTwoCommittedFile%" "%StorageFile%" >nul || exit /b 1
pushd "%RunDirectory%" || exit /b 1
"%WindowsApplication%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="0" exit /b 1
fc /b "%DepthTwoCommittedFile%" "%StorageFile%" >nul || exit /b 1

for %%S in (0 1 2) do (
    set "ScenarioDirectory=%TemporaryDirectory%\Engine-Recovery-%%S"
    call :verify_host_engine_recovery ^
        "%WindowsApplication%" "%DepthTwoCommittedFile%" %%S
    if errorlevel 1 exit /b 1
)
set "InvalidDirectory=%TemporaryDirectory%\Engine-Invalid-Header"
set "InvalidStorage=%InvalidDirectory%\Windvale-Database-Storage.bin"
mkdir "%InvalidDirectory%" || exit /b 1
copy /b "%DepthTwoCommittedFile%" "%InvalidStorage%" >nul || exit /b 1
fsutil file seteof "%InvalidStorage%" 511 >nul || exit /b 1
pushd "%InvalidDirectory%" || exit /b 1
"%WindowsApplication%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="91" exit /b 1

if "%Development%"=="1" (
    endlocal & set "ProjectCheckpointEngine=%EngineCheckpoint%" & set "ApplicationCheckpointEngine=%EngineApplicationCheckpoint%"
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
copy /b "%LinuxImage%" "%LinuxImagePrefix%.chunk-0" >nul || exit /b 1
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
    "%FirstWvb%" "%LinuxImagePrefix%" 1 %LinuxEntry% ^
    "%LinuxApplication%" linux >nul
if errorlevel 1 exit /b 1
endlocal
exit /b 0

:verify_host_engine_recovery
setlocal EnableExtensions DisableDelayedExpansion
set "Application=%~f1"
set "Committed=%~f2"
set "Step=%~3"
set "ScenarioDirectory=%TemporaryDirectory%\Engine-Recovery-%Step%"
set "ScenarioStorage=%ScenarioDirectory%\Windvale-Database-Storage.bin"
mkdir "%ScenarioDirectory%" || exit /b 1
copy /b "%Committed%" "%ScenarioStorage%" >nul || exit /b 1
set /a MarkerLength=20993+Step
fsutil file seteof "%ScenarioStorage%" %MarkerLength% >nul || exit /b 1
pushd "%ScenarioDirectory%" || exit /b 1
"%Application%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if "%Step%"=="0" set "ExpectedResult=100"
if "%Step%"=="1" set "ExpectedResult=101"
if "%Step%"=="2" set "ExpectedResult=0"
if not "%ApplicationResult%"=="%ExpectedResult%" exit /b 1
if "%Step%"=="0" (
    for %%F in ("%ScenarioStorage%") do if not "%%~zF"=="20993" exit /b 1
) else (
    for %%F in ("%ScenarioStorage%") do if not "%%~zF"=="20992" exit /b 1
    fc /b "%Committed%" "%ScenarioStorage%" >nul || exit /b 1
)
endlocal
exit /b 0

:verify_host_tree_writer
setlocal EnableExtensions DisableDelayedExpansion
set "ProjectPath=%~f1"
set "ProjectResource=%ProjectPath:\=/%"
set "FirstWvb=%TemporaryDirectory%\HostTreeWriter-First.wvb"
set "SecondWvb=%TemporaryDirectory%\HostTreeWriter-Second.wvb"
set "FirstWvbResource=%FirstWvb:\=/%"
set "SecondWvbResource=%SecondWvb:\=/%"
set "FirstWvo=%TemporaryDirectory%\HostTreeWriter-First.wvo"
set "SecondWvo=%TemporaryDirectory%\HostTreeWriter-Second.wvo"
set "Common=%TemporaryDirectory%\HostStorage-Common-First.wvo"
set "WindowsPlatform=%TemporaryDirectory%\HostStorage-Windows.wvo"
set "LinuxPlatform=%TemporaryDirectory%\HostStorage-Linux.wvo"
set "WindowsImage=%TemporaryDirectory%\HostTreeWriter-Windows.bin"
set "WindowsImagePrefix=%TemporaryDirectory%\HostTreeWriter-Windows-Image"
set "WindowsMap=%TemporaryDirectory%\HostTreeWriter-Windows.map"
set "WindowsApplication=%TemporaryDirectory%\HostTreeWriter.exe"
set "LinuxImage=%TemporaryDirectory%\HostTreeWriter-Linux.bin"
set "LinuxImagePrefix=%TemporaryDirectory%\HostTreeWriter-Linux-Image"
set "LinuxMap=%TemporaryDirectory%\HostTreeWriter-Linux.map"
set "LinuxApplication=%TemporaryDirectory%\HostTreeWriter.elf"
set "DepthTwoCommittedFile=%TemporaryDirectory%\HostTreeReader-Run\Windvale-Database-Storage.depth-two"
set "RunDirectory=%TemporaryDirectory%\HostTreeWriter-Run"
set "StorageFile=%RunDirectory%\Windvale-Database-Storage.bin"
set "CommittedFile=%RunDirectory%\Windvale-Database-Storage.committed"
set "HostTreeWriterCheckpoint=Rebuilt"
set "HostTreeWriterApplicationCheckpoint=Rebuilt"

if "%Development%"=="1" (
    call "%RepositoryRoot%\Tools\Native\Build-Cached-Project-Object.cmd" ^
        "%ProjectPath%" "%BuildDriver%" "%Lowerer%" "%FirstWvb%" "%FirstWvo%" ^
        >"%TemporaryDirectory%\HostTreeWriter-Cache.txt"
    if errorlevel 1 (
        >&2 echo The native host tree-writer project-object checkpoint failed.
        exit /b 1
    )
    set "HostTreeWriterCheckpoint="
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native project object cache status=" "%TemporaryDirectory%\HostTreeWriter-Cache.txt"') do set "HostTreeWriterCheckpoint=%%S"
    if not defined HostTreeWriterCheckpoint exit /b 1
) else (
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
)
if not exist "%Common%" exit /b 1
if not exist "%WindowsPlatform%" exit /b 1

call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 ^
    Storage_host_entry "%WindowsImage%" "%FirstWvo%" ^
    "%Common%" "%WindowsPlatform%" >"%WindowsMap%"
if errorlevel 1 exit /b 1
set "WindowsEntry="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Storage_host_entry address=" "%WindowsMap%"') do set "WindowsEntry=%%E"
if not defined WindowsEntry exit /b 1
echo(%WindowsEntry%| findstr /r /x "[0-9][0-9]*" >nul || exit /b 1
copy /b "%WindowsImage%" "%WindowsImagePrefix%.chunk-0" >nul || exit /b 1
if "%Development%"=="1" (
    call "%RepositoryRoot%\Tools\Native\Build-Cached-Hosted-Application.cmd" 6 ^
        "%FirstWvb%" "%WindowsImagePrefix%" 1 %WindowsEntry% ^
        "%WindowsApplication%" windows >"%TemporaryDirectory%\HostTreeWriter-Application-Cache.txt"
    if errorlevel 1 exit /b 1
    set "HostTreeWriterApplicationCheckpoint="
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native hosted application cache status=" "%TemporaryDirectory%\HostTreeWriter-Application-Cache.txt"') do set "HostTreeWriterApplicationCheckpoint=%%S"
    if not defined HostTreeWriterApplicationCheckpoint exit /b 1
) else (
    call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
        "%FirstWvb%" "%WindowsImagePrefix%" 1 %WindowsEntry% ^
        "%WindowsApplication%" windows >nul
    if errorlevel 1 exit /b 1
)

if not exist "%DepthTwoCommittedFile%" exit /b 1
mkdir "%RunDirectory%" || exit /b 1
copy /b "%DepthTwoCommittedFile%" "%StorageFile%" >nul || exit /b 1
pushd "%RunDirectory%" || exit /b 1
"%WindowsApplication%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="0" (
    >&2 echo The native host tree-writer publication returned %ApplicationResult%, expected 0.
    exit /b 1
)
for %%F in ("%StorageFile%") do if not "%%~zF"=="33280" exit /b 1
copy /b "%StorageFile%" "%CommittedFile%" >nul || exit /b 1
pushd "%RunDirectory%" || exit /b 1
"%WindowsApplication%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="0" exit /b 1
fc /b "%CommittedFile%" "%StorageFile%" >nul || exit /b 1
for %%S in (0 1 2 3 4) do (
    call :verify_host_tree_writer_interruption ^
        "%WindowsApplication%" "%DepthTwoCommittedFile%" %%S
    if errorlevel 1 exit /b 1
)

if "%Development%"=="1" (
    endlocal & set "ProjectCheckpointHostTreeWriter=%HostTreeWriterCheckpoint%" & set "ApplicationCheckpointHostTreeWriter=%HostTreeWriterApplicationCheckpoint%"
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
copy /b "%LinuxImage%" "%LinuxImagePrefix%.chunk-0" >nul || exit /b 1
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
    "%FirstWvb%" "%LinuxImagePrefix%" 1 %LinuxEntry% ^
    "%LinuxApplication%" linux >nul
if errorlevel 1 exit /b 1
endlocal
exit /b 0

:verify_host_tree_writer_interruption
setlocal EnableExtensions DisableDelayedExpansion
set "Application=%~f1"
set "Committed=%~f2"
set "Step=%~3"
set "ScenarioDirectory=%TemporaryDirectory%\HostTreeWriter-Interruption-%Step%"
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
    >&2 echo The native host tree-writer interruption %Step% returned %ApplicationResult%, expected %ExpectedResult%.
    exit /b 1
)
pushd "%ScenarioDirectory%" || exit /b 1
"%Application%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="0" exit /b 1
for %%F in ("%ScenarioStorage%") do if not "%%~zF"=="33280" exit /b 1
endlocal
exit /b 0

:verify_host_logical_tree_writer
setlocal EnableExtensions DisableDelayedExpansion
set "PutProject=%~f1"
set "GetProject=%~f2"
set "PutApplication=%TemporaryDirectory%\HostLogicalTree-Put.exe"
set "GetApplication=%TemporaryDirectory%\HostLogicalTree-Get.exe"
set "DepthTwoCommittedFile=%TemporaryDirectory%\HostTreeReader-Run\Windvale-Database-Storage.depth-two"
set "RunDirectory=%TemporaryDirectory%\HostLogicalTree-Run"
set "StorageFile=%RunDirectory%\Windvale-Database-Storage.bin"
set "CommittedFile=%RunDirectory%\Windvale-Database-Storage.committed"

call :build_host_local_component "%PutProject%" LogicalTreePut "%PutApplication%"
if errorlevel 1 exit /b 1
call :build_host_local_component "%GetProject%" LogicalTreeGet "%GetApplication%"
if errorlevel 1 exit /b 1

if "%Development%"=="1" (
    set "PutProjectCheckpoint="
    set "GetProjectCheckpoint="
    set "PutApplicationCheckpoint="
    set "GetApplicationCheckpoint="
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native project object cache status=" "%TemporaryDirectory%\HostLocal-LogicalTreePut-Cache.txt"') do set "PutProjectCheckpoint=%%S"
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native project object cache status=" "%TemporaryDirectory%\HostLocal-LogicalTreeGet-Cache.txt"') do set "GetProjectCheckpoint=%%S"
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native hosted application cache status=" "%TemporaryDirectory%\HostLocal-LogicalTreePut-Application-Cache.txt"') do set "PutApplicationCheckpoint=%%S"
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native hosted application cache status=" "%TemporaryDirectory%\HostLocal-LogicalTreeGet-Application-Cache.txt"') do set "GetApplicationCheckpoint=%%S"
    if not defined PutProjectCheckpoint exit /b 1
    if not defined GetProjectCheckpoint exit /b 1
    if not defined PutApplicationCheckpoint exit /b 1
    if not defined GetApplicationCheckpoint exit /b 1
)

if not exist "%DepthTwoCommittedFile%" exit /b 1
mkdir "%RunDirectory%" || exit /b 1
copy /b "%DepthTwoCommittedFile%" "%StorageFile%" >nul || exit /b 1
pushd "%RunDirectory%" || exit /b 1
"%PutApplication%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="0" (
    >&2 echo The native logical tree-writer put returned %ApplicationResult%, expected 0.
    exit /b 1
)
for %%F in ("%StorageFile%") do if not "%%~zF"=="33280" exit /b 1
copy /b "%StorageFile%" "%CommittedFile%" >nul || exit /b 1
pushd "%RunDirectory%" || exit /b 1
"%GetApplication%" >nul
set "ApplicationResult=%ERRORLEVEL%"
popd
if not "%ApplicationResult%"=="0" (
    >&2 echo The native logical tree-writer restart get returned %ApplicationResult%, expected 0.
    exit /b 1
)
fc /b "%CommittedFile%" "%StorageFile%" >nul || exit /b 1

if "%Development%"=="1" (
    call set "CombinedProject=%%ProjectCheckpointHostTreeWriter%%,LogicalPut:%%PutProjectCheckpoint%%,LogicalGet:%%GetProjectCheckpoint%%"
    call set "CombinedApplication=%%ApplicationCheckpointHostTreeWriter%%,LogicalPut:%%PutApplicationCheckpoint%%,LogicalGet:%%GetApplicationCheckpoint%%"
    endlocal & set "ProjectCheckpointHostTreeWriter=%CombinedProject%" & set "ApplicationCheckpointHostTreeWriter=%CombinedApplication%"
    exit /b 0
)
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
>&2 echo Usage: Tools\Native\Test-Database-Storage.cmd [--development^|--development-target ^<target^>^|--prepare-development-tools]
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
set "ProjectCheckpoint=Rebuilt"
set "LinkCheckpoint=Rebuilt"
set "WindowsApplicationCheckpoint=Rebuilt"
set "LinuxApplicationCheckpoint=Rebuilt"
if "%Development%"=="1" set "LinuxApplicationCheckpoint=NotRun"
if "%Development%"=="1" call :read_clock TargetStart

if "%Development%"=="1" (
    call "%RepositoryRoot%\Tools\Native\Build-Cached-Project-Object.cmd" ^
        "%ProjectPath%" "%BuildDriver%" "%Lowerer%" "%FirstWvb%" "%FirstWvo%" ^
        >"%TemporaryDirectory%\%~1-Project-Cache.txt"
    if errorlevel 1 exit /b 1
    set "ProjectCheckpoint="
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native project object cache status=" "%TemporaryDirectory%\%~1-Project-Cache.txt"') do set "ProjectCheckpoint=%%S"
    if not defined ProjectCheckpoint exit /b 1
) else (
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
)
if not "%Development%"=="1" (
    call "%RepositoryRoot%\Tools\Native\Check-Wvo.cmd" "%FirstWvo%" >nul
    if errorlevel 1 exit /b 1
)

if "%Development%"=="1" (
    call "%RepositoryRoot%\Tools\Native\Build-Cached-Linked-Image.cmd" ^
        0 Main "%FirstWvo%" "%Image%" "%Map%" ^
        >"%TemporaryDirectory%\%~1-Link-Cache.txt"
    if errorlevel 1 exit /b 1
    set "LinkCheckpoint="
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native linked image cache status=" "%TemporaryDirectory%\%~1-Link-Cache.txt"') do set "LinkCheckpoint=%%S"
    if not defined LinkCheckpoint exit /b 1
) else (
    call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main ^
        "%Image%" "%FirstWvo%" >"%Map%"
    if errorlevel 1 exit /b 1
)
set "EntryOffset="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Main address=" "%Map%"') do set "EntryOffset=%%E"
if not defined EntryOffset exit /b 1
echo(%EntryOffset%| findstr /r /x "[0-9][0-9]*" >nul || exit /b 1
copy /b "%Image%" "%ImagePrefix%.chunk-0" >nul
if errorlevel 1 exit /b 1

if "%Development%"=="1" (
    call "%RepositoryRoot%\Tools\Native\Build-Cached-Hosted-Application.cmd" 6 ^
        "%FirstWvb%" "%ImagePrefix%" 1 %EntryOffset% "%WindowsApplication%" windows ^
        >"%TemporaryDirectory%\%~1-Windows-Application-Cache.txt"
    if errorlevel 1 exit /b 1
    set "WindowsApplicationCheckpoint="
    for /f "tokens=6 delims== " %%S in ('findstr /b /c:"native hosted application cache status=" "%TemporaryDirectory%\%~1-Windows-Application-Cache.txt"') do set "WindowsApplicationCheckpoint=%%S"
    if not defined WindowsApplicationCheckpoint exit /b 1
) else (
    call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
        "%FirstWvb%" "%ImagePrefix%" 1 %EntryOffset% "%WindowsApplication%" windows >nul
    if errorlevel 1 exit /b 1
)
"%WindowsApplication%" >nul
if not "%ERRORLEVEL%"=="0" (
    >&2 echo The %Label% database-storage case did not return 0.
    exit /b 1
)

if not "%Development%"=="1" (
    call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
        "%FirstWvb%" "%ImagePrefix%" 1 %EntryOffset% "%LinuxApplication%" linux >nul
    if errorlevel 1 exit /b 1
)
if "%Development%"=="1" (
    call :read_clock TargetEnd
    call :elapsed_milliseconds TargetStart TargetEnd TargetElapsedMs
    call echo PASS  native database storage development step=portable-target item=%%ProgressCurrent%%/%ProgressTotal% target=%DevelopmentTarget% case=%Label% elapsed-ms=%%TargetElapsedMs%% project=%ProjectCheckpoint% link=%LinkCheckpoint% application=windows-%WindowsApplicationCheckpoint%
    endlocal & set "PortableProjectCheckpoints=%PortableProjectCheckpoints%%Label%:%ProjectCheckpoint%/link-%LinkCheckpoint%," & set "PortableApplicationCheckpoints=%PortableApplicationCheckpoints%%Label%:windows-%WindowsApplicationCheckpoint%,"
    exit /b 0
)
endlocal
exit /b 0

:read_clock
setlocal EnableExtensions DisableDelayedExpansion
set "Clock=%TIME: =0%"
set /a ClockHours=1%Clock:~0,2%-100
set /a ClockMinutes=1%Clock:~3,2%-100
set /a ClockSeconds=1%Clock:~6,2%-100
set /a ClockCentiseconds=1%Clock:~9,2%-100
set /a ClockTicks=ClockHours*360000+ClockMinutes*6000+ClockSeconds*100+ClockCentiseconds
endlocal & set "%~1=%ClockTicks%"
exit /b 0

:elapsed_milliseconds
setlocal EnableExtensions DisableDelayedExpansion
call set "ElapsedStart=%%%~1%%"
call set "ElapsedEnd=%%%~2%%"
set /a ElapsedTicks=ElapsedEnd-ElapsedStart
if %ElapsedTicks% LSS 0 set /a ElapsedTicks+=8640000
set /a ElapsedMs=ElapsedTicks*10
endlocal & set "%~3=%ElapsedMs%"
exit /b 0
