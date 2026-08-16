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
call :verify_file "%Verifier%" 1255936 a1dc701cc8d5ace0a680a15e19435c48b3bccde3cf6197bfdd07ee04a4bf9871 "Windows native WVB verifier"
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

call :verify_file "%Stage2%" 931035 13558d9dbc0d185b161b770824aa29ff90b8873903b2b5d7184a23950a6fc1e4 "Stage 2 compiler WVB"
if errorlevel 1 goto :cleanup
"%Verifier%" "%Stage2%" >"%TemporaryDirectory%\Verify.txt"
if errorlevel 1 goto :cleanup
fc /b "%Stage1%" "%Stage2%" >nul
if errorlevel 1 goto :cleanup

echo native compiler convergence status=Complete compiler-bytes=931035 compiler-sha256=13558d9dbc0d185b161b770824aa29ff90b8873903b2b5d7184a23950a6fc1e4
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
