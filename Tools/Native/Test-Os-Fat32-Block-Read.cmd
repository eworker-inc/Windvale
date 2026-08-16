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
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Operating-System\Windvale-Os-Fat32-Block-Image-Provider.wvproj" "%Work%\Image-Provider.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Image-Provider.wvb" 4639 60b56a15ad26ff54993e004768439f6a567353debd4a95e05efe60550b89a5bf
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Operating-System\Windvale-Os-Fat32-Block-Exchange-State.wvproj" "%Work%\Exchange.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Exchange.wvb" 20279 820617dc73799c5cbaea318d85a0e6352e539889eb6f3ea525c2dee22cca6690
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-Fat32-Block-Read-Transaction.wvproj" "%Work%\Test.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.wvb" 48627 d46c881e3313836e5f6293940e6a35d072344f4541787870dfe2b9bd61a53de6
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\Test.wvb" "%Work%\Test.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.wvo" 999688 363e58478646a9dac0ce9af53b33f4a650c17282f0019b990fe66c5b81bfa1a6
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\Test.bin" "%Work%\Test.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.bin" 997504 6cb0edc44d71524197e52a396731a1fb78aae60e2882b1744918961faaf25f91
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\Test.bin" 33885 "%Work%\Test.exe" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.exe" 999424 38079168f595ec6d488579b241a0e2018ee12ee52244cf905d7b9404de00588f
if errorlevel 1 goto :cleanup
"%Work%\Test.exe" >nul
if not "%ERRORLEVEL%"=="47" goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\Test.bin" 33885 "%Work%\Test.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.elf" 1003632 0659762421ef3ddf1c394ca7b4e305fec3bf571da020a6b377fa47020fd7db5c
if errorlevel 1 goto :cleanup
echo native os fat32 block image and exchange lifecycle status=Passed projects=5 cases=59 local-result=47 cross-host-images=Verified
set "Status=0"
:cleanup
if exist "%Work%\." rmdir /s /q "%Work%"
exit /b %Status%
:verify
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /i /x /c:"%~3" >nul
exit /b %ERRORLEVEL%
