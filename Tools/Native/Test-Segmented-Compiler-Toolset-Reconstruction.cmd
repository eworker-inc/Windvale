@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Candidate=%RepositoryRoot%\Artifacts\Native-Segmented-Compiler-Toolset-Candidate"
set /a Tests=0
set /a Passed=0

call "%RepositoryRoot%\Tools\Native\Construct-Segmented-Compiler-Toolset.cmd" >nul 2>nul
if not "%ERRORLEVEL%"=="64" goto :failed

:allocate
set "TestDirectory=%TEMP%\windvale-segmented-toolset-reconstruction-test-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TestDirectory%" goto :allocate
mkdir "%TestDirectory%" || goto :failed
call "%RepositoryRoot%\Tools\Native\Construct-Segmented-Compiler-Toolset.cmd" "%TestDirectory%" ^
    >"%TestDirectory%\Construct.out" 2>"%TestDirectory%\Construct.err"
if errorlevel 1 goto :failed
findstr /x /c:"native segmented compiler toolset construction status=Complete artifacts=9" "%TestDirectory%\Construct.out" >nul
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Construct.err") do if not "%%~zF"=="0" goto :failed

call :verify_family Wvo-Staging-Producer.wvb windows-x64-wvstage.exe linux-x64-wvstage.elf
if errorlevel 1 goto :failed
call :pass "WVO staging producer reconstruction"

call :verify_family Compiler-Image-Staging.wvb windows-x64-wvlinkstage.exe linux-x64-wvlinkstage.elf
if errorlevel 1 goto :failed
call :pass "compiler-image staging reconstruction"

call :verify_family Compiler-Image-Canonical-Transport.wvb windows-x64-wvimagetransport.exe linux-x64-wvimagetransport.elf
if errorlevel 1 goto :failed
call :pass "compiler-image transport reconstruction"

call :cleanup
echo Tests: %Tests%, Passed: %Passed%, Failed: 0
exit /b 0

:verify_family
call :verify_exact "%TestDirectory%\%~1" "%Candidate%\%~1"
if errorlevel 1 exit /b 1
call :verify_exact "%TestDirectory%\%~2" "%Candidate%\%~2"
if errorlevel 1 exit /b 1
call :verify_exact "%TestDirectory%\%~3" "%Candidate%\%~3"
exit /b %ERRORLEVEL%

:verify_exact
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
echo(%ResolvedTestDirectory%| findstr /b /i /c:"%TEMP%\windvale-segmented-toolset-reconstruction-test-" >nul || exit /b 1
if exist "%ResolvedTestDirectory%\." rmdir /s /q "%ResolvedTestDirectory%"
exit /b 0

:failed
call :cleanup >nul 2>nul
set /a Tests+=1
set /a Failed=Tests-Passed
echo FAIL  segmented compiler toolset reconstruction
echo Tests: %Tests%, Passed: %Passed%, Failed: %Failed%
exit /b 1
