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
set "Result=1"

echo native wvdb approval step=verify-source-records item=1/8
node "%Verifier%" verify "%Records%" >nul || goto :cleanup

echo native wvdb approval step=verify-installed-copy item=2/8
node "%Verifier%" verify "%Work%\Copy" >nul || goto :cleanup

echo native wvdb approval step=reject-extra-approval item=3/8
>>"%Work%\Extra\%Approval%" echo approve 5 network.connect ambient-network
node "%Verifier%" verify "%Work%\Extra" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native wvdb approval step=reject-capability-substitution item=4/8
pwsh -NoLogo -NoProfile -Command "$p='%Work%\Capability\%Approval%'; $t=[IO.File]::ReadAllText($p); [IO.File]::WriteAllText($p, $t.Replace('console.write_line', 'console.write'), [Text.UTF8Encoding]::new($false))" || goto :cleanup
node "%Verifier%" verify "%Work%\Capability" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native wvdb approval step=reject-writable-provider item=5/8
pwsh -NoLogo -NoProfile -Command "$p='%Work%\Writable\%Windows%'; $t=[IO.File]::ReadAllText($p); [IO.File]::WriteAllText($p, $t.Replace('fixed-read-only-object', 'mutable-directory-object'), [Text.UTF8Encoding]::new($false))" || goto :cleanup
node "%Verifier%" verify "%Work%\Writable" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native wvdb approval step=reject-target-substitution item=6/8
copy /y "%Work%\Target\%Linux%" "%Work%\Target\%Windows%" >nul || goto :cleanup
node "%Verifier%" verify "%Work%\Target" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native wvdb approval step=reject-approval-identity-substitution item=7/8
pwsh -NoLogo -NoProfile -Command "$p='%Work%\Approval-Identity\%Linux%'; $t=[IO.File]::ReadAllText($p); [IO.File]::WriteAllText($p, $t.Replace('3c4a968745cde9d5073c67c6c453443d54c74e779b509c2f00131b4d47e8ef71', ('0' * 64)), [Text.UTF8Encoding]::new($false))" || goto :cleanup
node "%Verifier%" verify "%Work%\Approval-Identity" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native wvdb approval step=reject-truncated-record item=8/8
>"%Work%\Truncated\%Windows%" echo windvale-launch-record 1
node "%Verifier%" verify "%Work%\Truncated" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native wvdb approval status=Passed cases=8 records=3 capabilities=5 targets=2
set "Result=0"

:cleanup
for %%R in ("%Work%") do set "ResolvedWork=%%~fR"
echo(%ResolvedWork%| findstr /b /i /c:"%TEMP%\windvale-wvdb-approval-" >nul || exit /b 1
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
exit /b %Result%
