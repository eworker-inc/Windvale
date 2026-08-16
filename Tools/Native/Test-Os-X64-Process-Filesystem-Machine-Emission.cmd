@echo off
setlocal EnableExtensions DisableDelayedExpansion
if not "%~1"=="" exit /b 64
set "RepositoryRoot=%~dp0\..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Native=%RepositoryRoot%\Tools\Native"
:allocate
set "Work=%TEMP%\windvale-os-x64-filesystem-machine-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Status=1"

echo step=filesystem-record item=1/3
call :case Record Windvale-Native-Test-Os-X64-Process-Filesystem-Record-Emission.wvproj 16323 3f1c122df05e8c3d6a963846b8d97a4dbbe6ff692a205d8a6b4d19c2ceccf329 236160 5b69225d659ddef67acd49634d04acb2dfbeb9814627e098c57737e39d466c0c 234416 df8ee4b1bddf4ea6fdc552868a2dcfac35c600e785e400e84029a12ea7dbd172 50 236544 98f573b13a8ac2f4301078a1fd92a95348341e2711b30a002764044aef4826e3 241776 25c5c09a0aed29175c8745c09944831269266b3a8be9e74b91d3c50afb907604
if errorlevel 1 goto :cleanup
echo step=filesystem-paging item=2/3
call :case Paging Windvale-Native-Test-Os-X64-Process-Filesystem-Paging-Emission.wvproj 14615 1e626b1775f34af1356a10287c23b04523f39cd9971e57f60bf1105c3ec6aeae 206117 ec0fde1786a994e0bcb7bad1be7b49e1206ef7e0b136b7e8a9bbc077a57b8027 204075 6f38a21ebbba64e6837dad02d71d911156b456a2d59b9327a62dabdddad92a82 51 205824 9fad8f8be8bfabaabd0d800ba3df4a8533ed7dc7df62a804d19d04a6a2e0db85 209008 aebe8ae480c0e57ff1014030152d987091d463f047b8cdcc3c1a7ad83b887cd1
if errorlevel 1 goto :cleanup
echo step=filesystem-image item=3/3
call :case Image Windvale-Native-Test-Os-X64-Process-Filesystem-Image-Emission.wvproj 12520 c2630d4100b2e3a8447f850ac0be9ffb21160431de199908584ed4b08c49a743 172259 d8f4b3b4567ac919293d97c78ee1833a9042bb57ead53ea97eb373b2accc137b 170855 a4cfd98ae0ce7450ac5f2f9ec0a192d3b22f9502cff6843afbf87ed2b069fe42 52 172544 8b1d4296461f2e553ba1b4ed2f42bdfdcd4706cb4f88de74bfaa408b37b2d384 176240 621ed9294cafae354b4b20099cb8bc0ef1f5fc1a5f1593b9efade98ce2aceee4
if errorlevel 1 goto :cleanup

echo native os x64 filesystem machine emission status=Passed cases=3 geometry=85/81/48/33
set "Status=0"
goto :cleanup

:case
set "Name=%~1"
set "Project=%RepositoryRoot%\Projects\Tests\%~2"
set "WvbBytes=%~3"
set "WvbDigest=%~4"
set "WvoBytes=%~5"
set "WvoDigest=%~6"
set "BinBytes=%~7"
set "BinDigest=%~8"
shift
shift
shift
shift
shift
shift
shift
shift
set "ExpectedResult=%~1"
set "WindowsBytes=%~2"
set "WindowsDigest=%~3"
set "LinuxBytes=%~4"
set "LinuxDigest=%~5"
call "%Native%\Build-Wvb.cmd" "%Project%" "%Work%\%Name%.wvb" >nul || exit /b 1
call :verify "%Work%\%Name%.wvb" %WvbBytes% %WvbDigest% || exit /b 1
call "%Native%\Lower-Wvb-To-Wvo.cmd" "%Work%\%Name%.wvb" "%Work%\%Name%.wvo" >nul || exit /b 1
call :verify "%Work%\%Name%.wvo" %WvoBytes% %WvoDigest% || exit /b 1
call "%Native%\Link-Wvo.cmd" 0 Main "%Work%\%Name%.bin" "%Work%\%Name%.wvo" >nul || exit /b 1
call :verify "%Work%\%Name%.bin" %BinBytes% %BinDigest% || exit /b 1
call "%Native%\Package-Console.cmd" windows-x64-console-v1 "%Work%\%Name%.bin" 0 "%Work%\%Name%.exe" >nul || exit /b 1
"%Work%\%Name%.exe" >nul
if not "%ERRORLEVEL%"=="%ExpectedResult%" exit /b 1
call :verify "%Work%\%Name%.exe" %WindowsBytes% %WindowsDigest% || exit /b 1
call "%Native%\Package-Console.cmd" linux-x64-console-v1 "%Work%\%Name%.bin" 0 "%Work%\%Name%.elf" >nul || exit /b 1
call :verify "%Work%\%Name%.elf" %LinuxBytes% %LinuxDigest% || exit /b 1
exit /b 0

:verify
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /i /x /c:"%~3" >nul
exit /b %ERRORLEVEL%

:cleanup
rmdir /s /q "%Work%" >nul 2>nul
exit /b %Status%
