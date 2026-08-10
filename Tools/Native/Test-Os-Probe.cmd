@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Builder=%RepositoryRoot%\Tools\Native\Build-Os-Probe.cmd"

:allocate
set "TemporaryDirectory=%TEMP%\windvale-native-os-probe-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" >nul 2>&1
if errorlevel 1 exit /b 1
set "Normal=%TemporaryDirectory%\Normal.efi"
set "InvalidOpcode=%TemporaryDirectory%\Invalid-Opcode.efi"
set "GeneralProtection=%TemporaryDirectory%\General-Protection.efi"
set "Status=1"

call :build normal "%Normal%" 080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9
if errorlevel 1 goto :failure

call "%Builder%" "%Normal%" normal >"%TemporaryDirectory%\Repeat.out" 2>"%TemporaryDirectory%\Repeat.err"
if not errorlevel 1 goto :failure
findstr /x /c:"The native Probe 40 output already exists." "%TemporaryDirectory%\Repeat.err" >nul
if errorlevel 1 goto :failure
call :verify "%Normal%" 080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9
if errorlevel 1 goto :failure
dir /b /a "%TemporaryDirectory%\.windvale-os-probe-native-*" >nul 2>&1
if not errorlevel 1 goto :failure

call :build invalid-opcode "%InvalidOpcode%" 8af8a705da7a63e895e39a94a1ff60dae52bfa1ad0b9c0984adeafe538bae734
if errorlevel 1 goto :failure
call :build general-protection "%GeneralProtection%" 47f5ae37b48edb0212c6d439237e43ee2ca8064061786010f9644acf70f7ad4b
if errorlevel 1 goto :failure

echo Tests: 4, Passed: 4, Failed: 0
set "Status=0"
goto :cleanup

:build
set "CaseScenario=%~1"
set "CaseOutput=%~2"
set "CaseDigest=%~3"
set "CaseStandardOutput=%TemporaryDirectory%\%CaseScenario%.out"
set "CaseStandardError=%TemporaryDirectory%\%CaseScenario%.err"
if "%CaseScenario%"=="normal" (
    call "%Builder%" "%CaseOutput%" >"%CaseStandardOutput%" 2>"%CaseStandardError%"
) else (
    call "%Builder%" "%CaseOutput%" "%CaseScenario%" >"%CaseStandardOutput%" 2>"%CaseStandardError%"
)
if errorlevel 1 exit /b 1
for %%F in ("%CaseStandardError%") do if not "%%~zF"=="0" exit /b 1
findstr /x /c:"windvale-os-probe-native-build 40" "%CaseStandardOutput%" >nul
if errorlevel 1 exit /b 1
findstr /x /c:"scenario=%CaseScenario%" "%CaseStandardOutput%" >nul
if errorlevel 1 exit /b 1
findstr /x /c:"efi-bytes=683008" "%CaseStandardOutput%" >nul
if errorlevel 1 exit /b 1
findstr /x /c:"efi-sha256=%CaseDigest%" "%CaseStandardOutput%" >nul
if errorlevel 1 exit /b 1
call :verify "%CaseOutput%" %CaseDigest%
exit /b %ERRORLEVEL%

:verify
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="683008" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /i /x /c:"%~2" >nul
exit /b %ERRORLEVEL%

:failure
>&2 echo The native Probe 40 focused test failed.
for %%F in ("%TemporaryDirectory%\*.out" "%TemporaryDirectory%\*.err") do if exist "%%~F" type "%%~F" 1>&2

:cleanup
if exist "%TemporaryDirectory%" rmdir /s /q "%TemporaryDirectory%"
exit /b %Status%
