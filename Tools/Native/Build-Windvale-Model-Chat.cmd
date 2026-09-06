@echo off
setlocal EnableExtensions EnableDelayedExpansion

if "%~1"=="" goto :usage
if not "%~2"=="" goto :usage
if /I not "%~x1"==".exe" goto :usage

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Native=%RepositoryRoot%\Tools\Native"
set "Output=%~f1"
for %%D in ("%Output%") do if not exist "%%~dpD." mkdir "%%~dpD" || exit /b 1

:allocate
set "Work=%TEMP%\windvale-model-chat-build-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "WorkResource=%Work:\=/%"
set "Result=1"

echo START Windvale model chat build phase=self-host item=1/5
set "LowererProject=%RepositoryRoot%\Projects\Compiler\Windvale-Native-X64-Lowering-Tool.wvproj"
set "TerminalProject=%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Terminal-Line-Input-Core.wvproj"
set "ChatProject=%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Model-Chat-Core.wvproj"
set "ModelChatProject=%RepositoryRoot%\Projects\Applications\Windvale-Application-Model-Chat.wvproj"
node "%Native%\Build-Current-Split-Project-Wvb.mjs" ^
    "%LowererProject%" "%Work%\Lowerer.wvb" ^
    "%TerminalProject%" "%Work%\Terminal-A.wvb" ^
    "%TerminalProject%" "%Work%\Terminal-B.wvb" ^
    "%ChatProject%" "%Work%\Chat-A.wvb" ^
    "%ChatProject%" "%Work%\Chat-B.wvb" ^
    "%ModelChatProject%" "%Work%\Model-Chat-A.wvb" ^
    "%ModelChatProject%" "%Work%\Model-Chat-B.wvb" || goto :cleanup
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 6 "%Work%\Lowerer.wvb" ^
    "%Work%\Lowerer.exe" --development-cache >nul || goto :cleanup
echo PASS  Windvale model chat build phase=self-host item=1/5

echo START Windvale model chat build phase=compile item=2/5
for %%N in (Terminal Chat) do (
    fc /b "%Work%\%%N-A.wvb" "%Work%\%%N-B.wvb" >nul || goto :cleanup
    call "%Native%\Package-Hosted-Wvb.cmd" 2 "%Work%\%%N-A.wvb" "%Work%\%%N.exe" windows >nul || goto :cleanup
    call "%Native%\Package-Hosted-Wvb.cmd" 2 "%Work%\%%N-A.wvb" "%Work%\%%N.elf" linux >nul || goto :cleanup
    "%Work%\%%N.exe" >nul 2>&1
    if not "!ERRORLEVEL!"=="42" goto :cleanup
)
fc /b "%Work%\Model-Chat-A.wvb" "%Work%\Model-Chat-B.wvb" >nul || goto :cleanup
"%Work%\Lowerer.exe" "%Work%\Model-Chat-A.wvb" "%Work%\Model-Chat-A.wvo" >nul || goto :cleanup
"%Work%\Lowerer.exe" "%Work%\Model-Chat-B.wvb" "%Work%\Model-Chat-B.wvo" >nul || goto :cleanup
fc /b "%Work%\Model-Chat-A.wvo" "%Work%\Model-Chat-B.wvo" >nul || goto :cleanup
call "%Native%\Check-Wvo.cmd" "%Work%\Model-Chat-A.wvo" >nul || goto :cleanup
echo PASS  Windvale model chat build phase=compile item=2/5

echo START Windvale model chat build phase=providers item=3/5
for %%N in (Host Windows Linux) do (
    if /I "%%N"=="Host" set "Source=%RepositoryRoot%\Runtime\Native\X64-External-Model-Gateway-Host.wva"
    if /I "%%N"=="Windows" set "Source=%RepositoryRoot%\Runtime\Native\Windows-X64-External-Model-Gateway.wva"
    if /I "%%N"=="Linux" set "Source=%RepositoryRoot%\Runtime\Native\Linux-X64-External-Model-Gateway.wva"
    call "%Native%\Assemble-Wva.cmd" "!Source!" "%Work%\%%N-A.wvo" >nul || goto :cleanup
    call "%Native%\Assemble-Wva.cmd" "!Source!" "%Work%\%%N-B.wvo" >nul || goto :cleanup
    fc /b "%Work%\%%N-A.wvo" "%Work%\%%N-B.wvo" >nul || goto :cleanup
    call "%Native%\Check-Wvo.cmd" "%Work%\%%N-A.wvo" >nul || goto :cleanup
)
echo PASS  Windvale model chat build phase=providers item=3/5

echo START Windvale model chat build phase=link item=4/5
call "%Native%\Link-Wvo.cmd" 0 Model_gateway_host_entry "%Work%\Model-Chat-Image.chunk-0" ^
    "%Work%\Model-Chat-A.wvo" "%Work%\Host-A.wvo" "%Work%\Windows-A.wvo" ^
    >"%Work%\Link.txt" || goto :cleanup
set "Entry="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Model_gateway_host_entry address=" "%Work%\Link.txt"') do set "Entry=%%E"
if not defined Entry goto :cleanup
call "%Native%\Link-Wvo.cmd" 0 Model_gateway_host_entry "%Work%\Model-Chat-Linux-Image.chunk-0" ^
    "%Work%\Model-Chat-A.wvo" "%Work%\Host-A.wvo" "%Work%\Linux-A.wvo" ^
    >"%Work%\Linux-Link.txt" || goto :cleanup
set "LinuxEntry="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Model_gateway_host_entry address=" "%Work%\Linux-Link.txt"') do set "LinuxEntry=%%E"
if not defined LinuxEntry goto :cleanup
echo PASS  Windvale model chat build phase=link item=4/5

echo START Windvale model chat build phase=package item=5/5
call "%Native%\Build-Cached-Hosted-Application.cmd" 6 "%Work%\Model-Chat-A.wvb" ^
    "%Work%\Model-Chat-Image" 1 %Entry% "%Work%\Windvale-Model-Chat.exe" windows ^
    >nul || goto :cleanup
call "%Native%\Build-Cached-Hosted-Application.cmd" 6 "%Work%\Model-Chat-A.wvb" ^
    "%Work%\Model-Chat-Linux-Image" 1 %LinuxEntry% "%Work%\Windvale-Model-Chat.elf" linux ^
    >nul || goto :cleanup
copy /b /y "%Work%\Windvale-Model-Chat.exe" "%Output%" >nul || goto :cleanup
echo PASS  Windvale model chat build phase=package item=5/5
set "Result=0"

:cleanup
for %%R in ("%Work%") do set "ResolvedWork=%%~fR"
echo(%ResolvedWork%| findstr /b /i /c:"%TEMP%\windvale-model-chat-build-" >nul || exit /b 1
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
if not "%Result%"=="0" exit /b %Result%
echo Windvale model chat build status=Published target=windows output=%Output% core-cases=32 cross-host-images=Verified
exit /b 0

:usage
>&2 echo Usage: Tools\Native\Build-Windvale-Model-Chat.cmd ^<output.exe^>
exit /b 64
