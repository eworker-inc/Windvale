@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if not "%~2"=="" goto :usage
if not exist "%~f1\." goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "OutputRoot=%~f1"
set "CandidateRoot=%RepositoryRoot%\Artifacts\Native-Wv-Linker-Candidate"
if /I "%OutputRoot%"=="%CandidateRoot%" (
    >&2 echo The Wv-Linker reconstruction must use a separate output directory.
    exit /b 64
)
fsutil reparsepoint query "%OutputRoot%" >nul 2>nul
if not errorlevel 1 (
    >&2 echo The Wv-Linker reconstruction output directory must not be a reparse point.
    exit /b 64
)

:allocate
set "TemporaryDirectory=%TEMP%\windvale-wv-linker-reconstruction-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "Result=1"

set "Wvb=%OutputRoot%\Wv-Linker.wvb"
set "Wvo=%OutputRoot%\Wv-Linker.wvo"
set "Fragment=%OutputRoot%\Wv-Linker.bin"
set "WindowsApplication=%OutputRoot%\Wv-Linker.exe"
set "LinuxApplication=%OutputRoot%\Wv-Linker.elf"
set "ObjectPrefix=%TemporaryDirectory%\Object"
set "ObjectManifest=%TemporaryDirectory%\Object.wvop"
set "ImagePrefix=%TemporaryDirectory%\Image"
set "ImageManifest=%TemporaryDirectory%\Image.wvli"
set "CanonicalPrefix=%TemporaryDirectory%\Canonical"
set "CanonicalManifest=%TemporaryDirectory%\Canonical.wvli"

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects/Linker/Windvale-Wv-Linker.wvproj" "%Wvb%" ^
    >"%TemporaryDirectory%\Build.out" 2>"%TemporaryDirectory%\Build.err"
if errorlevel 1 goto :cleanup
call :verify_file "%Wvb%" 135740 02f727a8ce2d6826c8414cada0933c7d5a54893ea061621d08147984c3d6f874 "Wv-Linker WVB"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Wvb%" "%Wvo%" ^
    >"%TemporaryDirectory%\Lower.out" 2>"%TemporaryDirectory%\Lower.err"
if errorlevel 1 goto :cleanup
call :verify_file "%Wvo%" 1786271 0141219773241e8780e2520f30ab8377914bf89a72f57da091871ac40d68a287 "Wv-Linker WVO"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Stage-Compiler-Wvb.cmd" ^
    "%Wvb%" "%ObjectPrefix%" "%ObjectManifest%" ^
    >"%TemporaryDirectory%\Stage.out" 2>"%TemporaryDirectory%\Stage.err"
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Staged-Compiler-Wvo.cmd" ^
    "%ObjectPrefix%" "%ObjectManifest%" "%ImagePrefix%" "%ImageManifest%" ^
    >"%TemporaryDirectory%\Link.out" 2>"%TemporaryDirectory%\Link.err"
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Transport-Compiler-Image.cmd" ^
    "%ImagePrefix%" "%ImageManifest%" "%CanonicalPrefix%" "%CanonicalManifest%" ^
    >"%TemporaryDirectory%\Transport.out" 2>"%TemporaryDirectory%\Transport.err"
if errorlevel 1 goto :cleanup

set "NativeEntry="
set "FragmentCount="
for /f "tokens=9,11 delims== " %%E in ('findstr /b /c:"compiler image transport status=Complete " "%TemporaryDirectory%\Transport.out"') do (
    set "NativeEntry=%%E"
    set "FragmentCount=%%F"
)
if not "%NativeEntry%"=="884630" goto :cleanup
if not "%FragmentCount%"=="1" goto :cleanup
copy /b "%CanonicalPrefix%.chunk-0" "%Fragment%" >nul || goto :cleanup
call :verify_file "%Fragment%" 1777781 d30e0c4dce7159bf98c546a0200e8b541797612ab67d6f21e3d8ee876af27480 "Wv-Linker linked fragment"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 4 ^
    "%Wvb%" "%CanonicalPrefix%" %FragmentCount% %NativeEntry% "%WindowsApplication%" windows ^
    >"%TemporaryDirectory%\Windows.out" 2>"%TemporaryDirectory%\Windows.err"
if errorlevel 1 goto :cleanup
call :verify_file "%WindowsApplication%" 1796608 f47a952867203fbff53abb131ea155b4fe9e14a8be153cc61c0ca5fd8e4a74e0 "Windows Wv-Linker application"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image 4 ^
    "%Wvb%" "%CanonicalPrefix%" %FragmentCount% %NativeEntry% "%LinuxApplication%" linux ^
    >"%TemporaryDirectory%\Linux.out" 2>"%TemporaryDirectory%\Linux.err"
if errorlevel 1 goto :cleanup
call :verify_file "%LinuxApplication%" 1798144 8a220bfd6c7ef684897583e728419ecd6d383c8e8cf40094edbcfb695e3d6d7a "Linux Wv-Linker application"
if errorlevel 1 goto :cleanup

echo native Wv-Linker reconstruction status=Complete artifacts=5
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
>&2 echo Usage: Tools\Native\Construct-Wv-Linker-Reconstruction.cmd ^<existing-separate-output-directory^>
exit /b 64
