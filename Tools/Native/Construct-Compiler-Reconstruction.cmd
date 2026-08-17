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
set "BuildCompiler=%TemporaryDirectory%\wvcompiler-build.exe"
set "DriverWvb=%WvbDirectory%\Compiler-Build-Driver.wvb"
set "DriverWindows=%WindowsDirectory%\wvbuild.exe"
set "DriverLinux=%LinuxDirectory%\wvbuild.elf"
set "ObjectPrefix=%TemporaryDirectory%\Object"
set "ObjectManifest=%TemporaryDirectory%\Object.wvop"
set "ImagePrefix=%TemporaryDirectory%\Image"
set "ImageManifest=%TemporaryDirectory%\Image.wvli"
set "CanonicalPrefix=%TemporaryDirectory%\Canonical"
set "CanonicalManifest=%TemporaryDirectory%\Canonical.wvli"
set "Result=1"

echo native compiler reconstruction step=bootstrap item=1/14
call "%RepositoryRoot%\Tools\Native\Bootstrap-Compiler.cmd" ^
    "%RepositoryRoot%\Artifacts" "%RepositoryRoot%" "%Wvb%" ^
    >"%TemporaryDirectory%\Bootstrap.txt" 2>"%TemporaryDirectory%\Bootstrap.err"
if errorlevel 1 goto :cleanup
echo native compiler reconstruction step=stage-compiler item=2/14
call "%RepositoryRoot%\Tools\Native\Stage-Compiler-Wvb.cmd" ^
    "%Wvb%" "%ObjectPrefix%" "%ObjectManifest%" ^
    >"%TemporaryDirectory%\Stage.txt" 2>"%TemporaryDirectory%\Stage.err"
if errorlevel 1 goto :cleanup
echo native compiler reconstruction step=link-compiler item=3/14
call "%RepositoryRoot%\Tools\Native\Link-Staged-Compiler-Wvo.cmd" ^
    "%ObjectPrefix%" "%ObjectManifest%" "%ImagePrefix%" "%ImageManifest%" ^
    >"%TemporaryDirectory%\Link.txt" 2>"%TemporaryDirectory%\Link.err"
if errorlevel 1 goto :cleanup
echo native compiler reconstruction step=transport-compiler item=4/14
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

echo native compiler reconstruction step=package-compiler-windows item=5/14
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 1 ^
    "%Wvb%" "%CanonicalPrefix%" %FragmentCount% %NativeEntry% "%Windows%" windows ^
    >"%TemporaryDirectory%\Windows.txt" 2>"%TemporaryDirectory%\Windows.err"
if errorlevel 1 goto :cleanup

echo native compiler reconstruction step=package-build-compiler item=6/14
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 2 ^
    "%Wvb%" "%CanonicalPrefix%" %FragmentCount% %NativeEntry% "%BuildCompiler%" windows ^
    >"%TemporaryDirectory%\Build-Compiler.txt" 2>"%TemporaryDirectory%\Build-Compiler.err"
if errorlevel 1 goto :cleanup
echo native compiler reconstruction step=compile-build-driver item=7/14
call "%RepositoryRoot%\Tools\Native\Compile-Compiler-Build-Driver-Source-Set.cmd" ^
    "%BuildCompiler%" "%RepositoryRoot%" "%DriverWvb%" ^
    >"%TemporaryDirectory%\Driver-Build.txt" 2>"%TemporaryDirectory%\Driver-Build.err"
if errorlevel 1 goto :cleanup
set "DriverObjectPrefix=%TemporaryDirectory%\Driver-Object"
set "DriverObjectManifest=%TemporaryDirectory%\Driver-Object.wvop"
set "DriverImagePrefix=%TemporaryDirectory%\Driver-Image"
set "DriverImageManifest=%TemporaryDirectory%\Driver-Image.wvli"
set "DriverCanonicalPrefix=%TemporaryDirectory%\Driver-Canonical"
set "DriverCanonicalManifest=%TemporaryDirectory%\Driver-Canonical.wvli"
echo native compiler reconstruction step=stage-build-driver item=8/14
call "%RepositoryRoot%\Tools\Native\Stage-Compiler-Wvb.cmd" ^
    "%DriverWvb%" "%DriverObjectPrefix%" "%DriverObjectManifest%" ^
    >"%TemporaryDirectory%\Driver-Stage.txt" 2>"%TemporaryDirectory%\Driver-Stage.err"
