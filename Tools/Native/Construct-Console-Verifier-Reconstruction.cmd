@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if not "%~2"=="" goto :usage
if not exist "%~f1\." goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "OutputRoot=%~f1"
set "CandidateRoot=%RepositoryRoot%\Artifacts\Native-Console-Application-Verifier-Candidate"
if /I "%OutputRoot%"=="%CandidateRoot%" (
    >&2 echo The console-verifier reconstruction must use a separate output directory.
    exit /b 64
)
fsutil reparsepoint query "%OutputRoot%" >nul 2>nul
if not errorlevel 1 (
    >&2 echo The console-verifier reconstruction output directory must not be a reparse point.
    exit /b 64
)

set "HostedToolset=%RepositoryRoot%\Artifacts\Native-Hosted-Container-Toolset-Candidate"
set "Construction=%RepositoryRoot%\Artifacts\Native-Hosted-Verifier-Publisher-Construction-Candidate"
set "HostedTools=%HostedToolset%\windows-x64"
set "ConstructionTools=%Construction%\windows-x64"
set "ServiceRoot=%RepositoryRoot%\Runtime\Windvale.Native\Consumers"
set "StartupRoot=%RepositoryRoot%\Linker\Startup"

call :verify_file "%HostedToolset%\SHA256SUMS" 6927 d8b23c4b371c4841b6386f64940166be57a81930a2987a541a7c04648ddb016a "hosted toolset inventory"
if errorlevel 1 exit /b 1
for /f "usebackq tokens=1,*" %%H in ("%HostedToolset%\SHA256SUMS") do (
    call :verify_digest "%HostedToolset%\%%I" %%H "hosted toolset artifact"
    if errorlevel 1 exit /b 1
)
call :verify_file "%Construction%\SHA256SUMS" 5064 15502d44e9578a1ce332fe390764c811a82fee8b3a0f8d9ee80aa158c9bbb334 "publisher construction inventory"
if errorlevel 1 exit /b 1
for /f "usebackq tokens=1,*" %%H in ("%Construction%\SHA256SUMS") do (
    call :verify_digest "%Construction%\%%I" %%H "publisher construction artifact"
    if errorlevel 1 exit /b 1
)
call :verify_file "%StartupRoot%\Windows-X64-Hosted-Inspector.wva" 9617 865c29d2f83740e70be173f6116b29b0fa9eb4836f52e96200eb508f6fdbb789 "Windows inspector startup source"
if errorlevel 1 exit /b 1
call :verify_file "%StartupRoot%\Linux-X64-Hosted-Inspector.wva" 5214 01603c6b945b4e03ebef1d3d5bf691a5e05bf2e2630d6466e1db1028b8c9c005 "Linux inspector startup source"
if errorlevel 1 exit /b 1

:allocate
set "TemporaryDirectory=%TEMP%\windvale-console-verifier-reconstruction-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "Result=1"

