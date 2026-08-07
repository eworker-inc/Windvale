@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Console-Packager-Source-Reconstruction.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "TemporaryDirectory=%TEMP%\windvale-console-packager-source-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "BuildOutput=%TemporaryDirectory%\Build.out"
set "BuildError=%TemporaryDirectory%\Build.err"
set /a Total=0
set /a Passed=0

call :run_case "ordinary-packager-source" "Windvale-Console-Application-Packager.wvproj" "58127" "7b055d4e6a456680a79eb28eaafa577e0019ea0ff1e34d9e713e9178428acc29" "de75af11831f8d681042df015a13c33e243f613b9738c5a7177747d63538b892"
if errorlevel 1 goto :failed
call :run_case "segmented-packager-source" "Windvale-Console-Application-Segmented-Packager.wvproj" "68451" "33d7619c6115295a9eb612fd559031ab99c85196e3133a9405f880a19ac9ded2" "003dea772fb69bbfc4a485dd6a024e9c0e451726745675e38afab4292f75f61b"
if errorlevel 1 goto :failed

if not "%Total%"=="2" goto :count_failed
call :cleanup
echo Tests: %Total%, Passed: %Passed%, Failed: 0
exit /b 0

:run_case
set /a Total+=1
set "Case=%~1"
set "Project=%RepositoryRoot%\%~2"
set "Candidate=%TemporaryDirectory%\%~1.wvb"
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%Project%" "%Candidate%" > "%BuildOutput%" 2> "%BuildError%"
if not "%ERRORLEVEL%"=="0" (
    >&2 echo FAIL  %Case%: native build exit differs
    type "%BuildError%" >&2
    exit /b 1
)
for %%S in ("%BuildError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  %Case%: native build wrote a diagnostic
    type "%BuildError%" >&2
    exit /b 1
)
for %%S in ("%Candidate%") do if not "%%~zS"=="%~3" (
    >&2 echo FAIL  %Case%: reconstructed WVB size differs
    exit /b 1
)
call :check_hash "%Candidate%" "%~4" "%Case% reconstructed WVB identity differs"
if errorlevel 1 exit /b 1
call :check_hash "%BuildOutput%" "%~5" "%Case% build report differs"
if errorlevel 1 exit /b 1
set /a Passed+=1
echo PASS  %Case%
del /f /q "%Candidate%" "%BuildOutput%" "%BuildError%" >nul 2>nul
exit /b 0

:check_hash
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~2" >nul
if errorlevel 1 (
    >&2 echo FAIL  console-packager-source-reconstruction: %~3
    >&2 echo Expected SHA-256: %~2
    certutil -hashfile "%~1" SHA256 >&2
    exit /b 1
)
exit /b 0

:count_failed
>&2 echo FAIL  console-packager-source-reconstruction: total case count differs
:failed
set /a Failed=Total-Passed
call :cleanup
>&2 echo Tests: %Total%, Passed: %Passed%, Failed: %Failed%
exit /b 1

:cleanup
for %%F in (ordinary-packager-source.wvb segmented-packager-source.wvb Build.out Build.err) do if exist "%TemporaryDirectory%\%%F" del /f /q "%TemporaryDirectory%\%%F" >nul 2>nul
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0
