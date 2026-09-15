@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Package-Format.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"

:allocate
set "TemporaryDirectory=%TEMP%\windvale-package-format-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "Result=1"
set "CanonicalProject=%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Canonical-Package-Text.wvproj"
set "CanonicalWvb=%TemporaryDirectory%\Canonical.wvb"
set "CanonicalWindowsApplication=%TemporaryDirectory%\Canonical.exe"
set "CanonicalLinuxApplication=%TemporaryDirectory%\Canonical.elf"
set "Project=%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Package-Manifest.wvproj"
set "First=%TemporaryDirectory%\First.wvb"
set "Second=%TemporaryDirectory%\Second.wvb"
set "WindowsApplication=%TemporaryDirectory%\Package-Format.exe"
set "LinuxApplication=%TemporaryDirectory%\Package-Format.elf"
set "LockProject=%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Package-Lock.wvproj"
set "LockWvb=%TemporaryDirectory%\Lock.wvb"
set "LockWindowsApplication=%TemporaryDirectory%\Lock.exe"
set "LockLinuxApplication=%TemporaryDirectory%\Lock.elf"
set "ConsistencyProject=%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Package-Consistency.wvproj"
set "ConsistencyWvb=%TemporaryDirectory%\Consistency.wvb"
set "ConsistencyWindowsApplication=%TemporaryDirectory%\Consistency.exe"
set "ConsistencyLinuxApplication=%TemporaryDirectory%\Consistency.elf"
set "Manifest=%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvpack"
set "Lock=%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvlock"
set "InspectorManifest=%RepositoryRoot%\Distribution\Applications\Wvb-Inspector\Windvale-Wvb-Inspector.wvpack"
set "InspectorLock=%RepositoryRoot%\Distribution\Applications\Wvb-Inspector\Windvale-Wvb-Inspector.wvlock"
set "AdmissionProject=%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Package-Resource-Admission.wvproj"
set "AdmissionWvb=%TemporaryDirectory%\Admission.wvb"
set "AdmissionWindowsApplication=%TemporaryDirectory%\Admission.exe"
set "AdmissionLinuxApplication=%TemporaryDirectory%\Admission.elf"
set "GenerationProject=%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Installation-Generation.wvproj"
set "GenerationWvb=%TemporaryDirectory%\Generation.wvb"
set "GenerationWindowsApplication=%TemporaryDirectory%\Generation.exe"
set "GenerationLinuxApplication=%TemporaryDirectory%\Generation.elf"

set "Step=canonical-build"
echo native package format step=canonical-build status=Started
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%CanonicalProject%" "%CanonicalWvb%" >nul
if errorlevel 1 goto :cleanup
set "Step=canonical-windows-container"
echo native package format step=canonical-windows-container status=Started
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" current ^
    6 "%CanonicalWvb%" "%CanonicalWindowsApplication%" windows >nul
if errorlevel 1 goto :cleanup
set "Step=canonical-linux-container"
echo native package format step=canonical-linux-container status=Started
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" current ^
    6 "%CanonicalWvb%" "%CanonicalLinuxApplication%" linux >nul
if errorlevel 1 goto :cleanup
set "Step=canonical-windows-execution"
echo native package format step=canonical-windows-execution status=Started
"%CanonicalWindowsApplication%" >nul
if not "%ERRORLEVEL%"=="42" goto :cleanup

set "Step=first-build"
echo native package format step=first-build status=Started
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%Project%" "%First%" >nul
if errorlevel 1 goto :cleanup
set "Step=second-build"
echo native package format step=second-build status=Started
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%Project%" "%Second%" >nul
if errorlevel 1 goto :cleanup
set "Step=deterministic-wvb"
echo native package format step=deterministic-wvb status=Started
fc /b "%First%" "%Second%" >nul
if errorlevel 1 goto :cleanup

set "Step=windows-container"
echo native package format step=windows-container status=Started
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" current ^
    6 "%First%" "%WindowsApplication%" windows >nul
if errorlevel 1 goto :cleanup
set "Step=linux-container"
echo native package format step=linux-container status=Started
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" current ^
    6 "%First%" "%LinuxApplication%" linux >nul
if errorlevel 1 goto :cleanup
set "Step=windows-execution"
echo native package format step=windows-execution status=Started
"%WindowsApplication%" >nul
if not "%ERRORLEVEL%"=="42" goto :cleanup

