@echo off
setlocal EnableExtensions DisableDelayedExpansion
if not "%~1"=="" exit /b 64
set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "Work=%TEMP%\windvale-os-endpoint-transfer-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Status=1"
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Operating-System\Windvale-Os-Endpoint-Transfer-Profile.wvproj" "%Work%\Profile.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Profile.wvb" 9657 ef7801e909dd24105e6260cb8f88845e1b8d966fb90dc78350f63eeb8d1bf619
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-Endpoint-Transfer-Profile.wvproj" "%Work%\Test.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.wvb" 19678 5bf6f45a3090056931b63d17edf0a14db8ac05169f5422b3a75a3c6703d82846
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\Test.wvb" "%Work%\Test.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.wvo" 281515 dfd6bd9711e3d51c69965a662994281b80d4d4326a9c70e6004ca2c809f5cb43
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\Test.bin" "%Work%\Test.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.bin" 280932 4ce17841274e3e94a2b3c0878119d715faae51d08b84e24e779e158a0a5e2594
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\Test.bin" 0 "%Work%\Test.exe" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.exe" 282624 359a5e4a9eeb2fc26bd15f73bc91a1c7947b5c6c06da25d2e323ddfcb914d026
if errorlevel 1 goto :cleanup
"%Work%\Test.exe" >nul
if not "%ERRORLEVEL%"=="47" goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\Test.bin" 0 "%Work%\Test.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.elf" 286832 7f4b47b73a04d7b24f323dbc00147393d26607c06968e59a57420c727a43de58
if errorlevel 1 goto :cleanup
echo native os endpoint transfer profiles status=Passed projects=2 cases=29 profiles=3 max-pages=17 local-result=47 cross-host-images=Verified
set "Status=0"
:cleanup
if exist "%Work%\." rmdir /s /q "%Work%"
exit /b %Status%
:verify
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /i /x /c:"%~3" >nul
exit /b %ERRORLEVEL%
