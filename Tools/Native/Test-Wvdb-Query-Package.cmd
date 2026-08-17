@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Wvdb-Query-Package.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Manifest=%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvpack"
set "Lock=%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvlock"
set "InspectorManifest=%RepositoryRoot%\Distribution\Applications\Wvb-Inspector\Windvale-Wvb-Inspector.wvpack"
set "InspectorLock=%RepositoryRoot%\Distribution\Applications\Wvb-Inspector\Windvale-Wvb-Inspector.wvlock"

:allocate
set "TemporaryDirectory=%TEMP%\windvale-wvdb-package-test-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "Result=1"

call "%RepositoryRoot%\Tools\Native\Build-Wvdb-Query-Package.cmd" ^
    "%Manifest%" "%Lock%" "%TemporaryDirectory%\First.wvb" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvdb-Query-Package.cmd" ^
    "%Manifest%" "%Lock%" "%TemporaryDirectory%\Second.wvb" >nul
if errorlevel 1 goto :cleanup
fc /b "%TemporaryDirectory%\First.wvb" "%TemporaryDirectory%\Second.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify_file "%TemporaryDirectory%\First.wvb" 26145 77cb6034402942734be316b9a135d6c1b46ace5cb43a198b2aafe2d1b098027b
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Build-Wvb-Inspector-Package.cmd" ^
    "%InspectorManifest%" "%InspectorLock%" "%TemporaryDirectory%\Inspector-First.wvb" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb-Inspector-Package.cmd" ^
    "%InspectorManifest%" "%InspectorLock%" "%TemporaryDirectory%\Inspector-Second.wvb" >nul
if errorlevel 1 goto :cleanup
fc /b "%TemporaryDirectory%\Inspector-First.wvb" "%TemporaryDirectory%\Inspector-Second.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify_file "%TemporaryDirectory%\Inspector-First.wvb" 76527 293be3267ff95f9272e96684e036a5647abc060f2bc87a9e654beac7140af753
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Inspect-Wvb.cmd" ^
    "%TemporaryDirectory%\First.wvb" >"%TemporaryDirectory%\Inspect.txt"
if errorlevel 1 goto :cleanup
for /f %%C in ('findstr /b /c:"capability index=" "%TemporaryDirectory%\Inspect.txt" ^| find /c /v ""') do set "CapabilityCount=%%C"
if not "%CapabilityCount%"=="5" goto :cleanup
for %%C in (
    console.write_line
    diagnostic.write_line
    filesystem.directory_read_v1
    process.argument
    process.argument_count
) do (
    findstr /b /c:"capability index=" "%TemporaryDirectory%\Inspect.txt" | findstr /c:"name=\"%%C\"" >nul
    if errorlevel 1 goto :cleanup
)

call "%RepositoryRoot%\Tools\Native\Inspect-Wvb.cmd" ^
    "%TemporaryDirectory%\Inspector-First.wvb" >"%TemporaryDirectory%\Inspector-Inspect.txt"
if errorlevel 1 goto :cleanup
for /f %%C in ('findstr /b /c:"capability index=" "%TemporaryDirectory%\Inspector-Inspect.txt" ^| find /c /v ""') do set "InspectorCapabilityCount=%%C"
if not "%InspectorCapabilityCount%"=="5" goto :cleanup
for %%C in (
    console.write_line
    diagnostic.write_line
    file.read_bytes
    process.argument
    process.argument_count
) do (
    findstr /b /c:"capability index=" "%TemporaryDirectory%\Inspector-Inspect.txt" | findstr /c:"name=\"%%C\"" >nul
    if errorlevel 1 goto :cleanup
)

>"%TemporaryDirectory%\Bad.wvlock" echo windvale-lock 1
>"%TemporaryDirectory%\Preserved.wvb" echo preserved-output
copy /b "%TemporaryDirectory%\Preserved.wvb" "%TemporaryDirectory%\Expected.wvb" >nul || goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvdb-Query-Package.cmd" ^
    "%Manifest%" "%TemporaryDirectory%\Bad.wvlock" "%TemporaryDirectory%\Preserved.wvb" ^
    >"%TemporaryDirectory%\Bad.out" 2>"%TemporaryDirectory%\Bad.err"
if not errorlevel 1 goto :cleanup
fc /b "%TemporaryDirectory%\Expected.wvb" "%TemporaryDirectory%\Preserved.wvb" >nul
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Build-Wvdb-Query-Package.cmd" ^
    "%Manifest%" "%TemporaryDirectory%\Missing.wvlock" "%TemporaryDirectory%\Missing.wvb" ^
    >"%TemporaryDirectory%\Missing.out" 2>"%TemporaryDirectory%\Missing.err"
if not errorlevel 1 goto :cleanup
if exist "%TemporaryDirectory%\Missing.wvb" goto :cleanup

copy /b "%Manifest%" "%TemporaryDirectory%\Alias.wvpack" >nul || goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvdb-Query-Package.cmd" ^
    "%TemporaryDirectory%\Alias.wvpack" "%Lock%" "%TemporaryDirectory%\Alias.wvb" ^
    >"%TemporaryDirectory%\Alias.out" 2>"%TemporaryDirectory%\Alias.err"
if not errorlevel 1 goto :cleanup
if exist "%TemporaryDirectory%\Alias.wvb" goto :cleanup

echo native package status=Passed packages=2 builds=4 inspection=2 negative=3 preservation=1 cases=11
set "Result=0"

:cleanup
if exist "%TemporaryDirectory%\." (
    del /f /q "%TemporaryDirectory%\*" >nul 2>nul
    rmdir "%TemporaryDirectory%" >nul 2>nul
)
exit /b %Result%

:verify_file
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /i /c:"%~3" >nul
if errorlevel 1 exit /b 1
exit /b 0
