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

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" "%RepositoryRoot%\Windvale-Wvo-Object.wvproj" "%Wvb%" >"%TemporaryDirectory%\Build.out" 2>"%TemporaryDirectory%\Build.err"
if errorlevel 1 goto :cleanup
call :verify_file "%Wvb%" 61008 a630d49f0549c865644d8052fbff7e8bf2b6a6dcd013e1187d4356d49cd188db "WVO inspector WVB"
if errorlevel 1 goto :cleanup

"%HostedTools%\wvhostenumrequest.exe" "%Wvb%" "%EnumRequest%" >"%TemporaryDirectory%\Enum-Request.out" 2>"%TemporaryDirectory%\Enum-Request.err"
if errorlevel 1 goto :cleanup
call :verify_file "%EnumRequest%" 945 7129a003ae3d0e795f5aea61e4e8d8f25ba4fb93180f2538bea9f04a3c0bdab6 "WVO inspector enum request"
if errorlevel 1 goto :cleanup
"%HostedTools%\wvhostenumservice.exe" "%EnumRequest%" "%EnumService%" >"%TemporaryDirectory%\Enum-Service.out" 2>"%TemporaryDirectory%\Enum-Service.err"
if errorlevel 1 goto :cleanup
call :verify_file "%EnumService%" 1244 577ffaee02e64b0956f73d5ca44d65afa262cf476ae5eee86a899ffc575788d1 "WVO inspector enum service"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Wvb%" "%Wvo%" >"%TemporaryDirectory%\Lower.out" 2>"%TemporaryDirectory%\Lower.err"
if errorlevel 1 goto :cleanup
call :verify_file "%Wvo%" 591723 f45b14c33a7615209a2a16f6caf0bee041bdb5e2f46fd868792222e774fdb30c "WVO inspector native object"
if errorlevel 1 goto :cleanup

call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%Fragment%" "%Wvo%" >"%TemporaryDirectory%\Link.out" 2>"%TemporaryDirectory%\Link.err"
if errorlevel 1 goto :cleanup
findstr /b /c:"entry name=Main address=82280" "%TemporaryDirectory%\Link.out" >nul
if errorlevel 1 goto :cleanup
call :verify_file "%Fragment%" 587529 f318ee573b149aac169b67369e90dbacc6451fc129022bfb4e62b2ceff9cfba4 "WVO inspector linked fragment"
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
call :verify_file "%WindowsApplication%" 606720 a534b1c7a5ff9112c221a9576141842c4bb50c28b1d43d0ab02a8679bba6f366 "Windows WVO inspector application"
if errorlevel 1 goto :cleanup

call :construct_target linux 2 "%ServiceRoot%\Native-X64-Linux-Console-Output-Service.bin" "%ServiceRoot%\Native-X64-Linux-File-Input-Service.bin" "%ServiceRoot%\Native-X64-Linux-Diagnostic-Output-Service.bin" "%LinuxStartup%" "%LinuxApplication%"
if errorlevel 1 goto :cleanup
call :verify_file "%LinuxApplication%" 606208 f94d2e16da76c949e15978bd879bff38205685be08d7afa1670f48d3f6592ea1 "Linux WVO inspector application"
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
