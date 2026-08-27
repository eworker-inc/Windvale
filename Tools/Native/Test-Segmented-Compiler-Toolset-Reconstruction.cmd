@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Candidate=%RepositoryRoot%\Artifacts\Native-Segmented-Compiler-Toolset-Candidate"
set /a Tests=0
set /a Passed=0
set "FailureStep=usage-contract"

echo START segmented compiler toolset reconstruction step=construction
call "%RepositoryRoot%\Tools\Native\Construct-Segmented-Compiler-Toolset.cmd" >nul 2>nul
if not "%ERRORLEVEL%"=="64" goto :failed

set "FailureStep=construction"
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
echo INFO  segmented compiler toolset reconstruction step=construction status=Complete

set "FailureStep=WVO staging producer identity"
echo START segmented compiler toolset reconstruction phase=WVO-staging-producer item=1/4
call :verify_family Wvo-Staging-Producer.wvb windows-x64-wvstage.exe linux-x64-wvstage.elf
if errorlevel 1 goto :failed
call :pass "WVO staging producer reconstruction"

set "FailureStep=compiler-image staging identity"
echo START segmented compiler toolset reconstruction phase=compiler-image-staging item=2/4
call :verify_family Compiler-Image-Staging.wvb windows-x64-wvlinkstage.exe linux-x64-wvlinkstage.elf
if errorlevel 1 goto :failed
call :pass "compiler-image staging reconstruction"

set "FailureStep=compiler-image transport identity"
echo START segmented compiler toolset reconstruction phase=compiler-image-transport item=3/4
call :verify_family Compiler-Image-Canonical-Transport.wvb windows-x64-wvimagetransport.exe linux-x64-wvimagetransport.elf
if errorlevel 1 goto :failed
call :pass "compiler-image transport reconstruction"

set "FailureStep=compiler-scale bootstrap analyzer identity"
set "CompilerWvb=%RepositoryRoot%\Artifacts\Language-1.0-Target-Aware-Emission-Bootstrap\Wvb\wvanalyze.wvb"
for %%F in ("%CompilerWvb%") do if not "%%~zF"=="992412" goto :failed
certutil -hashfile "%CompilerWvb%" SHA256 | findstr /I /C:"26ea9bccfe8c2763fb887a5a14c2f0a086a27265523c3df84187b361616f9120" >nul
if errorlevel 1 goto :failed
echo START segmented compiler toolset reconstruction phase=compiler-scale item=4/4
echo INFO  segmented compiler toolset reconstruction phase=compiler-scale step=input-identity status=Complete bytes=992412
set "FailureStep=compiler-scale native staging"
echo START segmented compiler toolset reconstruction phase=compiler-scale step=native-staging
"%TestDirectory%\windows-x64-wvstage.exe" "%CompilerWvb%" ^
    "%TestDirectory%\Compiler-Object" "%TestDirectory%\Compiler-Object.wvop" ^
    >"%TestDirectory%\Compiler-Stage.out" 2>"%TestDirectory%\Compiler-Stage.err"
if errorlevel 1 goto :failed
set "FailureStep=compiler-scale native staging diagnostic"
for %%F in ("%TestDirectory%\Compiler-Stage.err") do if not "%%~zF"=="0" goto :failed
set "FailureStep=compiler-scale native staging report"
findstr /b /c:"native x64 staging status=Complete object-bytes=31736596 chunks=41 manifest-bytes=516" "%TestDirectory%\Compiler-Stage.out" >nul
if errorlevel 1 goto :failed
call :pass "compiler-scale WVB staging"

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
if defined FailureStep echo FAIL  step=%FailureStep%
if exist "%TestDirectory%\Construct.out" type "%TestDirectory%\Construct.out"
if exist "%TestDirectory%\Construct.err" type "%TestDirectory%\Construct.err" >&2
if exist "%TestDirectory%\Compiler-Build.err" type "%TestDirectory%\Compiler-Build.err"
if exist "%TestDirectory%\Compiler-Stage.out" type "%TestDirectory%\Compiler-Stage.out"
if exist "%TestDirectory%\Compiler-Stage.err" type "%TestDirectory%\Compiler-Stage.err"
call :cleanup >nul 2>nul
set /a Tests+=1
set /a Failed=Tests-Passed
echo FAIL  segmented compiler toolset reconstruction
echo Tests: %Tests%, Passed: %Passed%, Failed: %Failed%
exit /b 1
