@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if not "%~2"=="" goto :usage
if not exist "%~f1\." goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "OutputRoot=%~f1"
set "CandidateRoot=%RepositoryRoot%\Artifacts\Native-Wvb-Runner-Candidate"
if /I "%OutputRoot%"=="%CandidateRoot%" (
    >&2 echo The WVB-runner reconstruction must use a separate output directory.
    exit /b 64
)
fsutil reparsepoint query "%OutputRoot%" >nul 2>nul
if not errorlevel 1 (
    >&2 echo The WVB-runner reconstruction output directory must not be a reparse point.
    exit /b 64
)

set "SourceProject=%RepositoryRoot%\Windvale-Wvb-Runner.wvproj"
set "HostedToolset=%RepositoryRoot%\Artifacts\Native-Hosted-Container-Toolset-Candidate"
set "Construction=%RepositoryRoot%\Artifacts\Native-Hosted-Verifier-Publisher-Construction-Candidate"
set "HostedTools=%HostedToolset%\windows-x64"
set "ConstructionTools=%Construction%\windows-x64"
set "ServiceRoot=%RepositoryRoot%\Runtime\Windvale.Native\Consumers"
set "StartupRoot=%RepositoryRoot%\Linker\Startup"

call :verify_file "%HostedToolset%\SHA256SUMS" 6927 430171a9157560acb57e6f84aa772429b436059867892ee2408839057e0eeebc "hosted toolset inventory"
if errorlevel 1 exit /b 1
for /f "usebackq tokens=1,*" %%H in ("%HostedToolset%\SHA256SUMS") do (
    call :verify_digest "%HostedToolset%\%%I" %%H "hosted toolset artifact"
    if errorlevel 1 exit /b 1
)
call :verify_file "%Construction%\SHA256SUMS" 5064 ac41be9f59a7db47f721e0c0485cfe7e10cfc888e902f67e91a3c1c6330b68eb "publisher construction inventory"
if errorlevel 1 exit /b 1
for /f "usebackq tokens=1,*" %%H in ("%Construction%\SHA256SUMS") do (
    call :verify_digest "%Construction%\%%I" %%H "publisher construction artifact"
    if errorlevel 1 exit /b 1
)
call :verify_file "%StartupRoot%\Windows-X64-Hosted-Inspector.wva" 9437 f706848709e9c217f31dce6733b8aa3e94518b6f371cbd5ccc8af63603edb495 "Windows inspector startup source"
if errorlevel 1 exit /b 1
call :verify_file "%StartupRoot%\Linux-X64-Hosted-Inspector.wva" 5214 01603c6b945b4e03ebef1d3d5bf691a5e05bf2e2630d6466e1db1028b8c9c005 "Linux inspector startup source"
if errorlevel 1 exit /b 1

:allocate
set "TemporaryDirectory=%TEMP%\windvale-wvb-runner-reconstruction-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "Result=1"

set "Wvb=%OutputRoot%\Wvb-Runner.wvb"
set "Wvo=%OutputRoot%\Wvb-Runner.wvo"
set "Fragment=%TemporaryDirectory%\Wvb-Runner.bin"
set "WindowsApplication=%OutputRoot%\windows-x64-wvrun.exe"
set "LinuxApplication=%OutputRoot%\linux-x64-wvrun.elf"
set "WindowsStartup=%TemporaryDirectory%\Windows-Startup.wvo"
set "LinuxStartup=%TemporaryDirectory%\Linux-Startup.wvo"

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%SourceProject%" "%Wvb%" >"%TemporaryDirectory%\Build.out" 2>"%TemporaryDirectory%\Build.err"
if errorlevel 1 goto :cleanup
call :verify_file "%Wvb%" 121593 e58f653445cd717d19c32fe1a0fbc57f03f475187cdec571825b9fd6685b3097 "WVB-runner module"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Wvb%" "%Wvo%" >"%TemporaryDirectory%\Lower.out" 2>"%TemporaryDirectory%\Lower.err"
if errorlevel 1 goto :cleanup
call :verify_file "%Wvo%" 1078577 7d0ec719ade7e55d46c5a6dc6f7cb63102db4633172bcab1812e16651002106d "WVB-runner WVO"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Fragment%" "%Wvo%" >"%TemporaryDirectory%\Link.out" 2>"%TemporaryDirectory%\Link.err"
if errorlevel 1 goto :cleanup
set "EntryMatch=0"
for /f "usebackq delims=" %%L in ("%TemporaryDirectory%\Link.out") do if "%%L"=="entry name=Main address=14790" set "EntryMatch=1"
if not "%EntryMatch%"=="1" goto :cleanup
call :verify_file "%Fragment%" 1077675 83dc076c137557495a24e65894c26c7f794e0d67f31dd59a476e1dc7715828d1 "WVB-runner linked fragment"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" "%StartupRoot%\Windows-X64-Hosted-Inspector.wva" "%WindowsStartup%" >"%TemporaryDirectory%\Windows-Assemble.out" 2>"%TemporaryDirectory%\Windows-Assemble.err"
if errorlevel 1 goto :cleanup
call :verify_file "%WindowsStartup%" 3927 1bb785d5a06c40b91e45ebdc26b33ae33cb8ee7b244daffaa30ee59b9509edf3 "Windows inspector startup WVO"
if errorlevel 1 goto :cleanup
call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" "%StartupRoot%\Linux-X64-Hosted-Inspector.wva" "%LinuxStartup%" >"%TemporaryDirectory%\Linux-Assemble.out" 2>"%TemporaryDirectory%\Linux-Assemble.err"
if errorlevel 1 goto :cleanup
call :verify_file "%LinuxStartup%" 2291 5d316c109b5c8964c019c44f96f42370408820c7db1ec278268cef541ba17ebb "Linux inspector startup WVO"
if errorlevel 1 goto :cleanup

