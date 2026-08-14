@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"

:allocate
set "TemporaryDirectory=%TEMP%\windvale-native-wvo-export-renamer-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" >nul 2>&1
if errorlevel 1 exit /b 1
set "Input=%TemporaryDirectory%\Input.wvo"
set "Expected=%TemporaryDirectory%\Expected.wvo"
set "Output=%TemporaryDirectory%\Output.wvo"
set "CommandLog=%TemporaryDirectory%\Command.log"
set "Status=1"

set "Stage=assemble input fixture"
call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" "%RepositoryRoot%\Tests\Native\Wvo-Export-Renamer\Input.wva" "%Input%" >"%CommandLog%" 2>&1
if errorlevel 1 goto :failure
set "Stage=assemble expected fixture"
call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" "%RepositoryRoot%\Tests\Native\Wvo-Export-Renamer\Expected.wva" "%Expected%" >"%CommandLog%" 2>&1
if errorlevel 1 goto :failure
set "Stage=hash input fixture"
certutil -hashfile "%Input%" SHA256 >"%CommandLog%" 2>&1
if errorlevel 1 goto :failure
for /f "tokens=1" %%H in ('findstr /r /x "[0-9a-fA-F][0-9a-fA-F]*" ^< "%CommandLog%"') do set "InputDigest=%%H"
if not defined InputDigest goto :failure
set "Stage=hash expected fixture"
certutil -hashfile "%Expected%" SHA256 >"%CommandLog%" 2>&1
if errorlevel 1 goto :failure
for /f "tokens=1" %%H in ('findstr /r /x "[0-9a-fA-F][0-9a-fA-F]*" ^< "%CommandLog%"') do set "ExpectedDigest=%%H"
if not defined ExpectedDigest goto :failure

set "Stage=rename existing export"
call "%RepositoryRoot%\Tools\Native\Rename-Wvo-Export.cmd" "%Input%" Main Renamed_entry "%Output%" >"%CommandLog%" 2>&1
if errorlevel 1 goto :failure
set "Stage=compare renamed object"
type nul >"%CommandLog%"
fc /b "%Output%" "%Expected%" >nul
if errorlevel 1 goto :failure
set "Stage=verify fixtures after successful rename"
call :verify_preserved
if errorlevel 1 goto :failure

set "Stage=reject missing export"
call "%RepositoryRoot%\Tools\Native\Rename-Wvo-Export.cmd" "%Input%" Missing Renamed_entry "%TemporaryDirectory%\Missing.wvo" >"%CommandLog%" 2>&1
if not errorlevel 1 goto :failure
set "Stage=reject missing export without output"
if exist "%TemporaryDirectory%\Missing.wvo" goto :failure
set "Stage=verify fixtures after missing-export rejection"
call :verify_preserved
if errorlevel 1 goto :failure

set "Stage=reject invalid export name"
call "%RepositoryRoot%\Tools\Native\Rename-Wvo-Export.cmd" "%Input%" Main bad-name "%TemporaryDirectory%\Invalid.wvo" >"%CommandLog%" 2>&1
if not errorlevel 1 goto :failure
set "Stage=reject invalid export name without output"
if exist "%TemporaryDirectory%\Invalid.wvo" goto :failure
set "Stage=verify fixtures after invalid-name rejection"
call :verify_preserved
if errorlevel 1 goto :failure

set "Stage=reject destination overwrite"
call "%RepositoryRoot%\Tools\Native\Rename-Wvo-Export.cmd" "%Input%" Main Renamed_entry "%Expected%" >"%CommandLog%" 2>&1
if not errorlevel 1 goto :failure
set "Stage=verify fixtures after overwrite rejection"
call :verify_preserved
if errorlevel 1 goto :failure

echo Tests: 4, Passed: 4, Failed: 0
set "Status=0"
goto :cleanup

:verify_preserved
certutil -hashfile "%Input%" SHA256 >"%CommandLog%" 2>&1
if errorlevel 1 exit /b 1
findstr /i /c:"%InputDigest%" "%CommandLog%" >nul
if errorlevel 1 exit /b 1
certutil -hashfile "%Expected%" SHA256 >"%CommandLog%" 2>&1
if errorlevel 1 exit /b 1
findstr /i /c:"%ExpectedDigest%" "%CommandLog%" >nul
exit /b %ERRORLEVEL%

:failure
>&2 echo The native WVO export-renamer focused test failed at stage: %Stage%.
if exist "%CommandLog%" for %%F in ("%CommandLog%") do if not "%%~zF"=="0" type "%CommandLog%" >&2

:cleanup
if exist "%TemporaryDirectory%" rmdir /s /q "%TemporaryDirectory%"
exit /b %Status%
