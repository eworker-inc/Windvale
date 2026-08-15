@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Candidate=%RepositoryRoot%\Artifacts\Native-Wvb-To-Wvo-Candidate"
set /a Tests=0
set /a Passed=0

call :check_file "%Candidate%\Wvb-To-Wvo.wvb" 520966 ce190159783b48912ff71326d937a72a27b5178b07b7e52de71742a53cd12b56
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Wvb-To-Wvo.exe" 7483904 496d9e00c682af316b69c0e7639858fa0da171b4d467c4838b1ed774ac18b4cc
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Wvb-To-Wvo.elf" 7483392 30ffb3ce953b173d1bbee77c8e440e901806a676f7ec17683b5cfe3953ebb441
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Return-42.wvb" 174 7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Return-42.wvo" 479 0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Metadata.wvb" 369 94b41f5016722c9e5bf16ace5ec933acc35c14efdd4e08fe11fd582a62b58ffa
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Metadata.wvo" 1151 6f1cb53ec55448a7552f2ff5b380446964d16ed32a60aa28b8e55a9ca590845d
if errorlevel 1 goto :failed
call :pass "candidate inventory"

call "%RepositoryRoot%\Tools\Native\Construct-Wvb-To-Wvo-Reconstruction.cmd" >nul 2>nul
if not "%ERRORLEVEL%"=="64" goto :failed

:allocate
set "TestDirectory=%TEMP%\windvale-wvb-to-wvo-reconstruction-test-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TestDirectory%" goto :allocate
mkdir "%TestDirectory%" || goto :failed
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Wvb-Metadata-Normalization-Self-Test.wvproj" ^
    "%TestDirectory%\Metadata-Normalization.wvb" >nul 2>"%TestDirectory%\Metadata-Normalization-Build.err"
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Metadata-Normalization-Build.err") do if not "%%~zF"=="0" goto :failed
call "%RepositoryRoot%\Tools\Native\Run-Wvb.cmd" ^
    "%TestDirectory%\Metadata-Normalization.wvb" ^
    >"%TestDirectory%\Metadata-Normalization.out" 2>"%TestDirectory%\Metadata-Normalization.err"
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Metadata-Normalization.out") do if not "%%~zF"=="10" goto :failed
findstr /c:"Result: 0" "%TestDirectory%\Metadata-Normalization.out" >nul
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Metadata-Normalization.err") do if not "%%~zF"=="0" goto :failed
call :pass "portable metadata normalization"

call "%RepositoryRoot%\Tools\Native\Construct-Wvb-To-Wvo-Reconstruction.cmd" "%TestDirectory%" ^
    >"%TestDirectory%\Construct.out" 2>"%TestDirectory%\Construct.err"
if errorlevel 1 goto :failed
>"%TestDirectory%\Expected.out" echo native WVB-to-WVO reconstruction status=Complete artifacts=7
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

call :check_equal "%TestDirectory%\Metadata.wvb" "%Candidate%\Metadata.wvb"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Metadata.wvo" "%Candidate%\Metadata.wvo"
if errorlevel 1 goto :failed
call :pass "current-host independent-metadata lowering"

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