if errorlevel 1 goto :cleanup
echo native compiler reconstruction step=link-build-driver item=9/14
call "%RepositoryRoot%\Tools\Native\Link-Staged-Compiler-Wvo.cmd" ^
    "%DriverObjectPrefix%" "%DriverObjectManifest%" ^
    "%DriverImagePrefix%" "%DriverImageManifest%" ^
    >"%TemporaryDirectory%\Driver-Link.txt" 2>"%TemporaryDirectory%\Driver-Link.err"
if errorlevel 1 goto :cleanup
echo native compiler reconstruction step=transport-build-driver item=10/14
call "%RepositoryRoot%\Tools\Native\Transport-Compiler-Image.cmd" ^
    "%DriverImagePrefix%" "%DriverImageManifest%" ^
    "%DriverCanonicalPrefix%" "%DriverCanonicalManifest%" ^
    >"%TemporaryDirectory%\Driver-Transport.txt" 2>"%TemporaryDirectory%\Driver-Transport.err"
if errorlevel 1 goto :cleanup

set "DriverNativeEntry="
set "DriverFragmentCount="
for /f "tokens=9,11 delims== " %%E in ('findstr /b /c:"compiler image transport status=Complete " "%TemporaryDirectory%\Driver-Transport.txt"') do (
    set "DriverNativeEntry=%%E"
    set "DriverFragmentCount=%%F"
)
if not "%DriverNativeEntry%"=="220460" goto :cleanup
if not "%DriverFragmentCount%"=="8" goto :cleanup

echo native compiler reconstruction step=package-build-driver-windows item=11/14
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 2 ^
    "%DriverWvb%" "%DriverCanonicalPrefix%" %DriverFragmentCount% %DriverNativeEntry% ^
    "%DriverWindows%" windows ^
    >"%TemporaryDirectory%\Driver-Windows.txt" 2>"%TemporaryDirectory%\Driver-Windows.err"
if errorlevel 1 goto :cleanup
echo native compiler reconstruction step=package-build-driver-linux item=12/14
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 2 ^
    "%DriverWvb%" "%DriverCanonicalPrefix%" %DriverFragmentCount% %DriverNativeEntry% ^
    "%DriverLinux%" linux ^
    >"%TemporaryDirectory%\Driver-Linux.txt" 2>"%TemporaryDirectory%\Driver-Linux.err"
if errorlevel 1 goto :cleanup
echo native compiler reconstruction step=package-compiler-linux item=13/14
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 1 ^
    "%Wvb%" "%CanonicalPrefix%" %FragmentCount% %NativeEntry% "%Linux%" linux ^
    >"%TemporaryDirectory%\Linux.txt" 2>"%TemporaryDirectory%\Linux.err"
if errorlevel 1 goto :cleanup

echo native compiler reconstruction step=verify-identities item=14/14
call :verify_file "%Wvb%" 935163 a7d47b2de29faee089c7a22ef23eac4657f719331dc02044eb2d818457dac5b6 "current compiler WVB"
if errorlevel 1 goto :cleanup
call :verify_file "%Windows%" 28172800 a5db938a814471fdacda75efcf57d28934ae52b3b2290732627c14ba173fd70d "Windows current compiler"
if errorlevel 1 goto :cleanup
call :verify_file "%Linux%" 28172288 da11ab3b70b428087cbcb9de5614a2dbdccd31afc6861cc15881fd65c12ff19b "Linux current compiler"
if errorlevel 1 goto :cleanup
call :verify_file "%DriverWvb%" 1142818 125d2b4080889615877d843a36b2f9f6b50d049d011cc06fa8ab426ab83c0574 "current compiler build-driver WVB"
if errorlevel 1 goto :cleanup
call :verify_file "%DriverWindows%" 30071296 f556f0e2c794d9424cbcd9f5e3f8e5aee54f49373c7c18ea1d4829facea7dc6f "Windows current compiler build driver"
if errorlevel 1 goto :cleanup
call :verify_file "%DriverLinux%" 30072832 628fd60ea702c4a3b3ffb01d32cba7ba9708477acccf190cc6506a56f159d7a9 "Linux current compiler build driver"
if errorlevel 1 goto :cleanup

echo native compiler reconstruction status=Complete compiler-bytes=935163 native-bytes=28141686 entry-offset=51356 chunks=7 build-driver-bytes=1142818 build-driver-entry-offset=220460 build-driver-chunks=8
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
