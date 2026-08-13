@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Libraries.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"

:allocate
set "TemporaryDirectory=%TEMP%\windvale-libraries-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "Result=1"

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
) do (
    call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
        "%RepositoryRoot%\Projects\Libraries\%%P.wvproj" ^
        "%TemporaryDirectory%\%%P.wvb" >nul
    if errorlevel 1 goto :cleanup
)

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Tests\Fixtures\Libraries\Directory-Import-Smoke.wvproj" ^
    "%TemporaryDirectory%\Directory-Import-Smoke.wvb" >nul
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Tests\Fixtures\Libraries\Random-Access-Page-Import-Smoke.wvproj" ^
    "%TemporaryDirectory%\Random-Access-Page-Import-Smoke.wvb" >nul
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
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
) do (
    call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
        "%RepositoryRoot%\Projects\Tests\%%T.wvproj" ^
        "%TemporaryDirectory%\%%T.wvb" >nul
    if errorlevel 1 goto :cleanup
)

for %%N in (
    Capability-Import-No-Root-Declaration
    Capability-Profile-Rejection
) do (
    call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
        "%RepositoryRoot%\Tests\Fixtures\Libraries\%%N.wvproj" ^
        "%TemporaryDirectory%\%%N.wvb" >"%TemporaryDirectory%\%%N.out" 2>"%TemporaryDirectory%\%%N.err"
    if not errorlevel 1 goto :cleanup
    if exist "%TemporaryDirectory%\%%N.wvb" goto :cleanup
)

echo native libraries status=Passed projects=17 conformance-builds=7 negative=2 cases=26
set "Result=0"

:cleanup
if exist "%TemporaryDirectory%\." (
    del /f /q "%TemporaryDirectory%\*" >nul 2>nul
    rmdir "%TemporaryDirectory%" >nul 2>nul
)
exit /b %Result%
