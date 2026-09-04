@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Candidate=%RepositoryRoot%\Artifacts\Native-Wvb-To-Wvo-Candidate"
set /a Tests=0
set /a Passed=0

call :check_file "%Candidate%\Wvb-To-Wvo.wvb" 747242 7cc1867200d747c3b694f7bd35b3f9128dbb7bcc8223ebd46ead234a22680a3f
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Wvb-To-Wvo.exe" 10656768 0a0894901341d71ef09712fb63ed0a9f7ac2b93c64b357d123dd09674045cfda
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Wvb-To-Wvo.elf" 10657792 4f7aa0abdf870ada362defee6258ba4e6b8ce1f0f67329563d20ed3eb6c9ff24
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
set "Phase=metadata-normalizer-build"
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

set "Phase=data-limit-build"
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-X64-Lowering-Data-Limit.wvproj" ^
    "%TestDirectory%\Data-Limit.wvb" >nul 2>"%TestDirectory%\Data-Limit-Build.err"
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Data-Limit-Build.err") do if not "%%~zF"=="0" goto :failed
set "Phase=data-limit-lowering"
"%Candidate%\Wvb-To-Wvo.exe" ^
    "%TestDirectory%\Data-Limit.wvb" "%TestDirectory%\Data-Limit.wvo" ^
    >"%TestDirectory%\Data-Limit-Lower.out" 2>"%TestDirectory%\Data-Limit-Lower.err"
if errorlevel 1 goto :failed
set "Phase=data-limit-object-check"
call "%RepositoryRoot%\Tools\Native\Check-Wvo.cmd" ^
    "%TestDirectory%\Data-Limit.wvo" >nul
if errorlevel 1 goto :failed
set "Phase=data-limit-link"
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main ^
    "%TestDirectory%\Data-Limit.bin" "%TestDirectory%\Data-Limit.wvo" ^
    >"%TestDirectory%\Data-Limit-Link.out"
if errorlevel 1 goto :failed
set "Phase=data-limit-entry"
set "DataLimitEntry="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Main address=" "%TestDirectory%\Data-Limit-Link.out"') do set "DataLimitEntry=%%E"
if not defined DataLimitEntry goto :failed
echo(%DataLimitEntry%| findstr /r /x "[0-9][0-9]*" >nul
if errorlevel 1 goto :failed
copy /b "%TestDirectory%\Data-Limit.bin" "%TestDirectory%\Data-Limit-Image.chunk-0" >nul
if errorlevel 1 goto :failed
set "Phase=data-limit-windows-package"
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
    "%TestDirectory%\Data-Limit.wvb" "%TestDirectory%\Data-Limit-Image" ^
    1 %DataLimitEntry% "%TestDirectory%\Data-Limit.exe" windows >nul
if errorlevel 1 goto :failed
set "Phase=data-limit-windows-execution"
"%TestDirectory%\Data-Limit.exe" >nul
if not "%ERRORLEVEL%"=="42" goto :failed
set "Phase=data-limit-linux-package"
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
    "%TestDirectory%\Data-Limit.wvb" "%TestDirectory%\Data-Limit-Image" ^
    1 %DataLimitEntry% "%TestDirectory%\Data-Limit.elf" linux >nul
if errorlevel 1 goto :failed
call :pass "native 512/513 data and 256/257 type boundaries"

set "Phase=metadata-verifier-build"
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tools\Windvale-Compiler-Wvb-Verifier.wvproj" ^
    "%TestDirectory%\Metadata-Verifier.wvb" >nul 2>"%TestDirectory%\Metadata-Verifier-Build.err"
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Metadata-Verifier-Build.err") do if not "%%~zF"=="0" goto :failed
set "Phase=metadata-verifier-package"
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" 2 ^
    "%TestDirectory%\Metadata-Verifier.wvb" ^
    "%TestDirectory%\Metadata-Verifier.exe" windows ^
    >"%TestDirectory%\Metadata-Verifier-Package.out" 2>"%TestDirectory%\Metadata-Verifier-Package.err"
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Metadata-Verifier-Package.err") do if not "%%~zF"=="0" goto :failed
set "Phase=metadata-verifier-execution"
"%TestDirectory%\Metadata-Verifier.exe" "%Candidate%\Metadata.wvb" ^
    >"%TestDirectory%\Metadata-Verifier.out" 2>"%TestDirectory%\Metadata-Verifier.err"
if errorlevel 1 goto :failed
set "Phase=metadata-verifier-report"
for %%F in ("%TestDirectory%\Metadata-Verifier.out") do if not "%%~zF"=="42" goto :failed
findstr /c:"wvb status=Valid profile=compiler-aligned" "%TestDirectory%\Metadata-Verifier.out" >nul
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Metadata-Verifier.err") do if not "%%~zF"=="0" goto :failed
call :pass "compiler-aligned metadata verification"

set "Phase=lowerer-reconstruction"
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
if defined TestDirectory if exist "%TestDirectory%\Metadata-Verifier.out" (
    for %%F in ("%TestDirectory%\Metadata-Verifier.out") do echo DETAIL metadata-verifier-report bytes=%%~zF
    type "%TestDirectory%\Metadata-Verifier.out"
)
if defined TestDirectory if exist "%TestDirectory%\Construct.out" (
    for %%F in ("%TestDirectory%\Construct.out") do echo DETAIL lowerer-constructor-output bytes=%%~zF
    type "%TestDirectory%\Construct.out"
)
if defined TestDirectory if exist "%TestDirectory%\Construct.err" (
    for %%F in ("%TestDirectory%\Construct.err") do echo DETAIL lowerer-constructor-diagnostic bytes=%%~zF
    type "%TestDirectory%\Construct.err"
)
call :cleanup >nul 2>nul
set /a Tests+=1
set /a Failed=Tests-Passed
echo FAIL  WVB-to-WVO reconstruction phase=%Phase%
echo Tests: %Tests%, Passed: %Passed%, Failed: %Failed%
exit /b 1
