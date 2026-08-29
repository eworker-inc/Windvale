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

echo START segmented compiler toolset construction phase=build item=1/3 project=WVO-staging
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects/Compiler/Windvale-Native-X64-Lowering-Staging-Tool.wvproj" ^
    "%WvoStagingWvb%" >"%TemporaryDirectory%\Build-Wvo-Staging.txt" 2>"%TemporaryDirectory%\Build-Wvo-Staging.err"
if errorlevel 1 goto :cleanup
echo PASS  segmented compiler toolset construction phase=build item=1/3 project=WVO-staging
echo START segmented compiler toolset construction phase=build item=2/3 project=compiler-image-staging
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects/Linker/Windvale-Compiler-Image-Staging.wvproj" ^
    "%ImageStagingWvb%" >"%TemporaryDirectory%\Build-Image-Staging.txt" 2>"%TemporaryDirectory%\Build-Image-Staging.err"
if errorlevel 1 goto :cleanup
echo PASS  segmented compiler toolset construction phase=build item=2/3 project=compiler-image-staging
echo START segmented compiler toolset construction phase=build item=3/3 project=canonical-transport
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects/Linker/Windvale-Compiler-Image-Canonical-Transport.wvproj" ^
    "%TransportWvb%" >"%TemporaryDirectory%\Build-Transport.txt" 2>"%TemporaryDirectory%\Build-Transport.err"
if errorlevel 1 goto :cleanup
echo PASS  segmented compiler toolset construction phase=build item=3/3 project=canonical-transport

echo START segmented compiler toolset construction phase=package item=1/3 family=WVO-staging
call :construct_pair Wvo-Staging "%WvoStagingWvb%" ^
    "%OutputRoot%\windows-x64-wvstage.exe" ^
    "%OutputRoot%\linux-x64-wvstage.elf"
if errorlevel 1 goto :cleanup
echo PASS  segmented compiler toolset construction phase=package item=1/3 family=WVO-staging
echo START segmented compiler toolset construction phase=package item=2/3 family=compiler-image-staging
call :construct_pair Image-Staging "%ImageStagingWvb%" ^
    "%OutputRoot%\windows-x64-wvlinkstage.exe" ^
    "%OutputRoot%\linux-x64-wvlinkstage.elf"
if errorlevel 1 goto :cleanup
echo PASS  segmented compiler toolset construction phase=package item=2/3 family=compiler-image-staging
echo START segmented compiler toolset construction phase=package item=3/3 family=canonical-transport
call :construct_pair Transport "%TransportWvb%" ^
    "%OutputRoot%\windows-x64-wvimagetransport.exe" ^
    "%OutputRoot%\linux-x64-wvimagetransport.elf"
if errorlevel 1 goto :cleanup
echo PASS  segmented compiler toolset construction phase=package item=3/3 family=canonical-transport

call :verify_file "%WvoStagingWvb%" 576810 0eca0f227cbc3f2e9ad54dce9439ac0c859c8176cf72964cdda627c88d33e5cc "WVO staging producer WVB"
if errorlevel 1 goto :cleanup
call :verify_file "%OutputRoot%\windows-x64-wvstage.exe" 8416768 ca19b920d59987762d423dd8e79e4569878f6da0fc31d455564ef827c0f19e54 "Windows WVO staging producer"
if errorlevel 1 goto :cleanup
call :verify_file "%OutputRoot%\linux-x64-wvstage.elf" 8417280 499032e30458c0b60ab7225e082aa00d5ebc9e79d49bfb71c9167b721d6c5280 "Linux WVO staging producer"
if errorlevel 1 goto :cleanup
call :verify_file "%ImageStagingWvb%" 75666 ac01daa598f67d34ae5ed9dbc83a168dc288c05f7369b0773713947f0d5a85cd "compiler-image staging WVB"
if errorlevel 1 goto :cleanup
call :verify_file "%OutputRoot%\windows-x64-wvlinkstage.exe" 854016 c46534cd0fbbd294d2aa242a3ed26ca3ef663d6b1e054290befe0f4edc426da4 "Windows compiler-image staging application"
if errorlevel 1 goto :cleanup
call :verify_file "%OutputRoot%\linux-x64-wvlinkstage.elf" 856064 360f05b19181f001439a8309f571d0979eef260285f4995d7dcae1f06679a445 "Linux compiler-image staging application"
if errorlevel 1 goto :cleanup
call :verify_file "%TransportWvb%" 23836 d4bdfa7588e4431432a300e0da257507d73846931f5dd1296855b03714d218c8 "compiler-image transport WVB"
if errorlevel 1 goto :cleanup
call :verify_file "%OutputRoot%\windows-x64-wvimagetransport.exe" 269312 e724a5efbffc233fda76f55bfb5cc01c044e221882b5de5f247b0ab236726f81 "Windows compiler-image transport application"
if errorlevel 1 goto :cleanup
call :verify_file "%OutputRoot%\linux-x64-wvimagetransport.elf" 270336 9ff5401eca1ffd93a49077dd6ebc56c446c59939379a481f22662465fc3cf6db "Linux compiler-image transport application"
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
echo(%FragmentCount%| findstr /r /x "[0-9][0-9]*" >nul || exit /b 1
if %FragmentCount% LSS 1 exit /b 1
if %FragmentCount% GTR 16 exit /b 1

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
