@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Offline-Package-Stage.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Native=%RepositoryRoot%\Tools\Native"
set "Creator=%RepositoryRoot%\Tools\Release\Create-Release-Envelope.mjs"
set "ReleaseVerifier=%RepositoryRoot%\Tools\Release\Verify-Release-Envelope.mjs"
set "ApprovalVerifier=%RepositoryRoot%\Tools\Release\Verify-Wvdb-Approval-Records.mjs"
set "StageTool=%RepositoryRoot%\Tools\Package\Create-Offline-Package-Stage-Input.mjs"
set "FixtureTool=%RepositoryRoot%\Tools\Native\Create-Release-Envelope-Fixture.mjs"
set "GenerationPublisher=%RepositoryRoot%\Tools\Package\Publish-Installation-Generation.mjs"

:allocate
set "Work=%TEMP%\windvale-offline-package-stage-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
for %%D in (Stage-Input Root-Key Release-Key Policy First Second Tampered Installed) do mkdir "%Work%\%%D" || goto :cleanup
set "Result=1"

echo native offline package stage step=build-tools item=1/8
call "%Native%\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tools\Windvale-Package-Bundle-Writer.wvproj" ^
    "%Work%\Writer.wvb" || goto :cleanup
call :verify_file "%Work%\Writer.wvb" 265268 5e6090061127550d8eb38dd3b3cdfbf3eab30d1cba4af6692711a2c2e094fb31 "bundle writer WVB" || goto :cleanup
call "%Native%\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tools\Windvale-Package-Bundle-Verifier.wvproj" ^
    "%Work%\Verifier.wvb" || goto :cleanup
call :verify_file "%Work%\Verifier.wvb" 284561 a4f381e9e2dec1c7f415aeb9be24973a971e337b7aff861ed3f84f8b1d7e29fb "bundle verifier WVB" || goto :cleanup
call "%Native%\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tools\Windvale-Installation-Generation-Verifier.wvproj" ^
    "%Work%\Generation-Verifier.wvb" || goto :cleanup
call :verify_file "%Work%\Generation-Verifier.wvb" 42364 2beb02ba0ea13b1552a0c3bf9b92bebe438ac65b2eb49000a4fc1762ed8f7e9f "generation verifier WVB" || goto :cleanup
call "%Native%\Package-Hosted-Wvb.cmd" 6 "%Work%\Writer.wvb" "%Work%\Writer.exe" windows || goto :cleanup
call "%Native%\Package-Hosted-Wvb.cmd" 6 "%Work%\Verifier.wvb" "%Work%\Verifier.exe" windows || goto :cleanup
call "%Native%\Package-Hosted-Wvb.cmd" 6 "%Work%\Generation-Verifier.wvb" "%Work%\Generation-Verifier.exe" windows || goto :cleanup

echo native offline package stage step=build-packages item=2/8 packages=2
call "%Native%\Build-Wvdb-Query-Package.cmd" ^
    "%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvpack" ^
    "%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvlock" ^
    "%Work%\Wvdb-Query.wvb" || goto :cleanup
call "%Native%\Build-Wvb-Inspector-Package.cmd" ^
    "%RepositoryRoot%\Distribution\Applications\Wvb-Inspector\Windvale-Wvb-Inspector.wvpack" ^
    "%RepositoryRoot%\Distribution\Applications\Wvb-Inspector\Windvale-Wvb-Inspector.wvlock" ^
    "%Work%\Wvb-Inspector.wvb" || goto :cleanup

echo native offline package stage step=write-and-admit-bundles item=3/8 packages=2
node -e "const fs=require('node:fs');const input=fs.readFileSync(process.argv[1],'utf8');const output=input.replaceAll('\r\n','\n');if(output.includes('\r')||output.includes('\0'))process.exit(1);fs.writeFileSync(process.argv[2],output);" "%RepositoryRoot%\LICENSE.md" "%Work%\LICENSE.md" || goto :cleanup
"%Work%\Writer.exe" ^
    "%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvpack" ^
    "%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvlock" ^
    "%Work%\Wvdb-Query.wvb" "%Work%\LICENSE.md" ^
    "%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvprov" ^
    "%Work%\Wvdb-Query.wvbundle" || goto :cleanup
"%Work%\Verifier.exe" "%Work%\Wvdb-Query.wvbundle" >nul || goto :cleanup
"%Work%\Writer.exe" ^
    "%RepositoryRoot%\Distribution\Applications\Wvb-Inspector\Windvale-Wvb-Inspector.wvpack" ^
    "%RepositoryRoot%\Distribution\Applications\Wvb-Inspector\Windvale-Wvb-Inspector.wvlock" ^
    "%Work%\Wvb-Inspector.wvb" "%Work%\LICENSE.md" ^
    "%RepositoryRoot%\Distribution\Applications\Wvb-Inspector\Windvale-Wvb-Inspector.wvprov" ^
    "%Work%\Wvb-Inspector.wvbundle" || goto :cleanup
"%Work%\Verifier.exe" "%Work%\Wvb-Inspector.wvbundle" >nul || goto :cleanup

