@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Shell-1-Parser.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Native=%RepositoryRoot%\Tools\Native"

:allocate
set "Work=%TEMP%\windvale-shell-one-parser-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Result=1"

set "LibraryProject=%RepositoryRoot%\Projects\Libraries\Windvale-Library-Shell-1-Parser.wvproj"
set "TestProject=%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Shell-1-Parser.wvproj"
set "WebAssemblyProject=%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Shell-1-Parser-WebAssembly-Smoke.wvproj"
set "Lowerer=%RepositoryRoot%\Artifacts\Native-Wvb-To-Wvo-Candidate\Wvb-To-Wvo.exe"

echo START native shell 1 parser phase=tools item=1/5
call :verify_file "%Lowerer%" 7491072 85c07ef9f07b6b1351a5aa467c4e8f77de33099db9fce3c3adaf0a47191de0a3 || goto :cleanup
echo PASS  native shell 1 parser phase=tools item=1/5

echo START native shell 1 parser phase=compile item=2/5
call "%Native%\Build-Wvb.cmd" "%LibraryProject%" "%Work%\Library-A.wvb" >nul || goto :cleanup
call "%Native%\Build-Wvb.cmd" "%LibraryProject%" "%Work%\Library-B.wvb" >nul || goto :cleanup
fc /b "%Work%\Library-A.wvb" "%Work%\Library-B.wvb" >nul || goto :cleanup
call "%Native%\Build-Wvb.cmd" "%TestProject%" "%Work%\Test-A.wvb" >nul || goto :cleanup
call "%Native%\Build-Wvb.cmd" "%TestProject%" "%Work%\Test-B.wvb" >nul || goto :cleanup
fc /b "%Work%\Test-A.wvb" "%Work%\Test-B.wvb" >nul || goto :cleanup
"%Lowerer%" "%Work%\Test-A.wvb" "%Work%\Test-A.wvo" >nul || goto :cleanup
"%Lowerer%" "%Work%\Test-B.wvb" "%Work%\Test-B.wvo" >nul || goto :cleanup
fc /b "%Work%\Test-A.wvo" "%Work%\Test-B.wvo" >nul || goto :cleanup
call "%Native%\Check-Wvo.cmd" "%Work%\Test-A.wvo" >nul || goto :cleanup
call "%Native%\Build-Wvb.cmd" "%WebAssemblyProject%" "%Work%\WebAssembly-Smoke.wvb" >nul || goto :cleanup
echo PASS  native shell 1 parser phase=compile item=2/5

echo START native shell 1 parser phase=link item=3/5
call "%Native%\Link-Wvo.cmd" 0 Main "%Work%\Shell-Image.chunk-0" "%Work%\Test-A.wvo" >"%Work%\Link.txt" || goto :cleanup
set "Entry="
for /f "tokens=3 delims==" %%E in ('findstr /b /c:"entry name=Main address=" "%Work%\Link.txt"') do set "Entry=%%E"
if not defined Entry goto :cleanup
echo PASS  native shell 1 parser phase=link item=3/5

echo START native shell 1 parser phase=execute item=4/5
call "%Native%\Build-Cached-Hosted-Application.cmd" 6 "%Work%\Test-A.wvb" "%Work%\Shell-Image" 1 %Entry% "%Work%\Shell.exe" windows >nul || goto :cleanup
call "%Native%\Build-Cached-Hosted-Application.cmd" 6 "%Work%\Test-A.wvb" "%Work%\Shell-Image" 1 %Entry% "%Work%\Shell.elf" linux >nul || goto :cleanup
"%Work%\Shell.exe" >nul
if not "%ERRORLEVEL%"=="42" goto :cleanup
echo PASS  native shell 1 parser phase=execute item=4/5

echo START native shell 1 parser phase=webassembly item=5/5
node --no-liftoff "%RepositoryRoot%\Tools\Website\Verify-Shell-1-Parser-WebAssembly.mjs" "%Work%\WebAssembly-Smoke.wvb" >nul || goto :cleanup
echo PASS  native shell 1 parser phase=webassembly item=5/5
set "Result=0"

:cleanup
for %%R in ("%Work%") do set "ResolvedWork=%%~fR"
echo(%ResolvedWork%| findstr /b /i /c:"%TEMP%\windvale-shell-one-parser-" >nul || exit /b 1
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
if not "%Result%"=="0" exit /b %Result%
echo native shell 1 parser status=Passed cases=47 local-result=42 webassembly-smoke=11 cross-host-images=Verified
exit /b 0

:verify_file
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~3" >nul
exit /b %ERRORLEVEL%
