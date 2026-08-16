@echo off
setlocal EnableExtensions DisableDelayedExpansion
if not "%~1"=="" exit /b 64
set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "Work=%TEMP%\windvale-os-application-launch-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Status=1"
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Operating-System\Windvale-Os-Application-Launch-Policy.wvproj" "%Work%\Policy.wvb" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Operating-System\Windvale-Os-Application-Start-Request.wvproj" "%Work%\Request.wvb" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-Application-Launch.wvproj" "%Work%\Test.wvb" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-Application-Start-Request.wvproj" "%Work%\Request-Test.wvb" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Operating-System\Windvale-Os-Application-Start-User-Copy.wvproj" "%Work%\Copy-Policy.wvb" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-Application-Start-User-Copy.wvproj" "%Work%\Copy-Test.wvb" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Operating-System\Windvale-Os-Service-Launch-Policy.wvproj" "%Work%\Service-Policy.wvb" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-Service-Launch.wvproj" "%Work%\Service-Test.wvb" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Operating-System\Windvale-Os-Application-Machine-Construction-Policy.wvproj" "%Work%\Machine-Policy.wvb" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-Application-Machine-Construction.wvproj" "%Work%\Machine-Test.wvb" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Run-Wvb.cmd" "%Work%\Test.wvb" >"%Work%\Run.out" 2>"%Work%\Run.err"
if errorlevel 1 goto :cleanup
set "Actual="
set /p "Actual=" <"%Work%\Run.out"
if not "%Actual%"=="Result: 42" goto :cleanup
for %%E in ("%Work%\Run.err") do if not "%%~zE"=="0" goto :cleanup
call "%RepositoryRoot%\Tools\Native\Run-Wvb.cmd" "%Work%\Request-Test.wvb" >"%Work%\Request-Run.out" 2>"%Work%\Request-Run.err"
if errorlevel 1 goto :cleanup
set "Actual="
set /p "Actual=" <"%Work%\Request-Run.out"
if not "%Actual%"=="Result: 44" goto :cleanup
for %%E in ("%Work%\Request-Run.err") do if not "%%~zE"=="0" goto :cleanup
call "%RepositoryRoot%\Tools\Native\Run-Wvb.cmd" "%Work%\Copy-Test.wvb" >"%Work%\Copy-Run.out" 2>"%Work%\Copy-Run.err"
if errorlevel 1 goto :cleanup
set "Actual="
set /p "Actual=" <"%Work%\Copy-Run.out"
if not "%Actual%"=="Result: 46" goto :cleanup
for %%E in ("%Work%\Copy-Run.err") do if not "%%~zE"=="0" goto :cleanup
call "%RepositoryRoot%\Tools\Native\Run-Wvb.cmd" "%Work%\Service-Test.wvb" >"%Work%\Service-Run.out" 2>"%Work%\Service-Run.err"
if errorlevel 1 goto :cleanup
set "Actual="
set /p "Actual=" <"%Work%\Service-Run.out"
if not "%Actual%"=="Result: 45" goto :cleanup
for %%E in ("%Work%\Service-Run.err") do if not "%%~zE"=="0" goto :cleanup
call "%RepositoryRoot%\Tools\Native\Run-Wvb.cmd" "%Work%\Machine-Test.wvb" >"%Work%\Machine-Run.out" 2>"%Work%\Machine-Run.err"
if errorlevel 1 goto :cleanup
set "Actual="
set /p "Actual=" <"%Work%\Machine-Run.out"
if not "%Actual%"=="Result: 43" goto :cleanup
for %%E in ("%Work%\Machine-Run.err") do if not "%%~zE"=="0" goto :cleanup
echo native os application launch status=Passed projects=7 behavior=5 cases=41
set "Status=0"
:cleanup
if exist "%Work%\." rmdir /s /q "%Work%"
exit /b %Status%
