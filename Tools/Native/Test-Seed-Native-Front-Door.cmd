@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "ArtifactRoot=%RepositoryRoot%\Artifacts\Native-Front-Door"
set "Manifest=%ArtifactRoot%\Manifest.json"
set "Inventory=%ArtifactRoot%\SHA256SUMS"
set "Verifier=%ArtifactRoot%\windows-x64\wvverify.exe"

call :verify_file "%Manifest%" 6170 b5a27bbd6d143c73907f2908bd1022f30de474c5d922473086213dad9ab5319a "front-door manifest"
if errorlevel 1 exit /b 1
call :verify_file "%Inventory%" 1605 cce8e76c7e16f9697241356490f18a87116b996a16f71284b161d6f0897996ad "front-door checksum inventory"
if errorlevel 1 exit /b 1

:allocate
set "TemporaryDirectory=%TEMP%\windvale-seed-front-door-smoke-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "VerifyOutput=%TemporaryDirectory%\Verify.out"
set "VerifyError=%TemporaryDirectory%\Verify.err"
set /a ArtifactCount=0
set /a ModuleCount=0

for /f "usebackq tokens=1,2" %%H in ("%Inventory%") do (
    call :verify_inventory_entry "%%H" "%%I"
    if errorlevel 1 goto :failed
)
if not "%ArtifactCount%"=="18" (
    >&2 echo The native front-door artifact count differs: %ArtifactCount%
    goto :failed
)
if not "%ModuleCount%"=="6" (
    >&2 echo The native front-door WVB admission count differs: %ModuleCount%
    goto :failed
)

call :cleanup
echo Tests: 1, Passed: 1, Failed: 0
exit /b 0

:verify_inventory_entry
set /a ArtifactCount+=1
set "ArtifactPath=%ArtifactRoot%\%~2"
call :verify_file "%ArtifactPath%" 0 %~1 "front-door artifact %~2"
if errorlevel 1 exit /b 1
if /I not "%~x2"==".wvb" exit /b 0
set /a ModuleCount+=1
"%Verifier%" "%ArtifactPath%" > "%VerifyOutput%" 2> "%VerifyError%"
if errorlevel 1 (
    >&2 echo The current-host verifier rejected the native front-door module: %~2
    if exist "%VerifyError%" type "%VerifyError%" >&2
    exit /b 1
)
for %%F in ("%VerifyError%") do if not "%%~zF"=="0" (
    >&2 echo The current-host verifier diagnosed a native front-door module: %~2
    type "%VerifyError%" >&2
    exit /b 1
)
set "ActualReport="
set /a ReportLines=0
for /f "usebackq delims=" %%L in ("%VerifyOutput%") do call :capture_report "%%L"
if not "%ReportLines%"=="1" (
    >&2 echo The native front-door admission report line count differs: %~2
    exit /b 1
)
if not "%ActualReport%"=="wvb status=Valid profile=compiler-aligned" (
    >&2 echo The native front-door admission report differs: %~2
    type "%VerifyOutput%" >&2
    exit /b 1
)
exit /b 0

:capture_report
set /a ReportLines+=1
set "ActualReport=%~1"
exit /b 0

:verify_file
if not exist "%~1" (
    >&2 echo The %~4 is missing: %~1
    exit /b 1
)
if not "%~2"=="0" for %%F in ("%~1") do if not "%%~zF"=="%~2" (
    >&2 echo The %~4 size differs: %~1
    exit /b 1
)
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~3" >nul
if errorlevel 1 (
    >&2 echo The %~4 digest differs: %~1
    exit /b 1
)
exit /b 0

:failed
call :cleanup
exit /b 1

:cleanup
if exist "%VerifyOutput%" del /f /q "%VerifyOutput%" >nul 2>nul
if exist "%VerifyError%" del /f /q "%VerifyError%" >nul 2>nul
if exist "%TemporaryDirectory%" rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0
