@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "TemporaryDirectory=%TEMP%\windvale-native-os-process-object-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" >nul 2>&1
if errorlevel 1 exit /b 1
set "Object=%TemporaryDirectory%\05-process.wvo"
set "Existing=%TemporaryDirectory%\Existing.wvo"
set "Status=1"

call "%RepositoryRoot%\Tools\Native\Build-Os-Process-Object.cmd" "%Object%" >nul 2>&1
if errorlevel 1 goto :failure
for %%F in ("%Object%") do if not "%%~zF"=="956230" goto :failure
certutil -hashfile "%Object%" SHA256 | findstr /i /x /c:"6c54a37dbe4e08d43068fed9bfb98edea536ae097666fa2c793a1c1bea9f9ac3" >nul
if errorlevel 1 goto :failure
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%Object%" >nul 2>&1
if errorlevel 1 goto :failure

>"%Existing%" echo preserved
for /f "tokens=1" %%H in ('certutil -hashfile "%Existing%" SHA256 ^| findstr /r /x "[0-9a-fA-F][0-9a-fA-F]*"') do set "ExistingDigest=%%H"
if not defined ExistingDigest goto :failure
call "%RepositoryRoot%\Tools\Native\Build-Os-Process-Object.cmd" "%Existing%" >nul 2>&1
if not errorlevel 1 goto :failure
certutil -hashfile "%Existing%" SHA256 | findstr /i /x /c:"%ExistingDigest%" >nul
if errorlevel 1 goto :failure

echo Tests: 2, Passed: 2, Failed: 0
set "Status=0"
goto :cleanup

:failure
>&2 echo The native OS process-object focused test failed.

:cleanup
if exist "%TemporaryDirectory%" rmdir /s /q "%TemporaryDirectory%"
exit /b %Status%
