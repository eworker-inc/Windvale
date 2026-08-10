@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "ImageMode=0"
if /I "%~1"=="image" goto :image_arguments
if "%~3"=="" goto :usage
if not "%~5"=="" goto :usage
echo(%~1| findstr /r /x "[1-7]" >nul || goto :usage
if /I not "%~x2"==".wvb" goto :usage
set "Profile=%~1"
set "Input=%~f2"
set "Output=%~f3"
set "Target=%~4"
if not defined Target set "Target=windows"
goto :arguments_ready

:image_arguments
if "%~7"=="" goto :usage
if not "%~9"=="" goto :usage
echo(%~2| findstr /r /x "[1-7]" >nul || goto :usage
if /I not "%~x3"==".wvb" goto :usage
echo(%~5| findstr /r /x "[1-8]" >nul || goto :usage
echo(%~6| findstr /r /x "[0-9][0-9]*" >nul || goto :usage
set "ImageMode=1"
set "Profile=%~2"
set "Input=%~f3"
set "ExternalBundleSources=%~f4"
set "FragmentCount=%~5"
set "NativeEntry=%~6"
set "Output=%~f7"
set "Target=%~8"
if not defined Target set "Target=windows"

:arguments_ready
set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Toolset=%RepositoryRoot%\Artifacts\Native-Hosted-Container-Toolset-Candidate"
set "ServiceRoot=%RepositoryRoot%\Runtime\Windvale.Native\Consumers"
if /I "%Target%"=="windows" goto :windows_target
if /I "%Target%"=="linux" goto :linux_target
goto :usage

:windows_target
if /I not "%~x7"==".exe" if "%ImageMode%"=="1" goto :usage
if /I not "%~x3"==".exe" if "%ImageMode%"=="0" goto :usage
set "Startup=%RepositoryRoot%\Linker\Reference\Consumers\Windows-X64-Hosted-Compiler.wvo"
set "StartupBytes=4398"
set "StartupSha256=dbf9314d43b47ffc5d3cdeef3c439456b295ac5c3a1cda0b1faaff6227910161"
set "ConsoleService=%ServiceRoot%\Native-X64-Windows-Console-Output-Service.bin"
set "ConsoleServiceBytes=258"
set "ConsoleServiceSha256=10f3a500aca7f0236cdf9f6c20658591df88bc612e677264cdaa0bcef59a0a48"
set "FileInputService=%ServiceRoot%\Native-X64-Windows-File-Input-Service.bin"
set "FileInputServiceBytes=1218"
set "FileInputServiceSha256=3e2fd284d4991d0f713301514d3fbf6af8ec84af7bd7289698c08a41d434c52d"
set "DiagnosticService=%ServiceRoot%\Native-X64-Windows-Diagnostic-Output-Service.bin"
set "DiagnosticServiceBytes=258"
set "DiagnosticServiceSha256=1b4068c01b2050c3055c78eb82303c71b8488e8766f7b628fab10ffb23e5ffe2"
set "FileOutputService=%ServiceRoot%\Native-X64-Windows-File-Output-Service.bin"
set "FileOutputServiceBytes=787"
set "FileOutputServiceSha256=a331248b12fc5830587f6fd8ddf06a546859b8f57366e205032aa2c37db48bb1"
goto :target_ready

:linux_target
if /I not "%~x7"==".elf" if "%ImageMode%"=="1" goto :usage
if /I not "%~x3"==".elf" if "%ImageMode%"=="0" goto :usage
set "Startup=%RepositoryRoot%\Linker\Reference\Consumers\Linux-X64-Hosted-Compiler.wvo"
set "StartupBytes=2454"
set "StartupSha256=1b8c08308d3f7320b741ae86022400ced6748352314b7f27954ec1c5a7345946"
set "ConsoleService=%ServiceRoot%\Native-X64-Linux-Console-Output-Service.bin"
set "ConsoleServiceBytes=213"
set "ConsoleServiceSha256=c5ea073a24c46dd634b1a67a7e7041d476dbce856d058aa8adc2c4e680d3d226"
set "FileInputService=%ServiceRoot%\Native-X64-Linux-File-Input-Service.bin"
set "FileInputServiceBytes=996"
set "FileInputServiceSha256=cbd78340641fa02589d0d96b73d233a67f9404ab76c3df2b1346b2e31ca43701"
set "DiagnosticService=%ServiceRoot%\Native-X64-Linux-Diagnostic-Output-Service.bin"
set "DiagnosticServiceBytes=213"
set "DiagnosticServiceSha256=1c81018143fa9b708373eaceda62722ca40fb1e11b20808f765fe5ece33406fe"
set "FileOutputService=%ServiceRoot%\Native-X64-Linux-File-Output-Service.bin"
set "FileOutputServiceBytes=823"
set "FileOutputServiceSha256=fc688f2a84936dc1082fcb5654667a8a60b0581bff29b1868d48ef2d4af77422"

:target_ready

call :verify_file "%Toolset%\SHA256SUMS" 6927 60f66c785c8dc7352ad394dee5ffd4da4b0f62370c47bdf2978ff0d7a34abd67 "hosted toolset inventory"
if errorlevel 1 exit /b 1
for /f "usebackq tokens=1,*" %%H in ("%Toolset%\SHA256SUMS") do (
    call :verify_digest "%Toolset%\%%I" %%H "hosted toolset artifact"
    if errorlevel 1 exit /b 1
)
call :verify_file "%ConsoleService%" %ConsoleServiceBytes% %ConsoleServiceSha256% "console service"
if errorlevel 1 exit /b 1
call :verify_file "%ServiceRoot%\Native-X64-Argument-Count-Service.bin" 5 2358e7e2c72d6476cfe05134db4f0eb5e6987fcca1b10894a8588a28d3929829 "argument-count service"
if errorlevel 1 exit /b 1
call :verify_file "%ServiceRoot%\Native-X64-Argument-Service.bin" 70 2253e1435f141df5b68f9f7e9e9aa0de448410c42dcf33ad76dcf131afea65d1 "argument service"
if errorlevel 1 exit /b 1
call :verify_file "%FileInputService%" %FileInputServiceBytes% %FileInputServiceSha256% "file-input service"
if errorlevel 1 exit /b 1
call :verify_file "%ServiceRoot%\Native-X64-Utf8-Service.bin" 800 4c3d2e370d62c8d2f54a3c453f39b94cf46ddabd6db3c2f3d6b65f0713b68aaf "UTF-8 service"
if errorlevel 1 exit /b 1
call :verify_file "%DiagnosticService%" %DiagnosticServiceBytes% %DiagnosticServiceSha256% "diagnostic service"
if errorlevel 1 exit /b 1
call :verify_file "%ServiceRoot%\Native-X64-Text-Concat-Service.bin" 249 75c5588117e1f5f58a593a23aae6156a3a68a6302df5f50153b977bccbaaa3a0 "text-concat service"
if errorlevel 1 exit /b 1
call :verify_file "%ServiceRoot%\Native-X64-U32-Format-Service.bin" 191 b98f2d55e30bb7369e233f94e4ade5f3e8917a7730114446f1ebc81f353e1e43 "u32-format service"
if errorlevel 1 exit /b 1
call :verify_file "%FileOutputService%" %FileOutputServiceBytes% %FileOutputServiceSha256% "file-output service"
if errorlevel 1 exit /b 1
call :verify_file "%Startup%" %StartupBytes% %StartupSha256% "hosted startup object"
if errorlevel 1 exit /b 1

:allocate
set "TemporaryDirectory=%TEMP%\windvale-native-hosted-package-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "BundleSources=%TemporaryDirectory%\Bundle-Sources"
set "BundleSegments=%TemporaryDirectory%\Bundle-Segments"
set "ApplicationSources=%TemporaryDirectory%\Application-Sources"
set "ApplicationSegments=%TemporaryDirectory%\Application-Segments"
set "Result=1"

if "%ImageMode%"=="1" (
    set "BundleSources=%ExternalBundleSources%"
) else (
    call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" "%Input%" "%TemporaryDirectory%\Input.wvo" >"%TemporaryDirectory%\Lower.txt"
    if errorlevel 1 goto :cleanup
    call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%TemporaryDirectory%\Native.bin" "%TemporaryDirectory%\Input.wvo" >"%TemporaryDirectory%\Link.txt"
    if errorlevel 1 goto :cleanup
    set "NativeEntry="
    for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Main address=" "%TemporaryDirectory%\Link.txt"') do set "NativeEntry=%%E"
    if not defined NativeEntry goto :cleanup
    set "FragmentCount=1"
    copy /b "%TemporaryDirectory%\Native.bin" "%BundleSources%.chunk-0" >nul || goto :cleanup
)

"%Toolset%\windows-x64\wvhostfixedservices.exe" %Target% "%BundleSources%" %FragmentCount% ^
    "%ConsoleService%" ^
    "%ServiceRoot%\Native-X64-Argument-Count-Service.bin" ^
    "%ServiceRoot%\Native-X64-Argument-Service.bin" ^
    "%FileInputService%" ^
    "%ServiceRoot%\Native-X64-Utf8-Service.bin" ^
    "%DiagnosticService%" ^
    "%ServiceRoot%\Native-X64-Text-Concat-Service.bin" ^
    "%ServiceRoot%\Native-X64-U32-Format-Service.bin" ^
    "%FileOutputService%"
if errorlevel 1 goto :cleanup
"%Toolset%\windows-x64\wvhostenumrequest.exe" "%Input%" "%TemporaryDirectory%\Enum.wveq"
if errorlevel 1 goto :cleanup
set /a EnumSourceIndex=FragmentCount+6
"%Toolset%\windows-x64\wvhostenumservice.exe" "%TemporaryDirectory%\Enum.wveq" "%BundleSources%.chunk-%EnumSourceIndex%"
if errorlevel 1 goto :cleanup
"%Toolset%\windows-x64\wvhostsourcegeometry.exe" "%BundleSources%" %FragmentCount% "%TemporaryDirectory%\Bundle-Sources.wvsg"
if errorlevel 1 goto :cleanup
"%Toolset%\windows-x64\wvhostpublicationrequest.exe" "%TemporaryDirectory%\Bundle-Sources.wvsg" "%TemporaryDirectory%\Publication.wvpq"
if errorlevel 1 goto :cleanup
"%Toolset%\windows-x64\wvhostcontrol.exe" evidence "%TemporaryDirectory%\Bundle-Sources.wvsg" "%TemporaryDirectory%\Evidence.wvhs"
if errorlevel 1 goto :cleanup
"%Toolset%\windows-x64\wvhostcontrol.exe" metadata %Target% %Profile% %NativeEntry% "%TemporaryDirectory%\Metadata-Input.wvmi"
if errorlevel 1 goto :cleanup
"%Toolset%\windows-x64\wvhostrequest.exe" "%TemporaryDirectory%\Metadata-Input.wvmi" "%TemporaryDirectory%\Publication.wvpq" "%TemporaryDirectory%\Evidence.wvhs" "%BundleSources%" "%TemporaryDirectory%\Metadata-Request.wvhq"
if errorlevel 1 goto :cleanup
"%Toolset%\windows-x64\wvhostmetadata.exe" "%TemporaryDirectory%\Metadata-Request.wvhq" "%TemporaryDirectory%\Metadata.wvhm"
if errorlevel 1 goto :cleanup
"%Toolset%\windows-x64\wvhostruntime.exe" "%TemporaryDirectory%\Metadata.wvhm" "%TemporaryDirectory%\Runtime.wvhr"
if errorlevel 1 goto :cleanup
"%Toolset%\windows-x64\wvhostplan.exe" "%TemporaryDirectory%\Runtime.wvhr" "%TemporaryDirectory%\Plan.wvcd"
if errorlevel 1 goto :cleanup
"%Toolset%\windows-x64\wvhostbytes.exe" "%TemporaryDirectory%\Plan.wvcd" "%TemporaryDirectory%\Platform.wvhb"
if errorlevel 1 goto :cleanup
"%Toolset%\windows-x64\wvhoststartup.exe" "%TemporaryDirectory%\Plan.wvcd" "%Startup%" "%TemporaryDirectory%\Startup.wvsd"
if errorlevel 1 goto :cleanup

"%Toolset%\windows-x64\wvhostbundlerequest.exe" "%TemporaryDirectory%\Publication.wvpq" "%TemporaryDirectory%\Bundle-Sources.wvsg" "%BundleSources%" count >"%TemporaryDirectory%\Bundle-Count.txt"
if errorlevel 1 goto :cleanup
set "BundleCount="
for /f "tokens=3 delims==" %%N in ('findstr /b /c:"hosted service-bundle request status=Valid segments=" "%TemporaryDirectory%\Bundle-Count.txt"') do set "BundleCount=%%N"
if not defined BundleCount goto :cleanup
echo(%BundleCount%| findstr /r /x "[1-9]" >nul || goto :cleanup
set /a BundleLast=BundleCount-1
for /l %%N in (0,1,%BundleLast%) do (
    "%Toolset%\windows-x64\wvhostbundlerequest.exe" "%TemporaryDirectory%\Publication.wvpq" "%TemporaryDirectory%\Bundle-Sources.wvsg" "%BundleSources%" %%N "%BundleSegments%.request-%%N"
    if errorlevel 1 goto :cleanup
    "%Toolset%\windows-x64\wvhostbundle.exe" "%BundleSegments%.request-%%N" "%BundleSegments%.response-%%N"
    if errorlevel 1 goto :cleanup
)

"%Toolset%\windows-x64\wvhostsources.exe" "%TemporaryDirectory%\Plan.wvcd" "%TemporaryDirectory%\Platform.wvhb" "%TemporaryDirectory%\Startup.wvsd" "%BundleSegments%" "%TemporaryDirectory%\Runtime.wvhr" "%ApplicationSources%" "%TemporaryDirectory%\Application-Sources.wvsg"
if errorlevel 1 goto :cleanup
"%Toolset%\windows-x64\wvhostsegmentrequest.exe" "%TemporaryDirectory%\Plan.wvcd" "%TemporaryDirectory%\Application-Sources.wvsg" "%ApplicationSources%" count >"%TemporaryDirectory%\Application-Count.txt"
if errorlevel 1 goto :cleanup
set "ApplicationCount="
for /f "tokens=3 delims==" %%N in ('findstr /b /c:"hosted container segment request status=Valid segments=" "%TemporaryDirectory%\Application-Count.txt"') do set "ApplicationCount=%%N"
if not defined ApplicationCount goto :cleanup
echo(%ApplicationCount%| findstr /r /x "[1-9][0-9]*" >nul || goto :cleanup
if %ApplicationCount% GTR 31 goto :cleanup
set /a ApplicationLast=ApplicationCount-1
for /l %%N in (0,1,%ApplicationLast%) do (
    "%Toolset%\windows-x64\wvhostsegmentrequest.exe" "%TemporaryDirectory%\Plan.wvcd" "%TemporaryDirectory%\Application-Sources.wvsg" "%ApplicationSources%" %%N "%ApplicationSegments%.request-%%N"
    if errorlevel 1 goto :cleanup
    "%Toolset%\windows-x64\wvhostsegment.exe" "%ApplicationSegments%.request-%%N" "%ApplicationSegments%.response-%%N"
    if errorlevel 1 goto :cleanup
)
"%Toolset%\windows-x64\wvhostsegmentmanifest.exe" "%TemporaryDirectory%\Plan.wvcd" "%ApplicationSegments%" "%TemporaryDirectory%\Application-Segments.wvhm"
if errorlevel 1 goto :cleanup
"%Toolset%\windows-x64\wvhostpublish.exe" "%TemporaryDirectory%\Plan.wvcd" "%ApplicationSegments%" "%TemporaryDirectory%\Application-Segments.wvhm" "%Output%"
set "Result=%ERRORLEVEL%"

:cleanup
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
>&2 echo Usage: Tools\Native\Package-Hosted-Wvb.cmd ^<profile-1-through-7^> ^<input.wvb^> ^<output.exe^|output.elf^> [windows^|linux]
>&2 echo    or: Tools\Native\Package-Hosted-Wvb.cmd image ^<profile-1-through-7^> ^<input.wvb^> ^<chunk-prefix^> ^<fragment-chunks-1-through-8^> ^<entry-offset^> ^<output.exe^|output.elf^> [windows^|linux]
exit /b 64
