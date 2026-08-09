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
set "Existing=%TemporaryDirectory%\Existing.wvo"
set "Unknown=%TemporaryDirectory%\Unknown.wvo"
set "Invalid=%TemporaryDirectory%\Invalid.bin"
set "Status=1"

call "%RepositoryRoot%\Tools\Native\Produce-Os-Probe-Object.cmd" exceptions "%Exceptions%" >nul 2>&1
if errorlevel 1 goto :failure
call :verify_output "%Exceptions%" 483 9caeb7ce353bca33e3bbac729ecca0423d59f8ce6b65ccd6b54fa53c381d617c
if errorlevel 1 goto :failure
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%Exceptions%" >nul 2>&1
if errorlevel 1 goto :failure

call "%RepositoryRoot%\Tools\Native\Produce-Os-Probe-Object.cmd" wvb-admission-bridge "%Admission%" >nul 2>&1
if errorlevel 1 goto :failure
call :verify_output "%Admission%" 484 271c378b1f12bb4affa33474d865611cbf14e5b1b8996c703cb3d3cbe22eee7d
if errorlevel 1 goto :failure
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%Admission%" >nul 2>&1
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

echo Tests: 5, Passed: 5, Failed: 0
set "Status=0"
goto :cleanup

:verify_output
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /i /x /c:"%~3" >nul
exit /b %ERRORLEVEL%

:failure
>&2 echo The native OS Probe object producer focused test failed.

:cleanup
if exist "%TemporaryDirectory%" rmdir /s /q "%TemporaryDirectory%"
exit /b %Status%
