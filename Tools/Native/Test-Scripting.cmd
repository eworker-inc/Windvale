@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Scripting.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"

:allocate
set "Work=%TEMP%\windvale-scripting-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%\Installed\bin" || exit /b 1
set "Result=1"
set "Bin=%Work%\Installed\bin"
copy /y "%RepositoryRoot%\Distribution\Installers\Templates\windows-x64\wv.cmd" "%Bin%\wv.cmd" >nul || goto :cleanup
copy /y "%RepositoryRoot%\Distribution\Installers\Templates\windows-x64\wv-run.ps1" "%Bin%\wv-run.ps1" >nul || goto :cleanup
copy /y "%RepositoryRoot%\Artifacts\Native-Front-Door\windows-x64\wvbuild.exe" "%Bin%\wvbuild.exe" >nul || goto :cleanup
copy /y "%RepositoryRoot%\Artifacts\Native-Front-Door\windows-x64\wvverify.exe" "%Bin%\wvverify.exe" >nul || goto :cleanup
copy /y "%RepositoryRoot%\Artifacts\Native-Wvb-Runner-Candidate\windows-x64-wvrun.exe" "%Bin%\wvrun.exe" >nul || goto :cleanup

echo START native scripting cases=6
call "%Bin%\wv.cmd" run >"%Work%\Usage.out" 2>"%Work%\Usage.err"
if not "%ERRORLEVEL%"=="64" goto :cleanup
findstr /x /c:"Usage: wv run <source.wv> [argument ...]" "%Work%\Usage.err" >nul || goto :cleanup
echo PASS  native scripting case=usage

call "%Bin%\wv.cmd" run "%RepositoryRoot%\Tests\Fixtures\Scripting\Portable-Main.wv" >"%Work%\Portable.out" 2>"%Work%\Portable.err" || goto :cleanup
for %%F in ("%Work%\Portable.out" "%Work%\Portable.err") do if not "%%~zF"=="0" goto :cleanup
echo PASS  native scripting case=portable

call "%Bin%\wv.cmd" run "%RepositoryRoot%\Tests\Fixtures\Scripting\Arguments-And-Output.wv" -flag "snow day" >"%Work%\Arguments.out" 2>"%Work%\Arguments.err"
if not "%ERRORLEVEL%"=="7" (
    >&2 echo scripting argument case exit=%ERRORLEVEL%
    type "%Work%\Arguments.err" >&2
    goto :cleanup
)
powershell.exe -NoLogo -NoProfile -Command "$t=[IO.File]::ReadAllText('%Work%\Arguments.out'); if ($t -cne ('first=-flag'+[char]10)) { exit 1 }" || goto :cleanup
powershell.exe -NoLogo -NoProfile -Command "$l=[IO.File]::ReadAllLines('%Work%\Arguments.err'); if ($l.Count -ne 1 -or $l[0] -cne 'second=snow day') { exit 1 }" || goto :cleanup
echo PASS  native scripting case=arguments

call "%Bin%\wv.cmd" run "%RepositoryRoot%\Tests\Fixtures\Scripting\Unsupported-Authority.wv" >"%Work%\Authority.out" 2>"%Work%\Authority.err"
if not "%ERRORLEVEL%"=="1" goto :cleanup
powershell.exe -NoLogo -NoProfile -Command "$l=[IO.File]::ReadAllLines('%Work%\Authority.err'); if ($l.Count -ne 1 -or $l[0] -cne 'wvb run status=Unsupported profile=script-main-i32 phase=envelope') { exit 1 }" || (
    type "%Work%\Authority.err" >&2
    goto :cleanup
)
echo PASS  native scripting case=authority

call "%Bin%\wv.cmd" run "%RepositoryRoot%\Tests\Fixtures\Scripting\Malformed.wv" >"%Work%\Malformed.out" 2>"%Work%\Malformed.err"
if not "%ERRORLEVEL%"=="1" goto :cleanup
if exist "%RepositoryRoot%\Tests\Fixtures\Scripting\Malformed.wvb" goto :cleanup
echo PASS  native scripting case=malformed

call "%Bin%\wv.cmd" run "%RepositoryRoot%\Tests\Fixtures\Scripting\Arguments-And-Output.wv" -- value >"%Work%\Dash.out" 2>"%Work%\Dash.err"
if not "%ERRORLEVEL%"=="7" goto :cleanup
powershell.exe -NoLogo -NoProfile -Command "$t=[IO.File]::ReadAllText('%Work%\Dash.out'); if ($t -cne ('first=--'+[char]10)) { exit 1 }" || goto :cleanup
powershell.exe -NoLogo -NoProfile -Command "$l=[IO.File]::ReadAllLines('%Work%\Dash.err'); if ($l.Count -ne 1 -or $l[0] -cne 'second=value') { exit 1 }" || goto :cleanup
echo PASS  native scripting case=dash-argument

echo PASS  native scripting compile=hidden verification=mandatory arguments=immutable authority=base-only cleanup=verified
set "Result=0"

:cleanup
for %%R in ("%Work%") do set "ResolvedWork=%%~fR"
echo(%ResolvedWork%| findstr /b /i /c:"%TEMP%\windvale-scripting-" >nul || exit /b 1
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
if not "%Result%"=="0" exit /b %Result%
echo Tests: 6, Passed: 6, Failed: 0
exit /b 0
