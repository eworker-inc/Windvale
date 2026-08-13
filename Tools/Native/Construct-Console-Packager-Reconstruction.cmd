@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if not "%~2"=="" goto :usage
if not exist "%~f1\." goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "OutputRoot=%~f1"
set "OrdinaryCandidate=%RepositoryRoot%\Artifacts\Native-Console-Packager-Candidate"
set "SegmentedCandidate=%RepositoryRoot%\Artifacts\Native-Console-Segmented-Packager-Candidate"
set "OrdinaryOutput=%OutputRoot%\Native-Console-Packager-Candidate"
set "SegmentedOutput=%OutputRoot%\Native-Console-Segmented-Packager-Candidate"
for %%R in ("%OrdinaryOutput%") do set "OrdinaryOutput=%%~fR"
for %%R in ("%SegmentedOutput%") do set "SegmentedOutput=%%~fR"

if /I "%OutputRoot%"=="%OrdinaryCandidate%" goto :candidate_path
if /I "%OutputRoot%"=="%SegmentedCandidate%" goto :candidate_path
if /I "%OrdinaryOutput%"=="%OrdinaryCandidate%" goto :candidate_path
if /I "%SegmentedOutput%"=="%SegmentedCandidate%" goto :candidate_path
fsutil reparsepoint query "%OutputRoot%" >nul 2>nul
if not errorlevel 1 goto :reparse_path
if exist "%OrdinaryOutput%\." (
    fsutil reparsepoint query "%OrdinaryOutput%" >nul 2>nul
    if not errorlevel 1 goto :reparse_path
)
if exist "%SegmentedOutput%\." (
    fsutil reparsepoint query "%SegmentedOutput%" >nul 2>nul
    if not errorlevel 1 goto :reparse_path
)
if not exist "%OrdinaryOutput%\." mkdir "%OrdinaryOutput%" || exit /b 1
if not exist "%SegmentedOutput%\." mkdir "%SegmentedOutput%" || exit /b 1

:allocate
set "TemporaryDirectory=%TEMP%\windvale-console-packager-reconstruction-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "Result=1"

set "OrdinaryWvb=%OrdinaryOutput%\Console-Packager.wvb"
set "OrdinaryWindows=%OrdinaryOutput%\Console-Packager.exe"
set "OrdinaryLinux=%OrdinaryOutput%\Console-Packager.elf"
set "SegmentedWvb=%SegmentedOutput%\Console-Segmented-Packager.wvb"
set "SegmentedWindows=%SegmentedOutput%\Console-Segmented-Packager.exe"
set "SegmentedLinux=%SegmentedOutput%\Console-Segmented-Packager.elf"

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects/Linker/Windvale-Console-Application-Packager.wvproj" ^
    "%OrdinaryWvb%" >"%TemporaryDirectory%\Build-Ordinary.txt" 2>"%TemporaryDirectory%\Build-Ordinary.err"
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects/Linker/Windvale-Console-Application-Segmented-Packager.wvproj" ^
    "%SegmentedWvb%" >"%TemporaryDirectory%\Build-Segmented.txt" 2>"%TemporaryDirectory%\Build-Segmented.err"
if errorlevel 1 goto :cleanup

call :construct_pair Ordinary "%OrdinaryWvb%" "%OrdinaryWindows%" "%OrdinaryLinux%"
if errorlevel 1 goto :cleanup
call :construct_pair Segmented "%SegmentedWvb%" "%SegmentedWindows%" "%SegmentedLinux%"
if errorlevel 1 goto :cleanup

call :verify_file "%OrdinaryWvb%" 60797 f4c75495321736bbce22582213133e7cc09157a8439dc198d9848ec95683e89c "ordinary console-packager WVB"
if errorlevel 1 goto :cleanup
call :verify_file "%OrdinaryWindows%" 708608 ea8e666806618cd9c230bdc88882e9b30a98182f8486456a46c75b746a0cdab9 "Windows ordinary console-packager application"
if errorlevel 1 goto :cleanup
call :verify_file "%OrdinaryLinux%" 708608 d399c935e906ab42d7572e337226577055396cb6204766106e21790e22ea43af "Linux ordinary console-packager application"
if errorlevel 1 goto :cleanup
call :verify_file "%SegmentedWvb%" 70033 c4941f396f76467cb6455472f7f4711c21a6f65c12c09a9b5f4135987628f20e "segmented console-packager WVB"
if errorlevel 1 goto :cleanup
call :verify_file "%SegmentedWindows%" 805376 a6a6fd40a6becf0f65bbf995006e8e5410832da6f5ebc906f216f9e435032ef0 "Windows segmented console-packager application"
if errorlevel 1 goto :cleanup
call :verify_file "%SegmentedLinux%" 806912 8916fb509f81e29dabca7ed0202c0ad250f129e78b70b701630dbfcd55a1d30d "Linux segmented console-packager application"
if errorlevel 1 goto :cleanup

echo native console packager reconstruction status=Complete families=2 artifacts=6
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

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 5 ^
    "%ConstructionWvb%" "%CanonicalPrefix%" %FragmentCount% %NativeEntry% ^
    "%ConstructionWindows%" windows ^
    >"%WorkDirectory%\Windows.txt" 2>"%WorkDirectory%\Windows.err"
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 5 ^
    "%ConstructionWvb%" "%CanonicalPrefix%" %FragmentCount% %NativeEntry% ^
    "%ConstructionLinux%" linux ^
    >"%WorkDirectory%\Linux.txt" 2>"%WorkDirectory%\Linux.err"
exit /b %ERRORLEVEL%

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

:candidate_path
>&2 echo The console-packager reconstruction must not overwrite a live candidate directory.
exit /b 64

:reparse_path
>&2 echo The console-packager reconstruction output must not use a reparse-point directory.
exit /b 64

:usage
>&2 echo Usage: Tools\Native\Construct-Console-Packager-Reconstruction.cmd ^<existing-separate-output-directory^>
exit /b 64
