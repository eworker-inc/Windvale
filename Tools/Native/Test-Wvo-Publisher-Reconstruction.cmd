@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Candidate=%RepositoryRoot%\Artifacts\Native-Wvo-Publisher-Candidate"
set /a Tests=0
set /a Passed=0

call :check_file "%Candidate%\Wvo-Publisher.wvb" 41365 4e8c81da38f5eb06f9334c2d2c5e35120a13e73bac3a9375b5e6a2eff04438c5
if errorlevel 1 goto :failed
call :check_file "%Candidate%\windows-x64-wvopublish.exe" 430080 76f632ffa7998a6cce0386456fee98f02cbb5ec424d0d914a7e1f06ff3853910
if errorlevel 1 goto :failed
call :check_file "%Candidate%\linux-x64-wvopublish.elf" 426997 2889237d7fdb20b1d420c05834f19183d18b02112e3f4eea0ed7ff43414814f2
if errorlevel 1 goto :failed
call :pass "candidate inventory"

call "%RepositoryRoot%\Tools\Native\Construct-Wvo-Publisher.cmd" >nul 2>nul
if not "%ERRORLEVEL%"=="64" goto :failed

:allocate
set "TestDirectory=%TEMP%\windvale-wvo-publisher-reconstruction-test-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TestDirectory%" goto :allocate
mkdir "%TestDirectory%" || goto :failed

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Windvale-Wvo-Publisher.wvproj" "%TestDirectory%\Wvo-Publisher.wvb" ^
    >"%TestDirectory%\Build.out" 2>"%TestDirectory%\Build.err"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Build.out" 219 da952e943f937655a80d4847bbab9aaa701a31d8809d6c810ff6bde02590a396
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Build.err") do if not "%%~zF"=="0" goto :failed
call :check_equal "%TestDirectory%\Wvo-Publisher.wvb" "%Candidate%\Wvo-Publisher.wvb"
if errorlevel 1 goto :failed

call "%RepositoryRoot%\Tools\Native\Construct-Wvo-Publisher.cmd" windows "%TestDirectory%\Wvo-Publisher.exe" ^
    >"%TestDirectory%\Windows.out" 2>"%TestDirectory%\Windows.err"
if errorlevel 1 goto :failed
>"%TestDirectory%\Windows.expected" echo WVO publisher construction status=Valid target=windows bytes=430080
fc /b "%TestDirectory%\Windows.out" "%TestDirectory%\Windows.expected" >nul
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Windows.err") do if not "%%~zF"=="0" goto :failed

call "%RepositoryRoot%\Tools\Native\Construct-Wvo-Publisher.cmd" linux "%TestDirectory%\Wvo-Publisher.elf" ^
    >"%TestDirectory%\Linux.out" 2>"%TestDirectory%\Linux.err"
if errorlevel 1 goto :failed
>"%TestDirectory%\Linux.expected" echo WVO publisher construction status=Valid target=linux bytes=426997
fc /b "%TestDirectory%\Linux.out" "%TestDirectory%\Linux.expected" >nul
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Linux.err") do if not "%%~zF"=="0" goto :failed

call :check_equal "%TestDirectory%\Wvo-Publisher.exe" "%Candidate%\windows-x64-wvopublish.exe"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Wvo-Publisher.elf" "%Candidate%\linux-x64-wvopublish.elf"
if errorlevel 1 goto :failed
call :pass "native WVB and paired WVO publisher reconstruction"

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
echo(%ResolvedTestDirectory%| findstr /b /i /c:"%TEMP%\windvale-wvo-publisher-reconstruction-test-" >nul || exit /b 1
if exist "%ResolvedTestDirectory%\." rmdir /s /q "%ResolvedTestDirectory%"
exit /b 0

:failed
call :cleanup >nul 2>nul
set /a Tests+=1
set /a Failed=Tests-Passed
echo FAIL  WVO publisher reconstruction
echo Tests: %Tests%, Passed: %Passed%, Failed: %Failed%
exit /b 1
