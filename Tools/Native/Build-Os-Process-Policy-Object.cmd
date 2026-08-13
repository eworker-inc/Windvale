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
call :verify "%Work%\Process-Policy.wvb" 18764 c46c6b3780cad8d292607ed687a7e511e2e3c47fbc6fc21526ecc0ffeb937895
if errorlevel 1 goto :failure

call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" ^
    "%Work%\Process-Policy.wvb" "%Work%\Process-Policy-Main.wvo" ^
    >"%Work%\Lower.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Process-Policy-Main.wvo" 129284 11e1796c176dcdeb2f643108b646363751347707ca4b16b0e914b8c0b384987e
if errorlevel 1 goto :failure

call "%RepositoryRoot%\Tools\Native\Rename-Wvo-Export.cmd" ^
    "%Work%\Process-Policy-Main.wvo" Main Windvale_kernel_process_policy ^
    "%Work%\Process-Policy.wvo" >"%Work%\Rename.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Process-Policy.wvo" 129310 35d751147a7285fb926ba68e77da4ef554bcf68a58963520153f23ea3e8c4678
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
