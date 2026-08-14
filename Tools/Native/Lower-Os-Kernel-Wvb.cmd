@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if "%~2"=="" goto :usage
if not "%~3"=="" goto :usage
if /I not "%~x1"==".wvb" goto :usage
if /I not "%~x2"==".wvo" goto :usage

set "Input=%~f1"
set "Output=%~f2"
if not exist "%Input%" (
    >&2 echo The OS kernel target input is missing.
    exit /b 1
)
if exist "%Output%" (
    >&2 echo The OS kernel target output already exists.
    exit /b 1
)
for %%F in ("%Output%") do set "OutputDirectory=%%~dpF"
if not exist "%OutputDirectory%" (
    >&2 echo The OS kernel target output directory does not exist.
    exit /b 1
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Candidate=%RepositoryRoot%\Artifacts\Native-Os-Kernel-Target-Candidate"
set "Module=%Candidate%\Os-Kernel-Target.wvb"
set "Target=%Candidate%\windows-x64-os-kernel-target.exe"

call :verify "%Module%" 57129 9a7149ee7e0cb7533ef95baa199af24c36b5819217e634e362dd4f70e92bd3e8 "kernel-target module"
if errorlevel 1 exit /b 1
call :verify "%Target%" 613888 af00f5bdb8934b07e9cbfec6881446d9e7fdc19264c2248e96e2a5df5566c027 "Windows kernel target"
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Verify-Wvb.cmd" "%Input%" >nul 2>&1
if errorlevel 1 (
    >&2 echo The OS kernel target input is not a verified WVB module.
    exit /b 1
)

"%Target%" "%Input%" "%Output%"
if errorlevel 1 goto :failure
if not exist "%Output%" goto :failure
call "%RepositoryRoot%\Tools\Native\Check-Wvo.cmd" "%Output%" >nul 2>&1
if errorlevel 1 goto :failure
exit /b 0

:failure
if exist "%Output%" del /f /q "%Output%" >nul 2>nul
>&2 echo The OS kernel target rejected the module or produced an invalid object.
exit /b 1

:verify
if not exist "%~1" (
    >&2 echo The %~4 is missing.
    exit /b 1
)
for %%F in ("%~1") do if not "%%~zF"=="%~2" (
    >&2 echo The %~4 length is invalid.
    exit /b 1
)
certutil -hashfile "%~1" SHA256 | findstr /i /x /c:"%~3" >nul
if errorlevel 1 (
    >&2 echo The %~4 digest is invalid.
    exit /b 1
)
exit /b 0

:usage
>&2 echo Usage: Tools\Native\Lower-Os-Kernel-Wvb.cmd ^<input.wvb^> ^<output.wvo^>
exit /b 64
