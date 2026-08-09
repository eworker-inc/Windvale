@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Candidate=%RepositoryRoot%\Artifacts\Native-Segmented-Compiler-Toolset-Candidate"

:allocate
set "TemporaryDirectory=%TEMP%\windvale-segmented-compiler-package-test-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "Output=%TemporaryDirectory%\Compiler-Image-Staging.exe"
set "CurrentLowererWvb=%TemporaryDirectory%\Current-Lowerer.wvb"
set "CurrentLowerer=%TemporaryDirectory%\Current-Lowerer.exe"
set "DescriptorWvb=%TemporaryDirectory%\Descriptor-Main.wvb"
set "DescriptorWvo=%TemporaryDirectory%\Descriptor-Main.wvo"
set "BridgeWvo=%TemporaryDirectory%\Baseline-Jit-Patch-Plan-Bridge.wvo"
set "Result=1"

call "%RepositoryRoot%\Tools\Native\Package-Segmented-Compiler-Wvb.cmd" 6 ^
    "%Candidate%\Compiler-Image-Staging.wvb" "%Output%" >nul
if errorlevel 1 goto :cleanup
for %%F in ("%Output%") do if not "%%~zF"=="851968" goto :cleanup
certutil -hashfile "%Output%" SHA256 | findstr /I /C:"967827e4592c23f30e2a70b9a60a43837c1dfec6112584596c09d382058e2752" >nul
if errorlevel 1 goto :cleanup
fc /b "%Output%" "%Candidate%\windows-x64-wvlinkstage.exe" >nul
if errorlevel 1 goto :cleanup

echo PASS  segmented compiler packaging reproduces exact Windows application

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Windvale-Native-X64-Lowering-Tool.wvproj" ^
    "%CurrentLowererWvb%" >nul
if errorlevel 1 goto :cleanup
for %%F in ("%CurrentLowererWvb%") do if not "%%~zF"=="399691" goto :cleanup
certutil -hashfile "%CurrentLowererWvb%" SHA256 | findstr /I /C:"92655af0632b4dd3525c2b2de98353b095fa1df94b524a94aa47f16014f1e508" >nul
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Segmented-Compiler-Wvb.cmd" 6 ^
    "%CurrentLowererWvb%" "%CurrentLowerer%" >nul
if errorlevel 1 goto :cleanup
for %%F in ("%CurrentLowerer%") do if not "%%~zF"=="5792768" goto :cleanup
certutil -hashfile "%CurrentLowerer%" SHA256 | findstr /I /C:"e096dc7fec20e3318364da1f3b5289f772b53c16cc370f29622dfac35780e2bf" >nul
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Windvale-Native-Test-Wvb-To-Wvo-Descriptor-Main.wvproj" ^
    "%DescriptorWvb%" >nul
if errorlevel 1 goto :cleanup
"%CurrentLowerer%" "%DescriptorWvb%" "%DescriptorWvo%" >nul
if errorlevel 1 goto :cleanup
for %%F in ("%DescriptorWvo%") do if not "%%~zF"=="793" goto :cleanup
certutil -hashfile "%DescriptorWvo%" SHA256 | findstr /I /C:"9936663f45c194441bfc5e8464286e57f83cd3a18948597a8011af608a4faa51" >nul
if errorlevel 1 goto :cleanup

"%CurrentLowerer%" ^
    "%RepositoryRoot%\Artifacts\Baseline-Jit-Publisher\Wvb\Baseline-Jit-Patch-Plan-Bridge.wvb" ^
    "%BridgeWvo%" >nul
if errorlevel 1 goto :cleanup
fc /b "%BridgeWvo%" ^
    "%RepositoryRoot%\Artifacts\Baseline-Jit-Publisher\Wvo\Baseline-Jit-Patch-Plan-Bridge.wvo" >nul
if errorlevel 1 goto :cleanup

echo PASS  segmented compiler packaging reconstructs the current native lowerer
echo Tests: 2, Passed: 2, Failed: 0
set "Result=0"

:cleanup
del /f /q "%TemporaryDirectory%\*" >nul 2>nul
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b %Result%
