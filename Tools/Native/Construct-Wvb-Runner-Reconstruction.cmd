@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if not "%~2"=="" goto :usage
if not exist "%~f1\." goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "OutputRoot=%~f1"
set "CandidateRoot=%RepositoryRoot%\Artifacts\Native-Wvb-Runner-Candidate"
if /I "%OutputRoot%"=="%CandidateRoot%" (
    >&2 echo The WVB-runner reconstruction must use a separate output directory.
    exit /b 64
)
fsutil reparsepoint query "%OutputRoot%" >nul 2>nul
if not errorlevel 1 (
    >&2 echo The WVB-runner reconstruction output directory must not be a reparse point.
    exit /b 64
)

set "SourceProject=%RepositoryRoot%\Projects\Tools\Windvale-Wvb-Runner.wvproj"

:allocate
set "TemporaryDirectory=%TEMP%\windvale-wvb-runner-reconstruction-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "Result=1"

set "Wvb=%OutputRoot%\Wvb-Runner.wvb"
set "ObjectPrefix=%TemporaryDirectory%\Object"
set "ObjectManifest=%TemporaryDirectory%\Object.wvop"
set "ImagePrefix=%TemporaryDirectory%\Image"
set "ImageManifest=%TemporaryDirectory%\Image.wvli"
set "CanonicalPrefix=%TemporaryDirectory%\Canonical"
set "CanonicalManifest=%TemporaryDirectory%\Canonical.wvli"
set "WindowsApplication=%OutputRoot%\windows-x64-wvrun.exe"
set "LinuxApplication=%OutputRoot%\linux-x64-wvrun.elf"

node "%RepositoryRoot%\Tools\Native\Build-Current-Split-Project-Wvb.mjs" "%SourceProject%" "%Wvb%"
if errorlevel 1 goto :cleanup
call :verify_file "%Wvb%" 446532 56b208d1f892f4bdd1d9c309bb6d4d46257d533a76d79d22efc8f83f27896fbe "WVB-runner module"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Stage-Compiler-Wvb.cmd" "%Wvb%" "%ObjectPrefix%" "%ObjectManifest%" >"%TemporaryDirectory%\Stage.out" 2>"%TemporaryDirectory%\Stage.err"
if errorlevel 1 goto :stage_failed
findstr /c:"object-bytes=5357511 chunks=12 manifest-bytes=168" "%TemporaryDirectory%\Stage.out" >nul
if errorlevel 1 goto :stage_failed

call "%RepositoryRoot%\Tools\Native\Link-Staged-Compiler-Wvo.cmd" "%ObjectPrefix%" "%ObjectManifest%" "%ImagePrefix%" "%ImageManifest%" >"%TemporaryDirectory%\Link.out" 2>"%TemporaryDirectory%\Link.err"
if errorlevel 1 goto :link_failed
findstr /c:"image-bytes=5348533 entry-offset=105270 chunks=8 manifest-bytes=124" "%TemporaryDirectory%\Link.out" >nul
if errorlevel 1 goto :link_failed

call "%RepositoryRoot%\Tools\Native\Transport-Compiler-Image.cmd" "%ImagePrefix%" "%ImageManifest%" "%CanonicalPrefix%" "%CanonicalManifest%" >"%TemporaryDirectory%\Transport.out" 2>"%TemporaryDirectory%\Transport.err"
if errorlevel 1 goto :transport_failed
findstr /c:"image-bytes=5348533 entry-offset=105270 chunks=2 manifest-bytes=52" "%TemporaryDirectory%\Transport.out" >nul
if errorlevel 1 goto :transport_failed

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 5 "%Wvb%" "%CanonicalPrefix%" 2 105270 "%WindowsApplication%" windows >"%TemporaryDirectory%\Windows-Package.out" 2>"%TemporaryDirectory%\Windows-Package.err"
if errorlevel 1 goto :windows_package_failed
call :verify_file "%WindowsApplication%" 5366784 063de8f1fadcf9c37e9cef6526d628b410fa0cd21067fe6f3c795b97623cb519 "Windows WVB-runner application"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 5 "%Wvb%" "%CanonicalPrefix%" 2 105270 "%LinuxApplication%" linux >"%TemporaryDirectory%\Linux-Package.out" 2>"%TemporaryDirectory%\Linux-Package.err"
if errorlevel 1 goto :linux_package_failed
call :verify_file "%LinuxApplication%" 5365760 6e18c9c9480df40814b81244b3dcd039c8851ded646a240134d4e2969b9c2e71 "Linux WVB-runner application"
if errorlevel 1 goto :cleanup

echo native WVB runner reconstruction status=Complete artifacts=3
set "Result=0"
goto :cleanup

:stage_failed
type "%TemporaryDirectory%\Stage.out" >&2
type "%TemporaryDirectory%\Stage.err" >&2
goto :cleanup
:link_failed
type "%TemporaryDirectory%\Link.out" >&2
type "%TemporaryDirectory%\Link.err" >&2
goto :cleanup
:transport_failed
type "%TemporaryDirectory%\Transport.out" >&2
type "%TemporaryDirectory%\Transport.err" >&2
goto :cleanup
:windows_package_failed
type "%TemporaryDirectory%\Windows-Package.out" >&2
type "%TemporaryDirectory%\Windows-Package.err" >&2
goto :cleanup
:linux_package_failed
type "%TemporaryDirectory%\Linux-Package.out" >&2
type "%TemporaryDirectory%\Linux-Package.err" >&2

:cleanup
if exist "%TemporaryDirectory%\." rmdir /s /q "%TemporaryDirectory%"
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
>&2 echo Usage: Tools\Native\Construct-Wvb-Runner-Reconstruction.cmd ^<existing-output-directory^>
exit /b 64
