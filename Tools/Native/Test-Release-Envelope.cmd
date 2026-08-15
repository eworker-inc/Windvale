@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Release-Envelope.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Creator=%RepositoryRoot%\Tools\Release\Create-Release-Envelope.mjs"
set "Verifier=%RepositoryRoot%\Tools\Release\Verify-Release-Envelope.mjs"
set "FixtureTool=%RepositoryRoot%\Tools\Native\Create-Release-Envelope-Fixture.mjs"

:allocate
set "Work=%TEMP%\windvale-release-envelope-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
for %%D in (
    Fixture Root-Key Release-Key Other-Root-Key Other-Release-Key Policy
    First Second Tamper-Artifact Tamper-Manifest Tamper-Root Extra
    Unsafe-Out Missing-Out Wrong-Key-Out Changed-Out
    Protected-Fixture Protected-Root-Key Protected-Release-Key Protected-Policy Protected-First
    Protected-Wrong-Out Protected-Missing-Out Protected-Tampered-Key Protected-Tamper-Out
) do mkdir "%Work%\%%D" || goto :cleanup
set "Result=1"
set "TestPassphrase=windvale-test-passphrase-1"

echo native release envelope step=create-key-policy item=1/16
node "%FixtureTool%" create "%Work%\Fixture" || goto :cleanup
node "%Creator%" generate-test-key root "%Work%\Root-Key" >nul || goto :cleanup
node "%Creator%" generate-test-key release "%Work%\Release-Key" >nul || goto :cleanup
node "%Creator%" generate-test-key root "%Work%\Other-Root-Key" >nul || goto :cleanup
node "%Creator%" generate-test-key release "%Work%\Other-Release-Key" >nul || goto :cleanup
node "%Creator%" create-root ^
    "%Work%\Fixture\Root-Input.json" ^
    "%Work%\Root-Key\root-private.pem" ^
    "%Work%\Release-Key\release-public.pem" ^
    "%Work%\Policy" >nul || goto :cleanup
if not exist "%Work%\Root-Key\root-private.pem" goto :cleanup
if not exist "%Work%\Release-Key\release-private.pem" goto :cleanup

echo native release envelope step=create-first item=2/16
node "%Creator%" create-release ^
    "%Work%\Policy" ^
    "%Work%\Release-Key\release-private.pem" ^
    "%Work%\Fixture\Release-Input.json" ^
    "%Work%\Fixture\Sources" ^
    "%Work%\First" >nul || goto :cleanup

echo native release envelope step=prove-determinism item=3/16
node "%Creator%" create-release ^
    "%Work%\Policy" ^
    "%Work%\Release-Key\release-private.pem" ^
    "%Work%\Fixture\Release-Input.json" ^
    "%Work%\Fixture\Sources" ^
    "%Work%\Second" >nul || goto :cleanup
node "%FixtureTool%" compare "%Work%\First" "%Work%\Second" >nul || goto :cleanup

echo native release envelope step=verify-valid item=4/16
node "%Verifier%" verify "%Work%\Root-Key\root-public.pem" "%Work%\First" >nul || goto :cleanup
dir /s /b "%Work%\First\*private*" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native release envelope step=reject-artifact-tamper item=5/16
node "%FixtureTool%" copy "%Work%\First" "%Work%\Tamper-Artifact" || goto :cleanup
>>"%Work%\Tamper-Artifact\Artifacts\approval.txt" echo x
node "%Verifier%" verify "%Work%\Root-Key\root-public.pem" "%Work%\Tamper-Artifact" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native release envelope step=reject-manifest-signature-tamper item=6/16
node "%FixtureTool%" copy "%Work%\First" "%Work%\Tamper-Manifest" || goto :cleanup
>>"%Work%\Tamper-Manifest\Release-Manifest.sig" echo x
node "%Verifier%" verify "%Work%\Root-Key\root-public.pem" "%Work%\Tamper-Manifest" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native release envelope step=reject-root-signature-tamper item=7/16
node "%FixtureTool%" copy "%Work%\First" "%Work%\Tamper-Root" || goto :cleanup
>>"%Work%\Tamper-Root\Root-Policy.sig" echo x
node "%Verifier%" verify "%Work%\Root-Key\root-public.pem" "%Work%\Tamper-Root" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native release envelope step=reject-wrong-root item=8/16
node "%Verifier%" verify "%Work%\Other-Root-Key\root-public.pem" "%Work%\First" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native release envelope step=reject-undeclared-file item=9/16
node "%FixtureTool%" copy "%Work%\First" "%Work%\Extra" || goto :cleanup
>"%Work%\Extra\undeclared.txt" echo undeclared
node "%Verifier%" verify "%Work%\Root-Key\root-public.pem" "%Work%\Extra" >nul 2>nul
if not errorlevel 1 goto :cleanup
del "%Work%\Extra\undeclared.txt" || goto :cleanup
mkdir "%Work%\Extra\Artifacts\undeclared-directory" || goto :cleanup
node "%Verifier%" verify "%Work%\Root-Key\root-public.pem" "%Work%\Extra" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native release envelope step=reject-sequence-rollback item=10/16
node "%Verifier%" verify "%Work%\Root-Key\root-public.pem" "%Work%\First" 2 >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native release envelope step=reject-unsafe-path item=11/16
node "%FixtureTool%" mutate-input unsafe-path ^
    "%Work%\Fixture\Release-Input.json" "%Work%\Unsafe-Input.json" || goto :cleanup
