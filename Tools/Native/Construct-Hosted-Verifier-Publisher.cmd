@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~2"=="" goto :usage
set "Role=publisher"
if "%~3"=="" (
    set "TargetName=%~1"
    set "Output=%~f2"
) else (
    if not "%~4"=="" goto :usage
    if /I not "%~1"=="publisher" if /I not "%~1"=="promoter" if /I not "%~1"=="wvb-publisher" if /I not "%~1"=="wvo-publisher" if /I not "%~1"=="console-application-publisher" goto :usage
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
set "HostedStartupBytes=3651"
set "HostedStartupSha256=4d97a1f30d9c871f2a72911cea2644b32d3ea29a2dbbc76105ec4ab1d001b95f"
set "PublisherStartup=Windows-X64-Wvb-Publisher.wvo"
set "PublisherStartupBytes=168"
set "PublisherStartupSha256=bb136af0382b2f72efc8a07f58fb2368319fce7c119bc7bbfa1b94da6ded9367"
set "Adapter=Windows-X64-Wvb-Publication-Adapter.wvo"
set "AdapterBytes=9544"
set "AdapterSha256=ef795dabbced735e0808fca04d0205b87d3735b26dd53ca23ed57a7e74453e93"
set "BaseBytes=248832"
    set "BaseSha256=579ff68d6645797a08c71a3ead03be6a56c2b4fd7eda8a3db548038eb9ccc007"
set "ApplicationBytes=256000"
    set "ApplicationSha256=2b165f5029798a4d5467412b65cba0ddffb05dfc449144fd80161d6117784e12"
if /I "%Role%"=="promoter" (
    set "BaseBytes=674816"
    set "BaseSha256=818b1dcb4ad7145f2beee18c5e9afbb2e5aeab3bb56df905a5f07ae8eb3082ec"
    set "ApplicationBytes=681472"
    set "ApplicationSha256=5690fb32c7fec85551e0c5cd58e4f56589a5ad4c09108b5dde86fa9fc7b3fb92"
)
if /I "%Role%"=="wvb-publisher" (
    set "BaseBytes=1363968"
    set "BaseSha256=243b763d8b49b34108585c56f46c90190eac085a80c59873c8a2cb3e88d16102"
    set "ApplicationBytes=1371136"
    set "ApplicationSha256=b9fd1b11bc1e4a726e4a43b16830a9351fe573b30e547ba8d8f6660f688ed421"
)
if /I "%Role%"=="wvo-publisher" (
    set "BaseBytes=422912"
    set "BaseSha256=22534a8a0ae42e977cd79daa3ff8b6fde5ef39d719edda07726410f95df6683d"
    set "ApplicationBytes=430080"
    set "ApplicationSha256=76f632ffa7998a6cce0386456fee98f02cbb5ec424d0d914a7e1f06ff3853910"
)
if /I "%Role%"=="console-application-publisher" (
    set "BaseBytes=1151488"
    set "BaseSha256=23bf32201666f99af52015d9b3c10ab27d48f088cb766c8701f3f1973b7ab69b"
    set "ApplicationBytes=1158656"
    set "ApplicationSha256=0bafe84096859f4b88dc14be92c6cdc5336d791b7c5b0a332dccb76b913dd24e"
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
set "AdapterBytes=5559"
set "AdapterSha256=1a97195d846626276f38dbb44be68a696dd057f701918f66eb46f6e9d7b5999e"
set "BaseBytes=249856"
    set "BaseSha256=577bda8af2b1d8fca6f37e894c6b7f920e547f3e2b0bd1a28d2af518743a6629"
set "ApplicationBytes=254965"
    set "ApplicationSha256=8c9a1dbbb177041c61e4606696ce9ddf9225a98407a7d3af0a4338069a15979e"
if /I "%Role%"=="promoter" (
    set "BaseBytes=675840"
    set "BaseSha256=848ee9ed30ffc5094f77b4f79b72e3b4a426b4f9e0fc8e26631ed6619596f782"
    set "ApplicationBytes=680949"
    set "ApplicationSha256=3cd1c82807495e34445345b5e61b8c5911434c84d2a6f49a11b21fd2521423f5"
)
if /I "%Role%"=="wvb-publisher" (
    set "BaseBytes=1363968"
    set "BaseSha256=2fc0332887c96ad0fa34d1987091d60ddbbe61f019739d41734cd491b8ca4b64"
    set "ApplicationBytes=1369077"
    set "ApplicationSha256=b8efb90f7d7c4eae99de01df6c0a3c24a7396d9b9e717ff69d005282ed3d63af"
)
if /I "%Role%"=="wvo-publisher" (
    set "BaseBytes=421888"
    set "BaseSha256=af61a601f4cd8e7fb81704353160a518d2e4f199084fde4b29518d27c89774f7"
    set "ApplicationBytes=426997"
    set "ApplicationSha256=2889237d7fdb20b1d420c05834f19183d18b02112e3f4eea0ed7ff43414814f2"
)
if /I "%Role%"=="console-application-publisher" (
    set "BaseBytes=1150976"
    set "BaseSha256=a12ab6d136b53c53322d4b7ff612a5f41a2653c30210a4f5dbfb27027bc29f5e"
    set "ApplicationBytes=1156085"
    set "ApplicationSha256=e9b8771978c9fb06c3a8ecc55c7b9a3ba1acd24faa541dc669920c10ed792925"
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
set "PublisherWvbSha256=7ecbd7f0b11bdd7ce0ab578767b1d697bc16653e4f8182858e0ad8b8d808fb9e"
set "PublisherObjectBytes=233804"
set "PublisherObjectSha256=fa18dbf680fd30f4bc9a5ab5ea8806d958f8af3be304e4e7791337e1a043418a"
set "NativeEntry=3001"
set "FragmentBytes=232736"
set "FragmentSha256=c54b79c39810ba1e47adf332be46a05497b4e8436372376ea2080a526e6d89a8"
if /I "%Role%"=="promoter" (
    set "PublisherWvb=%Construction%\Publisher-Promoter.wvb"
    set "PublisherObject=%Construction%\Publisher-Promoter.wvo"
    set "Variant=1"
    set "PublisherWvbBytes=41268"
    set "PublisherWvbSha256=7ea1cda2842c4258f654ee17deb441c1b06a3fcedfc29f7382e9259b2f3800fe"
    set "PublisherObjectBytes=660123"
    set "PublisherObjectSha256=9ee875a6668b1661087dc6a59384c2427e6ef6febb5c83a4ed936e56cd13b44f"
    set "NativeEntry=1178"
    set "FragmentBytes=658339"
    set "FragmentSha256=843094cf8ba3de92697568abab6788a276f0ea7bd193e65abfb5c7b56918fb43"
)
if /I "%Role%"=="wvb-publisher" (
    set "PublisherWvb=%Construction%\Wvb-Publisher.wvb"
    set "PublisherObject=%Construction%\Wvb-Publisher.wvo"
    set "Variant=2"
    set "PublisherWvbBytes=163300"
    set "PublisherWvbSha256=9ebfe92eef070dfdcf18c4d176b5f32f64ad3f80751340b8a59ab2f1d567ec2a"
    set "PublisherObjectBytes=1349361"
    set "PublisherObjectSha256=43a594776b4e280575ac14e2866b4708961dd1290d643b41779a4933a8ba5991"
    set "NativeEntry=0"
    set "FragmentBytes=1347597"
    set "FragmentSha256=3d419d28b606408e7b2430cceacf4c0b7b109bcd511df4e98ca0d41b871f1c2d"
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
if /I "%Role%"=="console-application-publisher" (
    set "PublisherWvb=%RepositoryRoot%\Artifacts\Native-Console-Application-Publisher-Candidate\Console-Application-Publisher.wvb"
    set "PublisherObject=%RepositoryRoot%\Artifacts\Native-Console-Application-Publisher-Candidate\Console-Application-Publisher.wvo"
    set "Variant=4"
    set "PublisherWvbBytes=115107"
    set "PublisherWvbSha256=e8121fb76c7cc39b159d53a3c28d1da8bc2d44968d630495c692a7761656923d"
    set "PublisherObjectBytes=1139440"
    set "PublisherObjectSha256=259c7d746c3a217c32706bfd617cf66894066bd2e50850cbe5733ac3338e4952"
    set "NativeEntry=18902"
    set "FragmentBytes=1135424"
    set "FragmentSha256=c6b199644be8ca19cce0110a5090e84c736220a130f9b48a4366caf36254e6e2"
)
set "ServiceRoot=%RepositoryRoot%\Runtime\Windvale.Native\Consumers"
set "ConsumerRoot=%RepositoryRoot%\Linker\Reference\Consumers"
set "RawLowerer=%RepositoryRoot%\Artifacts\Native-Wvb-To-Wvo-Candidate\Wvb-To-Wvo.exe"

call :verify_file "%HostedToolset%\SHA256SUMS" 6927 3051a9c328c04a53dd0f0a54a8f83c7d1f12c3947df3bd19d7ad066ac3f09954 "hosted toolset inventory"
if errorlevel 1 exit /b 1
for /f "usebackq tokens=1,*" %%H in ("%HostedToolset%\SHA256SUMS") do (
    call :verify_digest "%HostedToolset%\%%I" %%H "hosted toolset artifact"
    if errorlevel 1 exit /b 1
)
call :verify_file "%Construction%\SHA256SUMS" 5064 d9a41516b7d5f768afe377fd957e897bcb1cd3552fdf4c9510af3fc6969a7edc "publisher construction inventory"
if errorlevel 1 exit /b 1
for /f "usebackq tokens=1,*" %%H in ("%Construction%\SHA256SUMS") do (
    call :verify_digest "%Construction%\%%I" %%H "publisher construction artifact"
    if errorlevel 1 exit /b 1
)
call :verify_file "%PublisherWvb%" %PublisherWvbBytes% %PublisherWvbSha256% "publisher WVB"
if errorlevel 1 exit /b 1
if /I "%Role%"=="wvo-publisher" (
    call :verify_file "%RawLowerer%" 5997056 4c2bf61306b81851424dbc02e7eb345ac34bcc9ffacd7727e8378c7b9e5dae07 "raw native WVB-to-WVO lowerer"
    if errorlevel 1 exit /b 1
)
if /I "%Role%"=="console-application-publisher" (
    call :verify_file "%RawLowerer%" 5997056 4c2bf61306b81851424dbc02e7eb345ac34bcc9ffacd7727e8378c7b9e5dae07 "raw native WVB-to-WVO lowerer"
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
) else if /I "%Role%"=="console-application-publisher" (
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
if /I "%Role%"=="console-application-publisher" echo console-application publisher construction status=Valid target=%TargetName% bytes=%ApplicationBytes%
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
    set "ActualSha256="
    for /f "skip=1 tokens=*" %%H in ('certutil -hashfile "%~1" SHA256') do if not defined ActualSha256 set "ActualSha256=%%H"
    >&2 call echo The %~3 digest is invalid: %~1 expected=%~2 actual=%%ActualSha256%%
    exit /b 1
)
exit /b 0

:usage
>&2 echo Usage: Tools\Native\Construct-Hosted-Verifier-Publisher.cmd [publisher^|promoter^|wvb-publisher^|wvo-publisher^|console-application-publisher] ^<windows^|linux^> ^<output.exe^|output.elf^>
exit /b 64
