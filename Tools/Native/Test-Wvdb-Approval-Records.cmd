@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Wvdb-Approval-Records.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Records=%RepositoryRoot%\Distribution\Applications\Wvdb-Query"
set "Verifier=%RepositoryRoot%\Tools\Release\Verify-Wvdb-Approval-Records.mjs"
set "Approval=Windvale-Wvdb-Query.wvapproval"
set "Windows=Windvale-Wvdb-Query.windows-x64.wvlaunch"
set "Linux=Windvale-Wvdb-Query.linux-x64.wvlaunch"
set "InspectorRecords=%RepositoryRoot%\Distribution\Applications\Wvb-Inspector"
set "InspectorApproval=Windvale-Wvb-Inspector.wvapproval"
set "InspectorWindows=Windvale-Wvb-Inspector.windows-x64.wvlaunch"
set "InspectorLinux=Windvale-Wvb-Inspector.linux-x64.wvlaunch"

:allocate
set "Work=%TEMP%\windvale-wvdb-approval-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
for %%D in (Copy Extra Capability Writable Target Approval-Identity Truncated) do (
    mkdir "%Work%\%%D" || goto :cleanup
    copy /y "%Records%\%Approval%" "%Work%\%%D\%Approval%" >nul || goto :cleanup
    copy /y "%Records%\%Windows%" "%Work%\%%D\%Windows%" >nul || goto :cleanup
    copy /y "%Records%\%Linux%" "%Work%\%%D\%Linux%" >nul || goto :cleanup
)
for %%D in (Inspector-Capability Inspector-Host Inspector-Target) do (
    mkdir "%Work%\%%D" || goto :cleanup
    copy /y "%InspectorRecords%\%InspectorApproval%" "%Work%\%%D\%InspectorApproval%" >nul || goto :cleanup
    copy /y "%InspectorRecords%\%InspectorWindows%" "%Work%\%%D\%InspectorWindows%" >nul || goto :cleanup
    copy /y "%InspectorRecords%\%InspectorLinux%" "%Work%\%%D\%InspectorLinux%" >nul || goto :cleanup
)
set "Result=1"

echo native application approval step=verify-wvdb-source item=1/13
node "%Verifier%" verify "%Records%" >nul || goto :cleanup

echo native application approval step=verify-wvdb-copy item=2/13
node "%Verifier%" verify "%Work%\Copy" >nul || goto :cleanup

echo native application approval step=verify-inspector-source item=3/13
node "%Verifier%" verify-inspector "%InspectorRecords%" >nul || goto :cleanup

echo native application approval step=execute-inspector-command item=4/13 target=windows-x64
"%RepositoryRoot%\Artifacts\Native-Front-Door\windows-x64\wvdump.exe" ^
    "%RepositoryRoot%\Artifacts\Native-Front-Door\Wvb\Wvb-Inspector.wvb" ^
    >"%Work%\Inspector-Run.txt" || goto :cleanup
node -e "const x=require('node:fs').readFileSync(process.argv[1],'utf8').replaceAll('\r\n','\n').split('\n');if(x[0]!=='wvdump 1'||!x[1].startsWith('module version=1.11 profile=hosted '))process.exit(1);" "%Work%\Inspector-Run.txt" || goto :cleanup

echo native application approval step=reject-inspector-capability-substitution item=5/13
pwsh -NoLogo -NoProfile -Command "$p='%Work%\Inspector-Capability\%InspectorApproval%'; $t=[IO.File]::ReadAllText($p); [IO.File]::WriteAllText($p, $t.Replace('file.read_bytes', 'file.write_bytes'), [Text.UTF8Encoding]::new($false))" || goto :cleanup
node "%Verifier%" verify-inspector "%Work%\Inspector-Capability" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native application approval step=reject-inspector-host-substitution item=6/13
pwsh -NoLogo -NoProfile -Command "$p='%Work%\Inspector-Host\%InspectorWindows%'; $t=[IO.File]::ReadAllText($p); [IO.File]::WriteAllText($p, $t.Replace('61512dae2941607b93da7d29dd59f973c690f0fec3ba24f772f2101c87ed5381', ('0' * 64)), [Text.UTF8Encoding]::new($false))" || goto :cleanup
node "%Verifier%" verify-inspector "%Work%\Inspector-Host" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native application approval step=reject-inspector-target-substitution item=7/13
copy /y "%Work%\Inspector-Target\%InspectorLinux%" "%Work%\Inspector-Target\%InspectorWindows%" >nul || goto :cleanup
node "%Verifier%" verify-inspector "%Work%\Inspector-Target" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native application approval step=reject-extra-approval item=8/13
>>"%Work%\Extra\%Approval%" echo approve 5 network.connect ambient-network
node "%Verifier%" verify "%Work%\Extra" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native application approval step=reject-wvdb-capability-substitution item=9/13
pwsh -NoLogo -NoProfile -Command "$p='%Work%\Capability\%Approval%'; $t=[IO.File]::ReadAllText($p); [IO.File]::WriteAllText($p, $t.Replace('console.write_line', 'console.write'), [Text.UTF8Encoding]::new($false))" || goto :cleanup
node "%Verifier%" verify "%Work%\Capability" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native application approval step=reject-writable-provider item=10/13
pwsh -NoLogo -NoProfile -Command "$p='%Work%\Writable\%Windows%'; $t=[IO.File]::ReadAllText($p); [IO.File]::WriteAllText($p, $t.Replace('fixed-read-only-object', 'mutable-directory-object'), [Text.UTF8Encoding]::new($false))" || goto :cleanup
node "%Verifier%" verify "%Work%\Writable" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native application approval step=reject-target-substitution item=11/13
copy /y "%Work%\Target\%Linux%" "%Work%\Target\%Windows%" >nul || goto :cleanup
node "%Verifier%" verify "%Work%\Target" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native application approval step=reject-approval-identity-substitution item=12/13
pwsh -NoLogo -NoProfile -Command "$p='%Work%\Approval-Identity\%Linux%'; $t=[IO.File]::ReadAllText($p); [IO.File]::WriteAllText($p, $t.Replace('3c4a968745cde9d5073c67c6c453443d54c74e779b509c2f00131b4d47e8ef71', ('0' * 64)), [Text.UTF8Encoding]::new($false))" || goto :cleanup
node "%Verifier%" verify "%Work%\Approval-Identity" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native application approval step=reject-truncated-record item=13/13
>"%Work%\Truncated\%Windows%" echo windvale-launch-record 1
node "%Verifier%" verify "%Work%\Truncated" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native application approval status=Passed cases=13 applications=2 records=6 capabilities=10 targets=4 executions=1
set "Result=0"

:cleanup
for %%R in ("%Work%") do set "ResolvedWork=%%~fR"
echo(%ResolvedWork%| findstr /b /i /c:"%TEMP%\windvale-wvdb-approval-" >nul || exit /b 1
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
exit /b %Result%
