@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Candidate=%RepositoryRoot%\Artifacts\Native-Compiler-Reconstruction-Candidate"
set /a Tests=0
set /a Passed=0

call :check_file "%Candidate%\Wvb\Windvale-Compiler.wvb" 931035 13558d9dbc0d185b161b770824aa29ff90b8873903b2b5d7184a23950a6fc1e4
if errorlevel 1 goto :failed
call :check_file "%Candidate%\windows-x64\wvcompiler.exe" 27898368 4009e6747bbf9a6d2b0b2ec90e2368ca50fda863d445534f15ef96e22a657b34
if errorlevel 1 goto :failed
call :check_file "%Candidate%\linux-x64\wvcompiler.elf" 27897856 c266adf20fe2927a446483f68880ef323c480f011b0c26384716ea2f651bcd65
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Wvb\Compiler-Build-Driver.wvb" 1162338 a214662da422443cd70c4be12c8f0bd06cbb5bce9fe3a56e2a52c46a37445a20
if errorlevel 1 goto :failed
call :check_file "%Candidate%\windows-x64\wvbuild.exe" 30381568 b0d58f4d8d6d32e09d45358035ce4521410ec1280f11fa72b66245e621ba49a3
if errorlevel 1 goto :failed
call :check_file "%Candidate%\linux-x64\wvbuild.elf" 30380032 b4fdc30a7242e03f7166491bf6b415aa3b5dce8ff0e16444f6ccb24c5bcb03a0
if errorlevel 1 goto :failed
call :pass "candidate inventory"

call "%RepositoryRoot%\Tools\Native\Construct-Compiler-Reconstruction.cmd" >nul 2>nul
if not "%ERRORLEVEL%"=="64" goto :failed
call :pass "usage rejection"

:allocate
set "TestDirectory=%TEMP%\windvale-compiler-reconstruction-test-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TestDirectory%" goto :allocate
mkdir "%TestDirectory%" || goto :failed
call "%RepositoryRoot%\Tools\Native\Construct-Compiler-Reconstruction.cmd" "%TestDirectory%" ^
    >"%TestDirectory%\Construct.out" 2>"%TestDirectory%\Construct.err"
if errorlevel 1 goto :failed
findstr /x /c:"native compiler reconstruction status=Complete compiler-bytes=931035 native-bytes=27867015 entry-offset=51356 chunks=7 build-driver-bytes=1162338 build-driver-entry-offset=220460 build-driver-chunks=8" "%TestDirectory%\Construct.out" >nul
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Construct.err") do if not "%%~zF"=="0" goto :failed
call :check_file "%TestDirectory%\Wvb\Windvale-Compiler.wvb" 931035 13558d9dbc0d185b161b770824aa29ff90b8873903b2b5d7184a23950a6fc1e4
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\windows-x64\wvcompiler.exe" 27898368 4009e6747bbf9a6d2b0b2ec90e2368ca50fda863d445534f15ef96e22a657b34
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\linux-x64\wvcompiler.elf" 27897856 c266adf20fe2927a446483f68880ef323c480f011b0c26384716ea2f651bcd65
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Wvb\Compiler-Build-Driver.wvb" 1162338 a214662da422443cd70c4be12c8f0bd06cbb5bce9fe3a56e2a52c46a37445a20
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\windows-x64\wvbuild.exe" 30381568 b0d58f4d8d6d32e09d45358035ce4521410ec1280f11fa72b66245e621ba49a3
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\linux-x64\wvbuild.elf" 30380032 b4fdc30a7242e03f7166491bf6b415aa3b5dce8ff0e16444f6ccb24c5bcb03a0
if errorlevel 1 goto :failed
call :pass "native paired reconstruction"

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

:failed
call :cleanup >nul 2>nul
set /a Tests+=1
set /a Failed=Tests-Passed
echo FAIL  compiler reconstruction
echo Tests: %Tests%, Passed: %Passed%, Failed: %Failed%
exit /b 1
