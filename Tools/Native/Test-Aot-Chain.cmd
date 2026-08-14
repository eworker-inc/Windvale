@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Aot-Chain.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "TemporaryDirectory=%TEMP%\windvale-native-aot-chain-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1

set "Wvb=%TemporaryDirectory%\Return-42.wvb"
set "Wvo=%TemporaryDirectory%\Return-42.wvo"
set "Image=%TemporaryDirectory%\Return-42.bin"
set "Application=%TemporaryDirectory%\Return-42.exe"
set "Map=%TemporaryDirectory%\Return-42.wvmap"

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects/Tests/Windvale-Native-Test-Wvb-To-Wvo-Return-42.wvproj" "%Wvb%" >nul
if errorlevel 1 goto :failed
certutil -hashfile "%Wvb%" SHA256 | findstr /I /C:"7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31" >nul
if errorlevel 1 (
    >&2 echo The native AOT chain WVB identity differs.
    goto :failed
)

call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Wvb%" "%Wvo%" >nul
if errorlevel 1 goto :failed
call "%RepositoryRoot%\Tools\Native\Check-Wvo.cmd" "%Wvo%" >nul
if errorlevel 1 goto :failed
certutil -hashfile "%Wvo%" SHA256 | findstr /I /C:"0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5" >nul
if errorlevel 1 (
    >&2 echo The native AOT chain WVO identity differs.
    goto :failed
)

call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 1048576 Main "%Image%" "%Wvo%" > "%Map%"
if errorlevel 1 goto :failed
certutil -hashfile "%Image%" SHA256 | findstr /I /C:"7c05565142850adab1d63d999479977a23ef50c7264c03ee55ce5b323df26408" >nul
if errorlevel 1 (
    >&2 echo The native AOT chain flat image identity differs.
    goto :failed
)
certutil -hashfile "%Map%" SHA256 | findstr /I /C:"857710249807d2fed4da847729d0244f08ccdc70156c043fdaa0516de394e2dc" >nul
if errorlevel 1 (
    >&2 echo The native AOT chain link map identity differs.
    goto :failed
)

call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 "%Image%" 0 "%Application%" >nul
if errorlevel 1 goto :failed
certutil -hashfile "%Application%" SHA256 | findstr /I /C:"8f2c3389dafa40c0231a0f5aeead3db5570697d54874f324a81f84a2d5b16eb6" >nul
if errorlevel 1 (
    >&2 echo The native AOT chain Windows application identity differs.
    goto :failed
)

"%Application%" >nul
set "ApplicationResult=%ERRORLEVEL%"
if not "%ApplicationResult%"=="42" (
    >&2 echo The native AOT application result is %ApplicationResult%, expected 42.
    goto :failed
)

del /f /q "%Wvb%" "%Wvo%" "%Image%" "%Application%" "%Map%" >nul 2>nul
rmdir "%TemporaryDirectory%" >nul 2>nul
echo native aot chain status=Passed result=42
exit /b 0

:failed
del /f /q "%Wvb%" "%Wvo%" "%Image%" "%Application%" "%Map%" >nul 2>nul
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 1
