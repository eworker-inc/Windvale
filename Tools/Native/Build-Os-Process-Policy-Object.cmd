@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if not "%~2"=="" goto :usage
if /I not "%~x1"==".wvo" goto :usage

set "Output=%~f1"
if exist "%Output%" (
    >&2 echo The OS process-policy output already exists.
    exit /b 1
)
for %%F in ("%Output%") do set "OutputDirectory=%%~dpF"
if not exist "%OutputDirectory%" (
    >&2 echo The OS process-policy output directory does not exist.
    exit /b 1
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "Work=%OutputDirectory%.windvale-os-process-policy-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" >nul 2>&1
if errorlevel 1 exit /b 1
set "Status=1"

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects/Operating-System/Windvale-Os-Process-Policy.wvproj" ^
    "%Work%\Process-Policy.wvb" >"%Work%\Build.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Process-Policy.wvb" 42027 22e40a95100c635a2bf8980ee6f81f5660e3ac6bf2251a2355e5c9b6106e3d55
if errorlevel 1 goto :failure

call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" ^
    "%Work%\Process-Policy.wvb" "%Work%\Process-Policy-Main.wvo" ^
    >"%Work%\Lower.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Process-Policy-Main.wvo" 699368 46844c80221180e039cfb9d45ed2493486d1b026d9712517f64025db202100a9
if errorlevel 1 goto :failure

call "%RepositoryRoot%\Tools\Native\Rename-Wvo-Export.cmd" ^
    "%Work%\Process-Policy-Main.wvo" Main Windvale_kernel_process_policy ^
    "%Work%\Process-Policy.wvo" >"%Work%\Rename.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Process-Policy.wvo" 699394 dea015f8cafac002eddb9383691e2de10cbdcd0c0a589a88d88fbef95241f5b5
if errorlevel 1 goto :failure
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%Work%\Process-Policy.wvo" >nul 2>&1
if errorlevel 1 goto :failure
call "%RepositoryRoot%\Tools\Native\Publish-Wvo.cmd" ^
    "%Work%\Process-Policy.wvo" "%Output%" >"%Work%\Publish.log" 2>&1
if errorlevel 1 goto :failure
set "Status=0"
goto :cleanup

:failure
if exist "%Work%\Build.log" type "%Work%\Build.log" 1>&2
if exist "%Work%\Lower.log" type "%Work%\Lower.log" 1>&2
if exist "%Work%\Rename.log" type "%Work%\Rename.log" 1>&2
if exist "%Work%\Publish.log" type "%Work%\Publish.log" 1>&2
>&2 echo The native OS process-policy object build failed.

:cleanup
if exist "%Work%" rmdir /s /q "%Work%"
exit /b %Status%

:verify
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /i /x /c:"%~3" >nul
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Build-Os-Process-Policy-Object.cmd ^<output.wvo^>
exit /b 64
