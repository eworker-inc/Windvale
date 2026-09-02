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
call :verify_file "%Wvb%" 993328 2e7f5390c95e74be2abb06c2b2cbb84d789c3d449a7577c40f9de45157a874a6 "WVB-runner module"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Stage-Compiler-Wvb.cmd" "%Wvb%" "%ObjectPrefix%" "%ObjectManifest%" >"%TemporaryDirectory%\Stage.out" 2>"%TemporaryDirectory%\Stage.err"
if errorlevel 1 goto :stage_failed
findstr /c:"object-bytes=10126504 chunks=15 manifest-bytes=204" "%TemporaryDirectory%\Stage.out" >nul
if errorlevel 1 goto :stage_failed

call "%RepositoryRoot%\Tools\Native\Link-Staged-Compiler-Wvo.cmd" "%ObjectPrefix%" "%ObjectManifest%" "%ImagePrefix%" "%ImageManifest%" >"%TemporaryDirectory%\Link.out" 2>"%TemporaryDirectory%\Link.err"
if errorlevel 1 goto :link_failed
findstr /c:"image-bytes=10109020 entry-offset=150541 chunks=11 manifest-bytes=160" "%TemporaryDirectory%\Link.out" >nul
if errorlevel 1 goto :link_failed

call "%RepositoryRoot%\Tools\Native\Transport-Compiler-Image.cmd" "%ImagePrefix%" "%ImageManifest%" "%CanonicalPrefix%" "%CanonicalManifest%" >"%TemporaryDirectory%\Transport.out" 2>"%TemporaryDirectory%\Transport.err"
if errorlevel 1 goto :transport_failed
findstr /c:"image-bytes=10109020 entry-offset=150541 chunks=3 manifest-bytes=64" "%TemporaryDirectory%\Transport.out" >nul
if errorlevel 1 goto :transport_failed

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 5 "%Wvb%" "%CanonicalPrefix%" 3 150541 "%WindowsApplication%" windows >"%TemporaryDirectory%\Windows-Package.out" 2>"%TemporaryDirectory%\Windows-Package.err"
if errorlevel 1 goto :windows_package_failed
call :verify_file "%WindowsApplication%" 10127360 c7e7a917622698a511ebb8b478c8075d943feaf987d0aae56c9b7c8cab21c5e4 "Windows WVB-runner application"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 5 "%Wvb%" "%CanonicalPrefix%" 3 150541 "%LinuxApplication%" linux >"%TemporaryDirectory%\Linux-Package.out" 2>"%TemporaryDirectory%\Linux-Package.err"
if errorlevel 1 goto :linux_package_failed
call :verify_file "%LinuxApplication%" 10129408 c5db1a90ce58f4807de13ca0082014e9ca09634a9ef487859166f15443e7149d "Linux WVB-runner application"
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
