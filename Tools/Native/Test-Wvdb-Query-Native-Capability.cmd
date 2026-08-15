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
call :verify_file "%Work%\Wvdb-Query.wvb" 26294 61f7b9d739a0f4ac9eece1cb79e554e373f49375109cf23d332921395ae37dc2 "locked WVDB query WVB" || goto :cleanup

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
call :verify_file "%Work%\Lowerer.wvb" 520966 ce190159783b48912ff71326d937a72a27b5178b07b7e52de71742a53cd12b56 "variant-capable lowerer WVB" || goto :cleanup

echo native wvdb query step=package-current-lowerer
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 6 "%Work%\Lowerer.wvb" "%Work%\Lowerer.exe" || goto :cleanup

echo native wvdb query step=lower-application
"%Work%\Lowerer.exe" "%Work%\Wvdb-Query.wvb" "%Work%\Wvdb-Query.wvo" || goto :cleanup
call :verify_file "%Work%\Wvdb-Query.wvo" 237210 b3d3bbde00136c230f6804215c352490bae9603b338d25186dba827be137edbf "WVDB query WVO" || goto :cleanup

echo native wvdb query step=assemble-rights-reduced-hosts
call "%Native%\Assemble-Wva.cmd" "%RepositoryRoot%\Runtime\Native\X64-Read-Only-Directory-Host.wva" "%Work%\Directory-Host.wvo" || goto :cleanup
call "%Native%\Assemble-Wva.cmd" "%RepositoryRoot%\Runtime\Native\Windows-X64-Read-Only-Directory.wva" "%Work%\Directory-Windows.wvo" || goto :cleanup
call "%Native%\Assemble-Wva.cmd" "%RepositoryRoot%\Runtime\Native\Linux-X64-Read-Only-Directory.wva" "%Work%\Directory-Linux.wvo" || goto :cleanup
call :verify_file "%Work%\Directory-Host.wvo" 2010 7ab58a817fe5dbc8e8f91b910654487ba62e10bc5aa5d1ae74b6bb07f2f6ca09 "directory host WVO" || goto :cleanup
call :verify_file "%Work%\Directory-Windows.wvo" 1731 d1dc38e751ab7a04cb115f2fc6f0e62a5452e2937cc1dd56867f3da8fe2ddc03 "Windows directory leaf WVO" || goto :cleanup
call :verify_file "%Work%\Directory-Linux.wvo" 608 53136d316adec7f6b7667ecc853764fc5207d25fc52e60d2175cd8e0f49c4c64 "Linux directory leaf WVO" || goto :cleanup

echo native wvdb query step=link-cross-host-images
call "%Native%\Link-Wvo.cmd" 0 Directory_host_entry "%Work%\Windows-Image.chunk-0" ^
    "%Work%\Wvdb-Query.wvo" "%Work%\Directory-Host.wvo" "%Work%\Directory-Windows.wvo" >"%Work%\Windows-Link.txt" || goto :cleanup
call "%Native%\Link-Wvo.cmd" 0 Directory_host_entry "%Work%\Linux-Image.chunk-0" ^
    "%Work%\Wvdb-Query.wvo" "%Work%\Directory-Host.wvo" "%Work%\Directory-Linux.wvo" >"%Work%\Linux-Link.txt" || goto :cleanup
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Directory_host_entry address=" "%Work%\Windows-Link.txt"') do set "WindowsEntry=%%E"
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Directory_host_entry address=" "%Work%\Linux-Link.txt"') do set "LinuxEntry=%%E"
if not "%WindowsEntry%"=="235440" goto :cleanup
if not "%LinuxEntry%"=="235440" goto :cleanup
call :verify_file "%Work%\Windows-Image.chunk-0" 238413 60bdf794d8fba0889a077eeec35fab75de9fd174a5a894eb78ef316ad1c8872c "Windows linked image" || goto :cleanup
call :verify_file "%Work%\Linux-Image.chunk-0" 237437 76b8327d6f970c467d76a4e9c2f64d7473897d2afe2a444c007f840e42a35632 "Linux linked image" || goto :cleanup

