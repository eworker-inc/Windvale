@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~3"=="" goto :usage
if not "%~4"=="" goto :usage
echo(%~1| findstr /r /x "[1-7]" >nul || goto :usage
if /I not "%~x2"==".wvb" goto :usage
if /I not "%~x3"==".exe" goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Input=%~f2"
set "Output=%~f3"

:allocate
set "TemporaryDirectory=%TEMP%\windvale-segmented-compiler-package-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "ObjectPrefix=%TemporaryDirectory%\Object"
set "ObjectManifest=%TemporaryDirectory%\Object.wvop"
set "ImagePrefix=%TemporaryDirectory%\Image"
set "ImageManifest=%TemporaryDirectory%\Image.wvli"
set "CanonicalPrefix=%TemporaryDirectory%\Canonical"
set "CanonicalManifest=%TemporaryDirectory%\Canonical.wvli"
set "Result=1"

call "%RepositoryRoot%\Tools\Native\Stage-Compiler-Wvb.cmd" "%Input%" "%ObjectPrefix%" "%ObjectManifest%" >"%TemporaryDirectory%\Stage.txt"
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Link-Staged-Compiler-Wvo.cmd" "%ObjectPrefix%" "%ObjectManifest%" "%ImagePrefix%" "%ImageManifest%" >"%TemporaryDirectory%\Link.txt"
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Transport-Compiler-Image.cmd" "%ImagePrefix%" "%ImageManifest%" "%CanonicalPrefix%" "%CanonicalManifest%" >"%TemporaryDirectory%\Transport.txt"
if errorlevel 1 goto :cleanup

set "NativeEntry="
set "FragmentCount="
for /f "tokens=9,11 delims== " %%E in ('findstr /b /c:"compiler image transport status=Complete " "%TemporaryDirectory%\Transport.txt"') do (
    set "NativeEntry=%%E"
    set "FragmentCount=%%F"
)
if not defined NativeEntry goto :cleanup
if not defined FragmentCount goto :cleanup
echo(%NativeEntry%| findstr /r /x "[0-9][0-9]*" >nul || goto :cleanup
echo(%FragmentCount%| findstr /r /x "[1-8]" >nul || goto :cleanup

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" image %~1 "%Input%" "%CanonicalPrefix%" %FragmentCount% %NativeEntry% "%Output%"
set "Result=%ERRORLEVEL%"

:cleanup
del /f /q "%TemporaryDirectory%\*" >nul 2>nul
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b %Result%

:usage
>&2 echo Usage: Tools\Native\Package-Segmented-Compiler-Wvb.cmd ^<profile-1-through-7^> ^<input.wvb^> ^<output.exe^>
exit /b 64
