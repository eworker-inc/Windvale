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

echo Tests: 6, Passed: 6, Failed: 0
set "Status=0"
goto :cleanup

:failure
>&2 echo The native OS Probe object producer focused test failed.

:cleanup
if exist "%TemporaryDirectory%" rmdir /s /q "%TemporaryDirectory%"
exit /b %Status%
