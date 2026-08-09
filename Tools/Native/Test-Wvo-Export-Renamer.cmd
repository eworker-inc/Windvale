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
set "Status=1"

call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" "%RepositoryRoot%\Tests\Native\Wvo-Export-Renamer\Input.wva" "%Input%" >nul 2>&1
if errorlevel 1 goto :failure
call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" "%RepositoryRoot%\Tests\Native\Wvo-Export-Renamer\Expected.wva" "%Expected%" >nul 2>&1
if errorlevel 1 goto :failure
for /f "tokens=1" %%H in ('certutil -hashfile "%Input%" SHA256 ^| findstr /r /x "[0-9a-fA-F][0-9a-fA-F]*"') do set "InputDigest=%%H"
for /f "tokens=1" %%H in ('certutil -hashfile "%Expected%" SHA256 ^| findstr /r /x "[0-9a-fA-F][0-9a-fA-F]*"') do set "ExpectedDigest=%%H"
if not defined InputDigest goto :failure
if not defined ExpectedDigest goto :failure

call "%RepositoryRoot%\Tools\Native\Rename-Wvo-Export.cmd" "%Input%" Main Renamed_entry "%Output%" >nul 2>&1
if errorlevel 1 goto :failure
fc /b "%Output%" "%Expected%" >nul
if errorlevel 1 goto :failure
call :verify_preserved
if errorlevel 1 goto :failure

call "%RepositoryRoot%\Tools\Native\Rename-Wvo-Export.cmd" "%Input%" Missing Renamed_entry "%TemporaryDirectory%\Missing.wvo" >nul 2>&1
if not errorlevel 1 goto :failure
if exist "%TemporaryDirectory%\Missing.wvo" goto :failure
call :verify_preserved
if errorlevel 1 goto :failure

call "%RepositoryRoot%\Tools\Native\Rename-Wvo-Export.cmd" "%Input%" Main bad-name "%TemporaryDirectory%\Invalid.wvo" >nul 2>&1
if not errorlevel 1 goto :failure
if exist "%TemporaryDirectory%\Invalid.wvo" goto :failure
call :verify_preserved
if errorlevel 1 goto :failure

call "%RepositoryRoot%\Tools\Native\Rename-Wvo-Export.cmd" "%Input%" Main Renamed_entry "%Expected%" >nul 2>&1
if not errorlevel 1 goto :failure
call :verify_preserved
if errorlevel 1 goto :failure

echo Tests: 4, Passed: 4, Failed: 0
set "Status=0"
goto :cleanup

:verify_preserved
certutil -hashfile "%Input%" SHA256 | findstr /i /c:"%InputDigest%" >nul
if errorlevel 1 exit /b 1
certutil -hashfile "%Expected%" SHA256 | findstr /i /c:"%ExpectedDigest%" >nul
exit /b %ERRORLEVEL%

:failure
>&2 echo The native WVO export-renamer focused test failed.

:cleanup
if exist "%TemporaryDirectory%" rmdir /s /q "%TemporaryDirectory%"
exit /b %Status%
