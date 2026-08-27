@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Wvdb-Query-Native-Capability.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Native=%RepositoryRoot%\Tools\Native"

:allocate
set "Work=%TEMP%\windvale-wvdb-query-native-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Result=1"
mkdir "%Work%\Run" || goto :cleanup
mkdir "%Work%\Empty" || goto :cleanup

echo native wvdb query step=locked-package
call "%Native%\Build-Wvdb-Query-Package.cmd" ^
    "%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvpack" ^
    "%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvlock" ^
    "%Work%\Wvdb-Query.wvb" || goto :cleanup
call :verify_file "%Work%\Wvdb-Query.wvb" 26145 77cb6034402942734be316b9a135d6c1b46ace5cb43a198b2aafe2d1b098027b "locked WVDB query WVB" || goto :cleanup

echo native wvdb query step=capability-directory
call "%Native%\Inspect-Wvb.cmd" "%Work%\Wvdb-Query.wvb" >"%Work%\Inspect.txt" || goto :cleanup
for /f %%C in ('findstr /b /c:"capability index=" "%Work%\Inspect.txt" ^| find /c /v ""') do set "CapabilityCount=%%C"
if not "%CapabilityCount%"=="5" goto :cleanup
for %%C in (
    console.write_line
    diagnostic.write_line
    filesystem.directory_read_v1
    process.argument
    process.argument_count
) do (
    findstr /b /c:"capability index=" "%Work%\Inspect.txt" | findstr /c:"name=\"%%C\"" >nul || goto :cleanup
)

echo native wvdb query step=build-current-lowerer
call "%Native%\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Compiler\Windvale-Native-X64-Lowering-Tool.wvproj" ^
    "%Work%\Lowerer.wvb" || goto :cleanup
call :verify_file "%Work%\Lowerer.wvb" 567615 77ce798c67281e2fa5d576a1d229f8ec947427a092f8720909a09e32e9711e60 "variant-capable lowerer WVB" || goto :cleanup

echo native wvdb query step=package-current-lowerer
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 6 "%Work%\Lowerer.wvb" "%Work%\Lowerer.exe" || goto :cleanup

echo native wvdb query step=lower-application
"%Work%\Lowerer.exe" "%Work%\Wvdb-Query.wvb" "%Work%\Wvdb-Query.wvo" || goto :cleanup
call :verify_file "%Work%\Wvdb-Query.wvo" 235564 182d5d49e56bff03de1fb30c179310a814fa120055884e60e23c53e01b41c0f3 "WVDB query WVO" || goto :cleanup

echo native wvdb query step=assemble-rights-reduced-hosts
call "%Native%\Assemble-Wva.cmd" "%RepositoryRoot%\Runtime\Native\X64-Read-Only-Directory-Host.wva" "%Work%\Directory-Host.wvo" || goto :cleanup
call "%Native%\Assemble-Wva.cmd" "%RepositoryRoot%\Runtime\Native\Windows-X64-Read-Only-Directory.wva" "%Work%\Directory-Windows.wvo" || goto :cleanup
call "%Native%\Assemble-Wva.cmd" "%RepositoryRoot%\Runtime\Native\Linux-X64-Read-Only-Directory.wva" "%Work%\Directory-Linux.wvo" || goto :cleanup
call :verify_file "%Work%\Directory-Host.wvo" 2010 7ab58a817fe5dbc8e8f91b910654487ba62e10bc5aa5d1ae74b6bb07f2f6ca09 "directory host WVO" || goto :cleanup
call :verify_file "%Work%\Directory-Windows.wvo" 1951 d2da1c67864c242aeb9797661028295922486de2cf7d37aa41024189afb10f34 "Windows directory leaf WVO" || goto :cleanup
call :verify_file "%Work%\Directory-Linux.wvo" 681 0ccbcda71b20eaa024946e4fbb2016853952a39f1fe58ed0a183bde502335d86 "Linux directory leaf WVO" || goto :cleanup

echo native wvdb query step=link-cross-host-images
call "%Native%\Link-Wvo.cmd" 0 Directory_host_entry "%Work%\Windows-Image.chunk-0" ^
    "%Work%\Wvdb-Query.wvo" "%Work%\Directory-Host.wvo" "%Work%\Directory-Windows.wvo" >"%Work%\Windows-Link.txt" || goto :cleanup
