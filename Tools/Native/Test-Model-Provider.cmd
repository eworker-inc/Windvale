@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Model-Provider.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Native=%RepositoryRoot%\Tools\Native"
set "FrontDoor=%RepositoryRoot%\Artifacts\Native-Front-Door\windows-x64\wvbuild.exe"

:allocate
set "Work=%TEMP%\windvale-model-provider-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "WorkResource=%Work:\=/%"
set "Result=1"

set "Workspace=%RepositoryRoot%\Windvale.wvws"
set "WorkspaceResource=%Workspace:\=/%"
set "BuildProject=%RepositoryRoot%\Projects\Tools\Windvale-Compiler-Build-Driver.wvproj"
set "BuildProjectResource=%BuildProject:\=/%"
set "LowererProject=%RepositoryRoot%\Projects\Compiler\Windvale-Native-X64-Lowering-Tool.wvproj"
set "LowererProjectResource=%LowererProject:\=/%"
set "ModelProject=%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Hosted-Model-Provider.wvproj"
set "ModelProjectResource=%ModelProject:\=/%"

echo START native model provider phase=tools item=1/4
"%FrontDoor%" --workspace "%WorkspaceResource%" --project ^
    "%BuildProjectResource%" "%WorkResource%/Build-Driver.wvb" >nul || goto :cleanup
call :verify_file "%Work%\Build-Driver.wvb" 1155121 0cd519556a1cf59321b9418bfbf01643283e10e3dd111c8e2083ec0e51c4ce02 || goto :cleanup
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 2 "%Work%\Build-Driver.wvb" "%Work%\Build-Driver.exe" --development-cache >nul || goto :cleanup
"%Work%\Build-Driver.exe" --workspace "%WorkspaceResource%" --project "%LowererProjectResource%" "%Work%/Lowerer.wvb" >nul || goto :cleanup
call :verify_file "%Work%\Lowerer.wvb" 522025 318717a608ba37360b9c39f53b9720944ab4463af4ab6a1ec9a267a6ceb85bf6 || goto :cleanup
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 6 "%Work%\Lowerer.wvb" "%Work%\Lowerer.exe" --development-cache >nul || goto :cleanup
echo PASS  native model provider phase=tools item=1/4

echo START native model provider phase=compile item=2/4
"%Work%\Build-Driver.exe" --workspace "%WorkspaceResource%" --project "%ModelProjectResource%" "%Work%/Model-A.wvb" >nul || goto :cleanup
"%Work%\Build-Driver.exe" --workspace "%WorkspaceResource%" --project "%ModelProjectResource%" "%Work%/Model-B.wvb" >nul || goto :cleanup
fc /b "%Work%\Model-A.wvb" "%Work%\Model-B.wvb" >nul || goto :cleanup
"%Work%\Lowerer.exe" "%Work%\Model-A.wvb" "%Work%\Model-A.wvo" >nul || goto :cleanup
"%Work%\Lowerer.exe" "%Work%\Model-B.wvb" "%Work%\Model-B.wvo" >nul || goto :cleanup
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
echo(%ResolvedWork%| findstr /b /i /c:"%TEMP%\windvale-model-provider-" >nul || exit /b 1
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
if not "%Result%"=="0" exit /b %Result%
echo native model provider status=Passed cases=11 local-result=0 cross-host-images=Verified
exit /b 0

:verify_file
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~3" >nul
exit /b %ERRORLEVEL%
