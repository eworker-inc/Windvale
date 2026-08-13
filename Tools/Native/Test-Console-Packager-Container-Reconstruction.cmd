@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "OrdinaryCandidate=%RepositoryRoot%\Artifacts\Native-Console-Packager-Candidate"
set "SegmentedCandidate=%RepositoryRoot%\Artifacts\Native-Console-Segmented-Packager-Candidate"
set /a Tests=0
set /a Passed=0

call :check_file "%OrdinaryCandidate%\Console-Packager.wvb" 60797 f4c75495321736bbce22582213133e7cc09157a8439dc198d9848ec95683e89c
if errorlevel 1 goto :failed
call :check_file "%OrdinaryCandidate%\Console-Packager.exe" 708608 0dddbe6cfd38c37e3fd5332567b3323480a5548a6fbeb41b6b50aed0e57ac3d2
if errorlevel 1 goto :failed
call :check_file "%OrdinaryCandidate%\Console-Packager.elf" 708608 d399c935e906ab42d7572e337226577055396cb6204766106e21790e22ea43af
if errorlevel 1 goto :failed
call :pass "ordinary candidate inventory"

call :check_file "%SegmentedCandidate%\Console-Segmented-Packager.wvb" 70033 c4941f396f76467cb6455472f7f4711c21a6f65c12c09a9b5f4135987628f20e
if errorlevel 1 goto :failed
call :check_file "%SegmentedCandidate%\Console-Segmented-Packager.exe" 805376 954c4b2aaba56149c21e16e19ca6f16434069513e1d1b3034423dab457635412
if errorlevel 1 goto :failed
call :check_file "%SegmentedCandidate%\Console-Segmented-Packager.elf" 806912 8916fb509f81e29dabca7ed0202c0ad250f129e78b70b701630dbfcd55a1d30d
if errorlevel 1 goto :failed
call :pass "segmented candidate inventory"

call "%RepositoryRoot%\Tools\Native\Construct-Console-Packager-Reconstruction.cmd" >nul 2>nul
if not "%ERRORLEVEL%"=="64" goto :failed

:allocate
set "TestDirectory=%TEMP%\windvale-console-packager-container-reconstruction-test-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TestDirectory%" goto :allocate
mkdir "%TestDirectory%" || goto :failed
call "%RepositoryRoot%\Tools\Native\Construct-Console-Packager-Reconstruction.cmd" "%TestDirectory%" ^
    >"%TestDirectory%\Construct.out" 2>"%TestDirectory%\Construct.err"
if errorlevel 1 goto :failed
>"%TestDirectory%\Expected.out" echo native console packager reconstruction status=Complete families=2 artifacts=6
fc /b "%TestDirectory%\Construct.out" "%TestDirectory%\Expected.out" >nul
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Construct.err") do if not "%%~zF"=="0" goto :failed

set "OrdinaryOutput=%TestDirectory%\Native-Console-Packager-Candidate"
call :check_equal "%OrdinaryOutput%\Console-Packager.wvb" "%OrdinaryCandidate%\Console-Packager.wvb"
if errorlevel 1 goto :failed
call :check_equal "%OrdinaryOutput%\Console-Packager.exe" "%OrdinaryCandidate%\Console-Packager.exe"
if errorlevel 1 goto :failed
call :check_equal "%OrdinaryOutput%\Console-Packager.elf" "%OrdinaryCandidate%\Console-Packager.elf"
if errorlevel 1 goto :failed
call :pass "ordinary container reconstruction"

set "SegmentedOutput=%TestDirectory%\Native-Console-Segmented-Packager-Candidate"
call :check_equal "%SegmentedOutput%\Console-Segmented-Packager.wvb" "%SegmentedCandidate%\Console-Segmented-Packager.wvb"
if errorlevel 1 goto :failed
call :check_equal "%SegmentedOutput%\Console-Segmented-Packager.exe" "%SegmentedCandidate%\Console-Segmented-Packager.exe"
if errorlevel 1 goto :failed
call :check_equal "%SegmentedOutput%\Console-Segmented-Packager.elf" "%SegmentedCandidate%\Console-Segmented-Packager.elf"
if errorlevel 1 goto :failed
call :pass "segmented container reconstruction"

call :cleanup
echo Tests: %Tests%, Passed: %Passed%, Failed: 0
exit /b 0

:check_file
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~3" >nul
exit /b %ERRORLEVEL%

:check_equal
if not exist "%~1" exit /b 1
if not exist "%~2" exit /b 1
fc /b "%~1" "%~2" >nul
exit /b %ERRORLEVEL%

:pass
set /a Tests+=1
set /a Passed+=1
echo PASS  %~1
exit /b 0

:cleanup
if not defined TestDirectory exit /b 0
for %%R in ("%TestDirectory%") do set "ResolvedTestDirectory=%%~fR"
echo(%ResolvedTestDirectory%| findstr /b /i /c:"%TEMP%\windvale-console-packager-container-reconstruction-test-" >nul || exit /b 1
if exist "%ResolvedTestDirectory%\." rmdir /s /q "%ResolvedTestDirectory%"
exit /b 0

:failed
call :cleanup >nul 2>nul
set /a Tests+=1
set /a Failed=Tests-Passed
echo FAIL  console packager container reconstruction
echo Tests: %Tests%, Passed: %Passed%, Failed: %Failed%
exit /b 1
