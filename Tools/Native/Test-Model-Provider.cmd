@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Model-Provider.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Native=%RepositoryRoot%\Tools\Native"
set "BuildDriver=%RepositoryRoot%\Artifacts\Native-Compiler-Reconstruction-Candidate\windows-x64\wvbuild.exe"
set "Lowerer=%RepositoryRoot%\Artifacts\Native-Wvb-To-Wvo-Candidate\Wvb-To-Wvo.exe"
for /f "usebackq delims=" %%T in (`node -p "require('node:fs').realpathSync.native(process.argv[1])" "%TEMP%"`) do set "TemporaryRoot=%%T"
if not defined TemporaryRoot exit /b 1

:allocate
set "Work=%TemporaryRoot%\windvale-model-provider-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "CanonicalWork="
for /f "usebackq delims=" %%W in (`node -p "require('node:fs').realpathSync.native(process.argv[1])" "%Work%"`) do set "CanonicalWork=%%W"
if not defined CanonicalWork (
    rmdir "%Work%" >nul 2>&1
    exit /b 1
)
set "Work=%CanonicalWork%"
set "CanonicalWork="
for /f "usebackq delims=" %%T in (`node -p "require('node:path').dirname(process.argv[1])" "%Work%"`) do set "TemporaryRoot=%%T"
if not defined TemporaryRoot exit /b 1
set "WorkResource=%Work:\=/%"
set "Result=1"

set "Workspace=%RepositoryRoot%\Windvale.wvws"
set "WorkspaceResource=%Workspace:\=/%"
set "ModelProject=%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Hosted-Model-Provider.wvproj"
set "ModelProjectResource=%ModelProject:\=/%"

echo START native model provider phase=tools item=1/4 retained-tools=2
echo Progress: step=model-provider-tools item=1/2 detail=verify-build-driver
call :verify_file "%BuildDriver%" 30071296 f556f0e2c794d9424cbcd9f5e3f8e5aee54f49373c7c18ea1d4829facea7dc6f || goto :cleanup
echo Progress: step=model-provider-tools item=2/2 detail=verify-lowerer
call :verify_file "%Lowerer%" 10661888 a46d73ada72fba9561e9db1fcfc5477bf19be2518ad9db2d8487184112923dfd || goto :cleanup
echo PASS  native model provider phase=tools item=1/4

echo START native model provider phase=compile item=2/4
"%BuildDriver%" --workspace "%WorkspaceResource%" --project "%ModelProjectResource%" "%WorkResource%/Model-A.wvb" >nul || goto :cleanup
"%BuildDriver%" --workspace "%WorkspaceResource%" --project "%ModelProjectResource%" "%WorkResource%/Model-B.wvb" >nul || goto :cleanup
fc /b "%Work%\Model-A.wvb" "%Work%\Model-B.wvb" >nul || goto :cleanup
"%Lowerer%" "%Work%\Model-A.wvb" "%Work%\Model-A.wvo" >nul || goto :cleanup
"%Lowerer%" "%Work%\Model-B.wvb" "%Work%\Model-B.wvo" >nul || goto :cleanup
fc /b "%Work%\Model-A.wvo" "%Work%\Model-B.wvo" >nul || goto :cleanup
call "%Native%\Check-Wvo.cmd" "%Work%\Model-A.wvo" >nul || goto :cleanup
echo PASS  native model provider phase=compile item=2/4

echo START native model provider phase=host item=3/4
call "%Native%\Assemble-Wva.cmd" "%RepositoryRoot%\Runtime\Native\X64-Scripted-Model-Provider-Host.wva" "%Work%\Host-A.wvo" >nul || goto :cleanup
call "%Native%\Assemble-Wva.cmd" "%RepositoryRoot%\Runtime\Native\X64-Scripted-Model-Provider-Host.wva" "%Work%\Host-B.wvo" >nul || goto :cleanup
fc /b "%Work%\Host-A.wvo" "%Work%\Host-B.wvo" >nul || goto :cleanup
call "%Native%\Check-Wvo.cmd" "%Work%\Host-A.wvo" >nul || goto :cleanup
call "%Native%\Link-Wvo.cmd" 0 Model_host_entry "%Work%\Model-Image.chunk-0" "%Work%\Model-A.wvo" "%Work%\Host-A.wvo" >"%Work%\Link.txt" || goto :cleanup
set "Entry="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Model_host_entry address=" "%Work%\Link.txt"') do set "Entry=%%E"
if not defined Entry goto :cleanup
echo PASS  native model provider phase=host item=3/4

echo START native model provider phase=execute item=4/4
call "%Native%\Build-Cached-Hosted-Application.cmd" 6 "%Work%\Model-A.wvb" "%Work%\Model-Image" 1 %Entry% "%Work%\Model.exe" windows >nul || goto :cleanup
call "%Native%\Build-Cached-Hosted-Application.cmd" 6 "%Work%\Model-A.wvb" "%Work%\Model-Image" 1 %Entry% "%Work%\Model.elf" linux >nul || goto :cleanup
"%Work%\Model.exe" >nul
if not "%ERRORLEVEL%"=="0" goto :cleanup
echo PASS  native model provider phase=execute item=4/4
set "Result=0"

:cleanup
for %%R in ("%Work%") do set "ResolvedWork=%%~fR"
echo(%ResolvedWork%| findstr /b /i /c:"%TemporaryRoot%\windvale-model-provider-" >nul || exit /b 1
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
if not "%Result%"=="0" exit /b %Result%
echo native model provider status=Passed cases=11 local-result=0 cross-host-images=Verified
exit /b 0

:verify_file
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~3" >nul
exit /b %ERRORLEVEL%
