@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Candidate=%RepositoryRoot%\Artifacts\Native-Wvb-Runner-Candidate"
set "Constructor=%RepositoryRoot%\Tools\Native\Construct-Wvb-Runner-Reconstruction.cmd"
set "Fixture=%RepositoryRoot%\Artifacts\Native-Wvb-To-Wvo-Candidate\Return-42.wvb"
set "InvalidFixture=%RepositoryRoot%\Artifacts\Native-Wvb-To-Wvo-Candidate\Return-42.wvo"
set "Passed=0"
set "Failed=0"

call :check_file "%Candidate%\Wvb-Runner.wvb" 183537 1926cf33e359c56c8b457cbd96c685ffee052feb9f1330053c43d77e18f38d3e
if errorlevel 1 goto :inventory_failed
call :check_file "%Candidate%\Wvb-Runner.wvo" 1808213 dfcfb2360d496a5ab873539b4d6dbcdfe3824e8593dfe3e007cc71cd9bc55480
if errorlevel 1 goto :inventory_failed
call :check_file "%Candidate%\windows-x64-wvrun.exe" 1822208 7a2f245b405d01c1f0f9c7f2b9e9cbe0d88370232e8cf1843616207aa155e7bd
if errorlevel 1 goto :inventory_failed
call :check_file "%Candidate%\linux-x64-wvrun.elf" 1822720 7dac00ed67f7622af2fcd4c9ededd17afced3ad54ea309d749320249188b15b4
if errorlevel 1 goto :inventory_failed
echo PASS candidate inventory
set /a Passed+=1
goto :inventory_done
:inventory_failed
echo FAIL candidate inventory
set /a Failed+=1
:inventory_done

:allocate
set "TestDirectory=%TEMP%\windvale-wvb-runner-reconstruction-test-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TestDirectory%" goto :allocate
mkdir "%TestDirectory%" || exit /b 1
mkdir "%TestDirectory%\Rebuilt" || exit /b 1

call "%Constructor%" >"%TestDirectory%\Usage.out" 2>"%TestDirectory%\Usage.err"
if not "%ERRORLEVEL%"=="64" goto :reconstruction_failed
call :check_file "%TestDirectory%\Usage.out" 0 e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855
if errorlevel 1 goto :reconstruction_failed
call :check_file "%TestDirectory%\Usage.err" 89 1bb8bfa0ec7c4cd78ff5d1cd89f0a0481bda2b18386323d819841446b6d2b5a8
if errorlevel 1 goto :reconstruction_failed
call "%Constructor%" "%TestDirectory%\Rebuilt" >"%TestDirectory%\Construct.out" 2>"%TestDirectory%\Construct.err"
if errorlevel 1 goto :reconstruction_failed
call :check_file "%TestDirectory%\Construct.err" 0 e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855
if errorlevel 1 goto :reconstruction_failed
findstr /x /c:"native WVB runner reconstruction status=Complete artifacts=4" "%TestDirectory%\Construct.out" >nul
if errorlevel 1 goto :reconstruction_failed
call :check_equal "%TestDirectory%\Rebuilt\Wvb-Runner.wvb" "%Candidate%\Wvb-Runner.wvb"
if errorlevel 1 goto :reconstruction_failed
call :check_equal "%TestDirectory%\Rebuilt\Wvb-Runner.wvo" "%Candidate%\Wvb-Runner.wvo"
if errorlevel 1 goto :reconstruction_failed
call :check_equal "%TestDirectory%\Rebuilt\windows-x64-wvrun.exe" "%Candidate%\windows-x64-wvrun.exe"
if errorlevel 1 goto :reconstruction_failed
call :check_equal "%TestDirectory%\Rebuilt\linux-x64-wvrun.elf" "%Candidate%\linux-x64-wvrun.elf"
if errorlevel 1 goto :reconstruction_failed
echo PASS exact source-built paired reconstruction
set /a Passed+=1
goto :reconstruction_done
:reconstruction_failed
echo FAIL exact source-built paired reconstruction
set /a Failed+=1
:reconstruction_done

call :check_file "%Fixture%" 174 7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31
if errorlevel 1 goto :runtime_failed
"%TestDirectory%\Rebuilt\windows-x64-wvrun.exe" "%Fixture%" >"%TestDirectory%\Run.out" 2>"%TestDirectory%\Run.err"
if errorlevel 1 goto :runtime_failed
call :check_file "%TestDirectory%\Run.out" 11 bf24325cd27b27403c7b8053820193dcce360f640f7f394742b660ce5fe3cd4e
if errorlevel 1 goto :runtime_failed
call :check_file "%TestDirectory%\Run.err" 0 e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855
if errorlevel 1 goto :runtime_failed
"%TestDirectory%\Rebuilt\windows-x64-wvrun.exe" "%Fixture%" --report-steps >"%TestDirectory%\Report.out" 2>"%TestDirectory%\Report.err"
if errorlevel 1 goto :runtime_failed
call :check_file "%TestDirectory%\Report.out" 27 16d83153e975eefdac7828db275b4cbd3cdd4a783ed5430c442ed4717936a3e5
if errorlevel 1 goto :runtime_failed
call :check_file "%TestDirectory%\Report.err" 0 e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855
if errorlevel 1 goto :runtime_failed
"%TestDirectory%\Rebuilt\windows-x64-wvrun.exe" "%Fixture%" --unknown >"%TestDirectory%\Option.out" 2>"%TestDirectory%\Option.err"
if not "%ERRORLEVEL%"=="64" goto :runtime_failed
call :check_file "%TestDirectory%\Option.out" 0 e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855
if errorlevel 1 goto :runtime_failed
call :check_file "%TestDirectory%\Option.err" 43 fd8455c7428eece156befe036c10c6927efee163a7315dad72c730f6e2bcef64
if errorlevel 1 goto :runtime_failed
copy /y "%InvalidFixture%" "%TestDirectory%\Invalid.wvb" >nul || goto :runtime_failed
call :check_file "%TestDirectory%\Invalid.wvb" 479 0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5
if errorlevel 1 goto :runtime_failed
"%TestDirectory%\Rebuilt\windows-x64-wvrun.exe" "%TestDirectory%\Invalid.wvb" >"%TestDirectory%\Reject.out" 2>"%TestDirectory%\Reject.err"
if not "%ERRORLEVEL%"=="1" goto :runtime_failed
call :check_file "%TestDirectory%\Reject.out" 0 e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855
if errorlevel 1 goto :runtime_failed
call :check_file "%TestDirectory%\Reject.err" 53 a2e698719194d86fe8d449d741af6b00bad06930727af6b513d23da909f1d28e
if errorlevel 1 goto :runtime_failed
call :check_file "%TestDirectory%\Invalid.wvb" 479 0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5
if errorlevel 1 goto :runtime_failed
echo PASS current-host execution reporting and rejection
set /a Passed+=1
goto :runtime_done
:runtime_failed
echo FAIL current-host execution reporting and rejection
set /a Failed+=1
:runtime_done

rmdir /s /q "%TestDirectory%"
set /a Total=Passed+Failed
echo Tests: %Total%, Passed: %Passed%, Failed: %Failed%
if not "%Failed%"=="0" exit /b 1
exit /b 0

:check_file
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
if "%~2"=="0" exit /b 0
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~3" >nul
exit /b %ERRORLEVEL%

:check_equal
fc /b "%~1" "%~2" >nul
exit /b %ERRORLEVEL%
