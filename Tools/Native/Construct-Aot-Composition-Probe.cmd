@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~2"=="" goto :usage
if "%~1"=="" goto :usage
if not exist "%~1\." goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
for %%R in ("%~1") do set "OutputRoot=%%~fR"
set "Wvb=%OutputRoot%\Return-42.wvb"
set "Wvo=%OutputRoot%\Return-42.wvo"
set "Image=%OutputRoot%\Return-42.bin"
set "Map=%OutputRoot%\Return-42.wvmap"
set "WindowsApplication=%OutputRoot%\Return-42.exe"
set "LinuxApplication=%OutputRoot%\Return-42.elf"

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects/Tests/Windvale-Native-Test-Wvb-To-Wvo-Return-42.wvproj" "%Wvb%" ^
    >"%OutputRoot%\Build.out" 2>"%OutputRoot%\Build.err"
if errorlevel 1 exit /b 1
call :check_empty "%OutputRoot%\Build.err"
if errorlevel 1 exit /b 1
call :check_file "%Wvb%" 7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31
if errorlevel 1 exit /b 1

call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Wvb%" "%Wvo%" ^
    >"%OutputRoot%\Lower.out" 2>"%OutputRoot%\Lower.err"
if errorlevel 1 (
    >&2 echo The native AOT composition probe lowering command failed.
    exit /b 1
)
call :check_empty "%OutputRoot%\Lower.err"
if errorlevel 1 (
    >&2 echo The native AOT composition probe lowering wrote a diagnostic.
    exit /b 1
)
call "%RepositoryRoot%\Tools\Native\Verify-Wvo.cmd" "%Wvo%" ^
    >"%OutputRoot%\Verify.out" 2>"%OutputRoot%\Verify.err"
if errorlevel 1 (
    >&2 echo The native AOT composition probe WVO verification failed.
    exit /b 1
)
call :check_empty "%OutputRoot%\Verify.err"
if errorlevel 1 exit /b 1
call :check_file "%Wvo%" 0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5
if errorlevel 1 exit /b 1

call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 1048576 Main "%Image%" "%Wvo%" ^
    >"%Map%" 2>"%OutputRoot%\Link.err"
if errorlevel 1 exit /b 1
call :check_empty "%OutputRoot%\Link.err"
if errorlevel 1 exit /b 1
call :check_file "%Image%" 7c05565142850adab1d63d999479977a23ef50c7264c03ee55ce5b323df26408
if errorlevel 1 exit /b 1
call :check_file "%Map%" 857710249807d2fed4da847729d0244f08ccdc70156c043fdaa0516de394e2dc
if errorlevel 1 exit /b 1

call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" windows-x64-console-v1 ^
    "%Image%" 0 "%WindowsApplication%" ^
    >"%OutputRoot%\Windows.out" 2>"%OutputRoot%\Windows.err"
if errorlevel 1 exit /b 1
call :check_empty "%OutputRoot%\Windows.err"
if errorlevel 1 exit /b 1
call :check_file "%WindowsApplication%" 8f2c3389dafa40c0231a0f5aeead3db5570697d54874f324a81f84a2d5b16eb6
if errorlevel 1 exit /b 1

call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" linux-x64-console-v1 ^
    "%Image%" 0 "%LinuxApplication%" ^
    >"%OutputRoot%\Linux.out" 2>"%OutputRoot%\Linux.err"
if errorlevel 1 exit /b 1
call :check_empty "%OutputRoot%\Linux.err"
if errorlevel 1 exit /b 1
call :check_file "%LinuxApplication%" fe525b84b9bf902677a5c7beb36872dfd72e7d6d0f12bfb5c95d491c4e1cd3f7
if errorlevel 1 exit /b 1

echo native AOT composition probe status=Complete artifacts=6
exit /b 0

:check_file
if not exist "%~1" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~2" >nul
exit /b %ERRORLEVEL%

:check_empty
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="0" exit /b 1
exit /b 0

:usage
>&2 echo Usage: Tools\Native\Construct-Aot-Composition-Probe.cmd ^<existing-output-directory^>
exit /b 64
