@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if not "%~2"=="" goto :usage
if not exist "%~f1\." goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "OutputRoot=%~f1"
set "CandidateRoot=%RepositoryRoot%\Artifacts\Native-Wvb-To-Wvo-Candidate"
if /I "%OutputRoot%"=="%CandidateRoot%" (
    >&2 echo The WVB-to-WVO reconstruction must use a separate output directory.
    exit /b 64
)
fsutil reparsepoint query "%OutputRoot%" >nul 2>nul
if not errorlevel 1 (
    >&2 echo The WVB-to-WVO reconstruction output directory must not be a reparse point.
    exit /b 64
)

:allocate
set "TemporaryDirectory=%TEMP%\windvale-wvb-to-wvo-reconstruction-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "Result=1"

set "LowererWvb=%OutputRoot%\Wvb-To-Wvo.wvb"
set "WindowsApplication=%OutputRoot%\Wvb-To-Wvo.exe"
set "LinuxApplication=%OutputRoot%\Wvb-To-Wvo.elf"
set "ReturnWvb=%OutputRoot%\Return-42.wvb"
set "ReturnWvo=%OutputRoot%\Return-42.wvo"
set "MetadataWvb=%OutputRoot%\Metadata.wvb"
set "MetadataWvo=%OutputRoot%\Metadata.wvo"
set "MetadataTestWvb=%TemporaryDirectory%\Metadata-Self-Test.wvb"
set "ObjectPrefix=%TemporaryDirectory%\Object"
set "ObjectManifest=%TemporaryDirectory%\Object.wvop"
set "ImagePrefix=%TemporaryDirectory%\Image"
set "ImageManifest=%TemporaryDirectory%\Image.wvli"
set "CanonicalPrefix=%TemporaryDirectory%\Canonical"
set "CanonicalManifest=%TemporaryDirectory%\Canonical.wvli"

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects/Compiler/Windvale-Native-X64-Lowering-Tool.wvproj" ^
    "%LowererWvb%" >"%TemporaryDirectory%\Build-Lowerer.txt" 2>"%TemporaryDirectory%\Build-Lowerer.err"
if errorlevel 1 goto :cleanup
"%RepositoryRoot%\Artifacts\Native-Compiler-Seed\windows-x64\wvcompiler.exe" ^
    "%RepositoryRoot%\Tests\Fixtures\Native-X64\Wvb-To-Wvo-Metadata.wv" ^
    "%MetadataWvb%" >"%TemporaryDirectory%\Build-Metadata.txt" ^
    2>"%TemporaryDirectory%\Build-Metadata.err"
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-X64-Lowering-Metadata.wvproj" ^
    "%MetadataTestWvb%" >"%TemporaryDirectory%\Build-Metadata-Test.txt" ^
    2>"%TemporaryDirectory%\Build-Metadata-Test.err"
if errorlevel 1 (
    type "%TemporaryDirectory%\Build-Metadata-Test.txt" >&2
    type "%TemporaryDirectory%\Build-Metadata-Test.err" >&2
    goto :cleanup
)
call "%RepositoryRoot%\Tools\Native\Run-Wvb.cmd" "%MetadataTestWvb%" ^
    >"%TemporaryDirectory%\Run-Metadata-Test.txt" 2>"%TemporaryDirectory%\Run-Metadata-Test.err"
if errorlevel 1 (
    type "%TemporaryDirectory%\Run-Metadata-Test.txt" >&2
    type "%TemporaryDirectory%\Run-Metadata-Test.err" >&2
    goto :cleanup
)
set "MetadataTestResult="
for /f "usebackq delims=" %%L in ("%TemporaryDirectory%\Run-Metadata-Test.txt") do (
    if "%%L"=="Result: 0" set "MetadataTestResult=Valid"
)
if not defined MetadataTestResult goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects/Tests/Windvale-Native-Test-Wvb-To-Wvo-Return-42.wvproj" ^
    "%ReturnWvb%" >"%TemporaryDirectory%\Build-Return-42.txt" 2>"%TemporaryDirectory%\Build-Return-42.err"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Stage-Compiler-Wvb.cmd" ^
    "%LowererWvb%" "%ObjectPrefix%" "%ObjectManifest%" ^
    >"%TemporaryDirectory%\Stage.txt" 2>"%TemporaryDirectory%\Stage.err"
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Staged-Compiler-Wvo.cmd" ^
    "%ObjectPrefix%" "%ObjectManifest%" "%ImagePrefix%" "%ImageManifest%" ^
    >"%TemporaryDirectory%\Link.txt" 2>"%TemporaryDirectory%\Link.err"
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Transport-Compiler-Image.cmd" ^
    "%ImagePrefix%" "%ImageManifest%" "%CanonicalPrefix%" "%CanonicalManifest%" ^
    >"%TemporaryDirectory%\Transport.txt" 2>"%TemporaryDirectory%\Transport.err"