echo native offline package stage step=create-exact-input item=4/8 policy-records=8
node "%ApprovalVerifier%" verify >nul || goto :cleanup
node "%ApprovalVerifier%" verify-inspector >nul || goto :cleanup
set "Revision="
set "Tree="
for /f %%H in ('git -C "%RepositoryRoot%" rev-parse HEAD') do set "Revision=%%H"
for /f %%H in ('git -C "%RepositoryRoot%" rev-parse HEAD:') do set "Tree=%%H"
if not defined Revision goto :cleanup
if not defined Tree goto :cleanup
node "%StageTool%" "%Work%\Wvdb-Query.wvbundle" "%Work%\Wvb-Inspector.wvbundle" ^
    %Revision% %Tree% "%Work%\Stage-Input" || goto :cleanup

echo native offline package stage step=sign-first item=5/8 channel=stage
node "%Creator%" generate-test-key root "%Work%\Root-Key" >nul || goto :cleanup
node "%Creator%" generate-test-key release "%Work%\Release-Key" >nul || goto :cleanup
node "%Creator%" create-root "%Work%\Stage-Input\Root-Input.json" ^
    "%Work%\Root-Key\root-private.pem" "%Work%\Release-Key\release-public.pem" ^
    "%Work%\Policy" >nul || goto :cleanup
node "%Creator%" create-release "%Work%\Policy" ^
    "%Work%\Release-Key\release-private.pem" "%Work%\Stage-Input\Release-Input.json" ^
    "%Work%\Stage-Input\Sources" "%Work%\First" >nul || goto :cleanup

echo native offline package stage step=prove-determinism item=6/8
node "%Creator%" create-release "%Work%\Policy" ^
    "%Work%\Release-Key\release-private.pem" "%Work%\Stage-Input\Release-Input.json" ^
    "%Work%\Stage-Input\Sources" "%Work%\Second" >nul || goto :cleanup
node "%FixtureTool%" compare "%Work%\First" "%Work%\Second" >nul || goto :cleanup

echo native offline package stage step=verify-offline-directory item=7/8 packages=2
node "%ReleaseVerifier%" verify "%Work%\Root-Key\root-public.pem" "%Work%\First" >"%Work%\Verify.txt" || goto :cleanup
findstr /c:"release verify status=Valid version=0.1.0 channel=stage" "%Work%\Verify.txt" >nul || goto :cleanup
findstr /c:"artifact package windvale.wvb-inspector a9be069d9eaab7a612a8833d8ce621d1598e01d250ba53a62a2ab4b2126fc4a9 92781" "%Work%\First\Release-Manifest.txt" >nul || goto :cleanup
findstr /c:"artifact package windvale.wvdb-query 3d7f035e15fa839d9a7a3f8df6a7fa152e115aba42c1b48bdd1ae0b1ba998474 43725" "%Work%\First\Release-Manifest.txt" >nul || goto :cleanup
findstr /c:"artifact generation linux-x64 a8e0aebbd379c892fae1d310531dc8903c89eb0405cbb00d03fbc5c0f8db8b56 726" "%Work%\First\Release-Manifest.txt" >nul || goto :cleanup
findstr /c:"artifact generation windows-x64 8cf5e55537565204f15010501a3ed50085b5cfde2b78aca06495f4e5422d741c 728" "%Work%\First\Release-Manifest.txt" >nul || goto :cleanup
"%Work%\Generation-Verifier.exe" "%Work%\First\Artifacts\Generations\Generation-1.windows-x64.txt" >"%Work%\Generation-Windows.txt" || goto :cleanup
findstr /c:"generation status=Valid target=windows-x64 packages=2 commands=2" "%Work%\Generation-Windows.txt" >nul || goto :cleanup
"%Work%\Generation-Verifier.exe" "%Work%\First\Artifacts\Generations\Generation-1.linux-x64.txt" >"%Work%\Generation-Linux.txt" || goto :cleanup
findstr /c:"generation status=Valid target=linux-x64 packages=2 commands=2" "%Work%\Generation-Linux.txt" >nul || goto :cleanup
node "%GenerationPublisher%" publish "%Work%\Installed" ^
    "%Work%\First\Artifacts\Generations\Generation-1.windows-x64.txt" ^
    8cf5e55537565204f15010501a3ed50085b5cfde2b78aca06495f4e5422d741c >nul || goto :cleanup
node "%GenerationPublisher%" verify "%Work%\Installed" ^
    8cf5e55537565204f15010501a3ed50085b5cfde2b78aca06495f4e5422d741c >nul || goto :cleanup

echo native offline package stage step=reject-package-tamper item=8/8
node "%FixtureTool%" copy "%Work%\First" "%Work%\Tampered" >nul || goto :cleanup
>>"%Work%\Tampered\Artifacts\Packages\Windvale-Wvb-Inspector.wvbundle" echo x
node "%ReleaseVerifier%" verify "%Work%\Root-Key\root-public.pem" "%Work%\Tampered" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native offline package stage status=Passed cases=8 packages=2 policy-records=8 generations=2 published=1 artifacts=14 deterministic=Verified tamper=Rejected
set "Result=0"

:cleanup
for %%R in ("%Work%") do set "ResolvedWork=%%~fR"
echo(%ResolvedWork%| findstr /b /i /c:"%TEMP%\windvale-offline-package-stage-" >nul || exit /b 1
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
exit /b %Result%

:verify_file
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /i /c:"%~3" >nul || exit /b 1
exit /b 0
