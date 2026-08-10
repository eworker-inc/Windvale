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
set "ObjectPrefix=%TemporaryDirectory%\Object"
set "ObjectManifest=%TemporaryDirectory%\Object.wvop"
set "ImagePrefix=%TemporaryDirectory%\Image"
set "ImageManifest=%TemporaryDirectory%\Image.wvli"
set "CanonicalPrefix=%TemporaryDirectory%\Canonical"
set "CanonicalManifest=%TemporaryDirectory%\Canonical.wvli"

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Windvale-Native-X64-Lowering-Tool.wvproj" ^
    "%LowererWvb%" >"%TemporaryDirectory%\Build-Lowerer.txt" 2>"%TemporaryDirectory%\Build-Lowerer.err"
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Windvale-Native-Test-Wvb-To-Wvo-Return-42.wvproj" ^
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

call :verify_file "%LowererWvb%" 414298 2d2c5cc91a13603d71bcd72786ae89e3e4afbaf19148fc6170a66df33c33ebef "WVB-to-WVO tool WVB"
if errorlevel 1 goto :cleanup
call :verify_file "%WindowsApplication%" 5972480 be6bfb487f00dcc9f8c785dfe05832263d5899bf2f6fca3f77edf163b51deac7 "Windows WVB-to-WVO application"
if errorlevel 1 goto :cleanup
call :verify_file "%LinuxApplication%" 5971968 09cb247bb427b3b40305d068ec798b1086c2255c3db823036d47e9f620091dd2 "Linux WVB-to-WVO application"
if errorlevel 1 goto :cleanup
call :verify_file "%ReturnWvb%" 174 7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31 "Return-42 WVB"
if errorlevel 1 goto :cleanup
call :verify_file "%ReturnWvo%" 479 0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5 "Return-42 WVO"
if errorlevel 1 goto :cleanup

echo native WVB-to-WVO reconstruction status=Complete artifacts=5
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
if exist "%TemporaryDirectory%\." rmdir /s /q "%TemporaryDirectory%"
exit /b %Result%

:usage
>&2 echo Usage: Tools\Native\Construct-Wvb-To-Wvo-Reconstruction.cmd ^<existing-separate-output-directory^>
exit /b 64
