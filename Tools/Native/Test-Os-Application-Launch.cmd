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
call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" "%RepositoryRoot%\Operating-System\Kernel\X64-Application-Start-User-Copy.wva" "%Work%\Start-Copy.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Start-Copy.wvo" 799 74978b1f6124517b44205cba52aaf6c161cf5d00e39ff9ab3ad883d527c87ddb
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" "%RepositoryRoot%\Tests\Native\X64-Application-Start-User-Copy-Self-Test.wva" "%Work%\Start-Copy-Test.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Start-Copy-Test.wvo" 1432 4a7b3fb803e8cea12a2c828ca1947f8ca90d554ad44c0eb7bbfa8a73c7dd691d
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\Start-Copy-Test.bin" "%Work%\Start-Copy-Test.wvo" "%Work%\Start-Copy.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Start-Copy-Test.bin" 4288 19411b99859049d7453bd17c3d473e0141122213b39d9c9f4be5356c6b495cc1
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\Start-Copy-Test.bin" 0 "%Work%\Start-Copy-Test.exe" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Start-Copy-Test.exe" 6144 cf4e8f6b531a2770c318e445e646ca776b6f8e167e7d569a92b3a8e8fcbda904
if errorlevel 1 goto :cleanup
"%Work%\Start-Copy-Test.exe" >nul
if not "%ERRORLEVEL%"=="47" goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\Start-Copy-Test.bin" 0 "%Work%\Start-Copy-Test.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Start-Copy-Test.elf" 12400 2cb4b5cedef3d82483a13f60e8be3ed6df9f63c3566abd894aa4da42ff5fbaaa
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
echo native os application launch status=Passed projects=7 native-leaves=1 behavior=6 cases=52 local-result=47 cross-host-images=Verified
set "Status=0"
:cleanup
if exist "%Work%\." rmdir /s /q "%Work%"
exit /b %Status%
:verify
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /i /x /c:"%~3" >nul
exit /b %ERRORLEVEL%