set "Step=lock-build"
echo native package format step=lock-build status=Started
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%LockProject%" "%LockWvb%" >nul
if errorlevel 1 goto :cleanup
set "Step=lock-windows-container"
echo native package format step=lock-windows-container status=Started
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" current ^
    6 "%LockWvb%" "%LockWindowsApplication%" windows >nul
if errorlevel 1 goto :cleanup
set "Step=lock-linux-container"
echo native package format step=lock-linux-container status=Started
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" current ^
    6 "%LockWvb%" "%LockLinuxApplication%" linux >nul
if errorlevel 1 goto :cleanup
set "Step=lock-windows-execution"
echo native package format step=lock-windows-execution status=Started
"%LockWindowsApplication%" >nul
if not "%ERRORLEVEL%"=="42" goto :cleanup

set "Step=consistency-build"
echo native package format step=consistency-build status=Started
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%ConsistencyProject%" "%ConsistencyWvb%" >nul
if errorlevel 1 goto :cleanup
set "Step=consistency-windows-container"
echo native package format step=consistency-windows-container status=Started
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" current ^
    6 "%ConsistencyWvb%" "%ConsistencyWindowsApplication%" windows >nul
if errorlevel 1 goto :cleanup
set "Step=consistency-linux-container"
echo native package format step=consistency-linux-container status=Started
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" current ^
    6 "%ConsistencyWvb%" "%ConsistencyLinuxApplication%" linux >nul
if errorlevel 1 goto :cleanup
set "Step=consistency-windows-execution"
echo native package format step=consistency-windows-execution status=Started
"%ConsistencyWindowsApplication%" "%Manifest%" "%Lock%" >nul
if not "%ERRORLEVEL%"=="42" goto :cleanup
set "Step=consistency-windows-inspector"
echo native package format step=consistency-windows-inspector status=Started
"%ConsistencyWindowsApplication%" "%InspectorManifest%" "%InspectorLock%" >nul
if not "%ERRORLEVEL%"=="42" goto :cleanup

set "Step=admission-build"
echo native package format step=admission-build status=Started
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%AdmissionProject%" "%AdmissionWvb%" >nul
if errorlevel 1 goto :cleanup
set "Step=admission-windows-container"
echo native package format step=admission-windows-container status=Started
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" current ^
    6 "%AdmissionWvb%" "%AdmissionWindowsApplication%" windows >nul
if errorlevel 1 goto :cleanup
set "Step=admission-linux-container"
echo native package format step=admission-linux-container status=Started
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" current ^
    6 "%AdmissionWvb%" "%AdmissionLinuxApplication%" linux >nul
if errorlevel 1 goto :cleanup
set "Step=admission-windows-execution"
echo native package format step=admission-windows-execution status=Started
"%AdmissionWindowsApplication%" "%RepositoryRoot%" >nul
if not "%ERRORLEVEL%"=="42" goto :cleanup

set "Step=generation-build"
echo native package format step=generation-build status=Started
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%GenerationProject%" "%GenerationWvb%" >nul
if errorlevel 1 goto :cleanup
set "Step=generation-windows-container"
echo native package format step=generation-windows-container status=Started
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" current ^
    6 "%GenerationWvb%" "%GenerationWindowsApplication%" windows >nul
if errorlevel 1 goto :cleanup
set "Step=generation-linux-container"
echo native package format step=generation-linux-container status=Started
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" current ^
    6 "%GenerationWvb%" "%GenerationLinuxApplication%" linux >nul
if errorlevel 1 goto :cleanup
set "Step=generation-windows-execution"
echo native package format step=generation-windows-execution status=Started
"%GenerationWindowsApplication%" >nul
if not "%ERRORLEVEL%"=="42" goto :cleanup

echo native package format status=Passed result=42 modules=6 builds=7 groups=82 cross-host-images=12
set "Result=0"

:cleanup
if not "%Result%"=="0" >&2 echo Native package-format step failed: %Step%
for %%R in ("%TemporaryDirectory%") do set "ResolvedTemporaryDirectory=%%~fR"
echo(%ResolvedTemporaryDirectory%| findstr /b /i /c:"%TEMP%\windvale-package-format-" >nul || exit /b 1
if exist "%ResolvedTemporaryDirectory%\." rmdir /s /q "%ResolvedTemporaryDirectory%"
exit /b %Result%
