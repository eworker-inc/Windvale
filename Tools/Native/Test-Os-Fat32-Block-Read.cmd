@echo off
setlocal EnableExtensions DisableDelayedExpansion
if not "%~1"=="" exit /b 64
set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "Work=%TEMP%\windvale-os-fat32-block-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Status=1"
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Operating-System\Windvale-Os-Fat32-Block-Read-Transaction.wvproj" "%Work%\Policy.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Policy.wvb" 5036 8e6d447b4ee2bcbb6b549d37d42d1093ac7c1aa18ffacaa3f2e09bb4fcc913b5
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Operating-System\Windvale-Os-Fat32-Block-Provider-Protocol.wvproj" "%Work%\Protocol.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Protocol.wvb" 8726 5d37a54cc6e6763aca7f1e2c76d128cedae49d5febeef6ffa85d1d4de7e1348e
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-Fat32-Block-Read-Transaction.wvproj" "%Work%\Test.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.wvb" 15872 f342cd005c88851805a82fd68d36deb9795b187862f71f250c9a455e9a6ba626
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\Test.wvb" "%Work%\Test.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.wvo" 203304 a53fe125aa70aa9708edba372d90d2f59ed7dcf8c56338ce2d39ff6749f64c87
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\Test.bin" "%Work%\Test.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.bin" 202208 1435288069e2eacab975e4ed998d0e268fe06cfb3bd4fadcdb4cd4dbeb804ed7
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\Test.bin" 7930 "%Work%\Test.exe" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.exe" 204288 29ec32f60c50262bb386cc15288d227205beea1b5f0ed63c888826cc7fed59fc
if errorlevel 1 goto :cleanup
"%Work%\Test.exe" >nul
if not "%ERRORLEVEL%"=="47" goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\Test.bin" 7930 "%Work%\Test.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.elf" 209008 e4da4f92c0f12367d662a93a489ce8246ca1d92baca3a93b042b4614967ac853
if errorlevel 1 goto :cleanup
echo native os fat32 block provider protocol status=Passed projects=3 cases=22 local-result=47 cross-host-images=Verified
set "Status=0"
:cleanup
if exist "%Work%\." rmdir /s /q "%Work%"
exit /b %Status%
:verify
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /i /x /c:"%~3" >nul
exit /b %ERRORLEVEL%
