@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Baseline-Jit-Patch-Plan.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "TemporaryDirectory=%TEMP%\windvale-baseline-jit-plan-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1

set "Wvb=%TemporaryDirectory%\Baseline-Jit-Patch-Plan.wvb"
set "Wvo=%TemporaryDirectory%\Baseline-Jit-Patch-Plan.wvo"
set "Image=%TemporaryDirectory%\Baseline-Jit-Patch-Plan.bin"
set "Application=%TemporaryDirectory%\Baseline-Jit-Patch-Plan.exe"
set "Map=%TemporaryDirectory%\Baseline-Jit-Patch-Plan.wvmap"
set "ApplicationError=%TemporaryDirectory%\Application.err"

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects/Tests/Windvale-Native-Baseline-Jit-Patch-Plan-Self-Test.wvproj" "%Wvb%" >nul
if errorlevel 1 goto :failed
call :verify_hash "%Wvb%" 2934df86db71047bfd325d50fd9549362bc60953e6924d6242b56eb79be658ea "WVB"
if errorlevel 1 goto :failed

call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Wvb%" "%Wvo%" >nul
if errorlevel 1 goto :failed
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%Wvo%" >nul
if errorlevel 1 goto :failed
call :verify_hash "%Wvo%" fe3f9af8cb9315b866bc898814e1d954807a3486256cc23cdcbfbfbfa2608149 "WVO"
if errorlevel 1 goto :failed

call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 1048576 Main "%Image%" "%Wvo%" > "%Map%"
if errorlevel 1 goto :failed
call :verify_hash "%Image%" cbc1a556659a3e7829e60b759920c931f68cb61d6f0a4696823b1094b0ebfdcc "flat image"
if errorlevel 1 goto :failed
call :verify_hash "%Map%" fb89f964f40deae96d46d157d78a69f6212865ad23347805e987c80dccbf5256 "link map"
if errorlevel 1 goto :failed

set "EntryAddress="
for /f "tokens=5 delims== " %%E in ('findstr /B /C:"entry name=Main address=" "%Map%"') do set "EntryAddress=%%E"
if not defined EntryAddress (
    >&2 echo The native baseline-JIT patch-plan entry is missing from the link map.
    goto :failed
)
set /a EntryOffset=EntryAddress-1048576
if not "%EntryOffset%"=="3808" (
    >&2 echo The native baseline-JIT patch-plan entry offset is %EntryOffset%, expected 3808.
    goto :failed
)

call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Image%" "%EntryOffset%" "%Application%" >nul
if errorlevel 1 goto :failed
call :verify_hash "%Application%" 61ee666ac34825f0f1aee30bf18708dcd43054256dc56bb44657fa71426980f2 "Windows application"
if errorlevel 1 goto :failed

"%Application%" >nul 2> "%ApplicationError%"
set "ApplicationResult=%ERRORLEVEL%"
if not "%ApplicationResult%"=="0" (
    >&2 echo The native baseline-JIT patch-plan result is %ApplicationResult%, expected 0.
    goto :failed
)
for %%F in ("%ApplicationError%") do if not "%%~zF"=="0" (
    >&2 echo The native baseline-JIT patch-plan application wrote a diagnostic.
    goto :failed
)

call :cleanup
echo native baseline jit patch plan status=Passed result=0 entry-offset=3808
exit /b 0

:verify_hash
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~2" >nul
if errorlevel 1 (
    >&2 echo The native baseline-JIT patch-plan %~3 identity differs.
    exit /b 1
)
exit /b 0

:failed
call :cleanup
exit /b 1

:cleanup
if exist "%Wvb%" del /f /q "%Wvb%" >nul 2>nul
if exist "%Wvo%" del /f /q "%Wvo%" >nul 2>nul
if exist "%Image%" del /f /q "%Image%" >nul 2>nul
if exist "%Application%" del /f /q "%Application%" >nul 2>nul
if exist "%Map%" del /f /q "%Map%" >nul 2>nul
if exist "%ApplicationError%" del /f /q "%ApplicationError%" >nul 2>nul
if exist "%TemporaryDirectory%" rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0