call :construct_target windows 1 "%ServiceRoot%\Native-X64-Windows-Console-Output-Service.bin" "%ServiceRoot%\Native-X64-Windows-File-Input-Service.bin" "%ServiceRoot%\Native-X64-Windows-Diagnostic-Output-Service.bin" "%WindowsStartup%" "%WindowsApplication%"
if errorlevel 1 goto :cleanup
call :verify_file "%WindowsApplication%" 1094656 6af8988f18c69a6757daeef8376c22ecbae406c31652813607fe2c3a6aa43ffc "Windows WVB-runner application"
if errorlevel 1 goto :cleanup

call :construct_target linux 2 "%ServiceRoot%\Native-X64-Linux-Console-Output-Service.bin" "%ServiceRoot%\Native-X64-Linux-File-Input-Service.bin" "%ServiceRoot%\Native-X64-Linux-Diagnostic-Output-Service.bin" "%LinuxStartup%" "%LinuxApplication%"
if errorlevel 1 goto :cleanup
call :verify_file "%LinuxApplication%" 1093632 a674b455aecaec48889318fd190a2123bc8bc784b1ee9b9eaa76b491ebebcb2d "Linux WVB-runner application"
if errorlevel 1 goto :cleanup

echo native WVB runner reconstruction status=Complete artifacts=4
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
"%HostedTools%\wvhostverifierbundle.exe" wvb-runner "%Fragment%" "%ConsoleLeaf%" "%ServiceRoot%\Native-X64-Argument-Count-Service.bin" "%ServiceRoot%\Native-X64-Argument-Service.bin" "%FileInputLeaf%" "%ServiceRoot%\Native-X64-Utf8-Service.bin" "%DiagnosticLeaf%" "%ServiceRoot%\Native-X64-Text-Concat-Service.bin" "%ServiceRoot%\Native-X64-I32-Format-Service.bin" "%ServiceRoot%\Native-X64-U32-Format-Service.bin" "%TargetDirectory%\Bundle.wvsq" >"%TemporaryDirectory%\%TargetName%-Bundle-Request.out" 2>"%TemporaryDirectory%\%TargetName%-Bundle-Request.err"
if errorlevel 1 exit /b 1
"%ConstructionTools%\wvhostverifierpublisherbasemetadata.exe" wvb-runner %Target% 14790 "%TargetDirectory%\Bundle.wvsq" "%TargetDirectory%\Metadata.wvhv" >"%TemporaryDirectory%\%TargetName%-Metadata.out" 2>"%TemporaryDirectory%\%TargetName%-Metadata.err"
if errorlevel 1 exit /b 1
"%ConstructionTools%\wvhostverifierpublisherbaseruntime.exe" "%TargetDirectory%\Metadata.wvhv" "%TargetDirectory%\Runtime.wvhr" >"%TemporaryDirectory%\%TargetName%-Runtime.out" 2>"%TemporaryDirectory%\%TargetName%-Runtime.err"
if errorlevel 1 exit /b 1
"%HostedTools%\wvhostbundle.exe" "%TargetDirectory%\Bundle.wvsq" "%TargetDirectory%\Bundle.wvsi" >"%TemporaryDirectory%\%TargetName%-Bundle.out" 2>"%TemporaryDirectory%\%TargetName%-Bundle.err"
if errorlevel 1 exit /b 1
"%HostedTools%\wvhostverifierbytes.exe" wvb-runner "%TargetDirectory%\Runtime.wvhr" "%TargetDirectory%\Platform.wvhb" >"%TemporaryDirectory%\%TargetName%-Platform.out" 2>"%TemporaryDirectory%\%TargetName%-Platform.err"
if errorlevel 1 exit /b 1
"%HostedTools%\wvhostverifierstartup.exe" wvb-runner "%TargetDirectory%\Runtime.wvhr" "%Startup%" "%TargetDirectory%\Startup.wvsd" >"%TemporaryDirectory%\%TargetName%-Startup.out" 2>"%TemporaryDirectory%\%TargetName%-Startup.err"
if errorlevel 1 exit /b 1
"%HostedTools%\wvhostverifiercompose.exe" wvb-runner "%TargetDirectory%\Runtime.wvhr" "%TargetDirectory%\Platform.wvhb" "%TargetDirectory%\Startup.wvsd" "%TargetDirectory%\Bundle.wvsi" "%Application%" >"%TemporaryDirectory%\%TargetName%-Compose.out" 2>"%TemporaryDirectory%\%TargetName%-Compose.err"
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
>&2 echo Usage: Tools\Native\Construct-Wvb-Runner-Reconstruction.cmd ^<existing-output-directory^>
exit /b 64
