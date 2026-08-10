@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Candidate=%RepositoryRoot%\Artifacts\Native-Compiler-Reconstruction-Candidate"
set /a Tests=0
set /a Passed=0

call :check_file "%Candidate%\Wvb\Windvale-Compiler.wvb" 921640 18a657f8d4192f01a5822274a7348c02fc30b9bb3a4a9283e4ba302590c3f754
if errorlevel 1 goto :failed
call :check_file "%Candidate%\windows-x64\wvcompiler.exe" 27666432 c1be8bd7e2c9496fee0cd3e486348804469d72621bcf45e30d8b6e8a1814da9c
if errorlevel 1 goto :failed
call :check_file "%Candidate%\linux-x64\wvcompiler.elf" 27668480 25905e75e836ad8015a851aa6a52531bf5ab73c9dd97596628c2226740f37a34
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
findstr /x /c:"native compiler reconstruction status=Complete compiler-bytes=921640 native-bytes=27635298 entry-offset=43146 chunks=7" "%TestDirectory%\Construct.out" >nul
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Construct.err") do if not "%%~zF"=="0" goto :failed
call :check_file "%TestDirectory%\Wvb\Windvale-Compiler.wvb" 921640 18a657f8d4192f01a5822274a7348c02fc30b9bb3a4a9283e4ba302590c3f754
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\windows-x64\wvcompiler.exe" 27666432 c1be8bd7e2c9496fee0cd3e486348804469d72621bcf45e30d8b6e8a1814da9c
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\linux-x64\wvcompiler.elf" 27668480 25905e75e836ad8015a851aa6a52531bf5ab73c9dd97596628c2226740f37a34
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
