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

call :check_file "%Candidate%\Wvb-Runner.wvb" 445196 4cdfb53bcd6fe49c7931ec8a0fed0f74aac3f4e10a465f0395c458af4d0a5d67
if errorlevel 1 goto :inventory_failed
call :check_file "%Candidate%\windows-x64-wvrun.exe" 5327872 7a8b97c68c3463af858b47178978f30507af947d7cf0e86e5ec71829702157c0
if errorlevel 1 goto :inventory_failed
call :check_file "%Candidate%\linux-x64-wvrun.elf" 5328896 3741a659a5bb3375fa2b0560679a19b746a03596ed8ca0c559e0f6c870f10f27
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
findstr /x /c:"native WVB runner reconstruction status=Complete artifacts=3" "%TestDirectory%\Construct.out" >nul
if errorlevel 1 goto :reconstruction_failed
call :check_equal "%TestDirectory%\Rebuilt\Wvb-Runner.wvb" "%Candidate%\Wvb-Runner.wvb"
if errorlevel 1 goto :reconstruction_failed
call :check_equal "%TestDirectory%\Rebuilt\windows-x64-wvrun.exe" "%Candidate%\windows-x64-wvrun.exe"
if errorlevel 1 goto :reconstruction_failed
call :check_equal "%TestDirectory%\Rebuilt\linux-x64-wvrun.elf" "%Candidate%\linux-x64-wvrun.elf"
if errorlevel 1 goto :reconstruction_failed
echo PASS exact source-built paired reconstruction
set /a Passed+=1
goto :reconstruction_done
:reconstruction_failed
if exist "%TestDirectory%\Construct.out" type "%TestDirectory%\Construct.out"
if exist "%TestDirectory%\Construct.err" type "%TestDirectory%\Construct.err" >&2
echo FAIL exact source-built paired reconstruction
set /a Failed+=1
:reconstruction_done

set "RuntimeFailure=fixture identity"
call :check_file "%Fixture%" 174 7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31
if errorlevel 1 goto :runtime_failed
set "RuntimeFailure=ordinary execution status"
"%TestDirectory%\Rebuilt\windows-x64-wvrun.exe" "%Fixture%" >"%TestDirectory%\Run.out" 2>"%TestDirectory%\Run.err"
if errorlevel 1 goto :runtime_failed
set "RuntimeFailure=ordinary execution output"
call :check_file "%TestDirectory%\Run.out" 11 bf24325cd27b27403c7b8053820193dcce360f640f7f394742b660ce5fe3cd4e
if errorlevel 1 goto :runtime_failed
set "RuntimeFailure=ordinary execution diagnostics"
call :check_file "%TestDirectory%\Run.err" 0 e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855
if errorlevel 1 goto :runtime_failed
set "RuntimeFailure=reported execution status"
"%TestDirectory%\Rebuilt\windows-x64-wvrun.exe" "%Fixture%" --report-steps >"%TestDirectory%\Report.out" 2>"%TestDirectory%\Report.err"
if errorlevel 1 goto :runtime_failed
set "RuntimeFailure=reported execution output"
call :check_file "%TestDirectory%\Report.out" 27 16d83153e975eefdac7828db275b4cbd3cdd4a783ed5430c442ed4717936a3e5
if errorlevel 1 goto :runtime_failed
set "RuntimeFailure=reported execution diagnostics"
call :check_file "%TestDirectory%\Report.err" 0 e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855
if errorlevel 1 goto :runtime_failed
set "RuntimeFailure=unknown option status"
"%TestDirectory%\Rebuilt\windows-x64-wvrun.exe" "%Fixture%" --unknown >"%TestDirectory%\Option.out" 2>"%TestDirectory%\Option.err"
if not "%ERRORLEVEL%"=="64" goto :runtime_failed
set "RuntimeFailure=unknown option output"
call :check_file "%TestDirectory%\Option.out" 0 e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855
if errorlevel 1 goto :runtime_failed
set "RuntimeFailure=unknown option diagnostics"
call :check_file "%TestDirectory%\Option.err" 43 fd8455c7428eece156befe036c10c6927efee163a7315dad72c730f6e2bcef64
if errorlevel 1 goto :runtime_failed
set "RuntimeFailure=invalid fixture copy"
copy /y "%InvalidFixture%" "%TestDirectory%\Invalid.wvb" >nul || goto :runtime_failed
set "RuntimeFailure=invalid fixture identity before execution"
call :check_file "%TestDirectory%\Invalid.wvb" 479 0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5
if errorlevel 1 goto :runtime_failed
set "RuntimeFailure=invalid fixture rejection status"
"%TestDirectory%\Rebuilt\windows-x64-wvrun.exe" "%TestDirectory%\Invalid.wvb" >"%TestDirectory%\Reject.out" 2>"%TestDirectory%\Reject.err"
if not "%ERRORLEVEL%"=="1" goto :runtime_failed
set "RuntimeFailure=invalid fixture rejection output"
call :check_file "%TestDirectory%\Reject.out" 0 e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855
if errorlevel 1 goto :runtime_failed
set "RuntimeFailure=invalid fixture rejection diagnostics"
call :check_file "%TestDirectory%\Reject.err" 68 a88ea127be32ffbde27b0944be4e8c232155bec2cbd8ba3ae0449d7d20dfac0a
if errorlevel 1 goto :runtime_failed
set "RuntimeFailure=invalid fixture identity after execution"
call :check_file "%TestDirectory%\Invalid.wvb" 479 0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5
if errorlevel 1 goto :runtime_failed
echo PASS current-host execution reporting and rejection
set /a Passed+=1
goto :runtime_done
:runtime_failed
echo diagnostic reason=%RuntimeFailure%
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
