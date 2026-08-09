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
set "ConsoleBytes=258"
set "ConsoleSha256=10f3a500aca7f0236cdf9f6c20658591df88bc612e677264cdaa0bcef59a0a48"
set "FileInputLeaf=Native-X64-Windows-File-Input-Service.bin"
set "FileInputBytes=1218"
set "FileInputSha256=3d2fffc028083cdc4cfd39e553dea603e9a1ae661bb5df3f14ca438c4d3e3cf8"
set "DiagnosticLeaf=Native-X64-Windows-Diagnostic-Output-Service.bin"
set "DiagnosticBytes=258"
set "DiagnosticSha256=1b4068c01b2050c3055c78eb82303c71b8488e8766f7b628fab10ffb23e5ffe2"
set "HostedStartup=Windows-X64-Hosted-Verifier.wvo"
set "HostedStartupBytes=3561"
set "HostedStartupSha256=755ffb99cba6a838dd9eec353ce72d4adfb3af130ec4bce5a2278828dd136616"
set "PublisherStartup=Windows-X64-Wvb-Publisher.wvo"
set "PublisherStartupBytes=168"
set "PublisherStartupSha256=bb136af0382b2f72efc8a07f58fb2368319fce7c119bc7bbfa1b94da6ded9367"
set "Adapter=Windows-X64-Wvb-Publication-Adapter.wvo"
set "AdapterBytes=9544"
set "AdapterSha256=ef795dabbced735e0808fca04d0205b87d3735b26dd53ca23ed57a7e74453e93"
set "BaseBytes=248832"
set "BaseSha256=cf204201e5c26d71e78da1112de2bc724d389a5222cc835d48dbe8cd8bbc5988"
set "ApplicationBytes=256000"
set "ApplicationSha256=735320b5ff33419d685925044add6f254bf402c0d49fc575c77f6110fac705f6"
goto :target_ready

:linux
if /I not "%~x2"==".elf" goto :usage
set "Target=2"
set "ConsoleLeaf=Native-X64-Linux-Console-Output-Service.bin"
set "ConsoleBytes=213"
set "ConsoleSha256=c5ea073a24c46dd634b1a67a7e7041d476dbce856d058aa8adc2c4e680d3d226"
set "FileInputLeaf=Native-X64-Linux-File-Input-Service.bin"
set "FileInputBytes=996"
set "FileInputSha256=55ae4524c463f064aee0964d7f9b64438701fb4375a97c53d11f2f17902c12cb"
set "DiagnosticLeaf=Native-X64-Linux-Diagnostic-Output-Service.bin"
set "DiagnosticBytes=213"
set "DiagnosticSha256=1c81018143fa9b708373eaceda62722ca40fb1e11b20808f765fe5ece33406fe"
set "HostedStartup=Linux-X64-Hosted-Verifier.wvo"
set "HostedStartupBytes=1925"
set "HostedStartupSha256=08a7afefb69904af8d8c899a86bec76e957dfe255d397dbd9015d9acaa018ae8"
set "PublisherStartup=Linux-X64-Wvb-Publisher.wvo"
set "PublisherStartupBytes=164"
set "PublisherStartupSha256=eee997412ced0d7edacaf39dae9c4a3c51e859dce4537045f3972be990b115a4"
set "Adapter=Linux-X64-Wvb-Publication-Adapter.wvo"
set "AdapterBytes=5507"
set "AdapterSha256=9272c17b0d7234218a6cd7c31131e9d25e62b6c1ccd976d94975e9b436b2ca5a"
set "BaseBytes=249856"
set "BaseSha256=0bdeee07a49f75781767934884cbbc7dd085abff4507e2f78210fa225638539a"
set "ApplicationBytes=254917"
set "ApplicationSha256=de4f06f6d837eb58457a31b4757c3410e389ecc3c11fd79daf229dbdeb23e02a"

