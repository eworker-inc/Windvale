@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Candidate=%RepositoryRoot%\Artifacts\Native-Wvo-Object-Candidate"
set /a Tests=0
set /a Passed=0

call :check_file "%Candidate%\Wvo-Object.wvb" 61008 a630d49f0549c865644d8052fbff7e8bf2b6a6dcd013e1187d4356d49cd188db
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Wvo-Object.wvo" 591723 f45b14c33a7615209a2a16f6caf0bee041bdb5e2f46fd868792222e774fdb30c
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Wvo-Object.exe" 606720 8c6f30b0b55898776d8dc394ea763313527650a361ceb6f478ffad48979084f1
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Wvo-Object.elf" 606208 f94d2e16da76c949e15978bd879bff38205685be08d7afa1670f48d3f6592ea1
if errorlevel 1 goto :failed
call :pass "candidate inventory"

call "%RepositoryRoot%\Tools\Native\Construct-Wvo-Inspector-Reconstruction.cmd" >nul 2>nul
if not "%ERRORLEVEL%"=="64" goto :failed

:allocate
set "TestDirectory=%TEMP%\windvale-wvo-inspector-reconstruction-test-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TestDirectory%" goto :allocate
mkdir "%TestDirectory%" || goto :failed

call "%RepositoryRoot%\Tools\Native\Construct-Wvo-Inspector-Reconstruction.cmd" "%TestDirectory%" >"%TestDirectory%\Construct.out" 2>"%TestDirectory%\Construct.err"
if errorlevel 1 goto :failed
>"%TestDirectory%\Construct.expected" echo native WVO inspector reconstruction status=Complete artifacts=4
fc /b "%TestDirectory%\Construct.out" "%TestDirectory%\Construct.expected" >nul
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Construct.err") do if not "%%~zF"=="0" goto :failed
call :check_equal "%TestDirectory%\Wvo-Object.wvb" "%Candidate%\Wvo-Object.wvb"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Wvo-Object.wvo" "%Candidate%\Wvo-Object.wvo"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Wvo-Object.exe" "%Candidate%\Wvo-Object.exe"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Wvo-Object.elf" "%Candidate%\Wvo-Object.elf"
if errorlevel 1 goto :failed
call :pass "exact paired reconstruction"

"%Candidate%\Wvo-Object.exe" >"%TestDirectory%\Self-Test.out" 2>"%TestDirectory%\Self-Test.err"
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Self-Test.out") do if not "%%~zF"=="0" goto :failed
for %%F in ("%TestDirectory%\Self-Test.err") do if not "%%~zF"=="0" goto :failed

"%Candidate%\Wvo-Object.exe" verify "%Candidate%\Wvo-Object.wvo" >"%TestDirectory%\Verify.out" 2>"%TestDirectory%\Verify.err"
if errorlevel 1 goto :failed
>"%TestDirectory%\Verify.hex" echo 56 65 72 69 66 69 65 64 20 6f 62 6a 65 63 74 3a 20 58 38 36 cb 89 36 34 0a 53 48 41 2d 32 35 36 3a 20 66 34 35 62 31 34 63 33 33 61 37 36 31 35 32 30 39 61 32 61 31 36 66 36 63 61 66 30 62 65 65 30 34 31 62 64 62 35 65 32 66 34 36 66 64 38 36 38 37 39 32 32 32 32 65 37 37 34 66 64 62 33 30 63 0a
certutil -f -decodehex "%TestDirectory%\Verify.hex" "%TestDirectory%\Verify.expected" 4 >nul
if errorlevel 1 goto :failed
fc /b "%TestDirectory%\Verify.out" "%TestDirectory%\Verify.expected" >nul
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Verify.err") do if not "%%~zF"=="0" goto :failed

call "%RepositoryRoot%\Tools\Native\Admit-Hosted-Verifier-Publisher.cmd" windows "%Candidate%\Wvo-Object.exe" >"%TestDirectory%\Isolation.out" 2>"%TestDirectory%\Isolation.err"
if not "%ERRORLEVEL%"=="2" goto :failed
for %%F in ("%TestDirectory%\Isolation.out") do if not "%%~zF"=="0" goto :failed
call :check_file "%TestDirectory%\Isolation.err" 61 ffadaf98e0978439eb19a97ccfe2d4c06f810b8c9926d5193eb4827f3c126b89
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Wvo-Object.wvo" 591723 f45b14c33a7615209a2a16f6caf0bee041bdb5e2f46fd868792222e774fdb30c
if errorlevel 1 goto :failed
call :pass "current-host compatibility and profile isolation"

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
echo(%ResolvedTestDirectory%| findstr /b /i /c:"%TEMP%\windvale-wvo-inspector-reconstruction-test-" >nul || exit /b 1
if exist "%ResolvedTestDirectory%\." rmdir /s /q "%ResolvedTestDirectory%"
exit /b 0

:failed
call :cleanup >nul 2>nul
set /a Tests+=1
set /a Failed=Tests-Passed
echo FAIL  WVO inspector reconstruction
echo Tests: %Tests%, Passed: %Passed%, Failed: %Failed%
exit /b 1
