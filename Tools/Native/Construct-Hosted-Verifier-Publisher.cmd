@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~2"=="" goto :usage
set "Role=publisher"
if "%~3"=="" (
    set "TargetName=%~1"
    set "Output=%~f2"
) else (
    if not "%~4"=="" goto :usage
    if /I not "%~1"=="publisher" if /I not "%~1"=="promoter" if /I not "%~1"=="wvb-publisher" goto :usage
    set "Role=%~1"
    set "TargetName=%~2"
    set "Output=%~f3"
)
for %%O in ("%Output%") do set "OutputExtension=%%~xO"
if /I "%TargetName%"=="windows" goto :windows
if /I "%TargetName%"=="linux" goto :linux
goto :usage

:windows
if /I not "%OutputExtension%"==".exe" goto :usage
set "Target=1"
set "ConsoleLeaf=Native-X64-Windows-Console-Output-Service.bin"
set "ConsoleBytes=258"
set "ConsoleSha256=10f3a500aca7f0236cdf9f6c20658591df88bc612e677264cdaa0bcef59a0a48"
set "FileInputLeaf=Native-X64-Windows-File-Input-Service.bin"
set "FileInputBytes=1218"
set "FileInputSha256=3e2fd284d4991d0f713301514d3fbf6af8ec84af7bd7289698c08a41d434c52d"
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
if /I "%Role%"=="promoter" (
    set "BaseBytes=674816"
    set "BaseSha256=eede4259e3e9bbf4099f82ff8f26cf6925139801863aeb72d771539b1a3ab9bd"
    set "ApplicationBytes=681472"
    set "ApplicationSha256=9cb234a57c9ff71b6ee44a0d687521e6fd7ccf82784b369e5e65b8ed40666069"
)
if /I "%Role%"=="wvb-publisher" (
    set "BaseBytes=1333760"
    set "BaseSha256=a06095df9ab46b3816c376c2bedc6b07c8e6aff0eaf6c92ff2c2a47d9b210466"
    set "ApplicationBytes=1340928"
    set "ApplicationSha256=9ee91e3044193e2e90461ecf4e7ddefa4b5583f55b041b31911044c6d65b92c7"
)
goto :target_ready

:linux
if /I not "%OutputExtension%"==".elf" goto :usage
set "Target=2"
set "ConsoleLeaf=Native-X64-Linux-Console-Output-Service.bin"
set "ConsoleBytes=213"
set "ConsoleSha256=c5ea073a24c46dd634b1a67a7e7041d476dbce856d058aa8adc2c4e680d3d226"
set "FileInputLeaf=Native-X64-Linux-File-Input-Service.bin"
set "FileInputBytes=996"
set "FileInputSha256=cbd78340641fa02589d0d96b73d233a67f9404ab76c3df2b1346b2e31ca43701"
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
if /I "%Role%"=="promoter" (
    set "BaseBytes=675840"
    set "BaseSha256=b2b299aea10987720714779c9f0b6e58bcea567946d82c4f39786915404039a4"
    set "ApplicationBytes=680901"
    set "ApplicationSha256=9406a1e2610db48e744a0912ab4abb2281856e92f7a0d870292c16105d9b9af0"
)
if /I "%Role%"=="wvb-publisher" (
    set "BaseBytes=1335296"
    set "BaseSha256=57cac655719571d20922bf6b3db33ec77781201ccd4dbd45fc41e14c651eb6ab"
    set "ApplicationBytes=1340357"
    set "ApplicationSha256=2ade91f624609c93a3b80a0802679bef79832c0a63db7996c889794d365f1188"
)

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
set "PublisherObject=%Construction%\Publisher.wvo"
set "Variant=0"
set "PublisherWvbBytes=29170"
set "PublisherWvbSha256=77c6f34a823fc41175647c4d0c4708507ab8b97c7b1726c983188f962fd5509f"
set "PublisherObjectBytes=233804"
set "PublisherObjectSha256=ef0f5e49a07450e3d957e5576f819201849b705097bfbf75432c76d2c438ec23"
set "NativeEntry=3001"
set "FragmentBytes=232736"
set "FragmentSha256=260e9f4f23c99dab13145ceb98724a4c74157fc579c5685194b7312c1a5cb115"
if /I "%Role%"=="promoter" (
    set "PublisherWvb=%Construction%\Publisher-Promoter.wvb"
    set "PublisherObject=%Construction%\Publisher-Promoter.wvo"
    set "Variant=1"
    set "PublisherWvbBytes=41268"
    set "PublisherWvbSha256=30eb1e8c93b01266592b322b9c5154b27782ea6c7cd2b6522a10781bf935bec9"
    set "PublisherObjectBytes=660123"
    set "PublisherObjectSha256=6f20c95c4c09958dcc09ee35b8f7a3a0330d67f26446206be5bdd85cd8cb042d"
    set "NativeEntry=1178"
    set "FragmentBytes=658339"
    set "FragmentSha256=a7c0ef19de332e00dcae74c9ab8c25b16b1e1ca73169d4485c85575412a28ed8"
)
if /I "%Role%"=="wvb-publisher" (
    set "PublisherWvb=%Construction%\Wvb-Publisher.wvb"
    set "PublisherObject=%Construction%\Wvb-Publisher.wvo"
    set "Variant=2"
    set "PublisherWvbBytes=159770"
    set "PublisherWvbSha256=8247539e0f4a5436b3902ec1fef33c6c39c231703de7bf505a6c65d66a764f96"
    set "PublisherObjectBytes=1319377"
    set "PublisherObjectSha256=edc49bbae0bfd16a38db4a08d9a6e636edfac35828e1c6b050c45d85d5e1f9e3"
    set "NativeEntry=0"
    set "FragmentBytes=1317613"
    set "FragmentSha256=9003479563a043bb69113be43100289f653f6772356c48a17098c1c6700f5271"
)
set "ServiceRoot=%RepositoryRoot%\Runtime\Windvale.Native\Consumers"
set "ConsumerRoot=%RepositoryRoot%\Linker\Reference\Consumers"

