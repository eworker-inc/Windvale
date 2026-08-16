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
set "DriverWvb=%WvbDirectory%\Compiler-Build-Driver.wvb"
set "DriverWindows=%WindowsDirectory%\wvbuild.exe"
set "DriverLinux=%LinuxDirectory%\wvbuild.elf"
set "FrozenBuildDriver=%RepositoryRoot%\Artifacts\Native-Front-Door\windows-x64\wvbuild.exe"
set "Workspace=%RepositoryRoot%\Windvale.wvws"
set "DriverProject=%RepositoryRoot%\Projects\Tools\Windvale-Compiler-Build-Driver.wvproj"
set "WorkspaceResource=%Workspace:\=/%"
set "DriverProjectResource=%DriverProject:\=/%"
set "DriverWvbResource=%DriverWvb:\=/%"
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

"%FrozenBuildDriver%" --workspace "%WorkspaceResource%" --project ^
    "%DriverProjectResource%" "%DriverWvbResource%" ^
    >"%TemporaryDirectory%\Driver-Build.txt" 2>"%TemporaryDirectory%\Driver-Build.err"
if errorlevel 1 goto :cleanup
set "DriverObjectPrefix=%TemporaryDirectory%\Driver-Object"
set "DriverObjectManifest=%TemporaryDirectory%\Driver-Object.wvop"
set "DriverImagePrefix=%TemporaryDirectory%\Driver-Image"
set "DriverImageManifest=%TemporaryDirectory%\Driver-Image.wvli"
set "DriverCanonicalPrefix=%TemporaryDirectory%\Driver-Canonical"
set "DriverCanonicalManifest=%TemporaryDirectory%\Driver-Canonical.wvli"
call "%RepositoryRoot%\Tools\Native\Stage-Compiler-Wvb.cmd" ^
    "%DriverWvb%" "%DriverObjectPrefix%" "%DriverObjectManifest%" ^
    >"%TemporaryDirectory%\Driver-Stage.txt" 2>"%TemporaryDirectory%\Driver-Stage.err"
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Staged-Compiler-Wvo.cmd" ^
    "%DriverObjectPrefix%" "%DriverObjectManifest%" ^
    "%DriverImagePrefix%" "%DriverImageManifest%" ^
    >"%TemporaryDirectory%\Driver-Link.txt" 2>"%TemporaryDirectory%\Driver-Link.err"
if errorlevel 1 goto :cleanup
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

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 2 ^
    "%DriverWvb%" "%DriverCanonicalPrefix%" %DriverFragmentCount% %DriverNativeEntry% ^
    "%DriverWindows%" windows ^
    >"%TemporaryDirectory%\Driver-Windows.txt" 2>"%TemporaryDirectory%\Driver-Windows.err"
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 2 ^
    "%DriverWvb%" "%DriverCanonicalPrefix%" %DriverFragmentCount% %DriverNativeEntry% ^
    "%DriverLinux%" linux ^
    >"%TemporaryDirectory%\Driver-Linux.txt" 2>"%TemporaryDirectory%\Driver-Linux.err"
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 1 ^
    "%Wvb%" "%CanonicalPrefix%" %FragmentCount% %NativeEntry% "%Linux%" linux ^
    >"%TemporaryDirectory%\Linux.txt" 2>"%TemporaryDirectory%\Linux.err"
if errorlevel 1 goto :cleanup

call :verify_file "%Wvb%" 931035 13558d9dbc0d185b161b770824aa29ff90b8873903b2b5d7184a23950a6fc1e4 "current compiler WVB"
if errorlevel 1 goto :cleanup
call :verify_file "%Windows%" 27898368 4009e6747bbf9a6d2b0b2ec90e2368ca50fda863d445534f15ef96e22a657b34 "Windows current compiler"
if errorlevel 1 goto :cleanup
call :verify_file "%Linux%" 27897856 c266adf20fe2927a446483f68880ef323c480f011b0c26384716ea2f651bcd65 "Linux current compiler"
if errorlevel 1 goto :cleanup
call :verify_file "%DriverWvb%" 1162338 a214662da422443cd70c4be12c8f0bd06cbb5bce9fe3a56e2a52c46a37445a20 "current compiler build-driver WVB"
if errorlevel 1 goto :cleanup
call :verify_file "%DriverWindows%" 30381568 b0d58f4d8d6d32e09d45358035ce4521410ec1280f11fa72b66245e621ba49a3 "Windows current compiler build driver"
if errorlevel 1 goto :cleanup
call :verify_file "%DriverLinux%" 30380032 b4fdc30a7242e03f7166491bf6b415aa3b5dce8ff0e16444f6ccb24c5bcb03a0 "Linux current compiler build driver"
if errorlevel 1 goto :cleanup

echo native compiler reconstruction status=Complete compiler-bytes=931035 native-bytes=27867015 entry-offset=51356 chunks=7 build-driver-bytes=1162338 build-driver-entry-offset=220460 build-driver-chunks=8
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
