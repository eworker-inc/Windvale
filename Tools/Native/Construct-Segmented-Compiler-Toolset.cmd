@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if not "%~2"=="" goto :usage
if not exist "%~f1\." goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "OutputRoot=%~f1"
set "CandidateRoot=%RepositoryRoot%\Artifacts\Native-Segmented-Compiler-Toolset-Candidate"
if /I "%OutputRoot%"=="%CandidateRoot%" (
    >&2 echo The segmented compiler toolset must be constructed in a separate output directory.
    exit /b 64
)
fsutil reparsepoint query "%OutputRoot%" >nul 2>nul
if not errorlevel 1 (
    >&2 echo The segmented compiler toolset output directory must not be a reparse point.
    exit /b 64
)

:allocate
set "TemporaryDirectory=%TEMP%\windvale-segmented-toolset-construction-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "Result=1"

set "WvoStagingWvb=%OutputRoot%\Wvo-Staging-Producer.wvb"
set "ImageStagingWvb=%OutputRoot%\Compiler-Image-Staging.wvb"
set "TransportWvb=%OutputRoot%\Compiler-Image-Canonical-Transport.wvb"

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects/Compiler/Windvale-Native-X64-Lowering-Staging-Tool.wvproj" ^
    "%WvoStagingWvb%" >"%TemporaryDirectory%\Build-Wvo-Staging.txt" 2>"%TemporaryDirectory%\Build-Wvo-Staging.err"
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects/Linker/Windvale-Compiler-Image-Staging.wvproj" ^
    "%ImageStagingWvb%" >"%TemporaryDirectory%\Build-Image-Staging.txt" 2>"%TemporaryDirectory%\Build-Image-Staging.err"
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects/Linker/Windvale-Compiler-Image-Canonical-Transport.wvproj" ^
    "%TransportWvb%" >"%TemporaryDirectory%\Build-Transport.txt" 2>"%TemporaryDirectory%\Build-Transport.err"
if errorlevel 1 goto :cleanup

call :construct_pair Wvo-Staging "%WvoStagingWvb%" ^
    "%OutputRoot%\windows-x64-wvstage.exe" ^
    "%OutputRoot%\linux-x64-wvstage.elf"
if errorlevel 1 goto :cleanup
call :construct_pair Image-Staging "%ImageStagingWvb%" ^
    "%OutputRoot%\windows-x64-wvlinkstage.exe" ^
    "%OutputRoot%\linux-x64-wvlinkstage.elf"
if errorlevel 1 goto :cleanup
call :construct_pair Transport "%TransportWvb%" ^
    "%OutputRoot%\windows-x64-wvimagetransport.exe" ^
    "%OutputRoot%\linux-x64-wvimagetransport.elf"
if errorlevel 1 goto :cleanup

call :verify_file "%WvoStagingWvb%" 482611 4a79ffad86630a7bf1efed7f3c4c28f7d7586c0432bdb0c34a14c428d57a8ade "WVO staging producer WVB"
if errorlevel 1 goto :cleanup
call :verify_file "%OutputRoot%\windows-x64-wvstage.exe" 6934528 50ea8ba23182802f577b1adf3865950558c626865b45e212792fda44b358f0da "Windows WVO staging producer"
if errorlevel 1 goto :cleanup
call :verify_file "%OutputRoot%\linux-x64-wvstage.elf" 6934528 e147ec43acbaec07c88b7c549df1fc1cf4ca7d5fdc06a48865b31ec95110d92a "Linux WVO staging producer"
if errorlevel 1 goto :cleanup
call :verify_file "%ImageStagingWvb%" 75553 67a7b2142f5a95b5ce2e49b9c329ad7908d37418bc6cfd2b2b773c6b97b06265 "compiler-image staging WVB"
if errorlevel 1 goto :cleanup
call :verify_file "%OutputRoot%\windows-x64-wvlinkstage.exe" 852480 32fc318be24b6dcd7f67720098242872c3b2d2b960b7c75e7418a89f92b7bf43 "Windows compiler-image staging application"
if errorlevel 1 goto :cleanup
call :verify_file "%OutputRoot%\linux-x64-wvlinkstage.elf" 851968 baa183ff2318ace7e29d9aed39b1261d7887403674e52466efeb5fa12d88c8b8 "Linux compiler-image staging application"
if errorlevel 1 goto :cleanup
call :verify_file "%TransportWvb%" 23836 dc5f460ce89bcce2678092030376c8ddc928e682b263af2a73ba2a57034b6d4d "compiler-image transport WVB"
if errorlevel 1 goto :cleanup
call :verify_file "%OutputRoot%\windows-x64-wvimagetransport.exe" 269312 3d1479e286f3486c9ae4cc48a542fb7654cc8bca52ec240f8f3ee030e7c79d92 "Windows compiler-image transport application"
if errorlevel 1 goto :cleanup
call :verify_file "%OutputRoot%\linux-x64-wvimagetransport.elf" 270336 30386b1e571b5b444befbfb7c15ee9ce5cb30e7744cf84ddfee89cbf1e2e8108 "Linux compiler-image transport application"
if errorlevel 1 goto :cleanup

