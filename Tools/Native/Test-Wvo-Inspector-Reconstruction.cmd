@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Candidate=%RepositoryRoot%\Artifacts\Native-Wvo-Object-Candidate"
set /a Tests=0
set /a Passed=0

call :check_file "%Candidate%\Wvo-Object.wvb" 73322 40f7b7efcff5b6e5bbc3c878cf5f0147ee92af208d43d54ab8a04f87ec1e9070
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Wvo-Object.wvo" 1022822 bab6b73e5edd6b0b2726380ba2ff10859fbbcc37481572457b508bbd0d67c2ae
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Wvo-Object.exe" 1037312 5362372e826958470eee7d90eb01938de5b91dcb3e1b0f952722e00578a82d03
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Wvo-Object.elf" 1036288 fcfd134222b05482a6ac432fc4acbfb72f3dfce92c3c646fc17595ddb078b840
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

"%Candidate%\Wvo-Object.exe" check "%Candidate%\Wvo-Object.wvo" >"%TestDirectory%\Check.out" 2>"%TestDirectory%\Check.err"
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Check.out") do if not "%%~zF"=="0" goto :failed
for %%F in ("%TestDirectory%\Check.err") do if not "%%~zF"=="0" goto :failed

"%Candidate%\Wvo-Object.exe" verify "%Candidate%\Wvo-Object.wvo" >"%TestDirectory%\Verify.out" 2>"%TestDirectory%\Verify.err"
if errorlevel 1 goto :failed
>"%TestDirectory%\Verify.hex" echo 56 65 72 69 66 69 65 64 20 6f 62 6a 65 63 74 3a 20 58 38 36 cb 89 36 34 0a 53 48 41 2d 32 35 36 3a 20 62 61 62 36 62 37 33 65 35 65 64 64 36 62 30 62 32 37 32 36 33 38 30 62 61 32 66 66 31 30 38 35 39 66 62 62 63 63 33 37 34 38 31 35 37 32 34 35 37 62 35 30 38 62 62 64 30 64 36 37 63 32 61 65 0a
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
call :check_file "%Candidate%\Wvo-Object.wvo" 1022822 bab6b73e5edd6b0b2726380ba2ff10859fbbcc37481572457b508bbd0d67c2ae
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
