@echo off
setlocal EnableExtensions DisableDelayedExpansion
if not "%~1"=="" exit /b 64
set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "Work=%TEMP%\windvale-os-filesystem-service-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Status=1"
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Os-Filesystem-Service.wvproj" "%Work%\Test.wvb" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.wvb" 33871 e2b9279e18676c1a6e3ede3a92d6dee21305c70b14e2f37826ad70b4f2637133
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Work%\Test.wvb" "%Work%\Test.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.wvo" 360745 8850cb504be473f7aef51fc07598c070cf6e82b2b445a702f1948efd492c28de
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Work%\Test.bin" "%Work%\Test.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.bin" 359187 da1c1d9d2e9048e35da9ba7661ee9f086dd1e566aa7ec41f0a79559063af76dd
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Work%\Test.bin" 0 "%Work%\Test.exe" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.exe" 360960 74aa3bde234216a0aa787585ac88ab1a748cca8bc181693412d67dfe3e92860c
if errorlevel 1 goto :cleanup
"%Work%\Test.exe" >nul
if not "%ERRORLEVEL%"=="43" goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 "%Work%\Test.bin" 0 "%Work%\Test.elf" >nul
if errorlevel 1 goto :cleanup
call :verify "%Work%\Test.elf" 364656 fca9aa51babcfd33b6ab051d565b16089c99f37a8e577e68f862bdcbb13548c4
if errorlevel 1 goto :cleanup
echo native os filesystem service status=Passed cases=19 local-result=43 cross-host-images=Verified
set "Status=0"
:cleanup
if exist "%Work%\." rmdir /s /q "%Work%"
exit /b %Status%
:verify
if not exist "%~1" (
    >&2 echo FAIL native os filesystem service artifact=%~nx1 check=exists
    exit /b 1
)
set "VerifyStatus=0"
for %%F in ("%~1") do if not "%%~zF"=="%~2" (
    >&2 echo FAIL native os filesystem service artifact=%~nx1 check=bytes expected=%~2 actual=%%~zF
    set "VerifyStatus=1"
)
certutil -hashfile "%~1" SHA256 | findstr /i /x /c:"%~3" >nul
if errorlevel 1 (
    >&2 echo FAIL native os filesystem service artifact=%~nx1 check=sha256 expected=%~3
    >&2 certutil -hashfile "%~1" SHA256
    set "VerifyStatus=1"
)
exit /b %VerifyStatus%
