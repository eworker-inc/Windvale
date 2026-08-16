@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Wvb-Inspector-Reconstruction.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Native=%RepositoryRoot%\Tools\Native"
set "Hosted=%RepositoryRoot%\Artifacts\Native-Hosted-Container-Toolset-Candidate\windows-x64"
set "ServiceRoot=%RepositoryRoot%\Runtime\Windvale.Native\Consumers"

:allocate
set "Work=%TEMP%\windvale-wvb-inspector-reconstruction-test-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Result=1"

echo native WVB inspector reconstruction step=construction-tools item=1/4
call :prepare_tool "%RepositoryRoot%\Projects\Linker\Windvale-Native-Hosted-Verifier-Container-Tool.wvproj" wvhostverifiercompose || goto :cleanup
call :prepare_tool "%RepositoryRoot%\Projects\Linker\Windvale-Native-Hosted-Verifier-Platform-Tool.wvproj" wvhostverifierbytes || goto :cleanup
call :prepare_tool "%RepositoryRoot%\Projects\Linker\Windvale-Native-Hosted-Verifier-Startup-Tool.wvproj" wvhostverifierstartup || goto :cleanup
call :prepare_tool "%RepositoryRoot%\Projects\Runtime\Windvale-Native-Hosted-Verifier-Service-Bundle-Request-Tool.wvproj" wvhostverifierbundle || goto :cleanup
call :prepare_tool "%RepositoryRoot%\Projects\Runtime\Windvale-Native-Hosted-Verifier-Publisher-Base-Metadata-Tool.wvproj" wvhostverifierpublisherbasemetadata || goto :cleanup
call :prepare_tool "%RepositoryRoot%\Projects\Runtime\Windvale-Native-Hosted-Verifier-Publisher-Base-Runtime-Tool.wvproj" wvhostverifierpublisherbaseruntime || goto :cleanup

echo native WVB inspector reconstruction step=application item=2/4
call "%Native%\Build-Cached-Project-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Examples\Windvale-Wvb-Inspector.wvproj" ^
    "%Work%\Wvb-Inspector.wvb" >"%Work%\Inspector-Build.txt" || goto :cleanup
"%Hosted%\wvhostenumrequest.exe" "%Work%\Wvb-Inspector.wvb" "%Work%\Inspector.wveq" >nul || goto :cleanup
"%Hosted%\wvhostenumservice.exe" "%Work%\Inspector.wveq" "%Work%\Enum-Service.bin" >nul || goto :cleanup
call "%Native%\Lower-Wvb-To-Wvo.cmd" "%Work%\Wvb-Inspector.wvb" "%Work%\Wvb-Inspector.wvo" >nul || goto :cleanup
call "%Native%\Link-Wvo.cmd" 0 Main "%Work%\Wvb-Inspector.bin" "%Work%\Wvb-Inspector.wvo" >"%Work%\Inspector.map" || goto :cleanup
set "InspectorEntry="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Main address=" "%Work%\Inspector.map"') do set "InspectorEntry=%%E"
if not defined InspectorEntry goto :cleanup
call "%Native%\Assemble-Wva.cmd" ^
    "%RepositoryRoot%\Linker\Startup\Windows-X64-Hosted-Inspector.wva" ^
    "%Work%\Inspector-Startup.wvo" >nul || goto :cleanup

"%Work%\wvhostverifierbundle.exe" wvb-inspector ^
    "%Work%\Wvb-Inspector.bin" ^
    "%ServiceRoot%\Native-X64-Windows-Console-Output-Service.bin" ^
    "%ServiceRoot%\Native-X64-Argument-Count-Service.bin" ^
    "%ServiceRoot%\Native-X64-Argument-Service.bin" ^
    "%ServiceRoot%\Native-X64-Windows-File-Input-Service.bin" ^
    "%ServiceRoot%\Native-X64-Utf8-Service.bin" ^
    "%ServiceRoot%\Native-X64-Windows-Diagnostic-Output-Service.bin" ^
    "%Work%\Enum-Service.bin" ^
    "%ServiceRoot%\Native-X64-Text-Concat-Service.bin" ^
    "%ServiceRoot%\Native-X64-Text-Quote-Service.bin" ^
    "%ServiceRoot%\Native-X64-I32-Format-Service.bin" ^
    "%ServiceRoot%\Native-X64-U32-Format-Service.bin" ^
    "%Work%\Bundle.wvsq" >nul || goto :cleanup
"%Work%\wvhostverifierpublisherbasemetadata.exe" wvb-inspector 1 %InspectorEntry% ^
    "%Work%\Bundle.wvsq" "%Work%\Metadata.wvhv" || goto :cleanup