:target_ready
if exist "%Output%" (
    >&2 echo Refusing to replace an existing publisher construction output.
    exit /b 1
)
set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "HostedToolset=%RepositoryRoot%\Artifacts\Native-Hosted-Container-Toolset-Candidate"
set "Construction=%RepositoryRoot%\Artifacts\Native-Hosted-Verifier-Publisher-Construction-Candidate"
set "PublisherTools=%Construction%\windows-x64"
set "PublisherWvb=%RepositoryRoot%\Artifacts\Native-Hosted-Verifier-Application-Publisher-Candidate\Hosted-Verifier-Application-Publisher.wvb"
set "ServiceRoot=%RepositoryRoot%\Runtime\Windvale.Native\Consumers"
set "ConsumerRoot=%RepositoryRoot%\Linker\Reference\Consumers"

call :verify_file "%HostedToolset%\SHA256SUMS" 6927 a7eb43d58a81ee57881f800b2c17b70c2014c26ce4454fa299feb2986348fb58 "hosted toolset inventory"
if errorlevel 1 exit /b 1
for /f "usebackq tokens=1,*" %%H in ("%HostedToolset%\SHA256SUMS") do (
    call :verify_digest "%HostedToolset%\%%I" %%H "hosted toolset artifact"
    if errorlevel 1 exit /b 1
)
call :verify_file "%Construction%\SHA256SUMS" 4420 430645441d930284089684ac125bfefc6d57d5cbd3e26612a951964767bcd6d5 "publisher construction inventory"
if errorlevel 1 exit /b 1
for /f "usebackq tokens=1,*" %%H in ("%Construction%\SHA256SUMS") do (
    call :verify_digest "%Construction%\%%I" %%H "publisher construction artifact"
    if errorlevel 1 exit /b 1
)
call :verify_file "%PublisherWvb%" 29170 77c6f34a823fc41175647c4d0c4708507ab8b97c7b1726c983188f962fd5509f "publisher WVB"
if errorlevel 1 exit /b 1
call :verify_file "%ServiceRoot%\%ConsoleLeaf%" %ConsoleBytes% %ConsoleSha256% "console service"
if errorlevel 1 exit /b 1
call :verify_file "%ServiceRoot%\Native-X64-Argument-Count-Service.bin" 5 2358e7e2c72d6476cfe05134db4f0eb5e6987fcca1b10894a8588a28d3929829 "argument-count service"
if errorlevel 1 exit /b 1
call :verify_file "%ServiceRoot%\Native-X64-Argument-Service.bin" 70 2253e1435f141df5b68f9f7e9e9aa0de448410c42dcf33ad76dcf131afea65d1 "argument service"
if errorlevel 1 exit /b 1
call :verify_file "%ServiceRoot%\%FileInputLeaf%" %FileInputBytes% %FileInputSha256% "file-input service"
if errorlevel 1 exit /b 1
call :verify_file "%ServiceRoot%\Native-X64-Utf8-Service.bin" 800 4c3d2e370d62c8d2f54a3c453f39b94cf46ddabd6db3c2f3d6b65f0713b68aaf "UTF-8 service"
if errorlevel 1 exit /b 1
call :verify_file "%ServiceRoot%\%DiagnosticLeaf%" %DiagnosticBytes% %DiagnosticSha256% "diagnostic service"
if errorlevel 1 exit /b 1
call :verify_file "%ConsumerRoot%\%HostedStartup%" %HostedStartupBytes% %HostedStartupSha256% "hosted-verifier startup object"
if errorlevel 1 exit /b 1
call :verify_file "%ConsumerRoot%\%PublisherStartup%" %PublisherStartupBytes% %PublisherStartupSha256% "publisher startup object"
if errorlevel 1 exit /b 1
call :verify_file "%ConsumerRoot%\%Adapter%" %AdapterBytes% %AdapterSha256% "publication adapter object"
if errorlevel 1 exit /b 1
call :verify_file "%ConsumerRoot%\X64-Wvb-Publication-Sha256.wvo" 2176 380af02cf29f85be1f63a4ea1f02ca3cc027e63091659e214a023b03730f6608 "publication SHA-256 object"
if errorlevel 1 exit /b 1

:allocate
set "TemporaryDirectory=%TEMP%\windvale-hosted-verifier-publisher-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "Result=1"
set "Phase=lower"

