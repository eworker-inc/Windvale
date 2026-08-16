@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Echo-Application.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Native=%RepositoryRoot%\Tools\Native"

:allocate
set "Work=%TEMP%\windvale-echo-application-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Result=1"

echo START native Windvale echo phase=compile item=1/4
call "%Native%\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Applications\Windvale-Echo.wvproj" "%Work%\Echo-A.wvb" >nul || goto :cleanup
call "%Native%\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Applications\Windvale-Echo.wvproj" "%Work%\Echo-B.wvb" >nul || goto :cleanup
fc /b "%Work%\Echo-A.wvb" "%Work%\Echo-B.wvb" >nul || goto :cleanup
call :verify_file "%Work%\Echo-A.wvb" 813 5d827b98be518a07a8dea60d79e70073535f78f07cf875d750021fa795c13c64 || goto :cleanup
echo PASS  native Windvale echo phase=compile item=1/4

echo START native Windvale echo phase=inspect item=2/4
call "%Native%\Inspect-Wvb.cmd" "%Work%\Echo-A.wvb" >"%Work%\Inspect.txt" || goto :cleanup
echo PASS  native Windvale echo phase=inspect item=2/4

echo START native Windvale echo phase=package item=3/4
call "%Native%\Package-Hosted-Wvb.cmd" 6 "%Work%\Echo-A.wvb" "%Work%\Echo.exe" windows >nul || goto :cleanup
call "%Native%\Package-Hosted-Wvb.cmd" 6 "%Work%\Echo-A.wvb" "%Work%\Echo.elf" linux >nul || goto :cleanup
echo PASS  native Windvale echo phase=package item=3/4

echo START native Windvale echo phase=execute item=4/4 cases=9
node "%Native%\Verify-Echo-Application.mjs" windows "%Work%\Echo-A.wvb" "%Work%\Echo.exe" "%Work%\Echo.elf" "%Work%\Inspect.txt" >nul || goto :cleanup
echo PASS  native Windvale echo phase=execute item=4/4 cases=9
set "Result=0"

:cleanup
for %%R in ("%Work%") do set "ResolvedWork=%%~fR"
echo(%ResolvedWork%| findstr /b /i /c:"%TEMP%\windvale-echo-application-" >nul || exit /b 1
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
if not "%Result%"=="0" exit /b %Result%
echo native Windvale echo status=Passed cases=9 capabilities=3 wvb=5d827b98be518a07a8dea60d79e70073535f78f07cf875d750021fa795c13c64 cross-host-applications=Verified
exit /b 0

:verify_file
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~3" >nul
exit /b %ERRORLEVEL%
