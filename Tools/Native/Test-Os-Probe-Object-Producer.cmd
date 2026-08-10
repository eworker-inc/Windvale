@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"

:allocate
set "TemporaryDirectory=%TEMP%\windvale-native-os-probe-object-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" >nul 2>&1
if errorlevel 1 exit /b 1
set "Exceptions=%TemporaryDirectory%\09-exceptions.wvo"
set "Admission=%TemporaryDirectory%\12-wvb-admission-bridge.wvo"
set "NativeBridge=%TemporaryDirectory%\13-native-bridge-and-support.wvo"
set "Paging=%TemporaryDirectory%\10-paging.wvo"
set "Memory=%TemporaryDirectory%\08-memory.wvo"
set "InvalidOpcodeMemory=%TemporaryDirectory%\08-memory-invalid-opcode.wvo"
set "GeneralProtectionMemory=%TemporaryDirectory%\08-memory-general-protection.wvo"
set "Loader=%TemporaryDirectory%\00-loader.wvo"
set "Existing=%TemporaryDirectory%\Existing.wvo"
set "Unknown=%TemporaryDirectory%\Unknown.wvo"
set "Invalid=%TemporaryDirectory%\Invalid.bin"
set "Status=1"

call "%RepositoryRoot%\Tools\Native\Produce-Os-Probe-Object.cmd" exceptions "%Exceptions%" >nul 2>&1
if errorlevel 1 goto :failure
if not exist "%Exceptions%" goto :failure
for %%F in ("%Exceptions%") do if not "%%~zF"=="483" goto :failure
certutil -hashfile "%Exceptions%" SHA256 | findstr /i /x /c:"9caeb7ce353bca33e3bbac729ecca0423d59f8ce6b65ccd6b54fa53c381d617c" >nul
if errorlevel 1 goto :failure
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%Exceptions%" >nul 2>&1
if errorlevel 1 goto :failure

call "%RepositoryRoot%\Tools\Native\Produce-Os-Probe-Object.cmd" wvb-admission-bridge "%Admission%" >nul 2>&1
if errorlevel 1 goto :failure
if not exist "%Admission%" goto :failure
for %%F in ("%Admission%") do if not "%%~zF"=="484" goto :failure
certutil -hashfile "%Admission%" SHA256 | findstr /i /x /c:"271c378b1f12bb4affa33474d865611cbf14e5b1b8996c703cb3d3cbe22eee7d" >nul
if errorlevel 1 goto :failure
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%Admission%" >nul 2>&1
if errorlevel 1 goto :failure

call "%RepositoryRoot%\Tools\Native\Produce-Os-Probe-Object.cmd" native-bridge-and-support "%NativeBridge%" >nul 2>&1
if errorlevel 1 goto :failure
if not exist "%NativeBridge%" goto :failure
for %%F in ("%NativeBridge%") do if not "%%~zF"=="461" goto :failure
certutil -hashfile "%NativeBridge%" SHA256 | findstr /i /x /c:"472a0fbe6497525e634a4785e92aa9ee62c3c7d70fff7510e45acbea644eea0b" >nul
if errorlevel 1 goto :failure
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%NativeBridge%" >nul 2>&1
if errorlevel 1 goto :failure

call "%RepositoryRoot%\Tools\Native\Produce-Os-Probe-Object.cmd" paging "%Paging%" >nul 2>&1
if errorlevel 1 goto :failure
if not exist "%Paging%" goto :failure
for %%F in ("%Paging%") do if not "%%~zF"=="1292" goto :failure
certutil -hashfile "%Paging%" SHA256 | findstr /i /x /c:"a6bcad24e4752acc1fbab75d6667e965f2ab4d5613edd2c8e6cda244616fba2d" >nul
if errorlevel 1 goto :failure
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%Paging%" >nul 2>&1
if errorlevel 1 goto :failure

