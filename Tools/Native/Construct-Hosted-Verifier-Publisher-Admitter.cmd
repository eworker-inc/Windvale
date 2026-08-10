@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~2"=="" goto :usage
if not "%~3"=="" goto :usage
set "TargetName=%~1"
set "Output=%~f2"
if /I "%TargetName%"=="windows" goto :windows
if /I "%TargetName%"=="linux" goto :linux
goto :usage

:windows
if /I not "%~x2"==".exe" goto :usage
set "Target=1"
set "ConsoleLeaf=Native-X64-Windows-Console-Output-Service.bin"
set "FileInputLeaf=Native-X64-Windows-File-Input-Service.bin"
set "DiagnosticLeaf=Native-X64-Windows-Diagnostic-Output-Service.bin"
set "HostedStartup=Windows-X64-Hosted-Verifier.wvo"
set "ApplicationBytes=570368"
    set "ApplicationSha256=4742ee299759728be1b72fed3d3b42620c21b10f77aed12cf150c1549b177b53"
goto :target_ready

:linux
if /I not "%~x2"==".elf" goto :usage
set "Target=2"
set "ConsoleLeaf=Native-X64-Linux-Console-Output-Service.bin"
set "FileInputLeaf=Native-X64-Linux-File-Input-Service.bin"
set "DiagnosticLeaf=Native-X64-Linux-Diagnostic-Output-Service.bin"
set "HostedStartup=Linux-X64-Hosted-Verifier.wvo"
set "ApplicationBytes=569344"
    set "ApplicationSha256=b03788fad58ce071788b2f30945ed1dc0992559bb04b6cad04e719ff1114dc0a"

:target_ready
if exist "%Output%" (
    >&2 echo Refusing to replace an existing publisher-admitter construction output.
    exit /b 1
)
set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "HostedToolset=%RepositoryRoot%\Artifacts\Native-Hosted-Container-Toolset-Candidate"
set "Construction=%RepositoryRoot%\Artifacts\Native-Hosted-Verifier-Publisher-Construction-Candidate"
set "Tools=%Construction%\windows-x64"
set "ServiceRoot=%RepositoryRoot%\Runtime\Windvale.Native\Consumers"
set "ConsumerRoot=%RepositoryRoot%\Linker\Reference\Consumers"

call :verify_file "%HostedToolset%\SHA256SUMS" 6927 bca5cead0b3698f060c4cc5a165eb75dc52aaad5e81202ef95c54f16976d0ded "hosted toolset inventory"
if errorlevel 1 exit /b 1
for /f "usebackq tokens=1,*" %%H in ("%HostedToolset%\SHA256SUMS") do (
    call :verify_digest "%HostedToolset%\%%I" %%H "hosted toolset artifact"
    if errorlevel 1 exit /b 1
)
call :verify_file "%Construction%\SHA256SUMS" 4980 4989e21858705df8fb1776b36a26350144b6bf02fab5bd8d910e1711f2a7691d "publisher construction inventory"
if errorlevel 1 exit /b 1
for /f "usebackq tokens=1,*" %%H in ("%Construction%\SHA256SUMS") do (
    call :verify_digest "%Construction%\%%I" %%H "publisher construction artifact"
    if errorlevel 1 exit /b 1
)

:allocate
set "TemporaryDirectory=%TEMP%\windvale-hosted-verifier-publisher-admitter-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "Result=1"
set "Phase=link"

call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%TemporaryDirectory%\Admission.bin" "%Construction%\Publisher-Application-Admission-Tool.wvo" >"%TemporaryDirectory%\Link.txt"
if errorlevel 1 goto :cleanup
findstr /b /c:"entry name=Main address=0" "%TemporaryDirectory%\Link.txt" >nul
if errorlevel 1 goto :cleanup
call :verify_file "%TemporaryDirectory%\Admission.bin" 554354 1abb04300b4f1e046884efa3d5ddfbb8934c86e34a2a01cb43f30561318652e4 "publisher-admission fragment"
if errorlevel 1 goto :cleanup

