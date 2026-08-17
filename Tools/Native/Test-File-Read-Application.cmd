@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-File-Read-Application.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Native=%RepositoryRoot%\Tools\Native"
set "FrontDoor=%RepositoryRoot%\Artifacts\Native-Front-Door\windows-x64\wvbuild.exe"
set "Workspace=%RepositoryRoot%\Windvale.wvws"
set "WorkspaceResource=%Workspace:\=/%"

:allocate
set "Work=%TEMP%\windvale-file-read-application-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "WorkResource=%Work:\=/%"
set "Result=1"

echo START native file read phase=self-host item=1/6
set "BuildProject=%RepositoryRoot%\Projects\Tools\Windvale-Compiler-Build-Driver.wvproj"
set "BuildProjectResource=%BuildProject:\=/%"
"%FrontDoor%" --workspace "%WorkspaceResource%" --project ^
    "%BuildProjectResource%" "%WorkResource%/Build-Driver.wvb" >nul || goto :cleanup
call :verify_file "%Work%\Build-Driver.wvb" 1142818 125d2b4080889615877d843a36b2f9f6b50d049d011cc06fa8ab426ab83c0574 "current build driver WVB" || goto :cleanup
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 2 ^
    "%Work%\Build-Driver.wvb" "%Work%\Build-Driver.exe" --development-cache >nul || goto :cleanup
set "LowererProject=%RepositoryRoot%\Projects\Compiler\Windvale-Native-X64-Lowering-Tool.wvproj"
set "LowererProjectResource=%LowererProject:\=/%"
"%Work%\Build-Driver.exe" --workspace "%WorkspaceResource%" --project ^
    "%LowererProjectResource%" "%WorkResource%/Lowerer.wvb" >nul || goto :cleanup
call :verify_file "%Work%\Lowerer.wvb" 523087 6b56da9c4ee12917fc4e59f1745ebbfd854335c011f1a5c2c27613abedc1db41 "current lowerer WVB" || goto :cleanup
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 6 ^
    "%Work%\Lowerer.wvb" "%Work%\Lowerer.exe" --development-cache >nul || goto :cleanup
echo PASS  native file read phase=self-host item=1/6

echo START native file read phase=compile item=2/6
echo native file read compile step=source
set "ResponseProject=%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Standard-Byte-Output-Response-Core.wvproj"
set "ResponseProjectResource=%ResponseProject:\=/%"
set "ApplicationProject=%RepositoryRoot%\Projects\Applications\Windvale-File-Read.wvproj"
set "ApplicationProjectResource=%ApplicationProject:\=/%"
"%Work%\Build-Driver.exe" --workspace "%WorkspaceResource%" --project ^
    "%ResponseProjectResource%" "%WorkResource%/Response.wvb" >nul || goto :cleanup
"%Work%\Build-Driver.exe" --workspace "%WorkspaceResource%" --project ^
    "%ApplicationProjectResource%" "%WorkResource%/File-Read.wvb" >nul || goto :cleanup
call :verify_file "%Work%\Response.wvb" 8417 868c9967432b3b5b2859de26bb3caf76dcbcc113d4a9c678625eecde73fd8193 "response-core self-test WVB" || goto :cleanup
call :verify_file "%Work%\File-Read.wvb" 76474 95eed93bf74b10214711efe9a8780c4c289c06bbf8b46e835c00119a36190dfb "file-read WVB" || goto :cleanup
echo native file read compile step=lower
"%Work%\Lowerer.exe" "%Work%\File-Read.wvb" "%Work%\File-Read.wvo" >nul || goto :cleanup
call :verify_file "%Work%\File-Read.wvo" 2410255 8ad63e3dbe87daccf6a9a94407ee0a661f177d6f812b300587b77fe36f7dd323 "file-read WVO" || goto :cleanup
echo native file read compile step=response-package
call "%Native%\Package-Hosted-Wvb.cmd" 2 "%Work%\Response.wvb" "%Work%\Response.exe" windows >nul || goto :cleanup
call "%Native%\Package-Hosted-Wvb.cmd" 2 "%Work%\Response.wvb" "%Work%\Response.elf" linux >nul || goto :cleanup
echo native file read compile step=response-identity
call :verify_file "%Work%\Response.exe" 91648 e6ed27e2a4946f09d0846ddc3a6cb61301b0ccf311b0f17d090d140bb6ddf9a6 "Windows response self-test" || goto :cleanup
call :verify_file "%Work%\Response.elf" 90112 bac55c2f144447501979aeba617435611fd777917fb9c1331d6bfb82419fbbdb "Linux response self-test" || goto :cleanup
echo PASS  native file read phase=compile item=2/6