"%Work%\wvhostverifierpublisherbaseruntime.exe" ^
    "%Work%\Metadata.wvhv" "%Work%\Runtime.wvhr" || goto :cleanup
"%Hosted%\wvhostbundle.exe" "%Work%\Bundle.wvsq" "%Work%\Bundle.wvsi" >nul || goto :cleanup
"%Work%\wvhostverifierbytes.exe" wvb-inspector ^
    "%Work%\Runtime.wvhr" "%Work%\Platform.wvhb" >nul || goto :cleanup
"%Work%\wvhostverifierstartup.exe" wvb-inspector ^
    "%Work%\Runtime.wvhr" "%Work%\Inspector-Startup.wvo" ^
    "%Work%\Startup.wvsd" >nul || goto :cleanup
"%Work%\wvhostverifiercompose.exe" wvb-inspector ^
    "%Work%\Runtime.wvhr" "%Work%\Platform.wvhb" "%Work%\Startup.wvsd" ^
    "%Work%\Bundle.wvsi" "%Work%\Wvb-Inspector.exe" >nul || goto :cleanup
"%Work%\wvhostverifiercompose.exe" wvb-inspector ^
    "%Work%\Runtime.wvhr" "%Work%\Platform.wvhb" "%Work%\Startup.wvsd" ^
    "%Work%\Bundle.wvsi" "%Work%\Wvb-Inspector-Second.exe" >nul || goto :cleanup
fc /b "%Work%\Wvb-Inspector.exe" "%Work%\Wvb-Inspector-Second.exe" >nul || goto :cleanup
echo PASS  WVB inspector reconstruction deterministic Windows application

echo native WVB inspector reconstruction step=execute item=3/4
"%Work%\Wvb-Inspector.exe" >"%Work%\Self.txt" 2>&1 || goto :cleanup
echo PASS  WVB inspector reconstruction self-tests
"%Work%\Wvb-Inspector.exe" ^
    "%RepositoryRoot%\Artifacts\Native-Wvb-To-Wvo-Candidate\Return-42.wvb" ^
    >"%Work%\Absent.txt" 2>&1 || goto :cleanup
findstr /b /c:"module version=1.11 profile=portable name=" "%Work%\Absent.txt" >nul || goto :cleanup
echo PASS  WVB inspector reconstruction metadata-absent module
"%Work%\Wvb-Inspector.exe" ^
    "%RepositoryRoot%\Artifacts\Native-Wvb-To-Wvo-Candidate\Metadata.wvb" ^
    >"%Work%\Present.txt" 2>&1 || goto :cleanup
findstr /b /c:"module version=1.11 profile=hosted name=" "%Work%\Present.txt" >nul || goto :cleanup
findstr /b /c:"capability index=0 name=\"process.argument_count\"" "%Work%\Present.txt" >nul || goto :cleanup
echo PASS  WVB inspector reconstruction metadata-present module

echo native WVB inspector reconstruction step=identity item=4/4
call :verify_file "%RepositoryRoot%\Artifacts\Native-Wvb-To-Wvo-Candidate\Metadata.wvb" ^
    369 94b41f5016722c9e5bf16ace5ec933acc35c14efdd4e08fe11fd582a62b58ffa ^
    "metadata-present fixture" || goto :cleanup
echo native WVB inspector reconstruction status=Passed profile=4 metadata=Present cases=4
echo Tests: 4, Passed: 4, Failed: 0
set "Result=0"

:cleanup
if exist "%Work%\." rmdir /s /q "%Work%"
exit /b %Result%

:prepare_tool
setlocal EnableExtensions DisableDelayedExpansion
set "Project=%~1"
set "Name=%~2"
call "%Native%\Build-Cached-Project-Wvb.cmd" "%Project%" "%Work%\%Name%.wvb" >nul || exit /b 1
call "%Native%\Lower-Wvb-To-Wvo.cmd" "%Work%\%Name%.wvb" "%Work%\%Name%.wvo" >nul || exit /b 1
call "%Native%\Link-Wvo.cmd" 0 Main "%Work%\%Name%-Image.chunk-0" "%Work%\%Name%.wvo" >"%Work%\%Name%.map" || exit /b 1
set "Entry="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Main address=" "%Work%\%Name%.map"') do set "Entry=%%E"
if not defined Entry exit /b 1
call "%Native%\Build-Cached-Hosted-Application.cmd" 1 ^
    "%Work%\%Name%.wvb" "%Work%\%Name%-Image" 1 %Entry% ^
    "%Work%\%Name%.exe" windows >nul || exit /b 1
endlocal
exit /b 0

:verify_file
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /i /c:"%~3" >nul
exit /b %ERRORLEVEL%
