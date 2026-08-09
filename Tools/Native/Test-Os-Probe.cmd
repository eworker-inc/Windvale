@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"

:allocate
set "TemporaryDirectory=%TEMP%\windvale-native-os-probe-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" >nul 2>&1
if errorlevel 1 exit /b 1
set "Output=%TemporaryDirectory%\Probe40.efi"
set "StandardOutput=%TemporaryDirectory%\Build.out"
set "StandardError=%TemporaryDirectory%\Build.err"
set "RepeatOutput=%TemporaryDirectory%\Repeat.out"
set "RepeatError=%TemporaryDirectory%\Repeat.err"
set "Status=1"

call "%RepositoryRoot%\Tools\Native\Build-Os-Probe.cmd" "%Output%" >"%StandardOutput%" 2>"%StandardError%"
if errorlevel 1 goto :failure
for %%F in ("%StandardError%") do if not "%%~zF"=="0" goto :failure
findstr /x /c:"windvale-os-probe-native-build 40" "%StandardOutput%" >nul
if errorlevel 1 goto :failure
findstr /x /c:"scenario=normal" "%StandardOutput%" >nul
if errorlevel 1 goto :failure
call :verify
if errorlevel 1 goto :failure

call "%RepositoryRoot%\Tools\Native\Build-Os-Probe.cmd" "%Output%" >"%RepeatOutput%" 2>"%RepeatError%"
if not errorlevel 1 goto :failure
findstr /x /c:"The native Probe 40 output already exists." "%RepeatError%" >nul
if errorlevel 1 goto :failure
call :verify
if errorlevel 1 goto :failure
dir /b /a "%TemporaryDirectory%\.windvale-os-probe-native-*" >nul 2>&1
if not errorlevel 1 goto :failure

echo Tests: 2, Passed: 2, Failed: 0
set "Status=0"
goto :cleanup

:verify
if not exist "%Output%" exit /b 1
for %%F in ("%Output%") do if not "%%~zF"=="683008" exit /b 1
certutil -hashfile "%Output%" SHA256 | findstr /i /c:"080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9" >nul
exit /b %ERRORLEVEL%

:failure
>&2 echo The native Probe 40 focused test failed.
if exist "%StandardOutput%" type "%StandardOutput%" 1>&2
if exist "%StandardError%" type "%StandardError%" 1>&2
if exist "%RepeatOutput%" type "%RepeatOutput%" 1>&2
if exist "%RepeatError%" type "%RepeatError%" 1>&2

:cleanup
if exist "%TemporaryDirectory%" rmdir /s /q "%TemporaryDirectory%"
exit /b %Status%
