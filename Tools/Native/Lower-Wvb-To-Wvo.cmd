@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if "%~2"=="" goto :usage
if not "%~3"=="" goto :usage
if /I not "%~x1"==".wvb" (
    >&2 echo The native lowerer input must use the .wvb extension.
    exit /b 2
)
if /I not "%~x2"==".wvo" (
    >&2 echo The native lowerer output must use the .wvo extension.
    exit /b 2
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Lowerer=%RepositoryRoot%\Artifacts\Native-Wvb-To-Wvo-Candidate\Wvb-To-Wvo.exe"
set "PublisherLauncher=%RepositoryRoot%\Tools\Native\Publish-Wvo.cmd"

certutil -hashfile "%Lowerer%" SHA256 | findstr /I /C:"a46d73ada72fba9561e9db1fcfc5477bf19be2518ad9db2d8487184112923dfd" >nul
if errorlevel 1 (
    >&2 echo The Windows native WVB-to-WVO lowerer artifact digest is invalid.
    exit /b 1
)

:allocate
set "TemporaryDirectory=%TEMP%\windvale-native-lower-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "CandidatePath=%TemporaryDirectory%\Candidate.wvo"

"%Lowerer%" "%~f1" "%CandidatePath%"
set "Result=%ERRORLEVEL%"
if not "%Result%"=="0" goto :cleanup
call "%PublisherLauncher%" "%CandidatePath%" "%~f2" >nul
set "Result=%ERRORLEVEL%"

:cleanup
if exist "%CandidatePath%" del /f /q "%CandidatePath%" >nul 2>nul
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b %Result%

:usage
>&2 echo Usage: Tools\Native\Lower-Wvb-To-Wvo.cmd ^<input.wvb^> ^<output.wvo^>
exit /b 2