call "%Native%\Link-Wvo.cmd" 0 Directory_host_entry "%Work%\Linux-Image.chunk-0" ^
    "%Work%\Wvdb-Query.wvo" "%Work%\Directory-Host.wvo" "%Work%\Directory-Linux.wvo" >"%Work%\Linux-Link.txt" || goto :cleanup
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Directory_host_entry address=" "%Work%\Windows-Link.txt"') do set "WindowsEntry=%%E"
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Directory_host_entry address=" "%Work%\Linux-Link.txt"') do set "LinuxEntry=%%E"
if not "%WindowsEntry%"=="233760" goto :cleanup
if not "%LinuxEntry%"=="233760" goto :cleanup
call :verify_file "%Work%\Windows-Image.chunk-0" 236856 0abfe3194e1d412bf78b812484ac157e858efad576573e64f901e323ae20175d "Windows linked image" || goto :cleanup
call :verify_file "%Work%\Linux-Image.chunk-0" 235837 c7b1971f792f90ed94b32abbbb4355b8ac84773ec8599127b5a37cbb40bec872 "Linux linked image" || goto :cleanup

echo native wvdb query step=package-cross-host-applications
call "%Native%\Build-Cached-Hosted-Application.cmd" 6 "%Work%\Wvdb-Query.wvb" "%Work%\Windows-Image" 1 233760 "%Work%\Wvdb-Query.exe" windows || goto :cleanup
call "%Native%\Build-Cached-Hosted-Application.cmd" 6 "%Work%\Wvdb-Query.wvb" "%Work%\Linux-Image" 1 233760 "%Work%\Wvdb-Query.elf" linux || goto :cleanup
call :verify_file "%Work%\Wvdb-Query.exe" 256512 5780f7416938fa1329c6e85314697c81a8a29fcd35f792b7c7b5353962e944d7 "Windows hosted application" || goto :cleanup
call :verify_file "%Work%\Wvdb-Query.elf" 258048 c457ac0470385fedc4e328abe29a4c56d2253abb3c5d91ffe2c8ead24257401c "Linux hosted application" || goto :cleanup

echo native wvdb query step=create-fixture
node "%Native%\Create-Wvdb-Query-Fixture.mjs" "%Work%\Run\Windvale-Database-Storage.bin" || goto :cleanup
call :verify_file "%Work%\Run\Windvale-Database-Storage.bin" 288 b0a940dca77a4b018f66d3be66023880746f077ff78446e88671688d5ad31892 "WVDB query fixture" || goto :cleanup

