@echo off
setlocal EnableExtensions EnableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Native-External-Model-Gateway.cmd
    exit /b 64
)
set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Native=%RepositoryRoot%\Tools\Native"

:allocate
set "Work=%TEMP%\windvale-native-model-gateway-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Result=1"

echo START native external model gateway phase=supervisor item=1/4
node "%RepositoryRoot%\Tools\Models\Test-Native-External-Model-Gateway-Supervisor.mjs" || goto :cleanup
echo PASS  native external model gateway phase=supervisor item=1/4

echo START native external model gateway phase=objects item=2/4
for %%N in (Probe Host Windows Linux) do (
    if /I "%%N"=="Probe" set "Source=%RepositoryRoot%\Tests\Native\X64-External-Model-Gateway-Probe.wva"
    if /I "%%N"=="Host" set "Source=%RepositoryRoot%\Runtime\Native\X64-External-Model-Gateway-Host.wva"
    if /I "%%N"=="Windows" set "Source=%RepositoryRoot%\Runtime\Native\Windows-X64-External-Model-Gateway.wva"
    if /I "%%N"=="Linux" set "Source=%RepositoryRoot%\Runtime\Native\Linux-X64-External-Model-Gateway.wva"
    call "%Native%\Assemble-Wva.cmd" "!Source!" "%Work%\%%N-A.wvo" >nul || goto :cleanup
    call "%Native%\Assemble-Wva.cmd" "!Source!" "%Work%\%%N-B.wvo" >nul || goto :cleanup
    fc /b "%Work%\%%N-A.wvo" "%Work%\%%N-B.wvo" >nul || goto :cleanup
    call "%Native%\Check-Wvo.cmd" "%Work%\%%N-A.wvo" >nul || goto :cleanup
)
echo PASS  native external model gateway phase=objects item=2/4

echo START native external model gateway phase=images item=3/4
call "%Native%\Link-Wvo.cmd" 0 Model_gateway_host_entry "%Work%\Windows-Image.chunk-0" "%Work%\Probe-A.wvo" "%Work%\Host-A.wvo" "%Work%\Windows-A.wvo" >"%Work%\Windows-Link.txt" || goto :cleanup
call "%Native%\Link-Wvo.cmd" 0 Model_gateway_host_entry "%Work%\Linux-Image.chunk-0" "%Work%\Probe-A.wvo" "%Work%\Host-A.wvo" "%Work%\Linux-A.wvo" >"%Work%\Linux-Link.txt" || goto :cleanup
set "WindowsEntry="
set "LinuxEntry="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Model_gateway_host_entry address=" "%Work%\Windows-Link.txt"') do set "WindowsEntry=%%E"
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Model_gateway_host_entry address=" "%Work%\Linux-Link.txt"') do set "LinuxEntry=%%E"
if not defined WindowsEntry goto :cleanup
if not defined LinuxEntry goto :cleanup
call "%Native%\Build-Cached-Hosted-Application.cmd" 6 "%RepositoryRoot%\Artifacts\Wvb-To-Wvo-Candidate.wvb" "%Work%\Windows-Image" 1 %WindowsEntry% "%Work%\Model-Worker.exe" windows >nul || goto :cleanup
call "%Native%\Build-Cached-Hosted-Application.cmd" 6 "%RepositoryRoot%\Artifacts\Wvb-To-Wvo-Candidate.wvb" "%Work%\Linux-Image" 1 %LinuxEntry% "%Work%\Model-Worker.elf" linux >nul || goto :cleanup
echo PASS  native external model gateway phase=images item=3/4

echo START native external model gateway phase=execute item=4/4
node "%RepositoryRoot%\Tools\Models\Test-Native-External-Model-Gateway-Execution.mjs" "%Work%\Model-Worker.exe" || goto :cleanup
echo PASS  native external model gateway phase=execute item=4/4
set "Result=0"

:cleanup
for %%R in ("%Work%") do set "ResolvedWork=%%~fR"
echo(%ResolvedWork%| findstr /b /i /c:"%TEMP%\windvale-native-model-gateway-" >nul || exit /b 1
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
if not "%Result%"=="0" exit /b %Result%
echo native external model gateway status=Passed cases=14 local-result=0 cross-host-images=Verified public-network=0 real-credentials=0
exit /b 0
