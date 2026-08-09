@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~2"=="" goto :usage
if not "%~3"=="" goto :usage

set "ArtifactRoot=%~f1"
set "SourceRoot=%~f2"
if not exist "%ArtifactRoot%\." goto :usage
if not exist "%SourceRoot%\." goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Verifier=%ArtifactRoot%\Native-Front-Door\windows-x64\wvverify.exe"
call :verify_file "%Verifier%" 1007104 f15422397ad890909f481f131f945e25651c858695ba5ce58b2a7305b34647f0 "Windows native WVB verifier"
if errorlevel 1 exit /b 1

:allocate
set "TemporaryDirectory=%TEMP%\windvale-native-compiler-convergence-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "Stage1=%TemporaryDirectory%\Stage1.wvb"
set "Stage1Compiler=%TemporaryDirectory%\Stage1-Compiler.exe"
set "Stage2=%TemporaryDirectory%\Stage2.wvb"
set "Result=1"

call "%RepositoryRoot%\Tools\Native\Bootstrap-Compiler.cmd" ^
    "%ArtifactRoot%" "%SourceRoot%" "%Stage1%"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Package-Segmented-Compiler-Wvb.cmd" ^
    1 "%Stage1%" "%Stage1Compiler%"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Compile-Compiler-Source-Set.cmd" ^
    "%Stage1Compiler%" "%SourceRoot%" "%Stage2%"
if errorlevel 1 goto :cleanup

call :verify_file "%Stage2%" 921900 fd96bd567d08a18107a9b149560ce9f2e38b49454250e934a4375f465d132556 "Stage 2 compiler WVB"
if errorlevel 1 goto :cleanup
"%Verifier%" "%Stage2%" >"%TemporaryDirectory%\Verify.txt"
if errorlevel 1 goto :cleanup
fc /b "%Stage1%" "%Stage2%" >nul
if errorlevel 1 goto :cleanup

echo native compiler convergence status=Complete compiler-bytes=921900 compiler-sha256=fd96bd567d08a18107a9b149560ce9f2e38b49454250e934a4375f465d132556
set "Result=0"

:cleanup
del /f /q "%TemporaryDirectory%\*" >nul 2>nul
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b %Result%

:verify_file
if not exist "%~1" (
    >&2 echo Missing %~4: %~1
    exit /b 1
)
for %%F in ("%~1") do if not "%%~zF"=="%~2" (
    >&2 echo The %~4 byte length is invalid.
    exit /b 1
)
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~3" >nul
if errorlevel 1 (
    >&2 echo The %~4 digest is invalid.
    exit /b 1
)
exit /b 0

:usage
>&2 echo Usage: Tools\Native\Verify-Compiler-Convergence.cmd ^<artifact-root^> ^<source-root^>
exit /b 64