set "Phase=bundle-request"
"%HostedToolset%\windows-x64\wvhostverifierbundle.exe" "%TemporaryDirectory%\Admission.bin" "%ServiceRoot%\%ConsoleLeaf%" "%ServiceRoot%\Native-X64-Argument-Count-Service.bin" "%ServiceRoot%\Native-X64-Argument-Service.bin" "%ServiceRoot%\%FileInputLeaf%" "%ServiceRoot%\Native-X64-Utf8-Service.bin" "%ServiceRoot%\%DiagnosticLeaf%" "%TemporaryDirectory%\Bundle.wvsq" >nul
if errorlevel 1 goto :cleanup
set "Phase=metadata"
"%Tools%\wvhostverifierpublisherbasemetadata.exe" publisher-admission %Target% 0 "%TemporaryDirectory%\Bundle.wvsq" "%TemporaryDirectory%\Metadata.wvhv"
if errorlevel 1 goto :cleanup
set "Phase=runtime"
"%Tools%\wvhostverifierpublisherbaseruntime.exe" "%TemporaryDirectory%\Metadata.wvhv" "%TemporaryDirectory%\Runtime.wvhr"
if errorlevel 1 goto :cleanup
set "Phase=bundle"
"%HostedToolset%\windows-x64\wvhostbundle.exe" "%TemporaryDirectory%\Bundle.wvsq" "%TemporaryDirectory%\Bundle.wvsi" >nul
if errorlevel 1 goto :cleanup
set "Phase=platform"
"%HostedToolset%\windows-x64\wvhostverifierbytes.exe" publisher-admission "%TemporaryDirectory%\Runtime.wvhr" "%TemporaryDirectory%\Platform.wvhb" >nul
if errorlevel 1 goto :cleanup
set "Phase=startup"
"%HostedToolset%\windows-x64\wvhostverifierstartup.exe" publisher-admission "%TemporaryDirectory%\Runtime.wvhr" "%ConsumerRoot%\%HostedStartup%" "%TemporaryDirectory%\Startup.wvsd" >nul
if errorlevel 1 goto :cleanup
set "Phase=compose"
"%HostedToolset%\windows-x64\wvhostverifiercompose.exe" publisher-admission "%TemporaryDirectory%\Runtime.wvhr" "%TemporaryDirectory%\Platform.wvhb" "%TemporaryDirectory%\Startup.wvsd" "%TemporaryDirectory%\Bundle.wvsi" "%TemporaryDirectory%\Admitter.application" >nul
if errorlevel 1 goto :cleanup
call :verify_file "%TemporaryDirectory%\Admitter.application" %ApplicationBytes% %ApplicationSha256% "completed publisher admitter"
if errorlevel 1 goto :cleanup
copy /b "%TemporaryDirectory%\Admitter.application" "%Output%" >nul
if errorlevel 1 goto :cleanup_output
call :verify_file "%Output%" %ApplicationBytes% %ApplicationSha256% "published construction output"
if errorlevel 1 goto :cleanup_output
echo publisher admitter construction status=Valid target=%TargetName% bytes=%ApplicationBytes%
set "Result=0"
goto :cleanup

:cleanup_output
if exist "%Output%" del /f /q "%Output%" >nul 2>nul

:cleanup
if not "%Result%"=="0" >&2 echo publisher admitter construction status=Rejected phase=%Phase%
del /f /q "%TemporaryDirectory%\*" >nul 2>nul
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b %Result%

:verify_file
if not exist "%~1" (
    >&2 echo Missing %~4: %~1
    exit /b 1
)
for %%F in ("%~1") do if not "%%~zF"=="%~2" (
    >&2 echo The %~4 byte length is invalid.
    exit /b 1
)
call :verify_digest "%~1" %~3 "%~4"
exit /b %ERRORLEVEL%

:verify_digest
certutil -hashfile "%~1" SHA256 | findstr /i /c:"%~2" >nul
if errorlevel 1 (
    >&2 echo The %~3 digest is invalid: %~1
    exit /b 1
)
exit /b 0

:usage
>&2 echo Usage: Tools\Native\Construct-Hosted-Verifier-Publisher-Admitter.cmd ^<windows^|linux^> ^<output.exe^|output.elf^>
exit /b 64
