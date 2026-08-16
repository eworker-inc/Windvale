@echo off
setlocal EnableExtensions DisableDelayedExpansion
if not "%~1"=="" exit /b 64
set "Root=%~dp0..\.."
for %%R in ("%Root%") do set "Root=%%~fR"
set "Native=%Root%\Tools\Native"
:allocate
set "Work=%TEMP%\windvale-os-provider-images-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Status=1"
call "%Native%\Build-Wvb.cmd" "%Root%\Projects\Operating-System\Windvale-Os-Filesystem-Process-Service.wvproj" "%Work%\Filesystem.wvb" >nul || goto :cleanup
call :verify "%Work%\Filesystem.wvb" 14812 054dc2c9b5c33e02e6263b644049fd84f1ed2e1219d642ec64c066af5bdc8fcf || goto :cleanup
call "%Native%\Lower-Wvb-To-Wvo.cmd" "%Work%\Filesystem.wvb" "%Work%\Filesystem-Main.wvo" >nul || goto :cleanup
call :verify "%Work%\Filesystem-Main.wvo" 196327 5ee235d5dca7bfdab8a5a1b7c54874b6545725e69da754e22e06f72f578ebdb3 || goto :cleanup
call "%Native%\Link-Wvo.cmd" 0 Main "%Work%\Filesystem-Readiness.bin" "%Work%\Filesystem-Main.wvo" >nul || goto :cleanup
call "%Native%\Package-Console.cmd" windows-x64-console-v1 "%Work%\Filesystem-Readiness.bin" 0 "%Work%\Filesystem-Readiness.exe" >nul || goto :cleanup
"%Work%\Filesystem-Readiness.exe" >nul
if not "%ERRORLEVEL%"=="46" goto :cleanup
call "%Native%\Rename-Wvo-Export.cmd" "%Work%\Filesystem-Main.wvo" Main Windvale_filesystem_process_service_main "%Work%\Filesystem.wvo" >nul || goto :cleanup
call "%Native%\Assemble-Wva.cmd" "%Root%\Operating-System\Kernel\Filesystem-Process-Service-Shim.wva" "%Work%\Filesystem-Shim.wvo" >nul || goto :cleanup
call :verify "%Work%\Filesystem-Shim.wvo" 302 dc212ce43b59102a05521531e6df4674291851c72a1be8990eff049ea46879dd || goto :cleanup
call "%Native%\Link-Wvo.cmd" 0 Windvale_filesystem_process_user_entry "%Work%\Filesystem.bin" "%Work%\Filesystem-Shim.wvo" "%Work%\Filesystem.wvo" >nul || goto :cleanup
call :verify "%Work%\Filesystem.bin" 195657 453cef870da3f375400d1c58cc8ebd385f761c2eafbdf3b3fb70603db8520dab || goto :cleanup
call "%Native%\Build-Wvb.cmd" "%Root%\Projects\Operating-System\Windvale-Os-Network-Process-Service.wvproj" "%Work%\Network.wvb" >nul || goto :cleanup
call :verify "%Work%\Network.wvb" 13543 32c595716af0a3706226d677924a5279ea2d7b97b0a4cbdf7c6c9eed808e1b2a || goto :cleanup
call "%Native%\Lower-Wvb-To-Wvo.cmd" "%Work%\Network.wvb" "%Work%\Network-Main.wvo" >nul || goto :cleanup
call :verify "%Work%\Network-Main.wvo" 243124 892cfe18b81667c9e4d3e82a1889a9b1f77c45e350d2e75144694db3c2f49ca0 || goto :cleanup
call "%Native%\Link-Wvo.cmd" 0 Main "%Work%\Network-Readiness.bin" "%Work%\Network-Main.wvo" >nul || goto :cleanup
call "%Native%\Package-Console.cmd" windows-x64-console-v1 "%Work%\Network-Readiness.bin" 0 "%Work%\Network-Readiness.exe" >nul || goto :cleanup
"%Work%\Network-Readiness.exe" >nul
if not "%ERRORLEVEL%"=="47" goto :cleanup
call "%Native%\Rename-Wvo-Export.cmd" "%Work%\Network-Main.wvo" Main Windvale_network_process_service_main "%Work%\Network.wvo" >nul || goto :cleanup
call "%Native%\Assemble-Wva.cmd" "%Root%\Operating-System\Kernel\Network-Process-Service-Shim.wva" "%Work%\Network-Shim.wvo" >nul || goto :cleanup
call :verify "%Work%\Network-Shim.wvo" 296 628852893fcbc32e610261517a79c3acd56714ce0c197beab1c0a3917dedf726 || goto :cleanup
call "%Native%\Link-Wvo.cmd" 0 Windvale_network_process_user_entry "%Work%\Network.bin" "%Work%\Network-Shim.wvo" "%Work%\Network.wvo" >nul || goto :cleanup
call :verify "%Work%\Network.bin" 242571 57067da10da68fc1d35b41784e147d8f60ed1e05441cb68bc803ad5a9682f6d1 || goto :cleanup
echo native os provider images status=Passed services=2 readiness=2 cases=8
set "Status=0"
goto :cleanup
:verify
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /i /x /c:"%~3" >nul
exit /b %ERRORLEVEL%
:cleanup
if exist "%Work%" rmdir /s /q "%Work%"
exit /b %Status%
