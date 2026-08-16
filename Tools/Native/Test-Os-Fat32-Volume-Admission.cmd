@echo off
setlocal EnableExtensions DisableDelayedExpansion
if not "%~1"=="" exit /b 64
set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "Work=%TEMP%\windvale-os-fat32-volume-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Status=1"
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Operating-System\Windvale-Os-Fat32-Volume-Admission.wvproj" "%Work%\Volume.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Volume.wvb" 7654 564793e2af919a9adf7623f28775f653ac89cc642c5bb0cd22624cde896645e8
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Operating-System\Windvale-Os-Fat32-Cluster-Chain.wvproj" "%Work%\Chain.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Chain.wvb" 6359 75470d2a1c48c86754e2f91cd5919306fe73d76c567b87f7490fc87cc1eeeb1a
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Operating-System\Windvale-Os-Fat32-Directory-Admission.wvproj" "%Work%\Directory.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Directory.wvb" 6340 14548e1da399a95bb8c25be9c9224b4d524c729457992c2dc26ef153561b7733
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-Fat32-Volume-Admission.wvproj" "%Work%\Test.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.wvb" 25600 c978805d2dec9acb9ba08e3fa9466d5f21aab013aff0f6d6c807666ac986bcd9
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\Test.wvb" "%Work%\Test.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.wvo" 264918 f76bbf03b2ea89434c089480d89f825b55b39c822a07cfcefb8f655400b99c6c
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\Test.bin" "%Work%\Test.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.bin" 264072 13cfc508c60525df095300d4c97696db27795c069eecfdbe6c7030be27362b81
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\Test.bin" 2483 "%Work%\Test.exe" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.exe" 265728 3caf2067fcdaefcc142d9b9c92c23f2e4056e2b0303f52793a9c50908b1c61ee
if errorlevel 1 goto :cleanup
"%Work%\Test.exe" >nul
if not "%ERRORLEVEL%"=="47" goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\Test.bin" 2483 "%Work%\Test.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.elf" 270448 6b19c412245ecaec65d0e83371f1d160b71d030c47d76d63e1cf67d6856b1ae4
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-Fat32-Directory-Admission.wvproj" "%Work%\Directory-Test.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Directory-Test.wvb" 12922 adfd7e04136874ab60157cf5a718565a8aa73f19cebe61a32d2c219dfb6c0dc7
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\Directory-Test.wvb" "%Work%\Directory-Test.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Directory-Test.wvo" 132089 88ace5e3672136f1834b81ca54c16141c7583aa7f5461f2bb0a9716b593a393e
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\Directory-Test.bin" "%Work%\Directory-Test.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Directory-Test.bin" 130449 eeafe20120fcce8e657c0f3b70c57306436fe1d548c72536fb12720fe3f724b9
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\Directory-Test.bin" 5611 "%Work%\Directory-Test.exe" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Directory-Test.exe" 132608 1debd01031f349f444aa66f09f7bd248fdf65333cbaa279336701503f88651c0
if errorlevel 1 goto :cleanup
"%Work%\Directory-Test.exe" >nul
if not "%ERRORLEVEL%"=="47" goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\Directory-Test.bin" 5611 "%Work%\Directory-Test.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Directory-Test.elf" 135280 b2f9e66d2a78b9f85ac4aed80d6a517cdad154313265fe6925c917ff7502b1a5
if errorlevel 1 goto :cleanup
echo native os fat32 volume chain and directory admission status=Passed projects=5 cases=64 local-result=47 cross-host-images=Verified
set "Status=0"
:cleanup
if exist "%Work%\." rmdir /s /q "%Work%"
exit /b %Status%
:verify
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /i /x /c:"%~3" >nul
exit /b %ERRORLEVEL%