call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%PublisherWvb%" "%TemporaryDirectory%\Publisher.wvo" >nul
if errorlevel 1 goto :cleanup
call :verify_file "%TemporaryDirectory%\Publisher.wvo" 233804 ef0f5e49a07450e3d957e5576f819201849b705097bfbf75432c76d2c438ec23 "lowered publisher object"
if errorlevel 1 goto :cleanup
fc /b "%TemporaryDirectory%\Publisher.wvo" "%Construction%\Publisher.wvo" >nul
if errorlevel 1 goto :cleanup
set "Phase=link-command"
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%TemporaryDirectory%\Publisher.bin" "%TemporaryDirectory%\Publisher.wvo" >"%TemporaryDirectory%\Link.txt"
if errorlevel 1 goto :cleanup
set "Phase=link-entry"
set "NativeEntry="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Main address=" "%TemporaryDirectory%\Link.txt"') do set "NativeEntry=%%E"
if not "%NativeEntry%"=="3001" goto :cleanup
set "Phase=link-identity"
call :verify_file "%TemporaryDirectory%\Publisher.bin" 232736 260e9f4f23c99dab13145ceb98724a4c74157fc579c5685194b7312c1a5cb115 "linked publisher fragment"
if errorlevel 1 goto :cleanup

set "Phase=base-bundle-request"
"%HostedToolset%\windows-x64\wvhostverifierbundle.exe" "%TemporaryDirectory%\Publisher.bin" "%ServiceRoot%\%ConsoleLeaf%" "%ServiceRoot%\Native-X64-Argument-Count-Service.bin" "%ServiceRoot%\Native-X64-Argument-Service.bin" "%ServiceRoot%\%FileInputLeaf%" "%ServiceRoot%\Native-X64-Utf8-Service.bin" "%ServiceRoot%\%DiagnosticLeaf%" "%TemporaryDirectory%\Bundle-Request.wvsq" >nul
if errorlevel 1 goto :cleanup
set "Phase=base-metadata"
"%PublisherTools%\wvhostverifierpublisherbasemetadata.exe" %Target% 3001 "%TemporaryDirectory%\Bundle-Request.wvsq" "%TemporaryDirectory%\Metadata.wvhv"
if errorlevel 1 goto :cleanup
set "Phase=base-runtime"
"%PublisherTools%\wvhostverifierpublisherbaseruntime.exe" "%TemporaryDirectory%\Metadata.wvhv" "%TemporaryDirectory%\Runtime.wvhr"
if errorlevel 1 goto :cleanup
set "Phase=base-bundle"
"%HostedToolset%\windows-x64\wvhostbundle.exe" "%TemporaryDirectory%\Bundle-Request.wvsq" "%TemporaryDirectory%\Bundle.wvsi" >nul
if errorlevel 1 goto :cleanup
set "Phase=base-platform"
"%HostedToolset%\windows-x64\wvhostverifierbytes.exe" "%TemporaryDirectory%\Runtime.wvhr" "%TemporaryDirectory%\Platform.wvhb" >nul
if errorlevel 1 goto :cleanup
set "Phase=base-startup"
"%HostedToolset%\windows-x64\wvhostverifierstartup.exe" "%TemporaryDirectory%\Runtime.wvhr" "%ConsumerRoot%\%HostedStartup%" "%TemporaryDirectory%\Startup.wvsd" >nul
if errorlevel 1 goto :cleanup
set "Phase=base-compose"
"%HostedToolset%\windows-x64\wvhostverifiercompose.exe" "%TemporaryDirectory%\Runtime.wvhr" "%TemporaryDirectory%\Platform.wvhb" "%TemporaryDirectory%\Startup.wvsd" "%TemporaryDirectory%\Bundle.wvsi" "%TemporaryDirectory%\Base.application" >nul
if errorlevel 1 goto :cleanup
call :verify_file "%TemporaryDirectory%\Base.application" %BaseBytes% %BaseSha256% "publisher base application"
if errorlevel 1 goto :cleanup

