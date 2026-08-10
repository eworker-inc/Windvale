@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~2"=="" goto :usage
set "Role=publisher"
if "%~3"=="" (
    set "TargetName=%~1"
    set "Output=%~f2"
) else (
    if not "%~4"=="" goto :usage
    if /I not "%~1"=="publisher" if /I not "%~1"=="promoter" if /I not "%~1"=="wvb-publisher" if /I not "%~1"=="wvo-publisher" goto :usage
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
    set "BaseSha256=2afd9d92422b063abd3cd20d8da6056efbbbff9e7ac8baeef9c8b60b391686c5"
set "ApplicationBytes=256000"
    set "ApplicationSha256=17cb5c4228e8448693b17f1b73695fd0ecfd03d7ada922794a5bf3bd7594fc96"
if /I "%Role%"=="promoter" (
    set "BaseBytes=674816"
    set "BaseSha256=927476ca389c7449fb0c72341f26d68577a6a9e0c0ed02fa45ac8c4af935c77f"
    set "ApplicationBytes=681472"
    set "ApplicationSha256=598bd2de8247abd19d931efa1edcc8323adef7f56da51da1d41256933667eb23"
)
if /I "%Role%"=="wvb-publisher" (
    set "BaseBytes=1333760"
    set "BaseSha256=8fcdcfc755439ebae5086c72d88113fb52f397ba0687c785af247230a7732fff"
    set "ApplicationBytes=1340928"
    set "ApplicationSha256=71794a6a254ccfd652ffe3bad556c32f86e2d9210a5a3099bad576f97476a8f3"
)
if /I "%Role%"=="wvo-publisher" (
    set "BaseBytes=422912"
    set "BaseSha256=1f9361126c368f133693222cbaa4c21e2d0948e79df7bf945b7b037ac815e884"
    set "ApplicationBytes=430080"
    set "ApplicationSha256=ad4c2a05115b2acdb074c0f53b6d7470c8bcacfdfea86583043bdd0ff511188a"
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
    set "BaseSha256=687338281ca78c9d3a4d08b601c1efbcc198ec3c8fcc96fbf34f5dc349cafae2"
set "ApplicationBytes=254917"
    set "ApplicationSha256=babe721a573e29f89ec095c35677880077ff465d4e2129063f6742cd47591a97"
if /I "%Role%"=="promoter" (
    set "BaseBytes=675840"
    set "BaseSha256=768ca223c99e901d17a1c5d86744515e4b571a6feae329fb6fc3cf225215a133"
    set "ApplicationBytes=680901"
    set "ApplicationSha256=422332fb4f2824ae558bf93adadb6470597399d07810f5428f71aa4d971a4f58"
)
if /I "%Role%"=="wvb-publisher" (
    set "BaseBytes=1335296"
    set "BaseSha256=f53a4c8c5d292e999735cf5fd337b7c6997c0a8e6d2ba316ec94cd6b0838b090"
    set "ApplicationBytes=1340357"
    set "ApplicationSha256=7f2dbfaecf2734c5afdbd6e2e54263a5a74038b8a498eeb1e155ee71788b630c"
)
if /I "%Role%"=="wvo-publisher" (
    set "BaseBytes=421888"
    set "BaseSha256=af61a601f4cd8e7fb81704353160a518d2e4f199084fde4b29518d27c89774f7"
    set "ApplicationBytes=426949"
    set "ApplicationSha256=4b0ce2d332648e3dd572596db4490748bf62ee4448a9550d83c152de60f7e51d"
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
    set "PublisherWvbSha256=c0c7c88996ef837bc5a2ec3ceb1de61254b025fbd6504e4f3d7dc055c4140672"
    set "PublisherObjectBytes=660123"
    set "PublisherObjectSha256=ba5d9c5afde115fede472369d24c3d1fe466806de523773d2e445e6a9e004667"
    set "NativeEntry=1178"
    set "FragmentBytes=658339"
    set "FragmentSha256=e06189a37c038a5237787ffd16fb53466df3d10519efd4129b219bd814f4def2"
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
if /I "%Role%"=="wvo-publisher" (
    set "PublisherWvb=%RepositoryRoot%\Artifacts\Native-Wvo-Publisher-Candidate\Wvo-Publisher.wvb"
    set "PublisherObject=%Construction%\Wvo-Publisher.wvo"
    set "Variant=3"
    set "PublisherWvbBytes=41365"
    set "PublisherWvbSha256=4e8c81da38f5eb06f9334c2d2c5e35120a13e73bac3a9375b5e6a2eff04438c5"
    set "PublisherObjectBytes=408284"
    set "PublisherObjectSha256=29c1cc269b9387944b4d43fe9215392044996ad47da55be45a1d177f26e5bafb"
    set "NativeEntry=0"
    set "FragmentBytes=406840"
    set "FragmentSha256=591231b7900aecea5700e139dfd67e36afa3e04a68a87d255aa2be3eb852c828"
)
set "ServiceRoot=%RepositoryRoot%\Runtime\Windvale.Native\Consumers"
set "ConsumerRoot=%RepositoryRoot%\Linker\Reference\Consumers"
set "RawLowerer=%RepositoryRoot%\Artifacts\Native-Wvb-To-Wvo-Candidate\Wvb-To-Wvo.exe"

call :verify_file "%HostedToolset%\SHA256SUMS" 6927 bca5cead0b3698f060c4cc5a165eb75dc52aaad5e81202ef95c54f16976d0ded "hosted toolset inventory"
if errorlevel 1 exit /b 1
for /f "usebackq tokens=1,*" %%H in ("%HostedToolset%\SHA256SUMS") do (
    call :verify_digest "%HostedToolset%\%%I" %%H "hosted toolset artifact"
    if errorlevel 1 exit /b 1
)
call :verify_file "%Construction%\SHA256SUMS" 5064 90538e48d5ad87509f070b4c8cc954d0ae1d4dae3f1b0f0a3c629b58bb0e990c "publisher construction inventory"
if errorlevel 1 exit /b 1
for /f "usebackq tokens=1,*" %%H in ("%Construction%\SHA256SUMS") do (
    call :verify_digest "%Construction%\%%I" %%H "publisher construction artifact"
    if errorlevel 1 exit /b 1
)
call :verify_file "%PublisherWvb%" %PublisherWvbBytes% %PublisherWvbSha256% "publisher WVB"
if errorlevel 1 exit /b 1
if /I "%Role%"=="wvo-publisher" (
    call :verify_file "%RawLowerer%" 5958144 927cbdf8b89269538ea2af1131276e4edca3e8810c1edaa3c7fd096e3528a267 "raw native WVB-to-WVO lowerer"
    if errorlevel 1 exit /b 1
)
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

if /I "%Role%"=="wvo-publisher" (
    "%RawLowerer%" "%PublisherWvb%" "%TemporaryDirectory%\Publisher.wvo" >nul
) else (
    call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%PublisherWvb%" "%TemporaryDirectory%\Publisher.wvo" >nul
)
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
if /I "%Role%"=="wvo-publisher" echo WVO publisher construction status=Valid target=%TargetName% bytes=%ApplicationBytes%
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
>&2 echo Usage: Tools\Native\Construct-Hosted-Verifier-Publisher.cmd [publisher^|promoter^|wvb-publisher^|wvo-publisher] ^<windows^|linux^> ^<output.exe^|output.elf^>
exit /b 64