echo START native file read phase=providers item=3/6
call "%Native%\Assemble-Wva.cmd" "%RepositoryRoot%\Runtime\Native\X64-File-Read-Host.wva" "%Work%\Host.wvo" >nul || goto :cleanup
call "%Native%\Assemble-Wva.cmd" "%RepositoryRoot%\Runtime\Native\Windows-X64-Read-Only-Directory.wva" "%Work%\Directory-Windows.wvo" >nul || goto :cleanup
call "%Native%\Assemble-Wva.cmd" "%RepositoryRoot%\Runtime\Native\Linux-X64-Read-Only-Directory.wva" "%Work%\Directory-Linux.wvo" >nul || goto :cleanup
call "%Native%\Assemble-Wva.cmd" "%RepositoryRoot%\Runtime\Native\Windows-X64-Standard-Byte-Output.wva" "%Work%\Output-Windows.wvo" >nul || goto :cleanup
call "%Native%\Assemble-Wva.cmd" "%RepositoryRoot%\Runtime\Native\Linux-X64-Standard-Byte-Output.wva" "%Work%\Output-Linux.wvo" >nul || goto :cleanup
call :verify_file "%Work%\Host.wvo" 2569 ec306b202ba9820a6ccecdc188abb12e54a0c07166c7d3dd2a97a4921c14af20 "file-read host WVO" || goto :cleanup
call :verify_file "%Work%\Directory-Windows.wvo" 1951 d2da1c67864c242aeb9797661028295922486de2cf7d37aa41024189afb10f34 "Windows directory WVO" || goto :cleanup
call :verify_file "%Work%\Directory-Linux.wvo" 681 0ccbcda71b20eaa024946e4fbb2016853952a39f1fe58ed0a183bde502335d86 "Linux directory WVO" || goto :cleanup
call :verify_file "%Work%\Output-Windows.wvo" 430 68f7701dfc1065d8adfe65028ee52d6e4879f41ef4399318123cbc1870629c2f "Windows output WVO" || goto :cleanup
call :verify_file "%Work%\Output-Linux.wvo" 389 8d28e2a7913f647f105991a7c6112f2f63d014dfa7d5723ad7625b2fb5560ee0 "Linux output WVO" || goto :cleanup
echo PASS  native file read phase=providers item=3/6

echo START native file read phase=link item=4/6
call "%Native%\Link-Wvo.cmd" 0 File_read_host_entry "%Work%\Windows-Image.chunk-0" ^
    "%Work%\File-Read.wvo" "%Work%\Host.wvo" "%Work%\Directory-Windows.wvo" "%Work%\Output-Windows.wvo" >"%Work%\Windows-Link.txt" || goto :cleanup
call "%Native%\Link-Wvo.cmd" 0 File_read_host_entry "%Work%\Linux-Image.chunk-0" ^
    "%Work%\File-Read.wvo" "%Work%\Host.wvo" "%Work%\Directory-Linux.wvo" "%Work%\Output-Linux.wvo" >"%Work%\Linux-Link.txt" || goto :cleanup
set "WindowsEntry="
set "LinuxEntry="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=File_read_host_entry address=" "%Work%\Windows-Link.txt"') do set "WindowsEntry=%%E"
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=File_read_host_entry address=" "%Work%\Linux-Link.txt"') do set "LinuxEntry=%%E"
if not "%WindowsEntry%"=="2407616" goto :cleanup
if not "%LinuxEntry%"=="2407616" goto :cleanup
call :verify_file "%Work%\Windows-Image.chunk-0" 2411432 7905ace13aaea2715c622380177b3a4bdb7470d122d143729603c6ead0d17cfb "Windows linked image" || goto :cleanup
call :verify_file "%Work%\Linux-Image.chunk-0" 2410382 748d356ac947e2eb52fbe7b186a90f0b22aed6bbef6da10f908eff472f22ab05 "Linux linked image" || goto :cleanup
echo PASS  native file read phase=link item=4/6

echo START native file read phase=package item=5/6
call "%Native%\Build-Cached-Hosted-Application.cmd" 6 "%Work%\File-Read.wvb" "%Work%\Windows-Image" 1 2407616 "%Work%\File-Read.exe" windows >nul || goto :cleanup
call "%Native%\Build-Cached-Hosted-Application.cmd" 6 "%Work%\File-Read.wvb" "%Work%\Linux-Image" 1 2407616 "%Work%\File-Read.elf" linux >nul || goto :cleanup
call :verify_file "%Work%\File-Read.exe" 2432000 98c8ae185f9508d7ac6473b433cc8cb21429fe77cf3b218196bf25032e7ba7d5 "Windows file-read application" || goto :cleanup
call :verify_file "%Work%\File-Read.elf" 2433024 e24332de44b14766049742941742e31d8a6b55c62ee31510e95ef9a128de0f24 "Linux file-read application" || goto :cleanup
echo PASS  native file read phase=package item=5/6

echo START native file read phase=execute item=6/6 cases=32
"%Work%\Response.exe" >nul 2>&1
if not "%ERRORLEVEL%"=="42" goto :cleanup
node "%Native%\Verify-File-Read-Application.mjs" windows "%Work%\File-Read.wvb" ^
    "%Work%\File-Read.exe" "%Work%\File-Read.elf" >nul || goto :cleanup
echo PASS  native file read phase=execute item=6/6 cases=32
set "Result=0"

:cleanup
for %%R in ("%Work%") do set "ResolvedWork=%%~fR"
echo(%ResolvedWork%| findstr /b /i /c:"%TEMP%\windvale-file-read-application-" >nul || exit /b 1
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
if not "%Result%"=="0" exit /b %Result%
echo native file read application status=Passed cases=32 capabilities=5 wvb=95eed93bf74b10214711efe9a8780c4c289c06bbf8b46e835c00119a36190dfb cross-host-images=Verified
exit /b 0

:verify_file
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /i /c:"%~3" >nul
exit /b %ERRORLEVEL%
