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

set "FailureStep=compiler-scale current build-driver identity"
set "BuildDriver=%RepositoryRoot%\Artifacts\Native-Compiler-Reconstruction-Candidate\windows-x64\wvbuild.exe"
certutil -hashfile "%BuildDriver%" SHA256 | findstr /I /C:"f556f0e2c794d9424cbcd9f5e3f8e5aee54f49373c7c18ea1d4829facea7dc6f" >nul
if errorlevel 1 goto :failed
set "Workspace=%RepositoryRoot%\Windvale.wvws"
set "WorkspaceResource=%Workspace:\=/%"
set "CompilerProject=%RepositoryRoot%\Projects\Tools\Windvale-Compiler-Build-Driver.wvproj"
set "CompilerProjectResource=%CompilerProject:\=/%"
set "CompilerWvb=%TestDirectory%\Compiler-Build-Driver.wvb"
set "CompilerWvbResource=%CompilerWvb:\=/%"
set "FailureStep=compiler-scale WVB build"
"%BuildDriver%" --workspace "%WorkspaceResource%" --project ^
    "%CompilerProjectResource%" "%CompilerWvbResource%" ^
    >"%TestDirectory%\Compiler-Build.out" 2>"%TestDirectory%\Compiler-Build.err"
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Compiler-Build.err") do if not "%%~zF"=="0" goto :failed
for %%F in ("%CompilerWvb%") do if not "%%~zF"=="1182549" goto :failed
certutil -hashfile "%CompilerWvb%" SHA256 | findstr /I /C:"1c2fa49bdd35a12125072b361b244521d2a0f22ccb432c99f701d1f2c229ff6a" >nul
if errorlevel 1 goto :failed
set "FailureStep=compiler-scale native staging"
"%TestDirectory%\windows-x64-wvstage.exe" "%CompilerWvb%" ^
    "%TestDirectory%\Compiler-Object" "%TestDirectory%\Compiler-Object.wvop" ^
    >"%TestDirectory%\Compiler-Stage.out" 2>"%TestDirectory%\Compiler-Stage.err"
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Compiler-Stage.err") do if not "%%~zF"=="0" goto :failed
for %%F in ("%TestDirectory%\Compiler-Stage.out") do if not "%%~zF"=="86" goto :failed
findstr /b /c:"native x64 staging status=Complete object-bytes=31025972 chunks=40 manifest-bytes=504" "%TestDirectory%\Compiler-Stage.out" >nul
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
if exist "%TestDirectory%\Compiler-Build.err" type "%TestDirectory%\Compiler-Build.err"
if exist "%TestDirectory%\Compiler-Stage.out" type "%TestDirectory%\Compiler-Stage.out"
if exist "%TestDirectory%\Compiler-Stage.err" type "%TestDirectory%\Compiler-Stage.err"
call :cleanup >nul 2>nul
set /a Tests+=1
set /a Failed=Tests-Passed
echo FAIL  segmented compiler toolset reconstruction
echo Tests: %Tests%, Passed: %Passed%, Failed: %Failed%
exit /b 1
