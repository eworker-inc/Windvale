@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "Development=0"
if "%~1"=="" goto :arguments_complete
if /I not "%~1"=="--development" goto :usage
if not "%~2"=="" goto :usage
set "Development=1"

:arguments_complete
set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Candidate=%RepositoryRoot%\Artifacts\Native-Compiler-Reconstruction-Candidate"
set /a Tests=0
set /a Passed=0

call :check_file "%Candidate%\Wvb\Windvale-Compiler.wvb" 935163 a7d47b2de29faee089c7a22ef23eac4657f719331dc02044eb2d818457dac5b6
if errorlevel 1 goto :failed
call :check_file "%Candidate%\windows-x64\wvcompiler.exe" 28172800 a5db938a814471fdacda75efcf57d28934ae52b3b2290732627c14ba173fd70d
if errorlevel 1 goto :failed
call :check_file "%Candidate%\linux-x64\wvcompiler.elf" 28172288 da11ab3b70b428087cbcb9de5614a2dbdccd31afc6861cc15881fd65c12ff19b
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Wvb\Compiler-Build-Driver.wvb" 1142818 125d2b4080889615877d843a36b2f9f6b50d049d011cc06fa8ab426ab83c0574
if errorlevel 1 goto :failed
call :check_file "%Candidate%\windows-x64\wvbuild.exe" 30071296 f556f0e2c794d9424cbcd9f5e3f8e5aee54f49373c7c18ea1d4829facea7dc6f
if errorlevel 1 goto :failed
call :check_file "%Candidate%\linux-x64\wvbuild.elf" 30072832 628fd60ea702c4a3b3ffb01d32cba7ba9708477acccf190cc6506a56f159d7a9
if errorlevel 1 goto :failed
call :pass "retained candidate inventory"

call "%RepositoryRoot%\Tools\Native\Verify-Compiler-Convergence.cmd" unexpected >nul 2>nul
if not "%ERRORLEVEL%"=="64" goto :failed
call :pass "usage rejection"

:allocate
set "TestDirectory=%TEMP%\windvale-compiler-reconstruction-test-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TestDirectory%" goto :allocate
mkdir "%TestDirectory%" || goto :failed
"%Candidate%\windows-x64\wvcompiler.exe" ^
    "%RepositoryRoot%\Tests\Fixtures\Source-Wvb\Function-Only.wv" ^
    "%TestDirectory%\Direct.wvb" ^
    >"%TestDirectory%\Direct.out" 2>"%TestDirectory%\Direct.err"
if errorlevel 1 goto :failed
call "%RepositoryRoot%\Tools\Native\Build-Current-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Function-Only.wvproj" ^
    "%TestDirectory%\Project.wvb" ^
    >"%TestDirectory%\Project.out" 2>"%TestDirectory%\Project.err"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Direct.wvb" 816 28d215b982a7b7185cfa80c4cc5346666bd0181582fe80bec8b7035d514da936
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Project.wvb" 816 28d215b982a7b7185cfa80c4cc5346666bd0181582fe80bec8b7035d514da936
if errorlevel 1 goto :failed
fc /b "%TestDirectory%\Direct.wvb" "%TestDirectory%\Project.wvb" >nul
if errorlevel 1 goto :failed
call "%RepositoryRoot%\Tools\Native\Verify-Wvb.cmd" "%TestDirectory%\Direct.wvb" ^
    >"%TestDirectory%\Verify.out" 2>"%TestDirectory%\Verify.err"
if errorlevel 1 goto :failed
for %%F in (
    "%TestDirectory%\Direct.err"
    "%TestDirectory%\Project.err"
    "%TestDirectory%\Verify.err"
) do if not "%%~zF"=="0" goto :failed
call :pass "retained-to-current compiler differential smoke"

call :cleanup
echo Tests: %Tests%, Passed: %Passed%, Failed: 0
exit /b 0

:pass
set /a Tests+=1
set /a Passed+=1
echo PASS  %~1
exit /b 0

:check_file
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~3" >nul
exit /b %ERRORLEVEL%

:cleanup
if not defined TestDirectory exit /b 0
for %%R in ("%TestDirectory%") do set "ResolvedTestDirectory=%%~fR"
echo(%ResolvedTestDirectory%| findstr /b /i /c:"%TEMP%\windvale-compiler-reconstruction-test-" >nul || exit /b 1
if exist "%ResolvedTestDirectory%\." rmdir /s /q "%ResolvedTestDirectory%"
exit /b 0

:usage
>&2 echo Usage: Tools\Native\Test-Compiler-Reconstruction.cmd [--development]
exit /b 64

:failed
call :cleanup >nul 2>nul
set /a Tests+=1
set /a Failed=Tests-Passed
echo FAIL  compiler reconstruction
echo Tests: %Tests%, Passed: %Passed%, Failed: %Failed%
exit /b 1