set "Phase=publisher-metadata"
"%PublisherTools%\wvhostverifierproducemetadata.exe" %Target% "%PublisherWvb%" "%ConsumerRoot%\%PublisherStartup%" "%TemporaryDirectory%\Publisher-Metadata.wvvp" >nul
if errorlevel 1 goto :cleanup
set "Phase=publisher-identity"
"%PublisherTools%\wvhostverifieridentity.exe" %Target% "%PublisherWvb%" "%TemporaryDirectory%\Publisher.wvo" "%ConsumerRoot%\%PublisherStartup%" "%ConsumerRoot%\%Adapter%" "%ConsumerRoot%\X64-Wvb-Publication-Sha256.wvo" "%TemporaryDirectory%\Publisher-Metadata.wvvp" "%TemporaryDirectory%\Identity.wvpi" >nul
if errorlevel 1 goto :cleanup
set "Phase=publisher-structure"
"%PublisherTools%\wvhostverifierstructure.exe" "%TemporaryDirectory%\Identity.wvpi" "%TemporaryDirectory%\Structure.wvps" >nul
if errorlevel 1 goto :cleanup
set "Phase=publisher-construction-request"
"%PublisherTools%\wvhostverifierconstructrequest.exe" "%TemporaryDirectory%\Structure.wvps" "%TemporaryDirectory%\Construction.wvcr" >nul
if errorlevel 1 goto :cleanup
set "Phase=publisher-targets"
"%PublisherTools%\wvhostverifiertargets.exe" "%TemporaryDirectory%\Structure.wvps" "%TemporaryDirectory%\Targets.wvpt" >nul
if errorlevel 1 goto :cleanup
set "Phase=publisher-objects"
"%PublisherTools%\wvhostverifierpublishobjects.exe" "%TemporaryDirectory%\Construction.wvcr" "%TemporaryDirectory%\Targets.wvpt" "%ConsumerRoot%\%PublisherStartup%" "%ConsumerRoot%\%Adapter%" "%ConsumerRoot%\X64-Wvb-Publication-Sha256.wvo" "%TemporaryDirectory%\Objects.wvio" >nul
if errorlevel 1 goto :cleanup
if "%Target%"=="1" goto :materialize_windows
"%PublisherTools%\wvhostverifierpublishlinux.exe" "%TemporaryDirectory%\Base.application" "%TemporaryDirectory%\Construction.wvcr" "%TemporaryDirectory%\Objects.wvio" "%TemporaryDirectory%\Publisher-Metadata.wvvp" "%TemporaryDirectory%\Publisher.application" >nul
if errorlevel 1 goto :cleanup
goto :materialized

:materialize_windows
set "Phase=publisher-imports"
"%PublisherTools%\wvhostverifierpublishimports.exe" "%TemporaryDirectory%\Imports.wvim" >nul
if errorlevel 1 goto :cleanup
set "Phase=publisher-windows-materialization"
"%PublisherTools%\wvhostverifierpublishwindows.exe" "%TemporaryDirectory%\Base.application" "%TemporaryDirectory%\Construction.wvcr" "%TemporaryDirectory%\Objects.wvio" "%TemporaryDirectory%\Publisher-Metadata.wvvp" "%TemporaryDirectory%\Imports.wvim" "%TemporaryDirectory%\Publisher.application" >nul
if errorlevel 1 goto :cleanup

:materialized
set "Phase=completed-application"
call :verify_file "%TemporaryDirectory%\Publisher.application" %ApplicationBytes% %ApplicationSha256% "completed publisher application"
if errorlevel 1 goto :cleanup
copy /b "%TemporaryDirectory%\Publisher.application" "%Output%" >nul
if errorlevel 1 goto :cleanup_output
call :verify_file "%Output%" %ApplicationBytes% %ApplicationSha256% "published construction output"
if errorlevel 1 goto :cleanup_output
echo publisher construction status=Valid target=%TargetName% bytes=%ApplicationBytes%
set "Result=0"
goto :cleanup

:cleanup_output
if exist "%Output%" del /f /q "%Output%" >nul 2>nul

:cleanup
if not "%Result%"=="0" >&2 echo publisher construction status=Rejected phase=%Phase%
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
if not exist "%~1" (
    >&2 echo Missing %~3: %~1
    exit /b 1
)
certutil -hashfile "%~1" SHA256 | findstr /i /c:"%~2" >nul
if errorlevel 1 (
    >&2 echo The %~3 digest is invalid: %~1
    exit /b 1
)
exit /b 0

:usage
>&2 echo Usage: Tools\Native\Construct-Hosted-Verifier-Publisher.cmd ^<windows^|linux^> ^<output.exe^|output.elf^>
exit /b 64