set "Wvb=%OutputRoot%\Console-Application-Verifier.wvb"
set "Wvo=%OutputRoot%\Console-Application-Verifier.wvo"
set "Fragment=%TemporaryDirectory%\Console-Application-Verifier.bin"
set "WindowsApplication=%OutputRoot%\windows-x64-wvappverify.exe"
set "LinuxApplication=%OutputRoot%\linux-x64-wvappverify.elf"
set "WindowsStartup=%TemporaryDirectory%\Windows-Startup.wvo"
set "LinuxStartup=%TemporaryDirectory%\Linux-Startup.wvo"

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects/Tools/Windvale-Console-Application-Verifier.wvproj" "%Wvb%" >"%TemporaryDirectory%\Build.out" 2>"%TemporaryDirectory%\Build.err"
if errorlevel 1 goto :cleanup
call :verify_file "%Wvb%" 105006 1dcd5f2aeebd974649e64c90d9f473e1e75f7d13dbcde2814de1dded72cf2c0c "console-verifier WVB"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Wvb%" "%Wvo%" >"%TemporaryDirectory%\Lower.out" 2>"%TemporaryDirectory%\Lower.err"
if errorlevel 1 goto :cleanup
call :verify_file "%Wvo%" 1049519 51292e4d300d4a6bb6ce4879915bba5304de70c9deafdf4eb6ff6a54a6dbf150 "console-verifier WVO"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Fragment%" "%Wvo%" >"%TemporaryDirectory%\Link.out" 2>"%TemporaryDirectory%\Link.err"
if errorlevel 1 goto :cleanup
set "EntryMatch=0"
for /f "usebackq delims=" %%L in ("%TemporaryDirectory%\Link.out") do if "%%L"=="entry name=Main address=19221" set "EntryMatch=1"
if not "%EntryMatch%"=="1" goto :cleanup
call :verify_file "%Fragment%" 1045627 96fee2a235db667b161db2eff71625dc714f842f82e74dcf22c0aa03b1cdbffa "console-verifier linked fragment"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" "%StartupRoot%\Windows-X64-Hosted-Inspector.wva" "%WindowsStartup%" >"%TemporaryDirectory%\Windows-Assemble.out" 2>"%TemporaryDirectory%\Windows-Assemble.err"
if errorlevel 1 goto :cleanup
call :verify_file "%WindowsStartup%" 4017 95ff213a8e59f28d148eb8223a100a5b24dcbc3eb1b444264783a860f159fe49 "Windows inspector startup WVO"
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" "%StartupRoot%\Linux-X64-Hosted-Inspector.wva" "%LinuxStartup%" >"%TemporaryDirectory%\Linux-Assemble.out" 2>"%TemporaryDirectory%\Linux-Assemble.err"
if errorlevel 1 goto :cleanup
call :verify_file "%LinuxStartup%" 2291 5d316c109b5c8964c019c44f96f42370408820c7db1ec278268cef541ba17ebb "Linux inspector startup WVO"
if errorlevel 1 goto :cleanup

call :construct_target windows 1 "%ServiceRoot%\Native-X64-Windows-Console-Output-Service.bin" "%ServiceRoot%\Native-X64-Windows-File-Input-Service.bin" "%ServiceRoot%\Native-X64-Windows-Diagnostic-Output-Service.bin" "%WindowsStartup%" "%WindowsApplication%"
if errorlevel 1 goto :cleanup
call :verify_file "%WindowsApplication%" 1063936 a82027ab78ee5f4d7d9f34180392ee8b8364ea78616c11aeac1e684250fc3679 "Windows console-verifier application"
if errorlevel 1 goto :cleanup

call :construct_target linux 2 "%ServiceRoot%\Native-X64-Linux-Console-Output-Service.bin" "%ServiceRoot%\Native-X64-Linux-File-Input-Service.bin" "%ServiceRoot%\Native-X64-Linux-Diagnostic-Output-Service.bin" "%LinuxStartup%" "%LinuxApplication%"
if errorlevel 1 goto :cleanup
call :verify_file "%LinuxApplication%" 1064960 c2700e5e68711d7b8e8a8f7e9573d87dfa27c3676a034a314310ef59045e5f1a "Linux console-verifier application"
if errorlevel 1 goto :cleanup

echo native console verifier reconstruction status=Complete artifacts=4
set "Result=0"
goto :cleanup

