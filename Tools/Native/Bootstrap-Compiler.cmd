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
set "Project=%SourceRoot%\Windvale-Compiler.wvproj"

call :verify_file "%CompilerWvb%" 914746 48ff781359d9bab96ec3e19e4edba19a26ba82552d5bfd1c1a72d64b75f224a6 "native compiler seed WVB"
if errorlevel 1 exit /b 1
call :verify_file "%Compiler%" 27467776 344940f66b26b516b8b4e10a712a6b2c01cbff95aa7ff18aac0789ba9197f970 "Windows native compiler seed"
if errorlevel 1 exit /b 1
call :verify_file "%Publisher%" 1121792 f2502ecf9143cfa1343c5f5cb1de066bdf1f82f0e4782afae178f11c41afd735 "Windows native publisher"
if errorlevel 1 exit /b 1
call :verify_file "%Project%" 649 e097e9d007909a3cf17476ccfce41ace5fa89c566386d15ae24c7d91d9f91e7b "compiler project manifest"
if errorlevel 1 exit /b 1

:allocate
set "TemporaryDirectory=%TEMP%\windvale-native-compiler-bootstrap-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "Candidate=%TemporaryDirectory%\Candidate.wvb"

"%Compiler%" ^
    "%SourceRoot%\Examples\Compiler\Source-Wvb-Tool.wv" ^
    "%SourceRoot%\Compiler\Windvale\Source-Bindings-Core.wv" ^
    "%SourceRoot%\Compiler\Windvale\Source-Body-Parser.wv" ^
    "%SourceRoot%\Compiler\Windvale\Source-Declaration-Parser.wv" ^
    "%SourceRoot%\Compiler\Windvale\Source-Graph-Core.wv" ^
    "%SourceRoot%\Compiler\Windvale\Source-Lexer-Core.wv" ^
    "%SourceRoot%\Compiler\Windvale\Source-Set-Core.wv" ^
    "%SourceRoot%\Compiler\Windvale\Source-Symbols-Core.wv" ^
    "%SourceRoot%\Compiler\Windvale\Source-Wir-Core.wv" ^
    "%SourceRoot%\Compiler\Windvale\Source-Wvb-Core.wv" ^
    "%SourceRoot%\Compiler\Windvale\Source-Wvb-Temporary-Slots.wv" ^
    "%SourceRoot%\Foundation\Byte-Construction.wv" ^
    "%SourceRoot%\Foundation\Decimal-Parsing.wv" ^
    "%Candidate%"
set "Result=%ERRORLEVEL%"
if not "%Result%"=="0" goto :cleanup

call :verify_file "%Candidate%" 921900 fd96bd567d08a18107a9b149560ce9f2e38b49454250e934a4375f465d132556 "bootstrapped compiler WVB"
if errorlevel 1 (
    set "Result=1"
    goto :cleanup
)

"%Publisher%" "%Candidate%" "%OutputPath%"
set "Result=%ERRORLEVEL%"

:cleanup
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
>&2 echo Usage: Tools\Native\Bootstrap-Compiler.cmd ^<artifact-root^> ^<source-root^> ^<output.wvb^>
exit /b 64
