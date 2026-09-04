@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if not "%~2"=="" goto :usage
if not exist "%~f1\." goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "OutputRoot=%~f1"
set "CandidateRoot=%RepositoryRoot%\Artifacts\Native-Wvo-Object-Candidate"
if /I "%OutputRoot%"=="%CandidateRoot%" (
    >&2 echo The WVO inspector reconstruction must use a separate output directory.
    exit /b 64
)
fsutil reparsepoint query "%OutputRoot%" >nul 2>nul
if not errorlevel 1 (
    >&2 echo The WVO inspector reconstruction output directory must not be a reparse point.
    exit /b 64
)

set "HostedToolset=%RepositoryRoot%\Artifacts\Native-Hosted-Container-Toolset-Candidate"
set "Construction=%RepositoryRoot%\Artifacts\Native-Hosted-Verifier-Publisher-Construction-Candidate"
set "HostedTools=%HostedToolset%\windows-x64"
set "ConstructionTools=%Construction%\windows-x64"
set "ServiceRoot=%RepositoryRoot%\Runtime\Windvale.Native\Consumers"
set "StartupRoot=%RepositoryRoot%\Linker\Startup"

call :verify_file "%HostedToolset%\SHA256SUMS" 6927 1a17fa4ee16ba2f21613db6ac36bd7e8643d29a5a1cb26f42e322df19cdc9fd7 "hosted toolset inventory"
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
set "TemporaryDirectory=%TEMP%\windvale-wvo-inspector-reconstruction-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "Result=1"

set "Wvb=%OutputRoot%\Wvo-Object.wvb"
set "Wvo=%OutputRoot%\Wvo-Object.wvo"
set "WindowsApplication=%OutputRoot%\Wvo-Object.exe"
set "LinuxApplication=%OutputRoot%\Wvo-Object.elf"
set "Fragment=%TemporaryDirectory%\Wvo-Object.bin"
set "EnumRequest=%TemporaryDirectory%\Wvo-Object.wveq"
set "EnumService=%TemporaryDirectory%\Enum-Service.bin"
set "WindowsStartup=%TemporaryDirectory%\Windows-Startup.wvo"
set "LinuxStartup=%TemporaryDirectory%\Linux-Startup.wvo"

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Projects/Object-Model/Windvale-Wvo-Object.wvproj" "%Wvb%" >"%TemporaryDirectory%\Build.out" 2>"%TemporaryDirectory%\Build.err"
if errorlevel 1 goto :cleanup
call :verify_file "%Wvb%" 73322 40f7b7efcff5b6e5bbc3c878cf5f0147ee92af208d43d54ab8a04f87ec1e9070 "WVO inspector WVB"
if errorlevel 1 goto :cleanup

"%HostedTools%\wvhostenumrequest.exe" "%Wvb%" "%EnumRequest%" >"%TemporaryDirectory%\Enum-Request.out" 2>"%TemporaryDirectory%\Enum-Request.err"
if errorlevel 1 goto :cleanup
call :verify_file "%EnumRequest%" 977 cde0a8ba677b86e4c2bb4bb02a3d52df40e1c1d5412315aff7ffbec3b3f581d1 "WVO inspector enum request"
if errorlevel 1 goto :cleanup
"%HostedTools%\wvhostenumservice.exe" "%EnumRequest%" "%EnumService%" >"%TemporaryDirectory%\Enum-Service.out" 2>"%TemporaryDirectory%\Enum-Service.err"
if errorlevel 1 goto :cleanup
call :verify_file "%EnumService%" 1276 6403fa2c4343df14093ae9f63a7518b1c1e966b1d28eaace00a0ceffcb587f40 "WVO inspector enum service"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Wvb%" "%Wvo%" >"%TemporaryDirectory%\Lower.out" 2>"%TemporaryDirectory%\Lower.err"
if errorlevel 1 goto :cleanup
call :verify_file "%Wvo%" 1022822 bab6b73e5edd6b0b2726380ba2ff10859fbbcc37481572457b508bbd0d67c2ae "WVO inspector native object"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Fragment%" "%Wvo%" >"%TemporaryDirectory%\Link.out" 2>"%TemporaryDirectory%\Link.err"
if errorlevel 1 goto :cleanup
findstr /b /c:"entry name=Main address=82280" "%TemporaryDirectory%\Link.out" >nul
if errorlevel 1 goto :cleanup
call :verify_file "%Fragment%" 1017780 1410b92ebc614f17cbf6e8a1147cb2cd448ae687a3b776e8d4ec3eb96a434854 "WVO inspector linked fragment"
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
call :verify_file "%WindowsApplication%" 1037312 5362372e826958470eee7d90eb01938de5b91dcb3e1b0f952722e00578a82d03 "Windows WVO inspector application"
if errorlevel 1 goto :cleanup

