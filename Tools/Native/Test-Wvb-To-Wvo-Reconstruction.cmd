@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Candidate=%RepositoryRoot%\Artifacts\Native-Wvb-To-Wvo-Candidate"
set /a Tests=0
set /a Passed=0

call :check_file "%Candidate%\Wvb-To-Wvo.wvb" 435970 5f6240f7202a56269661ca3281578340695404de5718fee88e47e1f5bb864ff5
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Wvb-To-Wvo.exe" 6206976 0d58124ba389f93f352cf32f0eedc35773b2235ac0af8f5fee886a17001a9080
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Wvb-To-Wvo.elf" 6205440 8debddbbeefc325ba15aae700c77921bb077460067222b9100618d1ec6bec76f
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Return-42.wvb" 174 7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Return-42.wvo" 479 0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5
if errorlevel 1 goto :failed
call :pass "candidate inventory"

call "%RepositoryRoot%\Tools\Native\Construct-Wvb-To-Wvo-Reconstruction.cmd" >nul 2>nul
if not "%ERRORLEVEL%"=="64" goto :failed

:allocate
set "TestDirectory=%TEMP%\windvale-wvb-to-wvo-reconstruction-test-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TestDirectory%" goto :allocate
mkdir "%TestDirectory%" || goto :failed
call "%RepositoryRoot%\Tools\Native\Construct-Wvb-To-Wvo-Reconstruction.cmd" "%TestDirectory%" ^
    >"%TestDirectory%\Construct.out" 2>"%TestDirectory%\Construct.err"
if errorlevel 1 goto :failed
>"%TestDirectory%\Expected.out" echo native WVB-to-WVO reconstruction status=Complete artifacts=5
fc /b "%TestDirectory%\Construct.out" "%TestDirectory%\Expected.out" >nul
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Construct.err") do if not "%%~zF"=="0" goto :failed

call :check_equal "%TestDirectory%\Wvb-To-Wvo.wvb" "%Candidate%\Wvb-To-Wvo.wvb"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Wvb-To-Wvo.exe" "%Candidate%\Wvb-To-Wvo.exe"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Wvb-To-Wvo.elf" "%Candidate%\Wvb-To-Wvo.elf"
if errorlevel 1 goto :failed
call :pass "native paired lowerer reconstruction"

call :check_equal "%TestDirectory%\Return-42.wvb" "%Candidate%\Return-42.wvb"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Return-42.wvo" "%Candidate%\Return-42.wvo"
if errorlevel 1 goto :failed
call :pass "current-host Return-42 lowering"

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
echo(%ResolvedTestDirectory%| findstr /b /i /c:"%TEMP%\windvale-wvb-to-wvo-reconstruction-test-" >nul || exit /b 1
if exist "%ResolvedTestDirectory%\." rmdir /s /q "%ResolvedTestDirectory%"
exit /b 0

:failed
call :cleanup >nul 2>nul
set /a Tests+=1
set /a Failed=Tests-Passed
echo FAIL  WVB-to-WVO reconstruction
echo Tests: %Tests%, Passed: %Passed%, Failed: %Failed%
exit /b 1
