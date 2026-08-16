@echo off
setlocal EnableExtensions DisableDelayedExpansion
if not "%~1"=="" exit /b 64
set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "Work=%TEMP%\windvale-os-fat32-file-read-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Status=1"
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Operating-System\Windvale-Os-Fat32-Chain-Position.wvproj" "%Work%\Chain.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Chain.wvb" 7186 82eb95c9259e5ee851272c7698f5b2cbea69a9ef585398079346d0bdb7326393
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Operating-System\Windvale-Os-Fat32-File-Read-Transaction.wvproj" "%Work%\Transaction.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Transaction.wvb" 73587 ed6219dee7ef97ff3bef1fc62bb6fac81c67cf7d51a4613f74c27584aa5da005
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-Fat32-File-Read-Transaction.wvproj" "%Work%\Test.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.wvb" 85489 8471b8c6bafe850b07fdd501999a26921f61fe0a8e80e2e9e9294fcbf0276753
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\Test.wvb" "%Work%\Test.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.wvo" 2079830 e21ea358dee10eca74bf8c74beff449e38101b6a670220083a8691bd5a80b2d6
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\Test.bin" "%Work%\Test.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.bin" 2076252 88f38a00c6d8685b986184624e1285770ec2dc48dc74098776a68ab9fac434be
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\Test.bin" 0 "%Work%\Test.exe" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.exe" 2078208 cb77b19db1d2dd43383e40e9577abaf4994a07bd5a8b35bcc5426eea49d87f25
if errorlevel 1 goto :cleanup
"%Work%\Test.exe" >nul
if not "%ERRORLEVEL%"=="47" goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\Test.bin" 0 "%Work%\Test.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.elf" 2080880 897af8142136c74800a537b812598e17f95b166312107af0811c96c78e8f99f2
if errorlevel 1 goto :cleanup
echo native os fat32 file read transaction status=Passed projects=3 cases=18 exchanges=2 local-result=47 cross-host-images=Verified
set "Status=0"
:cleanup
if exist "%Work%\." rmdir /s /q "%Work%"
exit /b %Status%
:verify
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /i /x /c:"%~3" >nul
exit /b %ERRORLEVEL%
