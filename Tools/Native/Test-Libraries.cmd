@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "DevelopmentTarget="
if "%~1"=="" goto :arguments_complete
if /i not "%~1"=="--development-target" goto :usage
if "%~2"=="" goto :usage
if not "%~3"=="" goto :usage
set "DevelopmentTarget=%~2"
goto :arguments_complete

:usage
>&2 echo Usage: Tools\Native\Test-Libraries.cmd [--development-target ^<target^>]
exit /b 64

:arguments_complete

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"

:allocate
set "TemporaryDirectory=%TEMP%\windvale-libraries-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "Result=1"

if defined DevelopmentTarget goto :development

for %%P in (
    Windvale-Library-Resource-Store
    Windvale-Library-Database-Storage-Geometry
    Windvale-Library-Database-Storage-Page
    Windvale-Library-Database-Durable-Superblock
    Windvale-Library-Database-Durable-Page
    Windvale-Library-Database-Durable-Commit-Record
    Windvale-Library-Database-Commit-Publication
    Windvale-Library-Wvdb-Reader
    Windvale-Library-Hosted-Resource-Store
    Windvale-Library-Read-Only-Directory
    Windvale-Library-Random-Access-Storage
    Windvale-Library-Random-Access-Database-Page
    Windvale-Library-Native-Hosted-Snapshot-Page
    Windvale-Library-Read-Only-Wvdb
    Windvale-Library-Model-Protocol
    Windvale-Library-Scripted-Model-Provider
) do (
    call "%RepositoryRoot%\Tools\Native\Build-Current-Wvb.cmd" ^
        "%RepositoryRoot%\Projects\Libraries\%%P.wvproj" ^
        "%TemporaryDirectory%\%%P.wvb" >nul
    if errorlevel 1 (
        >&2 echo Native library project failed: %%P
        goto :cleanup
    )
)

call "%RepositoryRoot%\Tools\Native\Build-Current-Wvb.cmd" ^
    "%RepositoryRoot%\Tests\Fixtures\Libraries\Directory-Import-Smoke.wvproj" ^
    "%TemporaryDirectory%\Directory-Import-Smoke.wvb" >nul
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Build-Current-Wvb.cmd" ^
    "%RepositoryRoot%\Tests\Fixtures\Libraries\Random-Access-Page-Import-Smoke.wvproj" ^
    "%TemporaryDirectory%\Random-Access-Page-Import-Smoke.wvb" >nul
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Build-Current-Wvb.cmd" ^
    "%RepositoryRoot%\Tests\Fixtures\Libraries\Random-Access-Storage-Import-Smoke.wvproj" ^
    "%TemporaryDirectory%\Random-Access-Storage-Import-Smoke.wvb" >nul
if errorlevel 1 goto :cleanup

for %%T in (
    Windvale-Native-Test-Database-Geometry
    Windvale-Native-Test-Database-Storage-Page
    Windvale-Native-Test-Database-Storage-Page-Accept
    Windvale-Native-Test-Database-Durable-Superblock
    Windvale-Native-Test-Database-Durable-Commit
    Windvale-Native-Test-Native-Hosted-Snapshot-Page
    Windvale-Native-Test-Database-Reader
    Windvale-Native-Test-Model-Protocol
) do (
    call "%RepositoryRoot%\Tools\Native\Build-Current-Wvb.cmd" ^
        "%RepositoryRoot%\Projects\Tests\%%T.wvproj" ^
        "%TemporaryDirectory%\%%T.wvb" >nul
    if errorlevel 1 goto :cleanup
)

for %%N in (
    Capability-Import-No-Root-Declaration
    Capability-Profile-Rejection
) do (
    call "%RepositoryRoot%\Tools\Native\Build-Current-Wvb.cmd" ^
        "%RepositoryRoot%\Tests\Fixtures\Libraries\%%N.wvproj" ^
        "%TemporaryDirectory%\%%N.wvb" >"%TemporaryDirectory%\%%N.out" 2>"%TemporaryDirectory%\%%N.err"
    if not errorlevel 1 goto :cleanup
    if exist "%TemporaryDirectory%\%%N.wvb" goto :cleanup
)

echo native libraries status=Passed projects=19 conformance-builds=8 negative=2 cases=29
set "Result=0"
goto :cleanup

:development
set "TargetPlan=%RepositoryRoot%\Tests\Native\Library-Development-Targets.txt"
if not exist "%TargetPlan%" (
    >&2 echo Missing library development-target manifest.
    goto :cleanup
)
set "TargetHeader="
for /f "usebackq delims=" %%H in ("%TargetPlan%") do (
    set "TargetHeader=%%H"
    goto :target_header_read
)
:target_header_read
if not "%TargetHeader%"=="windvale-library-development-targets 1" (
    >&2 echo Invalid library development-target manifest.
    goto :cleanup
)
set "DevelopmentProjects=0"
set "DevelopmentConformance=0"
set "DevelopmentNegative=0"
set "DevelopmentCases=0"
set "DevelopmentFailed=0"
for /f "usebackq tokens=1,2,* delims=|" %%A in ("%TargetPlan%") do (
    if "%%A"=="%DevelopmentTarget%" call :run_development_entry "%%B" "%%C"
)
if "%DevelopmentCases%"=="0" (
    >&2 echo Unknown library development target: %DevelopmentTarget%
    set "Result=64"
    goto :cleanup
)
if not "%DevelopmentFailed%"=="0" goto :cleanup
echo native libraries development status=Passed target=%DevelopmentTarget% projects=%DevelopmentProjects% conformance-builds=%DevelopmentConformance% negative=%DevelopmentNegative% cases=%DevelopmentCases%
set "Result=0"
goto :cleanup

:run_development_entry
set /a DevelopmentCases+=1
if /i "%~1"=="project" set /a DevelopmentProjects+=1
if /i "%~1"=="conformance" set /a DevelopmentConformance+=1
if /i "%~1"=="negative" goto :run_development_negative
if /i not "%~1"=="project" if /i not "%~1"=="conformance" (
    >&2 echo Invalid library development-target kind: %~1
    set "DevelopmentFailed=1"
    exit /b 0
)
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\%~2" ^
    "%TemporaryDirectory%\development-%DevelopmentCases%.wvb" >nul
if errorlevel 1 (
    >&2 echo Native library development project failed: %~2
    set "DevelopmentFailed=1"
)
exit /b 0

:run_development_negative
set /a DevelopmentNegative+=1
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\%~2" ^
    "%TemporaryDirectory%\development-%DevelopmentCases%.wvb" ^
    >"%TemporaryDirectory%\development-%DevelopmentCases%.out" ^
    2>"%TemporaryDirectory%\development-%DevelopmentCases%.err"
if not errorlevel 1 set "DevelopmentFailed=1"
if exist "%TemporaryDirectory%\development-%DevelopmentCases%.wvb" set "DevelopmentFailed=1"
exit /b 0

:cleanup
if exist "%TemporaryDirectory%\." (
    del /f /q "%TemporaryDirectory%\*" >nul 2>nul
    rmdir "%TemporaryDirectory%" >nul 2>nul
)
exit /b %Result%