echo native wvdb query step=package-cross-host-applications
call "%Native%\Build-Cached-Hosted-Application.cmd" 6 "%Work%\Wvdb-Query.wvb" "%Work%\Windows-Image" 1 235440 "%Work%\Wvdb-Query.exe" windows || goto :cleanup
call "%Native%\Build-Cached-Hosted-Application.cmd" 6 "%Work%\Wvdb-Query.wvb" "%Work%\Linux-Image" 1 235440 "%Work%\Wvdb-Query.elf" linux || goto :cleanup
call :verify_file "%Work%\Wvdb-Query.exe" 258048 7cd60860e07294d9a45064495da33a42cc752849accfc672c35a69454cd963d8 "Windows hosted application" || goto :cleanup
call :verify_file "%Work%\Wvdb-Query.elf" 258048 29b4d4db7505daec94865d423e3805b02bde95751343b1fb7e4ceee8045a202d "Linux hosted application" || goto :cleanup

echo native wvdb query step=create-fixture
node "%Native%\Create-Wvdb-Query-Fixture.mjs" "%Work%\Run\Windvale-Database-Storage.bin" || goto :cleanup
call :verify_file "%Work%\Run\Windvale-Database-Storage.bin" 288 b0a940dca77a4b018f66d3be66023880746f077ff78446e88671688d5ad31892 "WVDB query fixture" || goto :cleanup

echo native wvdb query step=execute-windows-cases cases=5
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
echo native wvdb query cases status found=%FoundExit% negative=%NegativeExit% missing=%MissingExit% denied=%DeniedExit% unavailable=%UnavailableExit%
if not "%FoundExit%"=="0" goto :cleanup
if not "%NegativeExit%"=="0" goto :cleanup
if not "%MissingExit%"=="2" goto :cleanup
if not "%DeniedExit%"=="3" goto :cleanup
if not "%UnavailableExit%"=="3" goto :cleanup
echo native wvdb query output item=1/5 case=found
call :verify_file "%Work%\Found.txt" 21 cbd29940b14cde7eff85ca50290622c0b1a45cf984faba599d048e23291e291f "found output" || goto :cleanup
echo native wvdb query output item=2/5 case=negative
call :verify_file "%Work%\Negative.txt" 21 3c9e8339e9d9522a8f806c6076fde6bd8eb286cfd993e4d24b4f271d102490e8 "negative output" || goto :cleanup
echo native wvdb query output item=3/5 case=missing
call :verify_file "%Work%\Missing.txt" 14 d6592b511275d30bb5d995e669e7be2cc458bba9db8b656b3fc4ca88fe86b3d8 "missing output" || goto :cleanup
echo native wvdb query output item=4/5 case=denied
findstr /b /c:"storage-failure status=" "%Work%\Denied.txt" >nul
if errorlevel 1 (
    >&2 echo native wvdb query output status=Mismatch case=denied
    type "%Work%\Denied.txt" >&2
    goto :cleanup
)
echo native wvdb query output item=5/5 case=unavailable
findstr /b /c:"storage-failure status=" "%Work%\Unavailable.txt" >nul
if errorlevel 1 (
    >&2 echo native wvdb query output status=Mismatch case=unavailable
    type "%Work%\Unavailable.txt" >&2
    goto :cleanup
)

echo native wvdb query identity host=windows wvb=61f7b9d739a0f4ac9eece1cb79e554e373f49375109cf23d332921395ae37dc2 windows-application=7cd60860e07294d9a45064495da33a42cc752849accfc672c35a69454cd963d8 linux-application=29b4d4db7505daec94865d423e3805b02bde95751343b1fb7e4ceee8045a202d
echo native wvdb query capability status=Passed cases=5 capabilities=5 wvb=61f7b9d739a0f4ac9eece1cb79e554e373f49375109cf23d332921395ae37dc2 cross-host-images=Verified
set "Result=0"

:cleanup
if exist "%Work%\Run\." del /f /q "%Work%\Run\*" >nul 2>nul
if exist "%Work%\Run\." rmdir "%Work%\Run" >nul 2>nul
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