echo native segmented compiler toolset construction status=Complete artifacts=9
set "Result=0"
goto :cleanup

:construct_pair
set "ConstructionName=%~1"
set "ConstructionWvb=%~2"
set "ConstructionWindows=%~3"
set "ConstructionLinux=%~4"
set "WorkDirectory=%TemporaryDirectory%\%ConstructionName%"
mkdir "%WorkDirectory%" || exit /b 1
set "ObjectPrefix=%WorkDirectory%\Object"
set "ObjectManifest=%WorkDirectory%\Object.wvop"
set "ImagePrefix=%WorkDirectory%\Image"
set "ImageManifest=%WorkDirectory%\Image.wvli"
set "CanonicalPrefix=%WorkDirectory%\Canonical"
set "CanonicalManifest=%WorkDirectory%\Canonical.wvli"

call "%RepositoryRoot%\Tools\Native\Stage-Compiler-Wvb.cmd" ^
    "%ConstructionWvb%" "%ObjectPrefix%" "%ObjectManifest%" ^
    >"%WorkDirectory%\Stage.txt" 2>"%WorkDirectory%\Stage.err"
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Link-Staged-Compiler-Wvo.cmd" ^
    "%ObjectPrefix%" "%ObjectManifest%" "%ImagePrefix%" "%ImageManifest%" ^
    >"%WorkDirectory%\Link.txt" 2>"%WorkDirectory%\Link.err"
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Transport-Compiler-Image.cmd" ^
    "%ImagePrefix%" "%ImageManifest%" "%CanonicalPrefix%" "%CanonicalManifest%" ^
    >"%WorkDirectory%\Transport.txt" 2>"%WorkDirectory%\Transport.err"
if errorlevel 1 exit /b 1

set "NativeEntry="
set "FragmentCount="
for /f "tokens=9,11 delims== " %%E in ('findstr /b /c:"compiler image transport status=Complete " "%WorkDirectory%\Transport.txt"') do (
    set "NativeEntry=%%E"
    set "FragmentCount=%%F"
)
if not defined NativeEntry exit /b 1
if not defined FragmentCount exit /b 1
echo(%NativeEntry%| findstr /r /x "[0-9][0-9]*" >nul || exit /b 1
echo(%FragmentCount%| findstr /r /x "[1-8]" >nul || exit /b 1

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
    "%ConstructionWvb%" "%CanonicalPrefix%" %FragmentCount% %NativeEntry% ^
    "%ConstructionWindows%" windows ^
    >"%WorkDirectory%\Windows.txt" 2>"%WorkDirectory%\Windows.err"
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 6 ^
    "%ConstructionWvb%" "%CanonicalPrefix%" %FragmentCount% %NativeEntry% ^
    "%ConstructionLinux%" linux ^
    >"%WorkDirectory%\Linux.txt" 2>"%WorkDirectory%\Linux.err"
exit /b %ERRORLEVEL%

:verify_file
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
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
>&2 echo Usage: Tools\Native\Construct-Segmented-Compiler-Toolset.cmd ^<existing-separate-output-directory^>
exit /b 64