node "%Creator%" create-release "%Work%\Policy" ^
    "%Work%\Release-Key\release-private.pem" "%Work%\Unsafe-Input.json" ^
    "%Work%\Fixture\Sources" "%Work%\Unsafe-Out" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native release envelope step=reject-incomplete-profile item=12/16
node "%FixtureTool%" mutate-input missing-profile ^
    "%Work%\Fixture\Release-Input.json" "%Work%\Missing-Input.json" || goto :cleanup
node "%Creator%" create-release "%Work%\Policy" ^
    "%Work%\Release-Key\release-private.pem" "%Work%\Missing-Input.json" ^
    "%Work%\Fixture\Sources" "%Work%\Missing-Out" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native release envelope step=reject-key-and-source-substitution item=13/16
node "%Creator%" create-release "%Work%\Policy" ^
    "%Work%\Other-Release-Key\release-private.pem" "%Work%\Fixture\Release-Input.json" ^
    "%Work%\Fixture\Sources" "%Work%\Wrong-Key-Out" >nul 2>nul
if not errorlevel 1 goto :cleanup
>>"%Work%\Fixture\Sources\approval-all-approval.txt" echo x
node "%Creator%" create-release "%Work%\Policy" ^
    "%Work%\Release-Key\release-private.pem" "%Work%\Fixture\Release-Input.json" ^
    "%Work%\Fixture\Sources" "%Work%\Changed-Out" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native release envelope step=protected-key-roundtrip item=14/16
node "%FixtureTool%" create "%Work%\Protected-Fixture" >nul || goto :cleanup
node -e "process.stdout.write('%TestPassphrase%\n%TestPassphrase%\n')" | node "%Creator%" ^
    generate-key root "%Work%\Protected-Root-Key" --key-passphrase >nul || goto :cleanup
node -e "process.stdout.write('%TestPassphrase%\n%TestPassphrase%\n')" | node "%Creator%" ^
    generate-key release "%Work%\Protected-Release-Key" --key-passphrase >nul || goto :cleanup
if not exist "%Work%\Protected-Root-Key\root-private.wvkey" goto :cleanup
if not exist "%Work%\Protected-Release-Key\release-private.wvkey" goto :cleanup
node -e "process.stdout.write('%TestPassphrase%\n')" | node "%Creator%" create-root ^
    "%Work%\Protected-Fixture\Root-Input.json" ^
    "%Work%\Protected-Root-Key\root-private.wvkey" ^
    "%Work%\Protected-Release-Key\release-public.pem" ^
    "%Work%\Protected-Policy" --key-passphrase >nul || goto :cleanup
node -e "process.stdout.write('%TestPassphrase%\n')" | node "%Creator%" create-release ^
    "%Work%\Protected-Policy" ^
    "%Work%\Protected-Release-Key\release-private.wvkey" ^
    "%Work%\Protected-Fixture\Release-Input.json" ^
    "%Work%\Protected-Fixture\Sources" "%Work%\Protected-First" --key-passphrase >nul || goto :cleanup
node "%Verifier%" verify ^
    "%Work%\Protected-Root-Key\root-public.pem" "%Work%\Protected-First" >nul || goto :cleanup

echo native release envelope step=reject-protected-key-credential-errors item=15/16
node -e "process.stdout.write('windvale-test-wrong-passphrase\n')" | node "%Creator%" create-release ^
    "%Work%\Protected-Policy" ^
    "%Work%\Protected-Release-Key\release-private.wvkey" ^
    "%Work%\Protected-Fixture\Release-Input.json" ^
    "%Work%\Protected-Fixture\Sources" "%Work%\Protected-Wrong-Out" --key-passphrase >nul 2>nul
if not errorlevel 1 goto :cleanup
node "%Creator%" create-release ^
    "%Work%\Protected-Policy" ^
    "%Work%\Protected-Release-Key\release-private.wvkey" ^
    "%Work%\Protected-Fixture\Release-Input.json" ^
    "%Work%\Protected-Fixture\Sources" "%Work%\Protected-Missing-Out" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native release envelope step=reject-protected-key-tamper item=16/16
copy /b /y "%Work%\Protected-Root-Key\root-private.wvkey" ^
    "%Work%\Protected-Tampered-Key\root-private.wvkey" >nul || goto :cleanup
>>"%Work%\Protected-Tampered-Key\root-private.wvkey" echo x
node -e "process.stdout.write('%TestPassphrase%\n')" | node "%Creator%" create-root ^
    "%Work%\Protected-Fixture\Root-Input.json" ^
    "%Work%\Protected-Tampered-Key\root-private.wvkey" ^
    "%Work%\Protected-Release-Key\release-public.pem" ^
    "%Work%\Protected-Tamper-Out" --key-passphrase >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native release envelope status=Passed cases=16 signatures=4 artifacts=11 protected-private-keys=2
set "Result=0"

:cleanup
for %%R in ("%Work%") do set "ResolvedWork=%%~fR"
echo(%ResolvedWork%| findstr /b /i /c:"%TEMP%\windvale-release-envelope-" >nul || exit /b 1
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
exit /b %Result%