echo native wvdb query step=execute-windows-cases cases=6
pushd "%Work%\Run" || goto :cleanup
"%Work%\Wvdb-Query.exe" Windvale-Database-Storage.bin 7 >"%Work%\Found.txt" 2>&1
set "FoundExit=%ERRORLEVEL%"
"%Work%\Wvdb-Query.exe" Windvale-Database-Storage.bin 9 >"%Work%\Negative.txt" 2>&1
set "NegativeExit=%ERRORLEVEL%"
"%Work%\Wvdb-Query.exe" Windvale-Database-Storage.bin 8 >"%Work%\Missing.txt" 2>&1
set "MissingExit=%ERRORLEVEL%"
"%Work%\Wvdb-Query.exe" Xindvale-Database-Storage.bin 7 >"%Work%\Denied.txt" 2>&1
set "DeniedExit=%ERRORLEVEL%"
popd
pushd "%Work%\Empty" || goto :cleanup
"%Work%\Wvdb-Query.exe" Windvale-Database-Storage.bin 7 >"%Work%\Unavailable.txt" 2>&1
set "UnavailableExit=%ERRORLEVEL%"
popd
mklink /J "%Work%\Empty\Windvale-Database-Storage.bin" "%Work%\Run" >nul || goto :cleanup
pushd "%Work%\Empty" || goto :cleanup
"%Work%\Wvdb-Query.exe" Windvale-Database-Storage.bin 7 >"%Work%\NoLink.txt" 2>&1
set "NoLinkExit=%ERRORLEVEL%"
popd
rmdir "%Work%\Empty\Windvale-Database-Storage.bin" || goto :cleanup
echo native wvdb query cases status found=%FoundExit% negative=%NegativeExit% missing=%MissingExit% denied=%DeniedExit% unavailable=%UnavailableExit% no-link=%NoLinkExit%
if not "%FoundExit%"=="0" goto :cleanup
if not "%NegativeExit%"=="0" goto :cleanup
if not "%MissingExit%"=="2" goto :cleanup
if not "%DeniedExit%"=="3" goto :cleanup
if not "%UnavailableExit%"=="3" goto :cleanup
if not "%NoLinkExit%"=="3" goto :cleanup
echo native wvdb query output item=1/6 case=found
call :verify_file "%Work%\Found.txt" 21 cbd29940b14cde7eff85ca50290622c0b1a45cf984faba599d048e23291e291f "found output" || goto :cleanup
echo native wvdb query output item=2/6 case=negative
call :verify_file "%Work%\Negative.txt" 21 3c9e8339e9d9522a8f806c6076fde6bd8eb286cfd993e4d24b4f271d102490e8 "negative output" || goto :cleanup
echo native wvdb query output item=3/6 case=missing
call :verify_file "%Work%\Missing.txt" 14 d6592b511275d30bb5d995e669e7be2cc458bba9db8b656b3fc4ca88fe86b3d8 "missing output" || goto :cleanup
echo native wvdb query output item=4/6 case=denied
findstr /b /c:"storage-failure status=" "%Work%\Denied.txt" >nul
if errorlevel 1 (
    >&2 echo native wvdb query output status=Mismatch case=denied
    type "%Work%\Denied.txt" >&2
    goto :cleanup
)
echo native wvdb query output item=5/6 case=unavailable
findstr /b /c:"storage-failure status=" "%Work%\Unavailable.txt" >nul
if errorlevel 1 (
    >&2 echo native wvdb query output status=Mismatch case=unavailable
    type "%Work%\Unavailable.txt" >&2
    goto :cleanup
)
echo native wvdb query output item=6/6 case=no-link
findstr /b /c:"storage-failure status=" "%Work%\NoLink.txt" >nul
if errorlevel 1 (
    >&2 echo native wvdb query output status=Mismatch case=no-link
    type "%Work%\NoLink.txt" >&2
    goto :cleanup
)

echo native wvdb query identity host=windows wvb=77cb6034402942734be316b9a135d6c1b46ace5cb43a198b2aafe2d1b098027b windows-application=5780f7416938fa1329c6e85314697c81a8a29fcd35f792b7c7b5353962e944d7 linux-application=c457ac0470385fedc4e328abe29a4c56d2253abb3c5d91ffe2c8ead24257401c
echo native wvdb query capability status=Passed cases=6 capabilities=5 wvb=77cb6034402942734be316b9a135d6c1b46ace5cb43a198b2aafe2d1b098027b cross-host-images=Verified
set "Result=0"

:cleanup
if exist "%Work%\Run\." del /f /q "%Work%\Run\*" >nul 2>nul
if exist "%Work%\Run\." rmdir "%Work%\Run" >nul 2>nul
if exist "%Work%\Empty\Windvale-Database-Storage.bin\." rmdir "%Work%\Empty\Windvale-Database-Storage.bin" >nul 2>nul
if exist "%Work%\Empty\." del /f /q "%Work%\Empty\*" >nul 2>nul
if exist "%Work%\Empty\." rmdir "%Work%\Empty" >nul 2>nul
if exist "%Work%\." del /f /q "%Work%\*" >nul 2>nul
if exist "%Work%\." rmdir "%Work%" >nul 2>nul
exit /b %Result%

:verify_file
if not exist "%~1" (
    >&2 echo Missing %~4: %~1
    exit /b 1
)
for %%F in ("%~1") do if not "%%~zF"=="%~2" (
    >&2 echo Invalid byte length for %~4.
    exit /b 1
)
certutil -hashfile "%~1" SHA256 | findstr /i /c:"%~3" >nul
if errorlevel 1 (
    >&2 echo Invalid SHA-256 for %~4.
    exit /b 1
)
exit /b 0