call :verify_file "%HostedToolset%\SHA256SUMS" 6927 bca5cead0b3698f060c4cc5a165eb75dc52aaad5e81202ef95c54f16976d0ded "hosted toolset inventory"
if errorlevel 1 exit /b 1
for /f "usebackq tokens=1,*" %%H in ("%HostedToolset%\SHA256SUMS") do (
    call :verify_digest "%HostedToolset%\%%I" %%H "hosted toolset artifact"
    if errorlevel 1 exit /b 1
)
call :verify_file "%Construction%\SHA256SUMS" 4980 217c33c4163719f998a3cfbe6694a5f42d07d78e7c50c31fa0358d95f4bad11a "publisher construction inventory"
if errorlevel 1 exit /b 1
for /f "usebackq tokens=1,*" %%H in ("%Construction%\SHA256SUMS") do (
    call :verify_digest "%Construction%\%%I" %%H "publisher construction artifact"
    if errorlevel 1 exit /b 1
)
call :verify_file "%PublisherWvb%" %PublisherWvbBytes% %PublisherWvbSha256% "publisher WVB"
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
call :verify_file "%TemporaryDirectory%\Publisher.wvo" %PublisherObjectBytes% %PublisherObjectSha256% "lowered publisher object"
if errorlevel 1 goto :cleanup
fc /b "%TemporaryDirectory%\Publisher.wvo" "%PublisherObject%" >nul
if errorlevel 1 goto :cleanup
set "Phase=link-command"
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%TemporaryDirectory%\Publisher.bin" "%TemporaryDirectory%\Publisher.wvo" >"%TemporaryDirectory%\Link.txt"
if errorlevel 1 goto :cleanup
set "Phase=link-entry"
set "ReportedEntry="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Main address=" "%TemporaryDirectory%\Link.txt"') do set "ReportedEntry=%%E"
if not "%ReportedEntry%"=="%NativeEntry%" goto :cleanup
set "Phase=link-identity"
call :verify_file "%TemporaryDirectory%\Publisher.bin" %FragmentBytes% %FragmentSha256% "linked publisher fragment"
if errorlevel 1 goto :cleanup

set "Phase=base-bundle-request"
"%HostedToolset%\windows-x64\wvhostverifierbundle.exe" "%TemporaryDirectory%\Publisher.bin" "%ServiceRoot%\%ConsoleLeaf%" "%ServiceRoot%\Native-X64-Argument-Count-Service.bin" "%ServiceRoot%\Native-X64-Argument-Service.bin" "%ServiceRoot%\%FileInputLeaf%" "%ServiceRoot%\Native-X64-Utf8-Service.bin" "%ServiceRoot%\%DiagnosticLeaf%" "%TemporaryDirectory%\Bundle-Request.wvsq" >nul
if errorlevel 1 goto :cleanup
set "Phase=base-metadata"
"%PublisherTools%\wvhostverifierpublisherbasemetadata.exe" %Target% %NativeEntry% "%TemporaryDirectory%\Bundle-Request.wvsq" "%TemporaryDirectory%\Metadata.wvhv"
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
if "%Variant%"=="0" (
    "%PublisherTools%\wvhostverifierproducemetadata.exe" %Target% "%PublisherWvb%" "%ConsumerRoot%\%PublisherStartup%" "%TemporaryDirectory%\Publisher-Metadata.wvvp" >nul
) else (
    "%PublisherTools%\wvhostverifierproducemetadata.exe" %Variant% %Target% "%PublisherWvb%" "%ConsumerRoot%\%PublisherStartup%" "%TemporaryDirectory%\Publisher-Metadata.wvvp" >nul
)
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
if "%Variant%"=="0" (
    "%PublisherTools%\wvhostverifierpublishimports.exe" "%TemporaryDirectory%\Imports.wvim" >nul
) else (
    "%PublisherTools%\wvhostverifierpublishimports.exe" %Role% "%TemporaryDirectory%\Imports.wvim" >nul
)
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
if /I "%Role%"=="publisher" echo publisher construction status=Valid target=%TargetName% bytes=%ApplicationBytes%
if /I "%Role%"=="promoter" echo publisher promoter construction status=Valid target=%TargetName% bytes=%ApplicationBytes%
if /I "%Role%"=="wvb-publisher" echo WVB publisher construction status=Valid target=%TargetName% bytes=%ApplicationBytes%
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
>&2 echo Usage: Tools\Native\Construct-Hosted-Verifier-Publisher.cmd [publisher^|promoter^|wvb-publisher] ^<windows^|linux^> ^<output.exe^|output.elf^>
exit /b 64
