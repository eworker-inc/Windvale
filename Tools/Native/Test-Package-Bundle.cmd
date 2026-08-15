@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Package-Bundle.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Native=%RepositoryRoot%\Tools\Native"

:allocate
set "Work=%TEMP%\windvale-package-bundle-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Result=1"

echo native package bundle step=build-tools item=1/7
call "%Native%\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Package-Bundle.wvproj" "%Work%\Self-Test.wvb" || goto :cleanup
call :verify_file "%Work%\Self-Test.wvb" 312949 5bff1f4aeb5c535396acd2b58e89ad39a01299f2acb5ae3e13ef31730745dbd1 "bundle self-test WVB" || goto :cleanup
call "%Native%\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tools\Windvale-Package-Bundle-Writer.wvproj" "%Work%\Writer.wvb" || goto :cleanup
call :verify_file "%Work%\Writer.wvb" 265268 5e6090061127550d8eb38dd3b3cdfbf3eab30d1cba4af6692711a2c2e094fb31 "bundle writer WVB" || goto :cleanup
call "%Native%\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tools\Windvale-Package-Bundle-Verifier.wvproj" "%Work%\Verifier.wvb" || goto :cleanup
call :verify_file "%Work%\Verifier.wvb" 284561 a4f381e9e2dec1c7f415aeb9be24973a971e337b7aff861ed3f84f8b1d7e29fb "bundle verifier WVB" || goto :cleanup

echo native package bundle step=package-self-test item=2/7
call "%Native%\Package-Hosted-Wvb.cmd" 6 "%Work%\Self-Test.wvb" "%Work%\Self-Test.exe" windows || goto :cleanup
"%Work%\Self-Test.exe"
if not "%ERRORLEVEL%"=="42" goto :cleanup

echo native package bundle step=package-writer item=3/7
call "%Native%\Package-Hosted-Wvb.cmd" 6 "%Work%\Writer.wvb" "%Work%\Writer.exe" windows || goto :cleanup
echo native package bundle step=package-independent-verifier item=4/7
call "%Native%\Package-Hosted-Wvb.cmd" 6 "%Work%\Verifier.wvb" "%Work%\Verifier.exe" windows || goto :cleanup

echo native package bundle step=rebuild-locked-application item=5/7
call "%Native%\Build-Wvdb-Query-Package.cmd" ^
    "%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvpack" ^
    "%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvlock" ^
    "%Work%\Wvdb-Query.wvb" || goto :cleanup
call :verify_file "%Work%\Wvdb-Query.wvb" 26294 61f7b9d739a0f4ac9eece1cb79e554e373f49375109cf23d332921395ae37dc2 "locked WVDB Query WVB" || goto :cleanup

echo native package bundle step=write-and-admit item=6/7 candidates=2
for %%C in (First Second) do (
    "%Work%\Writer.exe" ^
        "%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvpack" ^
        "%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvlock" ^
        "%Work%\Wvdb-Query.wvb" ^
        "%RepositoryRoot%\LICENSE.md" ^
        "%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvprov" ^
        "%Work%\%%C.wvbundle" || goto :cleanup
    call :verify_file "%Work%\%%C.wvbundle" 43995 48dff6cf6ce4d5e58e0e13d5a75a514deb86aa98d0b43b5ffbf69d7155b04b6d "WVDB Query Bundle 1 candidate" || goto :cleanup
    "%Work%\Verifier.exe" "%Work%\%%C.wvbundle" || goto :cleanup
)
fc /b "%Work%\First.wvbundle" "%Work%\Second.wvbundle" >nul || goto :cleanup

echo native package bundle step=publish-immutable-store item=7/7 attempts=2
pwsh -NoProfile -File "%RepositoryRoot%\Tools\Package\Publish-Admitted-Bundle.ps1" ^
    "%Work%\First.wvbundle" ^
    48dff6cf6ce4d5e58e0e13d5a75a514deb86aa98d0b43b5ffbf69d7155b04b6d ^
    "%Work%\Store" >"%Work%\First-Publish.txt" || goto :cleanup
findstr /x /c:"package store status=Published bundle=48dff6cf6ce4d5e58e0e13d5a75a514deb86aa98d0b43b5ffbf69d7155b04b6d objects=5 created=6 existing=0" "%Work%\First-Publish.txt" >nul || goto :cleanup
type "%Work%\First-Publish.txt"
pwsh -NoProfile -File "%RepositoryRoot%\Tools\Package\Publish-Admitted-Bundle.ps1" ^
    "%Work%\First.wvbundle" ^
    48dff6cf6ce4d5e58e0e13d5a75a514deb86aa98d0b43b5ffbf69d7155b04b6d ^
    "%Work%\Store" >"%Work%\Second-Publish.txt" || goto :cleanup
findstr /x /c:"package store status=Published bundle=48dff6cf6ce4d5e58e0e13d5a75a514deb86aa98d0b43b5ffbf69d7155b04b6d objects=5 created=0 existing=6" "%Work%\Second-Publish.txt" >nul || goto :cleanup
type "%Work%\Second-Publish.txt"

echo native package bundle status=Passed cases=7 bundle=48dff6cf6ce4d5e58e0e13d5a75a514deb86aa98d0b43b5ffbf69d7155b04b6d objects=5 idempotent=Verified
set "Result=0"

:cleanup
for %%R in ("%Work%") do set "ResolvedWork=%%~fR"
echo(%ResolvedWork%| findstr /b /i /c:"%TEMP%\windvale-package-bundle-" >nul || exit /b 1
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
exit /b %Result%

:verify_file
if not exist "%~1" (
    >&2 echo Missing %~4: %~1
    exit /b 1
)
for %%F in ("%~1") do if not "%%~zF"=="%~2" (
    >&2 echo Invalid byte length for %~4.
    exit /b 1
)
certutil -hashfile "%~1" SHA256 | findstr /i /c:"%~3" >nul
if errorlevel 1 (
    >&2 echo Invalid SHA-256 for %~4.
    exit /b 1
)
exit /b 0
