@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Bounded-Operation-Core.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Native=%RepositoryRoot%\Tools\Native"

:allocate
set "Work=%TEMP%\windvale-bounded-operation-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Result=1"

set "Workspace=%RepositoryRoot%\Windvale.wvws"
set "WorkspaceResource=%Workspace:\=/%"
set "BuildProject=%RepositoryRoot%\Projects\Tools\Windvale-Compiler-Build-Driver.wvproj"
set "LibraryProject=%RepositoryRoot%\Projects\Libraries\Windvale-Library-Bounded-Operation-Core.wvproj"
set "TestProject=%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Bounded-Operation-Core.wvproj"
set "LibraryProjectResource=%LibraryProject:\=/%"
set "TestProjectResource=%TestProject:\=/%"
set "Lowerer=%RepositoryRoot%\Artifacts\Native-Wvb-To-Wvo-Candidate\Wvb-To-Wvo.exe"

echo START native bounded operation phase=tools item=1/4
call "%Native%\Build-Wvb.cmd" "%BuildProject%" "%Work%\Build-Driver.wvb" >nul || goto :cleanup
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 2 "%Work%\Build-Driver.wvb" "%Work%\Build-Driver.exe" >nul || goto :cleanup
call :verify_file "%Lowerer%" 7483904 496d9e00c682af316b69c0e7639858fa0da171b4d467c4838b1ed774ac18b4cc || goto :cleanup
echo PASS  native bounded operation phase=tools item=1/4

echo START native bounded operation phase=compile item=2/4
"%Work%\Build-Driver.exe" --workspace "%WorkspaceResource%" --project "%LibraryProjectResource%" "%Work%/Library-A.wvb" >nul || goto :cleanup
"%Work%\Build-Driver.exe" --workspace "%WorkspaceResource%" --project "%LibraryProjectResource%" "%Work%/Library-B.wvb" >nul || goto :cleanup
fc /b "%Work%\Library-A.wvb" "%Work%\Library-B.wvb" >nul || goto :cleanup
"%Work%\Build-Driver.exe" --workspace "%WorkspaceResource%" --project "%TestProjectResource%" "%Work%/Test-A.wvb" >nul || goto :cleanup
"%Work%\Build-Driver.exe" --workspace "%WorkspaceResource%" --project "%TestProjectResource%" "%Work%/Test-B.wvb" >nul || goto :cleanup
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
echo(%ResolvedWork%| findstr /b /i /c:"%TEMP%\windvale-bounded-operation-" >nul || exit /b 1
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
if not "%Result%"=="0" exit /b %Result%
echo native bounded operation status=Passed cases=10 local-result=42 cross-host-images=Verified
exit /b 0

:verify_file
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~3" >nul
exit /b %ERRORLEVEL%