:construct_target
set "TargetName=%~1"
set "Target=%~2"
set "ConsoleLeaf=%~3"
set "FileInputLeaf=%~4"
set "DiagnosticLeaf=%~5"
set "Startup=%~6"
set "Application=%~7"
set "TargetDirectory=%TemporaryDirectory%\%TargetName%"
mkdir "%TargetDirectory%" || exit /b 1
"%HostedTools%\wvhostverifierbundle.exe" console-verifier "%Fragment%" "%ConsoleLeaf%" "%ServiceRoot%\Native-X64-Argument-Count-Service.bin" "%ServiceRoot%\Native-X64-Argument-Service.bin" "%FileInputLeaf%" "%ServiceRoot%\Native-X64-Utf8-Service.bin" "%DiagnosticLeaf%" "%ServiceRoot%\Native-X64-Enum-Name-Service.bin" "%ServiceRoot%\Native-X64-Text-Concat-Service.bin" "%ServiceRoot%\Native-X64-Text-Quote-Service.bin" "%ServiceRoot%\Native-X64-I32-Format-Service.bin" "%ServiceRoot%\Native-X64-U32-Format-Service.bin" "%TargetDirectory%\Bundle.wvsq" >"%TemporaryDirectory%\%TargetName%-Bundle-Request.out" 2>"%TemporaryDirectory%\%TargetName%-Bundle-Request.err"
if errorlevel 1 exit /b 1
"%ConstructionTools%\wvhostverifierpublisherbasemetadata.exe" console-verifier %Target% 19221 "%TargetDirectory%\Bundle.wvsq" "%TargetDirectory%\Metadata.wvhv" >"%TemporaryDirectory%\%TargetName%-Metadata.out" 2>"%TemporaryDirectory%\%TargetName%-Metadata.err"
if errorlevel 1 exit /b 1
"%ConstructionTools%\wvhostverifierpublisherbaseruntime.exe" "%TargetDirectory%\Metadata.wvhv" "%TargetDirectory%\Runtime.wvhr" >"%TemporaryDirectory%\%TargetName%-Runtime.out" 2>"%TemporaryDirectory%\%TargetName%-Runtime.err"
if errorlevel 1 exit /b 1
"%HostedTools%\wvhostbundle.exe" "%TargetDirectory%\Bundle.wvsq" "%TargetDirectory%\Bundle.wvsi" >"%TemporaryDirectory%\%TargetName%-Bundle.out" 2>"%TemporaryDirectory%\%TargetName%-Bundle.err"
if errorlevel 1 exit /b 1
"%HostedTools%\wvhostverifierbytes.exe" console-verifier "%TargetDirectory%\Runtime.wvhr" "%TargetDirectory%\Platform.wvhb" >"%TemporaryDirectory%\%TargetName%-Platform.out" 2>"%TemporaryDirectory%\%TargetName%-Platform.err"
if errorlevel 1 exit /b 1
"%HostedTools%\wvhostverifierstartup.exe" console-verifier "%TargetDirectory%\Runtime.wvhr" "%Startup%" "%TargetDirectory%\Startup.wvsd" >"%TemporaryDirectory%\%TargetName%-Startup.out" 2>"%TemporaryDirectory%\%TargetName%-Startup.err"
if errorlevel 1 exit /b 1
"%HostedTools%\wvhostverifiercompose.exe" console-verifier "%TargetDirectory%\Runtime.wvhr" "%TargetDirectory%\Platform.wvhb" "%TargetDirectory%\Startup.wvsd" "%TargetDirectory%\Bundle.wvsi" "%Application%" >"%TemporaryDirectory%\%TargetName%-Compose.out" 2>"%TemporaryDirectory%\%TargetName%-Compose.err"
exit /b %ERRORLEVEL%

:verify_file
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" (
    >&2 echo The %~4 identity is invalid.
    exit /b 1
)
call :verify_digest "%~1" %~3 "%~4"
exit /b %ERRORLEVEL%

:verify_digest
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~2" >nul
if errorlevel 1 (
    >&2 echo The %~3 identity is invalid.
    exit /b 1
)
exit /b 0

:cleanup
if exist "%TemporaryDirectory%\." rmdir /s /q "%TemporaryDirectory%"
exit /b %Result%

:usage
>&2 echo Usage: Tools\Native\Construct-Console-Verifier-Reconstruction.cmd ^<existing-separate-output-directory^>
exit /b 64
