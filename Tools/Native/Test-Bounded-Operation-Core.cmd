@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Bounded-Operation-Core.cmd
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
set "Work=%TemporaryRoot%\windvale-bounded-operation-%RANDOM%-%RANDOM%-%RANDOM%"
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
set "LibraryProject=%RepositoryRoot%\Projects\Libraries\Windvale-Library-Bounded-Operation-Core.wvproj"
set "TestProject=%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Bounded-Operation-Core.wvproj"
set "LibraryProjectResource=%LibraryProject:\=/%"
set "TestProjectResource=%TestProject:\=/%"

echo START native bounded operation phase=tools item=1/4 retained-tools=2
call :verify_file "%BuildDriver%" 30071296 f556f0e2c794d9424cbcd9f5e3f8e5aee54f49373c7c18ea1d4829facea7dc6f || goto :cleanup
call :verify_file "%Lowerer%" 8160256 f21a0767685e6e29604625852794ae1118fe41060e639fc690baecb7c60dedad || goto :cleanup
echo PASS  native bounded operation phase=tools item=1/4

echo START native bounded operation phase=compile item=2/4
"%BuildDriver%" --workspace "%WorkspaceResource%" --project "%LibraryProjectResource%" "%WorkResource%/Library-A.wvb" >nul || goto :cleanup
"%BuildDriver%" --workspace "%WorkspaceResource%" --project "%LibraryProjectResource%" "%WorkResource%/Library-B.wvb" >nul || goto :cleanup
fc /b "%Work%\Library-A.wvb" "%Work%\Library-B.wvb" >nul || goto :cleanup
"%BuildDriver%" --workspace "%WorkspaceResource%" --project "%TestProjectResource%" "%WorkResource%/Test-A.wvb" >nul || goto :cleanup
"%BuildDriver%" --workspace "%WorkspaceResource%" --project "%TestProjectResource%" "%WorkResource%/Test-B.wvb" >nul || goto :cleanup
fc /b "%Work%\Test-A.wvb" "%Work%\Test-B.wvb" >nul || goto :cleanup
"%Lowerer%" "%Work%\Test-A.wvb" "%Work%\Test-A.wvo" >nul || goto :cleanup
"%Lowerer%" "%Work%\Test-B.wvb" "%Work%\Test-B.wvo" >nul || goto :cleanup
fc /b "%Work%\Test-A.wvo" "%Work%\Test-B.wvo" >nul || goto :cleanup
call "%Native%\Check-Wvo.cmd" "%Work%\Test-A.wvo" >nul || goto :cleanup
echo PASS  native bounded operation phase=compile item=2/4

echo START native bounded operation phase=link item=3/4
call "%Native%\Link-Wvo.cmd" 0 Main "%Work%\Operation-Image.chunk-0" "%Work%\Test-A.wvo" >"%Work%\Link.txt" || goto :cleanup
set "Entry="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Main address=" "%Work%\Link.txt"') do set "Entry=%%E"
if not defined Entry goto :cleanup
echo PASS  native bounded operation phase=link item=3/4

echo START native bounded operation phase=execute item=4/4
call "%Native%\Build-Cached-Hosted-Application.cmd" 6 "%Work%\Test-A.wvb" "%Work%\Operation-Image" 1 %Entry% "%Work%\Operation.exe" windows >nul || goto :cleanup
call "%Native%\Build-Cached-Hosted-Application.cmd" 6 "%Work%\Test-A.wvb" "%Work%\Operation-Image" 1 %Entry% "%Work%\Operation.elf" linux >nul || goto :cleanup
"%Work%\Operation.exe" >nul
if not "%ERRORLEVEL%"=="42" goto :cleanup
echo PASS  native bounded operation phase=execute item=4/4
set "Result=0"

:cleanup
for %%R in ("%Work%") do set "ResolvedWork=%%~fR"
echo(%ResolvedWork%| findstr /b /i /c:"%TemporaryRoot%\windvale-bounded-operation-" >nul || exit /b 1
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
if not "%Result%"=="0" exit /b %Result%
echo native bounded operation status=Passed cases=10 local-result=42 cross-host-images=Verified
exit /b 0

:verify_file
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~3" >nul
exit /b %ERRORLEVEL%
