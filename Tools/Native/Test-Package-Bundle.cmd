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

echo native package bundle step=rebuild-locked-applications item=5/7 applications=2
call "%Native%\Build-Wvdb-Query-Package.cmd" ^
    "%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvpack" ^
    "%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvlock" ^
    "%Work%\Wvdb-Query.wvb" || goto :cleanup
call :verify_file "%Work%\Wvdb-Query.wvb" 26294 61f7b9d739a0f4ac9eece1cb79e554e373f49375109cf23d332921395ae37dc2 "locked WVDB Query WVB" || goto :cleanup
call "%Native%\Build-Wvb-Inspector-Package.cmd" ^
    "%RepositoryRoot%\Distribution\Applications\Wvb-Inspector\Windvale-Wvb-Inspector.wvpack" ^
    "%RepositoryRoot%\Distribution\Applications\Wvb-Inspector\Windvale-Wvb-Inspector.wvlock" ^
    "%Work%\Wvb-Inspector.wvb" || goto :cleanup
call :verify_file "%Work%\Wvb-Inspector.wvb" 76527 293be3267ff95f9272e96684e036a5647abc060f2bc87a9e654beac7140af753 "locked WVB Inspector WVB" || goto :cleanup

echo native package bundle step=write-and-admit item=6/7 applications=2 candidates=4
node -e "const fs=require('node:fs');const input=fs.readFileSync(process.argv[1],'utf8');const output=input.replaceAll('\r\n','\n');if(output.includes('\r')||output.includes('\0'))process.exit(1);fs.writeFileSync(process.argv[2],output);" "%RepositoryRoot%\LICENSE.md" "%Work%\LICENSE.md" || goto :cleanup
call :verify_file "%Work%\LICENSE.md" 13249 26fc8ccf707d50fcd569353b594345ac234d4bf6e367b2b03cefe6027e108bef "canonical LF license" || goto :cleanup
for %%C in (First Second) do (
    "%Work%\Writer.exe" ^
        "%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvpack" ^
        "%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvlock" ^
        "%Work%\Wvdb-Query.wvb" ^
        "%Work%\LICENSE.md" ^
        "%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvprov" ^
        "%Work%\Wvdb-%%C.wvbundle" || goto :cleanup
    call :verify_file "%Work%\Wvdb-%%C.wvbundle" 43725 3d7f035e15fa839d9a7a3f8df6a7fa152e115aba42c1b48bdd1ae0b1ba998474 "WVDB Query Bundle 1 candidate" || goto :cleanup
    "%Work%\Verifier.exe" "%Work%\Wvdb-%%C.wvbundle" || goto :cleanup

    "%Work%\Writer.exe" ^
        "%RepositoryRoot%\Distribution\Applications\Wvb-Inspector\Windvale-Wvb-Inspector.wvpack" ^
        "%RepositoryRoot%\Distribution\Applications\Wvb-Inspector\Windvale-Wvb-Inspector.wvlock" ^
        "%Work%\Wvb-Inspector.wvb" ^
        "%Work%\LICENSE.md" ^
        "%RepositoryRoot%\Distribution\Applications\Wvb-Inspector\Windvale-Wvb-Inspector.wvprov" ^
        "%Work%\Inspector-%%C.wvbundle" || goto :cleanup
    call :verify_file "%Work%\Inspector-%%C.wvbundle" 92781 a9be069d9eaab7a612a8833d8ce621d1598e01d250ba53a62a2ab4b2126fc4a9 "WVB Inspector Bundle 1 candidate" || goto :cleanup
    "%Work%\Verifier.exe" "%Work%\Inspector-%%C.wvbundle" || goto :cleanup
)
fc /b "%Work%\Wvdb-First.wvbundle" "%Work%\Wvdb-Second.wvbundle" >nul || goto :cleanup
fc /b "%Work%\Inspector-First.wvbundle" "%Work%\Inspector-Second.wvbundle" >nul || goto :cleanup

echo native package bundle step=publish-shared-immutable-store item=7/7 applications=2 attempts=4
pwsh -NoProfile -File "%RepositoryRoot%\Tools\Package\Publish-Admitted-Bundle.ps1" ^
    "%Work%\Wvdb-First.wvbundle" ^
    3d7f035e15fa839d9a7a3f8df6a7fa152e115aba42c1b48bdd1ae0b1ba998474 ^
    "%Work%\Store" >"%Work%\First-Publish.txt" || goto :cleanup
findstr /x /c:"package store status=Published bundle=3d7f035e15fa839d9a7a3f8df6a7fa152e115aba42c1b48bdd1ae0b1ba998474 objects=5 created=6 existing=0" "%Work%\First-Publish.txt" >nul || goto :cleanup
type "%Work%\First-Publish.txt"
pwsh -NoProfile -File "%RepositoryRoot%\Tools\Package\Publish-Admitted-Bundle.ps1" ^
    "%Work%\Wvdb-First.wvbundle" ^
    3d7f035e15fa839d9a7a3f8df6a7fa152e115aba42c1b48bdd1ae0b1ba998474 ^
    "%Work%\Store" >"%Work%\Second-Publish.txt" || goto :cleanup
findstr /x /c:"package store status=Published bundle=3d7f035e15fa839d9a7a3f8df6a7fa152e115aba42c1b48bdd1ae0b1ba998474 objects=5 created=0 existing=6" "%Work%\Second-Publish.txt" >nul || goto :cleanup
type "%Work%\Second-Publish.txt"
pwsh -NoProfile -File "%RepositoryRoot%\Tools\Package\Publish-Admitted-Bundle.ps1" ^
    "%Work%\Inspector-First.wvbundle" ^
    a9be069d9eaab7a612a8833d8ce621d1598e01d250ba53a62a2ab4b2126fc4a9 ^
    "%Work%\Store" >"%Work%\Third-Publish.txt" || goto :cleanup
findstr /x /c:"package store status=Published bundle=a9be069d9eaab7a612a8833d8ce621d1598e01d250ba53a62a2ab4b2126fc4a9 objects=5 created=5 existing=1" "%Work%\Third-Publish.txt" >nul || goto :cleanup
type "%Work%\Third-Publish.txt"
pwsh -NoProfile -File "%RepositoryRoot%\Tools\Package\Publish-Admitted-Bundle.ps1" ^
    "%Work%\Inspector-First.wvbundle" ^
    a9be069d9eaab7a612a8833d8ce621d1598e01d250ba53a62a2ab4b2126fc4a9 ^
    "%Work%\Store" >"%Work%\Fourth-Publish.txt" || goto :cleanup
findstr /x /c:"package store status=Published bundle=a9be069d9eaab7a612a8833d8ce621d1598e01d250ba53a62a2ab4b2126fc4a9 objects=5 created=0 existing=6" "%Work%\Fourth-Publish.txt" >nul || goto :cleanup
type "%Work%\Fourth-Publish.txt"

echo native package bundle status=Passed cases=12 applications=2 bundles=2 objects=9 shared=1 idempotent=Verified
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
