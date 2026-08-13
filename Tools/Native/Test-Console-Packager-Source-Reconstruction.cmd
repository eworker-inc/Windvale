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
set "LowerOutput=%TemporaryDirectory%\Lower.out"
set "LowerError=%TemporaryDirectory%\Lower.err"
set /a Total=0
set /a Passed=0

call :run_case "ordinary-packager-source" "Projects/Linker/Windvale-Console-Application-Packager.wvproj" "60797" "f4c75495321736bbce22582213133e7cc09157a8439dc198d9848ec95683e89c" "341a870f592b06d7be116af995efae06bed3ba7e7c90ef19bc344ef8799730e5" "692425" "2a73e1a03d71cbec54de085cce2901580310105a1cb01e78563242242893186e"
if errorlevel 1 goto :failed
call :run_case "segmented-packager-source" "Projects/Linker/Windvale-Console-Application-Segmented-Packager.wvproj" "70033" "c4941f396f76467cb6455472f7f4711c21a6f65c12c09a9b5f4135987628f20e" "50488906e6b0bc9ae14da8194170ba5412bd441435e423d7e51392c45d12bbd4" "789653" "cd0d79b92ee1b80242732f4d7419a08e71c5c5e132e462c5ae4b39953c56ede9"
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
set "CandidateObject=%TemporaryDirectory%\%~1.wvo"
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
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Candidate%" "%CandidateObject%" > "%LowerOutput%" 2> "%LowerError%"
if not "%ERRORLEVEL%"=="0" (
    >&2 echo FAIL  %Case%: native lowering exit differs
    type "%LowerError%" >&2
    exit /b 1
)
for %%S in ("%LowerError%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  %Case%: native lowering wrote a diagnostic
    type "%LowerError%" >&2
    exit /b 1
)
for %%S in ("%CandidateObject%") do if not "%%~zS"=="%~6" (
    >&2 echo FAIL  %Case%: reconstructed WVO size differs
    exit /b 1
)
call :check_hash "%CandidateObject%" "%~7" "%Case% reconstructed WVO identity differs"
if errorlevel 1 exit /b 1
set /a Passed+=1
echo PASS  %Case%
del /f /q "%Candidate%" "%CandidateObject%" "%BuildOutput%" "%BuildError%" "%LowerOutput%" "%LowerError%" >nul 2>nul
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
for %%F in (ordinary-packager-source.wvb ordinary-packager-source.wvo segmented-packager-source.wvb segmented-packager-source.wvo Build.out Build.err Lower.out Lower.err) do if exist "%TemporaryDirectory%\%%F" del /f /q "%TemporaryDirectory%\%%F" >nul 2>nul
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0
