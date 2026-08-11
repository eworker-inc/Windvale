@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Console-Verifier-Reconstruction.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Candidate=%RepositoryRoot%\Artifacts\Native-Console-Application-Verifier-Candidate"
set /a Tests=0
set /a Passed=0

call :check_file "%Candidate%\Console-Application-Verifier.wvb" 105006 1dcd5f2aeebd974649e64c90d9f473e1e75f7d13dbcde2814de1dded72cf2c0c
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Console-Application-Verifier.wvo" 1049519 51292e4d300d4a6bb6ce4879915bba5304de70c9deafdf4eb6ff6a54a6dbf150
if errorlevel 1 goto :failed
call :check_file "%Candidate%\windows-x64-wvappverify.exe" 1063936 05b5f5b3e3999a0ef3537f0908967069a12f17de09753fc90e8a4c7542dc9d3f
if errorlevel 1 goto :failed
call :check_file "%Candidate%\linux-x64-wvappverify.elf" 1064960 c2700e5e68711d7b8e8a8f7e9573d87dfa27c3676a034a314310ef59045e5f1a
if errorlevel 1 goto :failed
call :pass "candidate inventory"

:allocate
set "TestDirectory=%TEMP%\windvale-console-verifier-reconstruction-test-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TestDirectory%" goto :allocate
mkdir "%TestDirectory%" || goto :failed
set "UsageOut=%TestDirectory%\Usage.out"
set "UsageErr=%TestDirectory%\Usage.err"
set "UsageExpected=%TestDirectory%\Usage.expected"
call "%RepositoryRoot%\Tools\Native\Construct-Console-Verifier-Reconstruction.cmd" >"%UsageOut%" 2>"%UsageErr%"
if not "%ERRORLEVEL%"=="64" goto :failed
for %%F in ("%UsageOut%") do if not "%%~zF"=="0" goto :failed
>"%UsageExpected%" echo Usage: Tools\Native\Construct-Console-Verifier-Reconstruction.cmd ^<existing-separate-output-directory^>
fc /b "%UsageErr%" "%UsageExpected%" >nul
if errorlevel 1 goto :failed
del /q "%UsageOut%" "%UsageErr%" "%UsageExpected%" >nul 2>nul
set "UsageOut="
set "UsageErr="
set "UsageExpected="
set "EmptySnapshot=%TestDirectory%\Empty.bin"
type nul >"%EmptySnapshot%"
set "Probe=%TestDirectory%\Aot-Probe"
mkdir "%Probe%" || goto :failed
call "%RepositoryRoot%\Tools\Native\Construct-Aot-Composition-Probe.cmd" "%Probe%" ^
    >"%TestDirectory%\Probe.out" 2>"%TestDirectory%\Probe.err"
if errorlevel 1 goto :failed
>"%TestDirectory%\Probe.expected" echo native AOT composition probe status=Complete artifacts=6
fc /b "%TestDirectory%\Probe.out" "%TestDirectory%\Probe.expected" >nul
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Probe.err") do if not "%%~zF"=="0" goto :failed
set "Fixture=%Probe%\Return-42.exe"

call "%RepositoryRoot%\Tools\Native\Construct-Console-Verifier-Reconstruction.cmd" "%TestDirectory%" >"%TestDirectory%\Construct.out" 2>"%TestDirectory%\Construct.err"
if errorlevel 1 goto :failed
>"%TestDirectory%\Construct.expected" echo native console verifier reconstruction status=Complete artifacts=4
fc /b "%TestDirectory%\Construct.out" "%TestDirectory%\Construct.expected" >nul
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Construct.err") do if not "%%~zF"=="0" goto :failed
call :check_equal "%TestDirectory%\Console-Application-Verifier.wvb" "%Candidate%\Console-Application-Verifier.wvb"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Console-Application-Verifier.wvo" "%Candidate%\Console-Application-Verifier.wvo"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\windows-x64-wvappverify.exe" "%Candidate%\windows-x64-wvappverify.exe"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\linux-x64-wvappverify.elf" "%Candidate%\linux-x64-wvappverify.elf"
if errorlevel 1 goto :failed
call :pass "usage and exact paired reconstruction"

set "Application=%TestDirectory%\windows-x64-wvappverify.exe"
call :check_file "%Fixture%" 2560 8f2c3389dafa40c0231a0f5aeead3db5570697d54874f324a81f84a2d5b16eb6
if errorlevel 1 goto :failed
"%Application%" "%Fixture%" "%EmptySnapshot%" >"%TestDirectory%\Compatibility.out" 2>"%TestDirectory%\Compatibility.err"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Compatibility.out" 78 fa657a34b8ff388aa8bcd81eb8ffbd70c1c61d11fe7edc1a8861511fe3510d60
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Compatibility.err") do if not "%%~zF"=="0" goto :failed

"%Application%" "%Application%" "%EmptySnapshot%" >"%TestDirectory%\Rejection.out" 2>"%TestDirectory%\Rejection.err"
if not "%ERRORLEVEL%"=="1" goto :failed
for %%F in ("%TestDirectory%\Rejection.out") do if not "%%~zF"=="0" goto :failed
call :check_file "%TestDirectory%\Rejection.err" 76 8a02b32387b434d07a3223b98bf966fd80571e5ca296d8c1100d01e9c3338105
if errorlevel 1 goto :failed
call :check_file "%Application%" 1063936 05b5f5b3e3999a0ef3537f0908967069a12f17de09753fc90e8a4c7542dc9d3f
if errorlevel 1 goto :failed
call :check_file "%Fixture%" 2560 8f2c3389dafa40c0231a0f5aeead3db5570697d54874f324a81f84a2d5b16eb6
if errorlevel 1 goto :failed
for %%F in ("%EmptySnapshot%") do if not "%%~zF"=="0" goto :failed
call :pass "current-host two-snapshot compatibility and exact hosted-container rejection"

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
if defined UsageOut del /q "%UsageOut%" >nul 2>nul
if defined UsageErr del /q "%UsageErr%" >nul 2>nul
if defined UsageExpected del /q "%UsageExpected%" >nul 2>nul
if not defined TestDirectory exit /b 0
for %%R in ("%TestDirectory%") do set "ResolvedTestDirectory=%%~fR"
echo(%ResolvedTestDirectory%| findstr /b /i /c:"%TEMP%\windvale-console-verifier-reconstruction-test-" >nul || exit /b 1
if exist "%ResolvedTestDirectory%\." rmdir /s /q "%ResolvedTestDirectory%"
exit /b 0

:failed
call :cleanup >nul 2>nul
set /a Tests+=1
set /a Failed=Tests-Passed
echo FAIL  console verifier reconstruction
echo Tests: %Tests%, Passed: %Passed%, Failed: %Failed%
exit /b 1