call :construct_target linux 2 "%ServiceRoot%\Native-X64-Linux-Console-Output-Service.bin" "%ServiceRoot%\Native-X64-Linux-File-Input-Service.bin" "%ServiceRoot%\Native-X64-Linux-Diagnostic-Output-Service.bin" "%LinuxStartup%" "%LinuxApplication%"
if errorlevel 1 goto :cleanup
call :verify_file "%LinuxApplication%" 1036288 fcfd134222b05482a6ac432fc4acbfb72f3dfce92c3c646fc17595ddb078b840 "Linux WVO inspector application"
if errorlevel 1 goto :cleanup

echo native WVO inspector reconstruction status=Complete artifacts=4
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
"%HostedTools%\wvhostverifierbundle.exe" wvo-inspector "%Fragment%" "%ConsoleLeaf%" "%ServiceRoot%\Native-X64-Argument-Count-Service.bin" "%ServiceRoot%\Native-X64-Argument-Service.bin" "%FileInputLeaf%" "%ServiceRoot%\Native-X64-Utf8-Service.bin" "%DiagnosticLeaf%" "%EnumService%" "%ServiceRoot%\Native-X64-Text-Concat-Service.bin" "%ServiceRoot%\Native-X64-Text-Quote-Service.bin" "%ServiceRoot%\Native-X64-I32-Format-Service.bin" "%ServiceRoot%\Native-X64-U32-Format-Service.bin" "%TargetDirectory%\Bundle.wvsq" >"%TemporaryDirectory%\%TargetName%-Bundle-Request.out" 2>"%TemporaryDirectory%\%TargetName%-Bundle-Request.err"
if errorlevel 1 exit /b 1
"%ConstructionTools%\wvhostverifierpublisherbasemetadata.exe" wvo-inspector %Target% 82280 "%TargetDirectory%\Bundle.wvsq" "%TargetDirectory%\Metadata.wvhv" >"%TemporaryDirectory%\%TargetName%-Metadata.out" 2>"%TemporaryDirectory%\%TargetName%-Metadata.err"
if errorlevel 1 exit /b 1
"%ConstructionTools%\wvhostverifierpublisherbaseruntime.exe" "%TargetDirectory%\Metadata.wvhv" "%TargetDirectory%\Runtime.wvhr" >"%TemporaryDirectory%\%TargetName%-Runtime.out" 2>"%TemporaryDirectory%\%TargetName%-Runtime.err"
if errorlevel 1 exit /b 1
"%HostedTools%\wvhostbundle.exe" "%TargetDirectory%\Bundle.wvsq" "%TargetDirectory%\Bundle.wvsi" >"%TemporaryDirectory%\%TargetName%-Bundle.out" 2>"%TemporaryDirectory%\%TargetName%-Bundle.err"
if errorlevel 1 exit /b 1
"%HostedTools%\wvhostverifierbytes.exe" wvo-inspector "%TargetDirectory%\Runtime.wvhr" "%TargetDirectory%\Platform.wvhb" >"%TemporaryDirectory%\%TargetName%-Platform.out" 2>"%TemporaryDirectory%\%TargetName%-Platform.err"
if errorlevel 1 exit /b 1
"%HostedTools%\wvhostverifierstartup.exe" wvo-inspector "%TargetDirectory%\Runtime.wvhr" "%Startup%" "%TargetDirectory%\Startup.wvsd" >"%TemporaryDirectory%\%TargetName%-Startup.out" 2>"%TemporaryDirectory%\%TargetName%-Startup.err"
if errorlevel 1 exit /b 1
"%HostedTools%\wvhostverifiercompose.exe" wvo-inspector "%TargetDirectory%\Runtime.wvhr" "%TargetDirectory%\Platform.wvhb" "%TargetDirectory%\Startup.wvsd" "%TargetDirectory%\Bundle.wvsi" "%Application%" >"%TemporaryDirectory%\%TargetName%-Compose.out" 2>"%TemporaryDirectory%\%TargetName%-Compose.err"
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
>&2 echo Usage: Tools\Native\Construct-Wvo-Inspector-Reconstruction.cmd ^<existing-separate-output-directory^>
exit /b 64
