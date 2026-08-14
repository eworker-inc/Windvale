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

set "Step=canonical-build"
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%CanonicalProject%" "%CanonicalWvb%" >nul
if errorlevel 1 goto :cleanup
set "Step=canonical-windows-container"
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" ^
    6 "%CanonicalWvb%" "%CanonicalWindowsApplication%" windows >nul
if errorlevel 1 goto :cleanup
set "Step=canonical-linux-container"
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" ^
    6 "%CanonicalWvb%" "%CanonicalLinuxApplication%" linux >nul
if errorlevel 1 goto :cleanup
set "Step=canonical-windows-execution"
"%CanonicalWindowsApplication%" >nul
if not "%ERRORLEVEL%"=="42" goto :cleanup

set "Step=first-build"
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%Project%" "%First%" >nul
if errorlevel 1 goto :cleanup
set "Step=second-build"
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%Project%" "%Second%" >nul
if errorlevel 1 goto :cleanup
set "Step=deterministic-wvb"
fc /b "%First%" "%Second%" >nul
if errorlevel 1 goto :cleanup

set "Step=windows-container"
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" ^
    6 "%First%" "%WindowsApplication%" windows >nul
if errorlevel 1 goto :cleanup
set "Step=linux-container"
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" ^
    6 "%First%" "%LinuxApplication%" linux >nul
if errorlevel 1 goto :cleanup
set "Step=windows-execution"
"%WindowsApplication%" >nul
if not "%ERRORLEVEL%"=="42" goto :cleanup

echo native package format status=Passed result=42 modules=2 builds=3 groups=13 cross-host-images=4
set "Result=0"

:cleanup
if not "%Result%"=="0" >&2 echo Native package-format step failed: %Step%
for %%R in ("%TemporaryDirectory%") do set "ResolvedTemporaryDirectory=%%~fR"
echo(%ResolvedTemporaryDirectory%| findstr /b /i /c:"%TEMP%\windvale-package-format-" >nul || exit /b 1
if exist "%ResolvedTemporaryDirectory%\." rmdir /s /q "%ResolvedTemporaryDirectory%"
exit /b %Result%
