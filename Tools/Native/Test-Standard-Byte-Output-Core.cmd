@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Standard-Byte-Output-Core.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Native=%RepositoryRoot%\Tools\Native"
set "RecoveryCommit=4aca9935679b67f46bfb97f37c2e566980bbab68"

:allocate
set "Work=%TEMP%\windvale-standard-byte-output-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Recovery=%Work%\compiler-recovery"
set "WorktreeAdded=0"
set "Result=1"

set "Workspace=%RepositoryRoot%\Windvale.wvws"
set "WorkspaceResource=%Workspace:\=/%"
set "LibraryProject=%RepositoryRoot%\Projects\Libraries\Windvale-Library-Standard-Byte-Output-Core.wvproj"
set "TestProject=%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Standard-Byte-Output-Core.wvproj"
set "LibraryProjectResource=%LibraryProject:\=/%"
set "TestProjectResource=%TestProject:\=/%"
set "Lowerer=%RepositoryRoot%\Artifacts\Native-Wvb-To-Wvo-Candidate\Wvb-To-Wvo.exe"

echo START native standard byte output phase=tools item=1/4
git -c "safe.directory=%RepositoryRoot%" -C "%RepositoryRoot%" cat-file -e "%RecoveryCommit%^{commit}" >nul 2>nul || goto :cleanup
git -c "safe.directory=%RepositoryRoot%" -C "%RepositoryRoot%" worktree add --detach "%Recovery%" "%RecoveryCommit%" >nul 2>nul || goto :cleanup
set "WorktreeAdded=1"
call "%Recovery%\Tools\Native\Build-Wvb.cmd" "%Recovery%\Projects\Tools\Windvale-Compiler-Build-Driver.wvproj" "%Work%\Build-Driver.wvb" >nul || goto :cleanup
call :verify_file "%Work%\Build-Driver.wvb" 1121370 ed5bbceaa0f1b4d889a7d17fe1d138d0bd5a01a593f6925ba34023ff0b0960ef || goto :cleanup
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 2 "%Work%\Build-Driver.wvb" "%Work%\Build-Driver.exe" --development-cache >nul || goto :cleanup
call :verify_file "%Lowerer%" 10075136 22826b9bb6f391e5ac0e7605fe3246cce16d977c6bed88a5bafec90262aea6ea || goto :cleanup
echo PASS  native standard byte output phase=tools item=1/4

echo START native standard byte output phase=compile item=2/4
"%Work%\Build-Driver.exe" --workspace "%WorkspaceResource%" --project "%LibraryProjectResource%" "%Work%/Library-A.wvb" >nul || goto :cleanup
"%Work%\Build-Driver.exe" --workspace "%WorkspaceResource%" --project "%LibraryProjectResource%" "%Work%/Library-B.wvb" >nul || goto :cleanup
fc /b "%Work%\Library-A.wvb" "%Work%\Library-B.wvb" >nul || goto :cleanup
call :verify_file "%Work%\Library-A.wvb" 55898 d80e98f785e8dfab0e357a7d74457f07775141bf31d2773e2d7745c061a7aa26 || goto :cleanup
"%Work%\Build-Driver.exe" --workspace "%WorkspaceResource%" --project "%TestProjectResource%" "%Work%/Test-A.wvb" >nul || goto :cleanup
"%Work%\Build-Driver.exe" --workspace "%WorkspaceResource%" --project "%TestProjectResource%" "%Work%/Test-B.wvb" >nul || goto :cleanup
fc /b "%Work%\Test-A.wvb" "%Work%\Test-B.wvb" >nul || goto :cleanup
call :verify_file "%Work%\Test-A.wvb" 75874 7fba163fd1087c324bf640879b72a5208375e49ab298950ba97d987a7c2a4d17 || goto :cleanup
"%Lowerer%" "%Work%\Test-A.wvb" "%Work%\Test-A.wvo" >nul || goto :cleanup
"%Lowerer%" "%Work%\Test-B.wvb" "%Work%\Test-B.wvo" >nul || goto :cleanup
fc /b "%Work%\Test-A.wvo" "%Work%\Test-B.wvo" >nul || goto :cleanup
call :verify_file "%Work%\Test-A.wvo" 2650952 2abd417b75f497c6f1b9c99395101fec722597bb38ce436ea1bea3fa9ba476b2 || goto :cleanup
call "%Native%\Check-Wvo.cmd" "%Work%\Test-A.wvo" >nul || goto :cleanup
echo PASS  native standard byte output phase=compile item=2/4

echo START native standard byte output phase=link item=3/4
call "%Native%\Link-Wvo.cmd" 0 Main "%Work%\Output-Image.chunk-0" "%Work%\Test-A.wvo" >"%Work%\Link.txt" || goto :cleanup
set "Entry="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Main address=" "%Work%\Link.txt"') do set "Entry=%%E"
if not defined Entry goto :cleanup
if not "%Entry%"=="356514" goto :cleanup
echo PASS  native standard byte output phase=link item=3/4

echo START native standard byte output phase=execute item=4/4
call "%Native%\Build-Cached-Hosted-Application.cmd" 6 "%Work%\Test-A.wvb" "%Work%\Output-Image" 1 %Entry% "%Work%\Output.exe" windows >nul || goto :cleanup
call "%Native%\Build-Cached-Hosted-Application.cmd" 6 "%Work%\Test-A.wvb" "%Work%\Output-Image" 1 %Entry% "%Work%\Output.elf" linux >nul || goto :cleanup
"%Work%\Output.exe" >nul
if not "%ERRORLEVEL%"=="42" goto :cleanup
echo PASS  native standard byte output phase=execute item=4/4
set "Result=0"

:cleanup
if "%WorktreeAdded%"=="1" git -c "safe.directory=%RepositoryRoot%" -C "%RepositoryRoot%" worktree remove "%Recovery%" >nul 2>nul
for %%R in ("%Work%") do set "ResolvedWork=%%~fR"
echo(%ResolvedWork%| findstr /b /i /c:"%TEMP%\windvale-standard-byte-output-" >nul || exit /b 1
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
if not "%Result%"=="0" exit /b %Result%
echo native standard byte output status=Passed cases=10 local-result=42 cross-host-images=Verified
exit /b 0

:verify_file
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~3" >nul
exit /b %ERRORLEVEL%
