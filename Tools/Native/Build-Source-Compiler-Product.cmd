@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~2"=="" goto :usage
if not "%~3"=="" goto :usage
if /I not "%~x2"==".wvb" goto :usage
if /I not "%~1"=="core" if /I not "%~1"=="demo" goto :usage

set "Product=%~1"
set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "SeedRoot=%RepositoryRoot%\Artifacts\Native-Compiler-Seed"
set "FrontDoorRoot=%RepositoryRoot%\Artifacts\Native-Front-Door"
set "CompilerWvb=%SeedRoot%\Wvb\Windvale-Compiler.wvb"
set "Compiler=%SeedRoot%\windows-x64\wvcompiler.exe"
set "Publisher=%FrontDoorRoot%\windows-x64\wvpublish.exe"
set "Output=%~f2"
if /I "%Product%"=="core" (
    set "Project=%RepositoryRoot%\Projects/Compiler/Windvale-Source-Wvb-Core.wvproj"
    set "ProjectBytes=603"
    set "ProjectSha256=62349b49ad2608a212e364f8e319f0f376bbc0c8457bcbef2f20a55bfdd0c8c7"
)
if /I "%Product%"=="demo" (
    set "Project=%RepositoryRoot%\Projects/Examples/Windvale-Source-Wvb-Demo.wvproj"
    set "ProjectBytes=649"
    set "ProjectSha256=7e595320777792b842d230b0033baab519f9d277719f0efdfb24edb3e55fb697"
)
call :verify_file "%CompilerWvb%" 914746 48ff781359d9bab96ec3e19e4edba19a26ba82552d5bfd1c1a72d64b75f224a6 "native compiler seed WVB"
if errorlevel 1 exit /b 1
call :verify_file "%Compiler%" 27467776 344940f66b26b516b8b4e10a712a6b2c01cbff95aa7ff18aac0789ba9197f970 "Windows native compiler seed"
if errorlevel 1 exit /b 1
call :verify_file "%Publisher%" 1371136 b9fd1b11bc1e4a726e4a43b16830a9351fe573b30e547ba8d8f6660f688ed421 "Windows native publisher"
if errorlevel 1 exit /b 1
call :verify_file "%Project%" %ProjectBytes% %ProjectSha256% "source compiler product manifest"
if errorlevel 1 exit /b 1

:allocate
set "TemporaryDirectory=%TEMP%\windvale-source-compiler-product-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "Candidate=%TemporaryDirectory%\Candidate.wvb"

if /I "%Product%"=="core" goto :compile_core
goto :compile_demo

:compile_core
"%Compiler%" ^
    "%RepositoryRoot%\Compiler\Windvale\Source-Wvb-Core.wv" ^
    "%RepositoryRoot%\Compiler\Windvale\Source-Bindings-Core.wv" ^
    "%RepositoryRoot%\Compiler\Windvale\Source-Body-Parser.wv" ^
    "%RepositoryRoot%\Compiler\Windvale\Source-Declaration-Parser.wv" ^
    "%RepositoryRoot%\Compiler\Windvale\Source-Graph-Core.wv" ^
    "%RepositoryRoot%\Compiler\Windvale\Source-Lexer-Core.wv" ^
    "%RepositoryRoot%\Compiler\Windvale\Source-Set-Core.wv" ^
    "%RepositoryRoot%\Compiler\Windvale\Source-Symbols-Core.wv" ^
    "%RepositoryRoot%\Compiler\Windvale\Source-Wir-Core.wv" ^
    "%RepositoryRoot%\Compiler\Windvale\Source-Wvb-Temporary-Slots.wv" ^
    "%RepositoryRoot%\Foundation\Byte-Construction.wv" ^
    "%RepositoryRoot%\Foundation\Decimal-Parsing.wv" ^
    "%Candidate%"
goto :publish

:compile_demo
"%Compiler%" ^
    "%RepositoryRoot%\Examples\Compiler\Source-Wvb-Demo.wv" ^
    "%RepositoryRoot%\Compiler\Windvale\Source-Bindings-Core.wv" ^
    "%RepositoryRoot%\Compiler\Windvale\Source-Body-Parser.wv" ^
    "%RepositoryRoot%\Compiler\Windvale\Source-Declaration-Parser.wv" ^
    "%RepositoryRoot%\Compiler\Windvale\Source-Graph-Core.wv" ^
    "%RepositoryRoot%\Compiler\Windvale\Source-Lexer-Core.wv" ^
    "%RepositoryRoot%\Compiler\Windvale\Source-Set-Core.wv" ^
    "%RepositoryRoot%\Compiler\Windvale\Source-Symbols-Core.wv" ^
    "%RepositoryRoot%\Compiler\Windvale\Source-Wir-Core.wv" ^
    "%RepositoryRoot%\Compiler\Windvale\Source-Wvb-Core.wv" ^
    "%RepositoryRoot%\Compiler\Windvale\Source-Wvb-Temporary-Slots.wv" ^
    "%RepositoryRoot%\Foundation\Byte-Construction.wv" ^
    "%RepositoryRoot%\Foundation\Decimal-Parsing.wv" ^
    "%Candidate%"
goto :publish

:publish
set "Result=%ERRORLEVEL%"
if "%Result%"=="0" (
    "%Publisher%" "%Candidate%" "%Output%"
    set "Result=%ERRORLEVEL%"
)
if exist "%Candidate%" del /f /q "%Candidate%" >nul 2>nul
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
>&2 echo Usage: Tools\Native\Build-Source-Compiler-Product.cmd ^<core^|demo^> ^<output.wvb^>
exit /b 64
