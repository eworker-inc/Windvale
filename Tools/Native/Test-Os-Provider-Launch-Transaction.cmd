@echo off
setlocal EnableExtensions DisableDelayedExpansion
if not "%~1"=="" exit /b 64
set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "Work=%TEMP%\windvale-os-provider-launch-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Status=1"

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Operating-System\Windvale-Os-Provider-Launch-Transaction-Policy.wvproj" ^
    "%Work%\Policy.wvb" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-Provider-Launch-Transaction.wvproj" ^
    "%Work%\Transaction-Test.wvb" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-Provider-Launch-Lifecycle.wvproj" ^
    "%Work%\Lifecycle-Test.wvb" >nul
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Run-Wvb.cmd" ^
    "%Work%\Transaction-Test.wvb" >"%Work%\Transaction.out" 2>"%Work%\Transaction.err"
if errorlevel 1 goto :cleanup
set "Actual="
set /p "Actual=" <"%Work%\Transaction.out"
if not "%Actual%"=="Result: 48" goto :cleanup
for %%E in ("%Work%\Transaction.err") do if not "%%~zE"=="0" goto :cleanup

call "%RepositoryRoot%\Tools\Native\Run-Wvb.cmd" ^
    "%Work%\Lifecycle-Test.wvb" >"%Work%\Lifecycle.out" 2>"%Work%\Lifecycle.err"
if errorlevel 1 goto :cleanup
set "Actual="
set /p "Actual=" <"%Work%\Lifecycle.out"
if not "%Actual%"=="Result: 49" goto :cleanup
for %%E in ("%Work%\Lifecycle.err") do if not "%%~zE"=="0" goto :cleanup

echo native os provider launch transaction status=Passed projects=3 behavior=13 cases=18
set "Status=0"
:cleanup
if exist "%Work%\." rmdir /s /q "%Work%"
exit /b %Status%
