@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Baseline-Jit-Publisher.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "TemporaryDirectory=%TEMP%\windvale-baseline-jit-publisher-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1

set "PlanWvo=%TemporaryDirectory%\Plan.wvo"
set "PlatformWvo=%TemporaryDirectory%\Windows.wvo"
set "Image=%TemporaryDirectory%\Windows.bin"
set "Map=%TemporaryDirectory%\Windows.wvmap"
set "UnpatchedApplication=%TemporaryDirectory%\Baseline-Jit-Publisher-Unpatched.exe"
set "ApplicationError=%TemporaryDirectory%\Application.err"
set "Application=%RepositoryRoot%\Artifacts\Baseline-Jit-Publisher\windows-x64\Baseline-Jit-Publisher.exe"

call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" "%RepositoryRoot%\Runtime\Native\Baseline-Jit-Patch-Plan-X64.wva" "%PlanWvo%" >nul
if errorlevel 1 goto :failed
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%PlanWvo%" >nul
if errorlevel 1 goto :failed
call :verify_hash "%PlanWvo%" 9074413259924bb50e8a98ca14690e0ec34a65b28c15f0d27a69799c7071f763 "shared-plan WVO"
if errorlevel 1 goto :failed

call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" "%RepositoryRoot%\Runtime\Native\Windows-X64-Baseline-Jit-Publisher.wva" "%PlatformWvo%" >nul
if errorlevel 1 goto :failed
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%PlatformWvo%" >nul
if errorlevel 1 goto :failed
call :verify_hash "%PlatformWvo%" 3f5069815b01798374b0974f20e8d344b562d1a08797c6f15dc9125373ba18d6 "Windows-adapter WVO"
if errorlevel 1 goto :failed

call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 4208 Main "%Image%" "%PlatformWvo%" "%PlanWvo%" > "%Map%"
if errorlevel 1 goto :failed
call :verify_hash "%Image%" 43c58d27a733f74fdec15413a2cc649356eade3c0b9f7651b0c8d81d47b219d9 "Windows flat image"
if errorlevel 1 goto :failed

set "EntryAddress="
for /f "tokens=5 delims== " %%E in ('findstr /B /C:"entry name=Main address=" "%Map%"') do set "EntryAddress=%%E"
if not defined EntryAddress (
    >&2 echo The native baseline-JIT publisher entry is missing from the link map.
    goto :failed
)
set /a EntryOffset=EntryAddress-4208
if not "%EntryOffset%"=="513" (
    >&2 echo The native baseline-JIT publisher entry offset is %EntryOffset%, expected 513.
    goto :failed
)

call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Image%" "%EntryOffset%" "%UnpatchedApplication%" >nul
if errorlevel 1 goto :failed
call :verify_hash "%UnpatchedApplication%" 0c27a724a85daa54fc23a5d7f09e1e6f9344d711080e46aef80cfc2d91b1ceed "unpatched Windows application"
if errorlevel 1 goto :failed
call :verify_hash "%Application%" fc7566f38457229444836b88aff48df09309b3bad242d1cac2eb2f432311ab39 "published Windows application"
if errorlevel 1 goto :failed

"%Application%" >nul 2> "%ApplicationError%"
set "ApplicationResult=%ERRORLEVEL%"
if not "%ApplicationResult%"=="0" (
    >&2 echo The native baseline-JIT publisher result is %ApplicationResult%, expected 0.
    goto :failed
)
for %%F in ("%ApplicationError%") do if not "%%~zF"=="0" (
    >&2 echo The native baseline-JIT publisher wrote a diagnostic.
    goto :failed
)

call :cleanup
echo native baseline jit publisher status=Passed result=0 platform=windows-x64
exit /b 0

:verify_hash
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~2" >nul
if errorlevel 1 (
    >&2 echo The native baseline-JIT publisher %~3 identity differs.
    exit /b 1
)
exit /b 0

:failed
call :cleanup
exit /b 1

:cleanup
if exist "%PlanWvo%" del /f /q "%PlanWvo%" >nul 2>nul
if exist "%PlatformWvo%" del /f /q "%PlatformWvo%" >nul 2>nul
if exist "%Image%" del /f /q "%Image%" >nul 2>nul
if exist "%Map%" del /f /q "%Map%" >nul 2>nul
if exist "%UnpatchedApplication%" del /f /q "%UnpatchedApplication%" >nul 2>nul
if exist "%ApplicationError%" del /f /q "%ApplicationError%" >nul 2>nul
if exist "%TemporaryDirectory%" rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0
