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
set "BridgeWvb=%TemporaryDirectory%\Bridge.wvb"
set "RetainedBridgeWvb=%RepositoryRoot%\Artifacts\Baseline-Jit-Publisher\Wvb\Baseline-Jit-Patch-Plan-Bridge.wvb"
set "BridgeWvo=%RepositoryRoot%\Artifacts\Baseline-Jit-Publisher\Wvo\Baseline-Jit-Patch-Plan-Bridge.wvo"
set "Image=%TemporaryDirectory%\Windows.bin"
set "Map=%TemporaryDirectory%\Windows.wvmap"
set "UnpatchedApplication=%TemporaryDirectory%\Baseline-Jit-Publisher-Unpatched.exe"
set "ApplicationError=%TemporaryDirectory%\Application.err"
set "Application=%RepositoryRoot%\Artifacts\Baseline-Jit-Publisher\windows-x64\Baseline-Jit-Publisher.exe"

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Windvale-Native-Baseline-Jit-Patch-Plan-Bridge.wvproj" "%BridgeWvb%" >nul
if errorlevel 1 goto :failed
call :verify_hash "%BridgeWvb%" 2dc536e9d3511d4fde3191e1084d9634543154a525623fd3c7c669f9d3bf20d9 "producer-bridge WVB"
if errorlevel 1 goto :failed
call :verify_hash "%RetainedBridgeWvb%" 2dc536e9d3511d4fde3191e1084d9634543154a525623fd3c7c669f9d3bf20d9 "retained producer-bridge WVB"
if errorlevel 1 goto :failed
fc /b "%BridgeWvb%" "%RetainedBridgeWvb%" >nul
if errorlevel 1 (
    >&2 echo The rebuilt and retained native baseline-JIT producer-bridge WVBs differ.
    goto :failed
)
call :verify_hash "%BridgeWvo%" bcc02cdc6134da2388265ad308d3dc739a7e10c1911effa918d5f2577c86ae8c "retained producer-bridge WVO"
if errorlevel 1 goto :failed
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%BridgeWvo%" >nul
if errorlevel 1 goto :failed

call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" "%RepositoryRoot%\Runtime\Native\Baseline-Jit-Patch-Plan-X64.wva" "%PlanWvo%" >nul
if errorlevel 1 goto :failed
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%PlanWvo%" >nul
if errorlevel 1 goto :failed
call :verify_hash "%PlanWvo%" 8cc9c7460229a479adf34631a970c9d196b37361ceaa35fdea85e15fce9d91b1 "shared-plan WVO"
if errorlevel 1 goto :failed

call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" "%RepositoryRoot%\Runtime\Native\Windows-X64-Baseline-Jit-Publisher.wva" "%PlatformWvo%" >nul
if errorlevel 1 goto :failed
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%PlatformWvo%" >nul
if errorlevel 1 goto :failed
call :verify_hash "%PlatformWvo%" fc9c59e7005a0c60dd1a9a0240635b4416e509ef5e273745e35f1b2aca94b4ca "Windows-adapter WVO"
if errorlevel 1 goto :failed

call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 4208 Windows_baseline_jit_entry "%Image%" "%PlatformWvo%" "%PlanWvo%" "%BridgeWvo%" > "%Map%"
if errorlevel 1 goto :failed
call :verify_hash "%Image%" db35482f2886077701c4a8a78f6783fae5adeeaf2821411cbcf21bb480f1bdd3 "Windows flat image"
if errorlevel 1 goto :failed

set "EntryAddress="
for /f "tokens=5 delims== " %%E in ('findstr /B /C:"entry name=Windows_baseline_jit_entry address=" "%Map%"') do set "EntryAddress=%%E"
if not defined EntryAddress (
    >&2 echo The native baseline-JIT publisher entry is missing from the link map.
    goto :failed
)
set /a EntryOffset=EntryAddress-4208
if not "%EntryOffset%"=="718" (
    >&2 echo The native baseline-JIT publisher entry offset is %EntryOffset%, expected 718.
    goto :failed
)

call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Image%" "%EntryOffset%" "%UnpatchedApplication%" >nul
if errorlevel 1 goto :failed
call :verify_hash "%UnpatchedApplication%" e53b7aa85eb65db57bb93a1ad00065ab1462219d8030096120f7ce32a1eeb599 "unpatched Windows application"
if errorlevel 1 goto :failed
call :verify_hash "%Application%" 8ea1a0d6371c9447031db4ae2b56ecfef5f022a83b6bdd7831020a2628bee01c "published Windows application"
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
if exist "%BridgeWvb%" del /f /q "%BridgeWvb%" >nul 2>nul
if exist "%Image%" del /f /q "%Image%" >nul 2>nul
if exist "%Map%" del /f /q "%Map%" >nul 2>nul
if exist "%UnpatchedApplication%" del /f /q "%UnpatchedApplication%" >nul 2>nul
if exist "%ApplicationError%" del /f /q "%ApplicationError%" >nul 2>nul
if exist "%TemporaryDirectory%" rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0
