@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Candidate=%RepositoryRoot%\Artifacts\Native-Wvb-To-Wvo-Candidate"
set /a Tests=0
set /a Passed=0

call :check_file "%Candidate%\Wvb-To-Wvo.wvb" 414298 2d2c5cc91a13603d71bcd72786ae89e3e4afbaf19148fc6170a66df33c33ebef
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Wvb-To-Wvo.exe" 5972480 be6bfb487f00dcc9f8c785dfe05832263d5899bf2f6fca3f77edf163b51deac7
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Wvb-To-Wvo.elf" 5971968 09cb247bb427b3b40305d068ec798b1086c2255c3db823036d47e9f620091dd2
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
