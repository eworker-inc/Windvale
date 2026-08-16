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

call :verify_file "%WvoStagingWvb%" 532490 72d738268580584b967deca648bb12bc80bf3243d10600921dfc8ddf670be623 "WVO staging producer WVB"
if errorlevel 1 goto :cleanup
call :verify_file "%OutputRoot%\windows-x64-wvstage.exe" 7756800 6cc939dc3f3e319f036d633626e867078c490564db83814add90b31936bc2bfd "Windows WVO staging producer"
if errorlevel 1 goto :cleanup
call :verify_file "%OutputRoot%\linux-x64-wvstage.elf" 7757824 7b9d1b1124b0d7cb09bc9b3d9bfd7c916e7272a40d3e029a39b444c788e1b758 "Linux WVO staging producer"
if errorlevel 1 goto :cleanup
call :verify_file "%ImageStagingWvb%" 75666 1a1614c4010baf47f5f1766de5f71806356ec14fa8f5bc67a62b5b2342269edd "compiler-image staging WVB"
if errorlevel 1 goto :cleanup
call :verify_file "%OutputRoot%\windows-x64-wvlinkstage.exe" 854016 e467d211d141ab75b838ece9b3c4625b6b5b2768b63dcacadd040368844e18db "Windows compiler-image staging application"
if errorlevel 1 goto :cleanup
call :verify_file "%OutputRoot%\linux-x64-wvlinkstage.elf" 856064 7ef825a8054cb8f63c10c957b234f9c371fe1507d7ee20f3e6dbabf73e550cb2 "Linux compiler-image staging application"
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
