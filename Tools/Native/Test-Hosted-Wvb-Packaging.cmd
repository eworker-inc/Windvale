@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Hosted-Wvb-Packaging.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Toolset=%RepositoryRoot%\Artifacts\Native-Hosted-Container-Toolset-Candidate"
set "OriginalTemp=%TEMP%"
:allocate
set "TestDirectory=%OriginalTemp%\windvale-native-hosted-package-test-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TestDirectory%" goto :allocate
mkdir "%TestDirectory%" || exit /b 1
set "TEMP=%TestDirectory%"
set "Result=1"

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" 1 ^
    "%Toolset%\Wvb\wvhostcontrol.wvb" "%TestDirectory%\Valid.exe" ^
    >"%TestDirectory%\Valid.out" 2>"%TestDirectory%\Valid.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Valid.err" "valid packaging wrote a diagnostic"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Valid.exe" 236032 eeec7c229b20ac006ed366849c91e2f03e035a9e3ee29da2e9aeb408c76b2709 "valid package"
if errorlevel 1 goto :failed
fc /b "%TestDirectory%\Valid.exe" "%Toolset%\windows-x64\wvhostcontrol.exe" >nul
if errorlevel 1 (
    >&2 echo FAIL  hosted packaging: valid package differs from the candidate
    goto :failed
)
call :check_no_scratch
if errorlevel 1 goto :failed
echo PASS  hosted packaging exact Windows application

copy /y "%Toolset%\SHA256SUMS" "%TestDirectory%\Invalid.wvb" >nul || goto :failed
copy /y "%Toolset%\SHA256SUMS" "%TestDirectory%\Destination.exe" >nul || goto :failed
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" 1 ^
    "%TestDirectory%\Invalid.wvb" "%TestDirectory%\Destination.exe" ^
    >"%TestDirectory%\Invalid.out" 2>"%TestDirectory%\Invalid.err"
if not errorlevel 1 (
    >&2 echo FAIL  hosted packaging: invalid WVB was accepted
    goto :failed
)
call :check_file "%TestDirectory%\Destination.exe" 5426 9d60316098f3854cc286a03982b59cce80ced7cd7ab08e8ceef6dc6ecf58b040 "preserved destination"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Invalid.wvb" 5426 9d60316098f3854cc286a03982b59cce80ced7cd7ab08e8ceef6dc6ecf58b040 "preserved input"
if errorlevel 1 goto :failed
call :check_no_scratch
if errorlevel 1 goto :failed
echo PASS  hosted packaging rejects invalid WVB and preserves resources
set "Result=0"
goto :cleanup

:check_file
if not exist "%~1" (
    >&2 echo FAIL  hosted packaging: missing %~4
    exit /b 1
)
for %%F in ("%~1") do if not "%%~zF"=="%~2" (
    >&2 echo FAIL  hosted packaging: %~4 byte length differs
    exit /b 1
)
certutil -hashfile "%~1" SHA256 | findstr /i /c:"%~3" >nul
if errorlevel 1 (
    >&2 echo FAIL  hosted packaging: %~4 digest differs
    exit /b 1
)
exit /b 0

:check_empty
for %%F in ("%~1") do if not "%%~zF"=="0" (
    >&2 echo FAIL  hosted packaging: %~2
    type "%~1" >&2
    exit /b 1
)
exit /b 0

:check_no_scratch
for /d %%D in ("%TestDirectory%\windvale-native-hosted-package-*") do if exist "%%~fD" (
    >&2 echo FAIL  hosted packaging: private package scratch remains
    exit /b 1
)
exit /b 0

:failed
set "Result=1"

:cleanup
set "TEMP=%OriginalTemp%"
for %%F in (Valid.exe Valid.out Valid.err Invalid.wvb Destination.exe Invalid.out Invalid.err) do (
    if exist "%TestDirectory%\%%F" del /f /q "%TestDirectory%\%%F" >nul 2>nul
)
rmdir "%TestDirectory%" >nul 2>nul
if "%Result%"=="0" echo Tests: 2, Passed: 2, Failed: 0
exit /b %Result%
