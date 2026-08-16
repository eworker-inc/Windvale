@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~3"=="" goto :usage
if not "%~4"=="" goto :usage
if /I not "%~x3"==".wvb" (
    >&2 echo The native compiler bootstrap output must use the .wvb extension.
    exit /b 64
)

set "ArtifactRoot=%~f1"
set "SourceRoot=%~f2"
set "OutputPath=%~f3"
set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
if not exist "%ArtifactRoot%\." (
    >&2 echo The native seed artifact root does not exist.
    exit /b 64
)
if not exist "%SourceRoot%\." (
    >&2 echo The compiler source root does not exist.
    exit /b 64
)

set "CompilerWvb=%ArtifactRoot%\Native-Compiler-Seed\Wvb\Windvale-Compiler.wvb"
set "Compiler=%ArtifactRoot%\Native-Compiler-Seed\windows-x64\wvcompiler.exe"
set "Publisher=%ArtifactRoot%\Native-Front-Door\windows-x64\wvpublish.exe"
set "Project=%SourceRoot%\Projects/Examples/Windvale-Compiler.wvproj"

call :verify_file "%CompilerWvb%" 914746 48ff781359d9bab96ec3e19e4edba19a26ba82552d5bfd1c1a72d64b75f224a6 "native compiler seed WVB"
if errorlevel 1 exit /b 1
call :verify_file "%Compiler%" 27467776 344940f66b26b516b8b4e10a712a6b2c01cbff95aa7ff18aac0789ba9197f970 "Windows native compiler seed"
if errorlevel 1 exit /b 1
call :verify_file "%Publisher%" 1371136 b9fd1b11bc1e4a726e4a43b16830a9351fe573b30e547ba8d8f6660f688ed421 "Windows native publisher"
if errorlevel 1 exit /b 1
call :verify_file "%Project%" 649 a180b171446a6b047b737913ead74fb77a2ecb8d5eedcef833e881dc93ec9b05 "compiler project manifest"
if errorlevel 1 exit /b 1

:allocate
set "TemporaryDirectory=%TEMP%\windvale-native-compiler-bootstrap-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "Stage1=%TemporaryDirectory%\Stage1.wvb"
set "Stage1Compiler=%TemporaryDirectory%\Stage1-Compiler.exe"
set "Candidate=%TemporaryDirectory%\Candidate.wvb"

call "%RepositoryRoot%\Tools\Native\Compile-Compiler-Source-Set.cmd" ^
    "%Compiler%" "%SourceRoot%" "%Stage1%"
set "Result=%ERRORLEVEL%"
if not "%Result%"=="0" goto :cleanup

call :verify_file "%Stage1%" 947975 c929d5123078272e33a3c32288c770d6c20c2abc8f8800a3e0a32b8bda5c2fcb "transitional Stage 1 compiler WVB"
if errorlevel 1 (
    set "Result=1"
    goto :cleanup
)

call "%RepositoryRoot%\Tools\Native\Package-Segmented-Compiler-Wvb.cmd" ^
    1 "%Stage1%" "%Stage1Compiler%"
set "Result=%ERRORLEVEL%"
if not "%Result%"=="0" goto :cleanup

call "%RepositoryRoot%\Tools\Native\Compile-Compiler-Source-Set.cmd" ^
    "%Stage1Compiler%" "%SourceRoot%" "%Candidate%"
set "Result=%ERRORLEVEL%"
if not "%Result%"=="0" goto :cleanup

call :verify_file "%Candidate%" 923818 49b5cbf040de4bcb22c071a5da9a4fbad47f4f0658ef910957a67b52c07607c2 "fixed-point Stage 2 compiler WVB"
if errorlevel 1 (
    set "Result=1"
    goto :cleanup
)

"%Publisher%" "%Candidate%" "%OutputPath%"
set "Result=%ERRORLEVEL%"

:cleanup
del /f /q "%Stage1%" "%Stage1Compiler%" "%Candidate%" >nul 2>nul
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
>&2 echo Usage: Tools\Native\Bootstrap-Compiler.cmd ^<artifact-root^> ^<source-root^> ^<output.wvb^>
exit /b 64
