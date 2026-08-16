@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if not "%~2"=="" goto :usage
if not exist "%~f1\." goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "OutputRoot=%~f1"
set "WvbDirectory=%OutputRoot%\Wvb"
set "WindowsDirectory=%OutputRoot%\windows-x64"
set "LinuxDirectory=%OutputRoot%\linux-x64"
if not exist "%WvbDirectory%\." mkdir "%WvbDirectory%" || exit /b 1
if not exist "%WindowsDirectory%\." mkdir "%WindowsDirectory%" || exit /b 1
if not exist "%LinuxDirectory%\." mkdir "%LinuxDirectory%" || exit /b 1

:allocate
set "TemporaryDirectory=%TEMP%\windvale-compiler-reconstruction-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "Wvb=%WvbDirectory%\Windvale-Compiler.wvb"
set "Windows=%WindowsDirectory%\wvcompiler.exe"
set "Linux=%LinuxDirectory%\wvcompiler.elf"
set "ObjectPrefix=%TemporaryDirectory%\Object"
set "ObjectManifest=%TemporaryDirectory%\Object.wvop"
set "ImagePrefix=%TemporaryDirectory%\Image"
set "ImageManifest=%TemporaryDirectory%\Image.wvli"
set "CanonicalPrefix=%TemporaryDirectory%\Canonical"
set "CanonicalManifest=%TemporaryDirectory%\Canonical.wvli"
set "Result=1"

call "%RepositoryRoot%\Tools\Native\Bootstrap-Compiler.cmd" ^
    "%RepositoryRoot%\Artifacts" "%RepositoryRoot%" "%Wvb%" ^
    >"%TemporaryDirectory%\Bootstrap.txt" 2>"%TemporaryDirectory%\Bootstrap.err"
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Stage-Compiler-Wvb.cmd" ^
    "%Wvb%" "%ObjectPrefix%" "%ObjectManifest%" ^
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
if not "%NativeEntry%"=="51356" goto :cleanup
if not "%FragmentCount%"=="7" goto :cleanup

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 1 ^
    "%Wvb%" "%CanonicalPrefix%" %FragmentCount% %NativeEntry% "%Windows%" windows ^
    >"%TemporaryDirectory%\Windows.txt" 2>"%TemporaryDirectory%\Windows.err"
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 1 ^
    "%Wvb%" "%CanonicalPrefix%" %FragmentCount% %NativeEntry% "%Linux%" linux ^
    >"%TemporaryDirectory%\Linux.txt" 2>"%TemporaryDirectory%\Linux.err"
if errorlevel 1 goto :cleanup

call :verify_file "%Wvb%" 923818 49b5cbf040de4bcb22c071a5da9a4fbad47f4f0658ef910957a67b52c07607c2 "current compiler WVB"
if errorlevel 1 goto :cleanup
call :verify_file "%Windows%" 27678720 6f266759e2d2524ad9ce2045cb21243538efc7bce35ab1f94a7da4009865eac8 "Windows current compiler"
if errorlevel 1 goto :cleanup
call :verify_file "%Linux%" 27680768 7a81bc84a433bec0b2dcebd1ec3be82de120b11427687b9926ec13592231dc37 "Linux current compiler"
if errorlevel 1 goto :cleanup

echo native compiler reconstruction status=Complete compiler-bytes=923818 native-bytes=27647511 entry-offset=51356 chunks=7
set "Result=0"

:cleanup
del /f /q "%TemporaryDirectory%\*" >nul 2>nul
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b %Result%

:verify_file
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~3" >nul
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Construct-Compiler-Reconstruction.cmd ^<existing-output-directory^>
exit /b 64
