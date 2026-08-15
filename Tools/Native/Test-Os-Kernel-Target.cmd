@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"

:allocate
set "TemporaryDirectory=%TEMP%\windvale-native-os-kernel-target-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" >nul 2>&1
if errorlevel 1 exit /b 1
set "Module=%TemporaryDirectory%\Hello-World.wvb"
set "Object=%TemporaryDirectory%\01-kernel.wvo"
set "Existing=%TemporaryDirectory%\Existing.wvo"
set "Unsupported=%TemporaryDirectory%\Unsupported.wvo"
set "UnsupportedModule=%RepositoryRoot%\Artifacts\Native-Os-Probe-Memory-Object-Producer-Candidate\Os-Probe-Memory-Object-Producer.wvb"
set "Malformed=%TemporaryDirectory%\Malformed.wvb"
set "MalformedOutput=%TemporaryDirectory%\Malformed.wvo"
set "Target=%RepositoryRoot%\Artifacts\Native-Os-Kernel-Target-Candidate\windows-x64-os-kernel-target.exe"
set "Status=1"

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects/Operating-System/Windvale-Os-Kernel-Markers.wvproj" "%Module%" >nul 2>&1
if errorlevel 1 goto :failure
call :verify "%Module%" 1581 795734982cded8b3605cb5cf0f110667b71140d5639185c3ef94cde3174b3bc0
if errorlevel 1 goto :failure
call "%RepositoryRoot%\Tools\Native\Lower-Os-Kernel-Wvb.cmd" "%Module%" "%Object%" >nul 2>&1
if errorlevel 1 goto :failure
call :verify "%Object%" 13454 4bf896ac2b349d9e786bbb7cae0165cb47273aa82ff2985a7ff33c3185978e8b
if errorlevel 1 goto :failure
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%Object%" >nul 2>&1
if errorlevel 1 goto :failure

>"%Existing%" echo preserved
for /f "tokens=1" %%H in ('certutil -hashfile "%Existing%" SHA256 ^| findstr /r /x "[0-9a-fA-F][0-9a-fA-F]*"') do set "ExistingDigest=%%H"
if not defined ExistingDigest goto :failure
call "%RepositoryRoot%\Tools\Native\Lower-Os-Kernel-Wvb.cmd" "%Module%" "%Existing%" >nul 2>&1
if not errorlevel 1 goto :failure
certutil -hashfile "%Existing%" SHA256 | findstr /i /x /c:"%ExistingDigest%" >nul
if errorlevel 1 goto :failure

call "%RepositoryRoot%\Tools\Native\Verify-Wvb.cmd" "%UnsupportedModule%" >nul 2>&1
if errorlevel 1 goto :failure
call "%RepositoryRoot%\Tools\Native\Lower-Os-Kernel-Wvb.cmd" "%UnsupportedModule%" "%Unsupported%" >nul 2>&1
if not errorlevel 1 goto :failure
if exist "%Unsupported%" goto :failure

call :direct_rejection "Tests\Native\Malformed-Wvb\Truncated.wvb.b64"
if errorlevel 1 goto :failure
call :direct_rejection "Tests\Native\Malformed-Wvb\Trailing.wvb.b64"
if errorlevel 1 goto :failure
call :direct_rejection "Tests\Native\Malformed-Wvb\Bad-Utf8.wvb.b64"
if errorlevel 1 goto :failure
call :direct_rejection "Tests\Native\Malformed-Wvb\Typed-Declared-Maximum-Stack.wvb.b64"
if errorlevel 1 goto :failure

echo Tests: 7, Passed: 7, Failed: 0
set "Status=0"
goto :cleanup

:verify
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /i /x /c:"%~3" >nul
exit /b %ERRORLEVEL%

:direct_rejection
certutil -f -decode "%RepositoryRoot%\%~1" "%Malformed%" >nul 2>&1
if errorlevel 1 exit /b 1
"%Target%" "%Malformed%" "%MalformedOutput%" >nul 2>&1
if not errorlevel 1 exit /b 1
if exist "%MalformedOutput%" exit /b 1
del /f /q "%Malformed%" >nul 2>nul
exit /b 0

:failure
>&2 echo The native OS kernel target focused test failed.

:cleanup
if exist "%TemporaryDirectory%" rmdir /s /q "%TemporaryDirectory%"
exit /b %Status%
