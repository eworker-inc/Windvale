@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if not "%~2"=="" goto :usage
if /I not "%~x1"==".wvo" goto :usage

set "Output=%~f1"
if exist "%Output%" (
    >&2 echo The OS process-object output already exists.
    exit /b 1
)
for %%F in ("%Output%") do set "OutputDirectory=%%~dpF"
if not exist "%OutputDirectory%" (
    >&2 echo The OS process-object output directory does not exist.
    exit /b 1
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Native=%RepositoryRoot%\Tools\Native"
set "Toolset=%RepositoryRoot%\Artifacts\Native-Os-Process-Object-Toolset-Candidate"

call :verify "%Toolset%\windows-x64-boot-resource-object.exe" 388608 1e8a89e2a351303786d263fc883eef97eba4b4c99c68755f3f8f549246525360
if errorlevel 1 goto :invalid_toolset
call :verify "%Toolset%\windows-x64-process-resource-store.exe" 50688 28275579df804028d516adcb1477e9a2eee556319cef7ae667812571ded91c4f
if errorlevel 1 goto :invalid_toolset
call :verify "%Toolset%\windows-x64-process-directory-snapshot.exe" 46592 73ba3ee9460bd8915e6f05031f44cd963aecf60d424544f48cf57ed9912c2779
if errorlevel 1 goto :invalid_toolset
call :verify "%Toolset%\windows-x64-process-object.exe" 236032 004cf05250441bf01275d234439e0256adc91c63f837cd0f9e1cc7020d609687
if errorlevel 1 goto :invalid_toolset
call :verify "%Toolset%\normal-x64-process.bin" 46678 112dd0cb06de269e069436720876829313fb0a20546d1d7b38ea336fea26e6fd
if errorlevel 1 goto :invalid_toolset

:allocate
set "Work=%OutputDirectory%.windvale-os-process-object-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" >nul 2>&1
if errorlevel 1 exit /b 1
set "Status=1"

set "FailureStep=build-init"
call "%Native%\Build-Wvb.cmd" "%RepositoryRoot%\Projects/Operating-System/Windvale-Os-Init-Resource-Service.wvproj" "%Work%\Init.wvb" >"%Work%\Build-Init.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Init.wvb" 526 7cefa7dcf82ed05d6b6e133aa79b7da90372e2d8f8f993abe7449513398ede83
if errorlevel 1 goto :failure
set "FailureStep=lower-init"
call "%Native%\Lower-Wvb-To-Wvo.cmd" "%Work%\Init.wvb" "%Work%\Init-Main.wvo" >"%Work%\Lower-Init.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Init-Main.wvo" 3424 1a1a8599e7e9f92ebdb9c8e8c2df202311de3ffe3a549f3f339efdce4ef47456
if errorlevel 1 goto :failure
set "FailureStep=rename-init"
call "%Native%\Rename-Wvo-Export.cmd" "%Work%\Init-Main.wvo" Main Windvale_init_resource_service_main "%Work%\Init.wvo" >"%Work%\Rename-Init.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Init.wvo" 3455 4b8126d1baa38054fc70165be3c2f9519e7bea7e1f4d5596bcae36f2567ddf11
if errorlevel 1 goto :failure

set "FailureStep=build-directory"
call "%Native%\Build-Wvb.cmd" "%RepositoryRoot%\Projects/Operating-System/Windvale-Os-Directory-Process-Service.wvproj" "%Work%\Directory.wvb" >"%Work%\Build-Directory.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Directory.wvb" 474 f7410595f9824e510da9399f52a463013ff41240b67308cdf28b4f5b7484ab2b
if errorlevel 1 goto :failure
set "FailureStep=lower-directory"
call "%Native%\Lower-Wvb-To-Wvo.cmd" "%Work%\Directory.wvb" "%Work%\Directory-Main.wvo" >"%Work%\Lower-Directory.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Directory-Main.wvo" 2768 f80f17b1ae73885eb8fa7b81d319a089ea680994439d4d7debad58ad952e179e
if errorlevel 1 goto :failure
set "FailureStep=rename-directory"
call "%Native%\Rename-Wvo-Export.cmd" "%Work%\Directory-Main.wvo" Main Windvale_directory_process_service_main "%Work%\Directory.wvo" >"%Work%\Rename-Directory.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Directory.wvo" 2803 04339b8fd627c6b765a16903ad339408c86eaa9877bdc52357cbafa33e98679a
if errorlevel 1 goto :failure

set "FailureStep=build-filesystem"
call "%Native%\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Operating-System\Windvale-Os-Filesystem-Process-Service.wvproj" "%Work%\Filesystem.wvb" >"%Work%\Build-Filesystem.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Filesystem.wvb" 14812 054dc2c9b5c33e02e6263b644049fd84f1ed2e1219d642ec64c066af5bdc8fcf
if errorlevel 1 goto :failure
set "FailureStep=lower-filesystem"
call "%Native%\Lower-Wvb-To-Wvo.cmd" "%Work%\Filesystem.wvb" "%Work%\Filesystem-Main.wvo" >"%Work%\Lower-Filesystem.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Filesystem-Main.wvo" 196327 5ee235d5dca7bfdab8a5a1b7c54874b6545725e69da754e22e06f72f578ebdb3
if errorlevel 1 goto :failure
set "FailureStep=rename-filesystem"
call "%Native%\Rename-Wvo-Export.cmd" "%Work%\Filesystem-Main.wvo" Main Windvale_filesystem_process_service_main "%Work%\Filesystem.wvo" >"%Work%\Rename-Filesystem.log" 2>&1
if errorlevel 1 goto :failure

set "FailureStep=build-network"
call "%Native%\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Operating-System\Windvale-Os-Network-Process-Service.wvproj" "%Work%\Network.wvb" >"%Work%\Build-Network.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Network.wvb" 13543 32c595716af0a3706226d677924a5279ea2d7b97b0a4cbdf7c6c9eed808e1b2a
if errorlevel 1 goto :failure
set "FailureStep=lower-network"
call "%Native%\Lower-Wvb-To-Wvo.cmd" "%Work%\Network.wvb" "%Work%\Network-Main.wvo" >"%Work%\Lower-Network.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Network-Main.wvo" 243124 892cfe18b81667c9e4d3e82a1889a9b1f77c45e350d2e75144694db3c2f49ca0
if errorlevel 1 goto :failure
set "FailureStep=rename-network"
call "%Native%\Rename-Wvo-Export.cmd" "%Work%\Network-Main.wvo" Main Windvale_network_process_service_main "%Work%\Network.wvo" >"%Work%\Rename-Network.log" 2>&1
if errorlevel 1 goto :failure

set "FailureStep=build-interpreter"
call "%Native%\Build-Wvb.cmd" "%RepositoryRoot%\Projects/Operating-System/Windvale-Os-Bytecode-Interpreter.wvproj" "%Work%\Interpreter.wvb" >"%Work%\Build-Interpreter.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Interpreter.wvb" 56307 e2024702919e9acd37c119a7afb9991a73904d97ef3bdb1defe8c5ea13e91a3d
if errorlevel 1 goto :failure
set "FailureStep=lower-interpreter"
call "%Native%\Lower-Wvb-To-Wvo.cmd" "%Work%\Interpreter.wvb" "%Work%\Interpreter-Main.wvo" >"%Work%\Lower-Interpreter.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Interpreter-Main.wvo" 448737 dca63103e751f74e528514b25cb8650a7361e94172381a93dbfc8d5014844d78
if errorlevel 1 goto :failure
set "FailureStep=rename-interpreter"
call "%Native%\Rename-Wvo-Export.cmd" "%Work%\Interpreter-Main.wvo" Main Windvale_user_bytecode_interpreter_main "%Work%\Interpreter.wvo" >"%Work%\Rename-Interpreter.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Interpreter.wvo" 448772 7fb4a3d3a4aca6f44f6ab8bed3a2891147e319f275c6c2af3eab42e8c5763c4d
if errorlevel 1 goto :failure

set "FailureStep=build-program"
call "%Native%\Build-Wvb.cmd" "%RepositoryRoot%\Projects/Tests/Windvale-Native-Test-Function-Only.wvproj" "%Work%\Program.wvb" >"%Work%\Build-Program.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Program.wvb" 816 28d215b982a7b7185cfa80c4cc5346666bd0181582fe80bec8b7035d514da936
if errorlevel 1 goto :failure

set "FailureStep=assemble-init-shim"
call "%Native%\Assemble-Wva.cmd" "%RepositoryRoot%\Operating-System\Kernel\Init-Resource-Service-Shim.wva" "%Work%\Init-Shim.wvo" >"%Work%\Assemble-Init.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Init-Shim.wvo" 2118 52098aac184961fda7c3a23c8577851df6c18736555cb169b340d7b0c7249359
if errorlevel 1 goto :failure
set "FailureStep=assemble-directory-shim"
call "%Native%\Assemble-Wva.cmd" "%RepositoryRoot%\Operating-System\Kernel\Directory-Process-Service-Shim.wva" "%Work%\Directory-Shim.wvo" >"%Work%\Assemble-Directory.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Directory-Shim.wvo" 1549 c0a7524130b8733ed17a3ce52fc04986cb449394c9ee509280120b86a3ed8c88
if errorlevel 1 goto :failure
set "FailureStep=assemble-filesystem-shim"
call "%Native%\Assemble-Wva.cmd" "%RepositoryRoot%\Operating-System\Kernel\Filesystem-Process-Service-Shim.wva" "%Work%\Filesystem-Shim.wvo" >"%Work%\Assemble-Filesystem.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Filesystem-Shim.wvo" 302 dc212ce43b59102a05521531e6df4674291851c72a1be8990eff049ea46879dd
if errorlevel 1 goto :failure
set "FailureStep=assemble-network-shim"
call "%Native%\Assemble-Wva.cmd" "%RepositoryRoot%\Operating-System\Kernel\Network-Process-Service-Shim.wva" "%Work%\Network-Shim.wvo" >"%Work%\Assemble-Network.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Network-Shim.wvo" 296 628852893fcbc32e610261517a79c3acd56714ce0c197beab1c0a3917dedf726
if errorlevel 1 goto :failure
set "FailureStep=assemble-boot-resource"
call "%Native%\Assemble-Wva.cmd" "%RepositoryRoot%\Operating-System\Runtime\Boot-Resource-Service.wva" "%Work%\Boot-Stencil.wvo" >"%Work%\Assemble-Boot.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Boot-Stencil.wvo" 462 fde44aad9549731d53c5ccf3a57733b3619df94369b61ef27a693e1059784bc9
if errorlevel 1 goto :failure
set "FailureStep=assemble-user-shim"
call "%Native%\Assemble-Wva.cmd" "%RepositoryRoot%\Operating-System\Kernel\Process-User-Shim.wva" "%Work%\User-Shim.wvo" >"%Work%\Assemble-User.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\User-Shim.wvo" 1510 69ea7402a3a752e5c4b45689aeeb902b7e2ff1ce87a34bc9bad81417a3992fe6
if errorlevel 1 goto :failure

set "FailureStep=publish-boot-resource"
"%Toolset%\windows-x64-boot-resource-object.exe" "%Work%\Boot-Stencil.wvo" "%Work%\Boot-Service.wvo" >"%Work%\Boot-Resource.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Boot-Service.wvo" 462 ecb940abb9de8086d50ae418853021cf1f7566a9415a5a3a3b4e5cc45ed5e78c
if errorlevel 1 goto :failure

set "FailureStep=link-init"
call "%Native%\Link-Wvo.cmd" 0 Windvale_init_resource_user_entry "%Work%\Init.bin" "%Work%\Init-Shim.wvo" "%Work%\Init.wvo" >"%Work%\Link-Init.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Init.bin" 5159 e9624ebe3b857b77d8b1024a4edfdaf23e040ee61f9dfc484e590ce1e5aa18f0
if errorlevel 1 goto :failure
set "FailureStep=link-directory"
call "%Native%\Link-Wvo.cmd" 0 Windvale_directory_process_user_entry "%Work%\Directory.bin" "%Work%\Directory-Shim.wvo" "%Work%\Directory.wvo" >"%Work%\Link-Directory.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Directory.bin" 3911 f4d047c6f311b1561a5621b98f3db2868a969c54bb81dac2f75d599b7207f3fb
if errorlevel 1 goto :failure
set "FailureStep=link-filesystem"
call "%Native%\Link-Wvo.cmd" 0 Windvale_filesystem_process_user_entry "%Work%\Filesystem.bin" "%Work%\Filesystem-Shim.wvo" "%Work%\Filesystem.wvo" >"%Work%\Link-Filesystem.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Filesystem.bin" 195657 453cef870da3f375400d1c58cc8ebd385f761c2eafbdf3b3fb70603db8520dab
if errorlevel 1 goto :failure
set "FailureStep=link-network"
call "%Native%\Link-Wvo.cmd" 0 Windvale_network_process_user_entry "%Work%\Network.bin" "%Work%\Network-Shim.wvo" "%Work%\Network.wvo" >"%Work%\Link-Network.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Network.bin" 242571 57067da10da68fc1d35b41784e147d8f60ed1e05441cb68bc803ad5a9682f6d1
if errorlevel 1 goto :failure
set "FailureStep=link-client"
call "%Native%\Link-Wvo.cmd" 0 Windvale_process_user_entry "%Work%\Client.bin" "%Work%\User-Shim.wvo" "%Work%\Interpreter.wvo" "%Work%\Boot-Service.wvo" >"%Work%\Link-Client.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Client.bin" 449261 be4f88ad2460a17e5902670a9ca2bf70021d8b5ce46e2414f00f940a8f4d32b6
if errorlevel 1 goto :failure

set "FailureStep=build-resource-store"
"%Toolset%\windows-x64-process-resource-store.exe" "%Work%\Program.wvb" "%Work%\Resources.wvrs" >"%Work%\Resource-Store.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Resources.wvrs" 1196 624ece2d2e032f6f0929675a8f79ceb223538d84bccace264ecbbfdce5eca4ad
if errorlevel 1 goto :failure
set "FailureStep=build-directory-snapshot"
"%Toolset%\windows-x64-process-directory-snapshot.exe" "%Work%\Directory.wvds" >"%Work%\Directory-Snapshot.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Directory.wvds" 3184 0f793a41a701240b9cf41179dafa252384b43cd23214646ff021d245657c235a
if errorlevel 1 goto :failure

set "FailureStep=build-process-object"
"%Toolset%\windows-x64-process-object.exe" "%Toolset%\normal-x64-process.bin" "%Work%\Init.bin" "%Work%\Client.bin" "%Work%\Program.wvb" "%Work%\Resources.wvrs" "%Work%\Directory.wvds" "%Work%\Directory.bin" "%Work%\Filesystem.bin" "%Work%\Network.bin" "%Work%\Process.wvo" >"%Work%\Process-Object.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Process.wvo" 952002 c4606029a8af59770b2022f710b26fcd6d4207dba9d9d939c25faf423ee96d50
if errorlevel 1 goto :failure
call "%Native%\Verify-Wvo.cmd" "%Work%\Process.wvo" >"%Work%\Verify-Process.log" 2>&1
if errorlevel 1 goto :failure
set "FailureStep=publish"
call "%Native%\Publish-Wvo.cmd" "%Work%\Process.wvo" "%Output%" >"%Work%\Publish.log" 2>&1
if errorlevel 1 goto :failure
set "Status=0"
goto :cleanup

:invalid_toolset
>&2 echo The native OS process-object toolset is invalid.
exit /b 1

:failure
for %%L in ("%Work%\*.log") do if exist "%%~fL" type "%%~fL" 1>&2
if exist "%Work%\Process.wvo" (
    for %%F in ("%Work%\Process.wvo") do >&2 echo Process.wvo bytes=%%~zF
    certutil -hashfile "%Work%\Process.wvo" SHA256 1>&2
)
>&2 echo The native OS process-object build failed at step %FailureStep%.

:cleanup
if exist "%Work%" rmdir /s /q "%Work%"
exit /b %Status%

:verify
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /i /x /c:"%~3" >nul
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Build-Os-Process-Object.cmd ^<output.wvo^>
exit /b 64
