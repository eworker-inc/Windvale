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

call :check_file "%Candidate%\Wvb-Runner.wvb" 993328 2e7f5390c95e74be2abb06c2b2cbb84d789c3d449a7577c40f9de45157a874a6
if errorlevel 1 goto :inventory_failed
call :check_file "%Candidate%\windows-x64-wvrun.exe" 10127360 c7e7a917622698a511ebb8b478c8075d943feaf987d0aae56c9b7c8cab21c5e4
if errorlevel 1 goto :inventory_failed
call :check_file "%Candidate%\linux-x64-wvrun.elf" 10129408 c5db1a90ce58f4807de13ca0082014e9ca09634a9ef487859166f15443e7149d
if errorlevel 1 goto :inventory_failed
echo PASS candidate inventory
set /a Passed+=1
goto :inventory_done
:inventory_failed
echo FAIL candidate inventory
echo Tests: 1, Passed: 0, Failed: 1
exit /b 1
:inventory_done

:allocate
set "TestDirectory=%TEMP%\windvale-wvb-runner-reconstruction-test-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TestDirectory%" goto :allocate
mkdir "%TestDirectory%" || exit /b 1
mkdir "%TestDirectory%\Rebuilt" || exit /b 1

call :check_runtime "%Candidate%\windows-x64-wvrun.exe"
if errorlevel 1 goto :preflight_failed
goto :preflight_done
:preflight_failed
echo diagnostic reason=%RuntimeFailure%
echo FAIL current-host candidate preflight
call :remove_test_directory
echo Tests: 2, Passed: 1, Failed: 1
exit /b 1
:preflight_done

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

call :check_runtime "%TestDirectory%\Rebuilt\windows-x64-wvrun.exe"
if errorlevel 1 goto :runtime_failed
echo PASS current-host execution reporting and rejection
set /a Passed+=1
goto :runtime_done
:runtime_failed
echo diagnostic reason=%RuntimeFailure%
echo FAIL current-host execution reporting and rejection
set /a Failed+=1
:runtime_done

call :remove_test_directory
if errorlevel 1 exit /b 1
set /a Total=Passed+Failed
echo Tests: %Total%, Passed: %Passed%, Failed: %Failed%
if not "%Failed%"=="0" exit /b 1
exit /b 0

:remove_test_directory
set "WINDVALE_TEST_CLEANUP_ROOT=%TestDirectory%"
pwsh -NoLogo -NoProfile -Command "$p=[IO.Path]::GetFullPath($env:WINDVALE_TEST_CLEANUP_ROOT); $t=[IO.Path]::TrimEndingDirectorySeparator([IO.Path]::GetFullPath([IO.Path]::GetTempPath())); if ([IO.Path]::GetDirectoryName($p) -ne $t -or -not [IO.Path]::GetFileName($p).StartsWith('windvale-wvb-runner-reconstruction-test-', [StringComparison]::Ordinal)) { exit 64 }; for ($i=0; $i -lt 20; $i++) { if (-not [IO.Directory]::Exists($p)) { exit 0 }; try { [IO.Directory]::Delete($p, $true) } catch {}; Start-Sleep -Milliseconds 100 }; exit 1"
set "CleanupResult=%ERRORLEVEL%"
set "WINDVALE_TEST_CLEANUP_ROOT="
if not "%CleanupResult%"=="0" >&2 echo FAIL bounded test cleanup
exit /b %CleanupResult%

:check_file
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
if "%~2"=="0" exit /b 0
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~3" >nul
exit /b %ERRORLEVEL%

:check_equal
fc /b "%~1" "%~2" >nul
exit /b %ERRORLEVEL%

:check_runtime
set "RuntimeRunner=%~1"
set "RuntimeFailure=fixture identity"
call :check_file "%Fixture%" 174 7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31
if errorlevel 1 exit /b 1
set "RuntimeFailure=ordinary execution status"
"%RuntimeRunner%" "%Fixture%" >"%TestDirectory%\Run.out" 2>"%TestDirectory%\Run.err"
if errorlevel 1 exit /b 1
set "RuntimeFailure=ordinary execution output"
call :check_file "%TestDirectory%\Run.out" 11 bf24325cd27b27403c7b8053820193dcce360f640f7f394742b660ce5fe3cd4e
if errorlevel 1 exit /b 1
set "RuntimeFailure=ordinary execution diagnostics"
call :check_file "%TestDirectory%\Run.err" 0 e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855
if errorlevel 1 exit /b 1
set "RuntimeFailure=reported execution status"
"%RuntimeRunner%" "%Fixture%" --report-steps >"%TestDirectory%\Report.out" 2>"%TestDirectory%\Report.err"
if errorlevel 1 exit /b 1
set "RuntimeFailure=reported execution output"
call :check_file "%TestDirectory%\Report.out" 27 16d83153e975eefdac7828db275b4cbd3cdd4a783ed5430c442ed4717936a3e5
if errorlevel 1 exit /b 1
set "RuntimeFailure=reported execution diagnostics"
call :check_file "%TestDirectory%\Report.err" 0 e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855
if errorlevel 1 exit /b 1
set "RuntimeFailure=unknown option status"
"%RuntimeRunner%" "%Fixture%" --unknown >"%TestDirectory%\Option.out" 2>"%TestDirectory%\Option.err"
if not "%ERRORLEVEL%"=="64" exit /b 1
set "RuntimeFailure=unknown option output"
call :check_file "%TestDirectory%\Option.out" 0 e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855
if errorlevel 1 exit /b 1
set "RuntimeFailure=unknown option diagnostics"
call :check_file "%TestDirectory%\Option.err" 43 fd8455c7428eece156befe036c10c6927efee163a7315dad72c730f6e2bcef64
if errorlevel 1 exit /b 1
set "RuntimeFailure=invalid fixture copy"
copy /y "%InvalidFixture%" "%TestDirectory%\Invalid.wvb" >nul || exit /b 1
set "RuntimeFailure=invalid fixture identity before execution"
call :check_file "%TestDirectory%\Invalid.wvb" 479 0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5
if errorlevel 1 exit /b 1
set "RuntimeFailure=invalid fixture rejection status"
"%RuntimeRunner%" "%TestDirectory%\Invalid.wvb" >"%TestDirectory%\Reject.out" 2>"%TestDirectory%\Reject.err"
if not "%ERRORLEVEL%"=="1" exit /b 1
set "RuntimeFailure=invalid fixture rejection output"
call :check_file "%TestDirectory%\Reject.out" 0 e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855
if errorlevel 1 exit /b 1
set "RuntimeFailure=invalid fixture rejection diagnostics"
call :check_file "%TestDirectory%\Reject.err" 68 a88ea127be32ffbde27b0944be4e8c232155bec2cbd8ba3ae0449d7d20dfac0a
if errorlevel 1 exit /b 1
set "RuntimeFailure=invalid fixture identity after execution"
call :check_file "%TestDirectory%\Invalid.wvb" 479 0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5
if errorlevel 1 exit /b 1
exit /b 0
