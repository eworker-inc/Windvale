@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Language-1.0-Memory-Budget-Accounting.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Native=%RepositoryRoot%\Tools\Native"
set "Project=%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Language-1-Memory-Budget-Accounting.wvproj"

:allocate
set "Work=%TEMP%\windvale-memory-budget-accounting-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Result=1"
set "FailureStep=build-a"

echo START language 1 memory budget accounting phase=build item=1/4
call "%Native%\Build-Wvb.cmd" "%Project%" "%Work%\Accounting-A.wvb" ^
    >"%Work%\Build-A.out" 2>"%Work%\Build-A.err" || goto :cleanup
for %%F in ("%Work%\Build-A.err") do if not "%%~zF"=="0" goto :cleanup

set "FailureStep=build-b"
echo START language 1 memory budget accounting phase=build item=2/4
call "%Native%\Build-Wvb.cmd" "%Project%" "%Work%\Accounting-B.wvb" ^
    >"%Work%\Build-B.out" 2>"%Work%\Build-B.err" || goto :cleanup
for %%F in ("%Work%\Build-B.err") do if not "%%~zF"=="0" goto :cleanup
fc /b "%Work%\Accounting-A.wvb" "%Work%\Accounting-B.wvb" >nul || goto :cleanup
for %%F in ("%Work%\Accounting-A.wvb") do set "WvbBytes=%%~zF"
if not "%WvbBytes%"=="35799" goto :cleanup

set "FailureStep=package"
echo START language 1 memory budget accounting phase=package item=3/4
call "%Native%\Package-Hosted-Wvb.cmd" 1 "%Work%\Accounting-A.wvb" ^
    "%Work%\Accounting.exe" windows ^
    >"%Work%\Package.out" 2>"%Work%\Package.err" || goto :cleanup
for %%F in ("%Work%\Package.err") do if not "%%~zF"=="0" goto :cleanup

set "FailureStep=execute"
echo START language 1 memory budget accounting phase=execute item=4/4
"%Work%\Accounting.exe" >"%Work%\Run.out" 2>"%Work%\Run.err"
set "ExecutionResult=%ERRORLEVEL%"
if not "%ExecutionResult%"=="42" goto :cleanup
for %%F in ("%Work%\Run.out" "%Work%\Run.err") do if not "%%~zF"=="0" goto :cleanup
set "Result=0"

:cleanup
if not "%Result%"=="0" (
    >&2 echo FAIL  language 1 memory budget accounting step=%FailureStep%
    if exist "%Work%\Build-A.err" type "%Work%\Build-A.err" >&2
    if exist "%Work%\Build-B.err" type "%Work%\Build-B.err" >&2
    if exist "%Work%\Package.err" type "%Work%\Package.err" >&2
    if exist "%Work%\Run.err" type "%Work%\Run.err" >&2
)
if exist "%Work%\." rmdir /s /q "%Work%"
if not "%Result%"=="0" exit /b %Result%
echo native language 1 memory budget accounting status=Passed cases=29 result=42 state-bytes=2616 capacity=65 lease-token-bytes=28 wvb-bytes=%WvbBytes%
exit /b 0