call "%RepositoryRoot%\Tools\Native\Produce-Os-Probe-Object.cmd" memory "%Memory%" >nul 2>&1
if errorlevel 1 goto :failure
if not exist "%Memory%" goto :failure
for %%F in ("%Memory%") do if not "%%~zF"=="1529" goto :failure
certutil -hashfile "%Memory%" SHA256 | findstr /i /x /c:"2668e17c3181e168415fb7bdee530873e2ddc8fa2d100af94bcc7b74909df3ed" >nul
if errorlevel 1 goto :failure
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%Memory%" >nul 2>&1
if errorlevel 1 goto :failure

call "%RepositoryRoot%\Tools\Native\Produce-Os-Probe-Object.cmd" memory-invalid-opcode "%InvalidOpcodeMemory%" >nul 2>&1
if errorlevel 1 goto :failure
if not exist "%InvalidOpcodeMemory%" goto :failure
for %%F in ("%InvalidOpcodeMemory%") do if not "%%~zF"=="1545" goto :failure
certutil -hashfile "%InvalidOpcodeMemory%" SHA256 | findstr /i /x /c:"09aa0fcfe12c561b79367cb26569dbc6f1f47ca3b98dc892426ca57b4328f868" >nul
if errorlevel 1 goto :failure
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%InvalidOpcodeMemory%" >nul 2>&1
if errorlevel 1 goto :failure

call "%RepositoryRoot%\Tools\Native\Produce-Os-Probe-Object.cmd" memory-general-protection "%GeneralProtectionMemory%" >nul 2>&1
if errorlevel 1 goto :failure
if not exist "%GeneralProtectionMemory%" goto :failure
for %%F in ("%GeneralProtectionMemory%") do if not "%%~zF"=="1545" goto :failure
certutil -hashfile "%GeneralProtectionMemory%" SHA256 | findstr /i /x /c:"23a052f9d47a9416618c9b7a50a382c68c46d3bf7834410cc79f8fef2aa461e0" >nul
if errorlevel 1 goto :failure
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%GeneralProtectionMemory%" >nul 2>&1
if errorlevel 1 goto :failure

call "%RepositoryRoot%\Tools\Native\Produce-Os-Probe-Object.cmd" loader "%Loader%" >nul 2>&1
if errorlevel 1 goto :failure
if not exist "%Loader%" goto :failure
for %%F in ("%Loader%") do if not "%%~zF"=="6336" goto :failure
certutil -hashfile "%Loader%" SHA256 | findstr /i /x /c:"b310bc0e9aebc7b14c0892bb3dd4b833d42539c2194427a8f333b511d6af3804" >nul
if errorlevel 1 goto :failure
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%Loader%" >nul 2>&1
if errorlevel 1 goto :failure

>"%Existing%" echo preserved
for /f "tokens=1" %%H in ('certutil -hashfile "%Existing%" SHA256 ^| findstr /r /x "[0-9a-fA-F][0-9a-fA-F]*"') do set "ExistingDigest=%%H"
if not defined ExistingDigest goto :failure
call "%RepositoryRoot%\Tools\Native\Produce-Os-Probe-Object.cmd" exceptions "%Existing%" >nul 2>&1
if not errorlevel 1 goto :failure
certutil -hashfile "%Existing%" SHA256 | findstr /i /x /c:"%ExistingDigest%" >nul
if errorlevel 1 goto :failure

call "%RepositoryRoot%\Tools\Native\Produce-Os-Probe-Object.cmd" unknown "%Unknown%" >nul 2>&1
if not "%ERRORLEVEL%"=="64" goto :failure
if exist "%Unknown%" goto :failure

call "%RepositoryRoot%\Tools\Native\Produce-Os-Probe-Object.cmd" exceptions "%Invalid%" >nul 2>&1
if not "%ERRORLEVEL%"=="64" goto :failure
if exist "%Invalid%" goto :failure

echo Tests: 11, Passed: 11, Failed: 0
set "Status=0"
goto :cleanup

:failure
>&2 echo The native OS Probe object producer focused test failed.

:cleanup
if exist "%TemporaryDirectory%" rmdir /s /q "%TemporaryDirectory%"
exit /b %Status%
