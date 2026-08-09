@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if not "%~2"=="" goto :usage
if /I not "%~x1"==".efi" goto :usage

set "Output=%~f1"
if exist "%Output%" (
    >&2 echo The native Probe 40 output already exists.
    exit /b 1
)
for %%F in ("%Output%") do set "OutputDirectory=%%~dpF"
if not exist "%OutputDirectory%" (
    >&2 echo The native Probe 40 output directory does not exist.
    exit /b 1
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Objects=%RepositoryRoot%\Artifacts\Native-Os-Probe-40-Object-Candidate"
set "Assembler=%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd"
set "Builder=%RepositoryRoot%\Tools\Native\Build-Wvb.cmd"
set "Lowerer=%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd"
set "Linker=%RepositoryRoot%\Tools\Native\Link-Wvo.cmd"
set "Packager=%RepositoryRoot%\Tools\Native\Package-Uefi.cmd"
set "Renamer=%RepositoryRoot%\Tools\Native\Rename-Wvo-Export.cmd"
set "ObjectProducer=%RepositoryRoot%\Tools\Native\Produce-Os-Probe-Object.cmd"
set "AdmissionProject=%RepositoryRoot%\Windvale-Os-Wvb-Admission.wvproj"
set "NativeProbeProject=%RepositoryRoot%\Windvale-Os-Native-Wvb-Probe.wvproj"

call :verify "%Objects%\01-kernel.wvo" 12134 bf13c1b103c297e87f4aa14f5bf7eba57ef2a30caa21b4c67dba34abc0a7f7a8
if errorlevel 1 exit /b 1
call :verify "%Objects%\04-process-policy.wvo" 129310 35d751147a7285fb926ba68e77da4ef554bcf68a58963520153f23ea3e8c4678
if errorlevel 1 exit /b 1
call :verify "%Objects%\05-process.wvo" 512978 dff07c3f6a52dedf6bcd96181221cba50c831359502ec763ee77f6aaaaafdfaa
if errorlevel 1 exit /b 1
set "Work=%OutputDirectory%.windvale-os-probe-native-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" (
    >&2 echo The native Probe 40 private path already exists.
    exit /b 1
)
mkdir "%Work%" >nul 2>&1
if errorlevel 1 (
    >&2 echo The native Probe 40 private path could not be created.
    exit /b 1
)
set "FailureStep=loader"
cmd /d /c call "%ObjectProducer%" loader "%Work%\00-loader.wvo" >"%Work%\00.log" 2>&1
if errorlevel 1 goto :failure
set "FailureStep=verify-loader"
call :verify "%Work%\00-loader.wvo" 6336 b310bc0e9aebc7b14c0892bb3dd4b833d42539c2194427a8f333b511d6af3804
if errorlevel 1 goto :failure
set "FailureStep=admission-build"
cmd /d /c call "%Builder%" "%AdmissionProject%" "%Work%\02-wvb-admission.wvb" >"%Work%\02-build.log" 2>&1
if errorlevel 1 goto :failure
set "FailureStep=admission-module-verify"
call :verify "%Work%\02-wvb-admission.wvb" 4071 69727bb8151aea164690be4f69adcda481532b965d9ae02ec92db21087f3d669
if errorlevel 1 goto :failure
set "FailureStep=admission-lower"
cmd /d /c call "%Lowerer%" "%Work%\02-wvb-admission.wvb" "%Work%\02-unrenamed.wvo" >"%Work%\02-lower.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\02-unrenamed.wvo" 20316 676a91062e7f1b4483ca9f332b17614a6b75988d21f9ff99caabcbfd51839568
if errorlevel 1 goto :failure
set "FailureStep=admission-rename"
cmd /d /c call "%Renamer%" "%Work%\02-unrenamed.wvo" Main Windvale_kernel_wvb_admit "%Work%\02-wvb-admission-native.wvo" >"%Work%\02-rename.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\02-wvb-admission-native.wvo" 20337 37e47bd2fed0242ad5cae9c9cc684927dc17041d4cd1d154658616be8b140c32
if errorlevel 1 goto :failure

set "FailureStep=native-probe-build"
cmd /d /c call "%Builder%" "%NativeProbeProject%" "%Work%\03-native-wvb-probe.wvb" >"%Work%\03-build.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\03-native-wvb-probe.wvb" 930 af5f93c881f006be06565f15857efb72b201b8f694a6c7e40a90deeaa86cd2c2
if errorlevel 1 goto :failure
set "FailureStep=native-probe-lower"
cmd /d /c call "%Lowerer%" "%Work%\03-native-wvb-probe.wvb" "%Work%\03-native-wvb-probe.wvo" >"%Work%\03-lower.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\03-native-wvb-probe.wvo" 7306 046f4fa32293b4f02bdc51a3ec71d562d7a064b31056ca77a43e2083b281cd2c
if errorlevel 1 goto :failure

set "FailureStep=memory-object-shims"
cmd /d /c call "%Assembler%" "%RepositoryRoot%\Operating-System\Kernel\X64-Memory-Object-Shims.wva" "%Work%\06-memory-object-shims.wvo" >"%Work%\06.log" 2>&1
if errorlevel 1 goto :failure
set "FailureStep=timer-shims"
cmd /d /c call "%Assembler%" "%RepositoryRoot%\Operating-System\Kernel\X64-Timer-Shims.wva" "%Work%\07-timer-shims.wvo" >"%Work%\07.log" 2>&1
if errorlevel 1 goto :failure
set "FailureStep=memory"
cmd /d /c call "%ObjectProducer%" memory "%Work%\08-memory.wvo" >"%Work%\08.log" 2>&1
if errorlevel 1 goto :failure
set "FailureStep=exceptions"
cmd /d /c call "%ObjectProducer%" exceptions "%Work%\09-exceptions.wvo" >"%Work%\09.log" 2>&1
if errorlevel 1 goto :failure
set "FailureStep=paging"
cmd /d /c call "%ObjectProducer%" paging "%Work%\10-paging.wvo" >"%Work%\10.log" 2>&1
if errorlevel 1 goto :failure
set "FailureStep=wvb-admission-bridge"
cmd /d /c call "%ObjectProducer%" wvb-admission-bridge "%Work%\12-wvb-admission-bridge.wvo" >"%Work%\12.log" 2>&1
if errorlevel 1 goto :failure
set "FailureStep=native-bridge-and-support"
cmd /d /c call "%ObjectProducer%" native-bridge-and-support "%Work%\13-native-bridge-and-support.wvo" >"%Work%\13.log" 2>&1
if errorlevel 1 goto :failure
set "FailureStep=kernel-shims"
cmd /d /c call "%Assembler%" "%RepositoryRoot%\Operating-System\Kernel\X64-Kernel-Shims.wva" "%Work%\11-kernel-shims.wvo" >"%Work%\11.log" 2>&1
if errorlevel 1 goto :failure

set "FailureStep=verify-memory-object-shims"
call :verify "%Work%\06-memory-object-shims.wvo" 2538 fe0a94461b743be58319d2e2f8b737840ec1216e61a98ee7e210f96f97f85bee
if errorlevel 1 goto :failure
set "FailureStep=verify-timer-shims"
call :verify "%Work%\07-timer-shims.wvo" 1202 e331a1db404b8b8359d35d410792496683a63acee621ff64f128a6eae128c344
if errorlevel 1 goto :failure
set "FailureStep=verify-memory"
call :verify "%Work%\08-memory.wvo" 1529 2668e17c3181e168415fb7bdee530873e2ddc8fa2d100af94bcc7b74909df3ed
if errorlevel 1 goto :failure
set "FailureStep=verify-exceptions"
call :verify "%Work%\09-exceptions.wvo" 483 9caeb7ce353bca33e3bbac729ecca0423d59f8ce6b65ccd6b54fa53c381d617c
if errorlevel 1 goto :failure
set "FailureStep=verify-paging"
call :verify "%Work%\10-paging.wvo" 1292 a6bcad24e4752acc1fbab75d6667e965f2ab4d5613edd2c8e6cda244616fba2d
if errorlevel 1 goto :failure
set "FailureStep=verify-wvb-admission-bridge"
call :verify "%Work%\12-wvb-admission-bridge.wvo" 484 271c378b1f12bb4affa33474d865611cbf14e5b1b8996c703cb3d3cbe22eee7d
if errorlevel 1 goto :failure
set "FailureStep=verify-native-bridge-and-support"
call :verify "%Work%\13-native-bridge-and-support.wvo" 461 472a0fbe6497525e634a4785e92aa9ee62c3c7d70fff7510e45acbea644eea0b
if errorlevel 1 goto :failure
set "FailureStep=verify-kernel-shims"
call :verify "%Work%\11-kernel-shims.wvo" 1894 845d45d6787ec819ca300ffc81a9ffe3e86c7b3998f3dd2a50a017a353d86193
if errorlevel 1 goto :failure

set "FailureStep=link"
cmd /d /c call "%Linker%" 0 Windvale_boot_probe "%Work%\Probe40.bin" ^
    "%Work%\00-loader.wvo" ^
    "%Objects%\01-kernel.wvo" ^
    "%Work%\02-wvb-admission-native.wvo" ^
    "%Work%\03-native-wvb-probe.wvo" ^
    "%Objects%\04-process-policy.wvo" ^
    "%Objects%\05-process.wvo" ^
    "%Work%\06-memory-object-shims.wvo" ^
    "%Work%\07-timer-shims.wvo" ^
    "%Work%\08-memory.wvo" ^
    "%Work%\09-exceptions.wvo" ^
    "%Work%\10-paging.wvo" ^
    "%Work%\11-kernel-shims.wvo" ^
    "%Work%\12-wvb-admission-bridge.wvo" ^
    "%Work%\13-native-bridge-and-support.wvo" >"%Work%\Link.map" 2>&1
if errorlevel 1 goto :failure
findstr /c:"entry name=Windvale_boot_probe address=0" "%Work%\Link.map" >nul
if errorlevel 1 goto :failure

set "FailureStep=package"
cmd /d /c call "%Packager%" "%Work%\Probe40.bin" 0 "%Work%\Probe40.efi" >"%Work%\Package.log" 2>&1
if errorlevel 1 goto :failure
call :verify "%Work%\Probe40.efi" 683008 080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9
if errorlevel 1 goto :failure

move /y "%Work%\Probe40.efi" "%Output%" >nul
if errorlevel 1 goto :failure
echo windvale-os-probe-native-build 40
echo scenario=normal
echo efi-bytes=683008
echo efi-sha256=080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9
echo output=%Output%
if exist "%Work%" rmdir /s /q "%Work%"
exit /b 0

:failure
>&2 echo The native Probe 40 build failed at step %FailureStep%.
if exist "%Work%\00.log" type "%Work%\00.log" 1>&2
if exist "%Work%\02-build.log" type "%Work%\02-build.log" 1>&2
if exist "%Work%\02-lower.log" type "%Work%\02-lower.log" 1>&2
if exist "%Work%\02-rename.log" type "%Work%\02-rename.log" 1>&2
if exist "%Work%\03-build.log" type "%Work%\03-build.log" 1>&2
if exist "%Work%\03-lower.log" type "%Work%\03-lower.log" 1>&2
if exist "%Work%\06.log" type "%Work%\06.log" 1>&2
if exist "%Work%\07.log" type "%Work%\07.log" 1>&2
if exist "%Work%\08.log" type "%Work%\08.log" 1>&2
if exist "%Work%\09.log" type "%Work%\09.log" 1>&2
if exist "%Work%\10.log" type "%Work%\10.log" 1>&2
if exist "%Work%\11.log" type "%Work%\11.log" 1>&2
if exist "%Work%\12.log" type "%Work%\12.log" 1>&2
if exist "%Work%\13.log" type "%Work%\13.log" 1>&2
if exist "%Work%\Link.map" type "%Work%\Link.map" 1>&2
if exist "%Work%\Package.log" type "%Work%\Package.log" 1>&2
if exist "%Work%" dir /b /a "%Work%" 1>&2
if exist "%Work%" rmdir /s /q "%Work%"
exit /b 1

:verify
if not exist "%~1" (
    >&2 echo The native Probe 40 input '%~nx1' is missing.
    exit /b 1
)
for %%F in ("%~1") do if not "%%~zF"=="%~2" (
    >&2 echo The native Probe 40 input '%%~nxF' has an invalid length.
    exit /b 1
)
certutil -hashfile "%~1" SHA256 | findstr /i /x /c:"%~3" >nul
if errorlevel 1 (
    >&2 echo The native Probe 40 input '%~nx1' has an invalid digest.
    exit /b 1
)
exit /b 0

:usage
>&2 echo Usage: Tools\Native\Build-Os-Probe.cmd ^<output.efi^>
exit /b 64