if errorlevel 1 goto :cleanup

set "NativeEntry="
set "FragmentCount="
for /f "tokens=9,11 delims== " %%E in ('findstr /b /c:"compiler image transport status=Complete " "%TemporaryDirectory%\Transport.txt"') do (
    set "NativeEntry=%%E"
    set "FragmentCount=%%F"
)
if not defined NativeEntry goto :cleanup
if not defined FragmentCount goto :cleanup
echo(%NativeEntry%| findstr /r /x "[0-9][0-9]*" >nul || goto :cleanup
echo(%FragmentCount%| findstr /r /x "[1-8]" >nul || goto :cleanup

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
    "%LowererWvb%" "%CanonicalPrefix%" %FragmentCount% %NativeEntry% ^
    "%WindowsApplication%" windows ^
    >"%TemporaryDirectory%\Windows.txt" 2>"%TemporaryDirectory%\Windows.err"
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
    "%LowererWvb%" "%CanonicalPrefix%" %FragmentCount% %NativeEntry% ^
    "%LinuxApplication%" linux ^
    >"%TemporaryDirectory%\Linux.txt" 2>"%TemporaryDirectory%\Linux.err"
if errorlevel 1 goto :cleanup

"%WindowsApplication%" "%ReturnWvb%" "%ReturnWvo%" ^
    >"%TemporaryDirectory%\Return-42.txt" 2>"%TemporaryDirectory%\Return-42.err"
if errorlevel 1 goto :cleanup
"%WindowsApplication%" "%MetadataWvb%" "%MetadataWvo%" ^
    >"%TemporaryDirectory%\Metadata.txt" 2>"%TemporaryDirectory%\Metadata.err"
if errorlevel 1 goto :cleanup

call :verify_file "%LowererWvb%" 747997 d5a514e72203ab530c6df6da8f444e6bd7f93130921e02042e70c7a7723942dc "WVB-to-WVO tool WVB"
if errorlevel 1 goto :cleanup
call :verify_file "%WindowsApplication%" 10661888 a46d73ada72fba9561e9db1fcfc5477bf19be2518ad9db2d8487184112923dfd "Windows WVB-to-WVO application"
if errorlevel 1 goto :cleanup
call :verify_file "%LinuxApplication%" 10661888 9c331308e5afe852d4c0441e22c1ff68a0ac0c86793c2e403f38556302c90fd3 "Linux WVB-to-WVO application"
if errorlevel 1 goto :cleanup
call :verify_file "%ReturnWvb%" 174 7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31 "Return-42 WVB"
if errorlevel 1 goto :cleanup
call :verify_file "%ReturnWvo%" 479 0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5 "Return-42 WVO"
if errorlevel 1 goto :cleanup
call :verify_file "%MetadataWvb%" 369 94b41f5016722c9e5bf16ace5ec933acc35c14efdd4e08fe11fd582a62b58ffa "metadata WVB"
if errorlevel 1 goto :cleanup
call :verify_file "%MetadataWvo%" 1151 6f1cb53ec55448a7552f2ff5b380446964d16ed32a60aa28b8e55a9ca590845d "metadata WVO"
if errorlevel 1 goto :cleanup

echo native WVB-to-WVO reconstruction status=Complete artifacts=7
set "Result=0"
goto :cleanup

:verify_file
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" (
    >&2 echo The %~4 identity is invalid.
    exit /b 1
)
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~3" >nul
if errorlevel 1 (
    >&2 echo The %~4 identity is invalid.
    exit /b 1
)
exit /b 0

:cleanup
if exist "%TemporaryDirectory%\." rmdir /s /q "%TemporaryDirectory%" >nul 2>nul
exit /b %Result%

:usage
>&2 echo Usage: Tools\Native\Construct-Wvb-To-Wvo-Reconstruction.cmd ^<existing-separate-output-directory^>
exit /b 64
