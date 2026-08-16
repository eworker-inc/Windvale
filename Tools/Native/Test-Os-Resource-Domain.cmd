@echo off
setlocal EnableExtensions DisableDelayedExpansion
if not "%~1"=="" exit /b 64
set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "Work=%TEMP%\windvale-os-resource-domain-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Status=1"
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Operating-System\Windvale-Os-Resource-Domain-Policy.wvproj" "%Work%\Policy.wvb" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Operating-System\Windvale-Os-Resource-Domain-Record.wvproj" "%Work%\Record.wvb" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-Resource-Domain.wvproj" "%Work%\Test.wvb" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Run-Wvb.cmd" "%Work%\Test.wvb" >"%Work%\Run.out" 2>"%Work%\Run.err"
if errorlevel 1 goto :cleanup
set "Actual="
set /p "Actual=" <"%Work%\Run.out"
if not "%Actual%"=="Result: 42" goto :cleanup
for %%E in ("%Work%\Run.err") do if not "%%~zE"=="0" goto :cleanup
echo native os resource domain status=Passed projects=2 behavior=2 cases=12
set "Status=0"
:cleanup
if exist "%Work%\." rmdir /s /q "%Work%"
exit /b %Status%
