@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Language-1.0-Front-Door.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Native=%RepositoryRoot%\Tools\Native"
set "ProfileRoot=%RepositoryRoot%\Documents\Project\Language-1.0-Localization-Workloads\01-Source-Profile-Admission\Reference-Artifacts"
set "SourceLock=%ProfileRoot%\Source-Inputs.wvlock"
set "SourceProfile=%ProfileRoot%\En-Source-Profile.wvsp"
set "SourceLockHash=4c5840af896924292a2ad3f3d5d986956211745a8e4a9bb60f0b45f10cecf9c3"

:allocate
set "Work=%TEMP%\windvale-language-1-front-door-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Result=1"
set "FailureStep=frozen-fixtures"

echo START language 1 front door phase=frozen-fixtures item=1/8
node "%Native%\Verify-Language-1.0-Migration-Fixtures.mjs" || goto :cleanup
echo PASS  language 1 front door phase=frozen-fixtures item=1/8

set "FailureStep=descriptor"
echo START language 1 front door phase=descriptor item=2/8
call "%Native%\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Source-Descriptor.wvproj" "%Work%\Descriptor-A.wvb" >nul || goto :cleanup
call "%Native%\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Source-Descriptor.wvproj" "%Work%\Descriptor-B.wvb" >nul || goto :cleanup
fc /b "%Work%\Descriptor-A.wvb" "%Work%\Descriptor-B.wvb" >nul || goto :cleanup
call "%Native%\Run-Wvb.cmd" "%Work%\Descriptor-A.wvb" >"%Work%\Run.out" 2>"%Work%\Run.err" || goto :cleanup
for %%F in ("%Work%\Run.err") do if not "%%~zF"=="0" goto :cleanup
set "RunLine="
set /a RunLines=0
for /f "usebackq delims=" %%L in ("%Work%\Run.out") do (
    set "RunLine=%%L"
    set /a RunLines+=1
)
if not "%RunLines%"=="1" goto :cleanup
if not "%RunLine%"=="Result: 42" goto :cleanup
echo PASS  language 1 front door phase=descriptor item=2/8

set "FailureStep=value-front-end"
echo START language 1 front door phase=value-front-end item=3/8
call "%Native%\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Language-1-Value-Front-End.wvproj" "%Work%\Value-Front-End.wvb" >nul || goto :cleanup
call "%Native%\Run-Wvb.cmd" "%Work%\Value-Front-End.wvb" >"%Work%\Value-Front-End.out" 2>"%Work%\Value-Front-End.err" || goto :cleanup
for %%F in ("%Work%\Value-Front-End.err") do if not "%%~zF"=="0" goto :cleanup
findstr /c:"Result: 42" "%Work%\Value-Front-End.out" >nul || goto :cleanup
echo PASS  language 1 front door phase=value-front-end item=3/8

set "FailureStep=compiler-segmented-cache"
echo START language 1 front door phase=compiler-slice item=4/8
call "%Native%\Build-Cached-Segmented-Project.cmd" ^
    "%RepositoryRoot%\Projects\Examples\Windvale-Compiler.wvproj" ^
    "%RepositoryRoot%\Artifacts\Native-Compiler-Reconstruction-Candidate\windows-x64\wvbuild.exe" ^
    "%Work%\Compiler.wvb" "%Work%\Compiler-Image" "%Work%\Compiler.wvli" ^
    >"%Work%\Segmented.out" 2>"%Work%\Segmented.err" || goto :cleanup
for %%F in ("%Work%\Segmented.err") do if not "%%~zF"=="0" goto :cleanup
type "%Work%\Segmented.out"
set "FailureStep=compiler-segmented-report"
set "CompilerEntry="
set "CompilerFragments="
for /f "tokens=10,12 delims== " %%E in ('findstr /b /c:"native segmented project cache status=" "%Work%\Segmented.out"') do (
    set "CompilerEntry=%%E"
    set "CompilerFragments=%%F"
)
if not defined CompilerEntry goto :cleanup
if not defined CompilerFragments goto :cleanup
set "FailureStep=compiler-hosted-cache"
call "%Native%\Build-Cached-Hosted-Application.cmd" 1 ^
    "%Work%\Compiler.wvb" "%Work%\Compiler-Image" ^
    %CompilerFragments% %CompilerEntry% "%Work%\Compiler.exe" windows ^
    >"%Work%\Hosted.out" 2>"%Work%\Hosted.err" || goto :cleanup
for %%F in ("%Work%\Hosted.err") do if not "%%~zF"=="0" goto :cleanup
type "%Work%\Hosted.out"
set "FailureStep=compiler-minimum-a"
"%Work%\Compiler.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Minimum-Program.wv" ^
    "%Work%\Minimum-A.wvb" >"%Work%\Compile-A.out" 2>"%Work%\Compile-A.err" || goto :cleanup
set "FailureStep=compiler-minimum-b"
"%Work%\Compiler.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Minimum-Program.wv" ^
    "%Work%\Minimum-B.wvb" >"%Work%\Compile-B.out" 2>"%Work%\Compile-B.err" || goto :cleanup
for %%F in ("%Work%\Compile-A.err" "%Work%\Compile-B.err") do if not "%%~zF"=="0" goto :cleanup
set "FailureStep=compiler-determinism"
fc /b "%Work%\Compile-A.out" "%Work%\Compile-B.out" >nul || goto :cleanup
fc /b "%Work%\Minimum-A.wvb" "%Work%\Minimum-B.wvb" >nul || goto :cleanup
set "FailureStep=compiler-execution-run"
call "%Native%\Run-Wvb.cmd" "%Work%\Minimum-A.wvb" >"%Work%\Minimum.out" 2>"%Work%\Minimum.err" || goto :cleanup
for %%F in ("%Work%\Minimum.err") do if not "%%~zF"=="0" goto :cleanup
set "FailureStep=compiler-execution-output"
set "MinimumLine="
set /a MinimumLines=0
for /f "usebackq delims=" %%L in ("%Work%\Minimum.out") do (
    set "MinimumLine=%%L"
    set /a MinimumLines+=1
)
if not "%MinimumLines%"=="1" goto :cleanup
if not "%MinimumLine%"=="Result: 42" goto :cleanup
set "FailureStep=compiler-unit-a"
"%Work%\Compiler.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Unit-Control.wv" ^
    "%Work%\Unit-A.wvb" >"%Work%\Unit-A.out" 2>"%Work%\Unit-A.err" || goto :cleanup
set "FailureStep=compiler-unit-b"
"%Work%\Compiler.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Unit-Control.wv" ^
    "%Work%\Unit-B.wvb" >"%Work%\Unit-B.out" 2>"%Work%\Unit-B.err" || goto :cleanup
for %%F in ("%Work%\Unit-A.err" "%Work%\Unit-B.err") do if not "%%~zF"=="0" goto :cleanup
set "FailureStep=compiler-unit-determinism"
fc /b "%Work%\Unit-A.out" "%Work%\Unit-B.out" >nul || goto :cleanup
fc /b "%Work%\Unit-A.wvb" "%Work%\Unit-B.wvb" >nul || goto :cleanup
type "%Work%\Unit-A.out"
for %%F in ("%Work%\Unit-A.wvb") do echo INFO  language 1 unit wvb-bytes=%%~zF
set "FailureStep=compiler-record-update-a"
"%Work%\Compiler.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Record-Update.wv" ^
    "%Work%\Record-Update-A.wvb" >"%Work%\Record-Update-A.out" 2>"%Work%\Record-Update-A.err" || goto :cleanup
set "FailureStep=compiler-record-update-b"
"%Work%\Compiler.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Record-Update.wv" ^
    "%Work%\Record-Update-B.wvb" >"%Work%\Record-Update-B.out" 2>"%Work%\Record-Update-B.err" || goto :cleanup
for %%F in ("%Work%\Record-Update-A.err" "%Work%\Record-Update-B.err") do if not "%%~zF"=="0" goto :cleanup
set "FailureStep=compiler-record-update-determinism"
fc /b "%Work%\Record-Update-A.out" "%Work%\Record-Update-B.out" >nul || goto :cleanup
fc /b "%Work%\Record-Update-A.wvb" "%Work%\Record-Update-B.wvb" >nul || goto :cleanup
type "%Work%\Record-Update-A.out"
for %%F in ("%Work%\Record-Update-A.wvb") do echo INFO  language 1 record-update wvb-bytes=%%~zF
set "FailureStep=compiler-record-update-execution"
call "%Native%\Run-Wvb.cmd" "%Work%\Record-Update-A.wvb" >"%Work%\Record-Update.out" 2>"%Work%\Record-Update.err" || goto :cleanup
for %%F in ("%Work%\Record-Update.err") do if not "%%~zF"=="0" goto :cleanup
set "RecordUpdateLine="
set /a RecordUpdateLines=0
for /f "usebackq delims=" %%L in ("%Work%\Record-Update.out") do (
    set "RecordUpdateLine=%%L"
    set /a RecordUpdateLines+=1
)
if not "%RecordUpdateLines%"=="1" goto :cleanup
if not "%RecordUpdateLine%"=="Result: 42" goto :cleanup
set "FailureStep=compiler-negative-unit-return-value"
call :expect_rejection "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Unit-Return-Value.wv" "%Work%\Unit-Return-Value.wvb" || goto :cleanup
set "FailureStep=compiler-negative-nonunit-return"
call :expect_rejection "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Unit-Return-From-I32.wv" "%Work%\Unit-Return-From-I32.wvb" || goto :cleanup
set "FailureStep=compiler-negative-seed-unit"
"%Work%\Compiler.exe" ^
    "%RepositoryRoot%\Tests\Fixtures\Source-Wvb\Invalid-Unit-Literal.wv" ^
    "%Work%\Seed-Unit.wvb" >"%Work%\Seed-Unit.out" 2>"%Work%\Seed-Unit.err"
if not errorlevel 1 goto :cleanup
if exist "%Work%\Seed-Unit.wvb" goto :cleanup
set "FailureStep=compiler-negative-record-update-base"
call :expect_rejection "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Record-Update-Wrong-Base.wv" "%Work%\Record-Update-Wrong-Base.wvb" || goto :cleanup
set "FailureStep=compiler-negative-record-update-duplicate"
call :expect_rejection "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Record-Update-Duplicate-Field.wv" "%Work%\Record-Update-Duplicate-Field.wvb" || goto :cleanup
set "FailureStep=compiler-negative-record-update-unknown"
call :expect_rejection "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Record-Update-Unknown-Field.wv" "%Work%\Record-Update-Unknown-Field.wvb" || goto :cleanup
set "FailureStep=compiler-negative-seed-record-update"
"%Work%\Compiler.exe" ^
    "%RepositoryRoot%\Tests\Fixtures\Source-Wvb\Invalid-Record-Update.wv" ^
    "%Work%\Seed-Record-Update.wvb" >"%Work%\Seed-Record-Update.out" 2>"%Work%\Seed-Record-Update.err"
if not errorlevel 1 goto :cleanup
if exist "%Work%\Seed-Record-Update.wvb" goto :cleanup
set "FailureStep=compiler-negative-unsupported-profile"
call :expect_rejection "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Unsupported-Source-Profile.wv" "%Work%\Unsupported.wvb" || goto :cleanup
set "FailureStep=compiler-negative-missing-profile"
call :expect_rejection "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Missing-Edition-Profile.wv" "%Work%\Missing-Profile.wvb" || goto :cleanup
set "FailureStep=compiler-negative-descriptorless"
call :expect_rejection "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Descriptorless-Edition-Header.wv" "%Work%\Descriptorless.wvb" || goto :cleanup
set "FailureStep=compiler-negative-seed-void"
call :expect_rejection "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Seed-Only-Void.wv" "%Work%\Seed-Only-Void.wvb" || goto :cleanup
set "FailureStep=compiler-negative-no-ambient-profile"
"%Work%\Compiler.exe" "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Minimum-Program.wv" "%Work%\Ambient.wvb" >"%Work%\Ambient.out" 2>"%Work%\Ambient.err"
if not errorlevel 1 goto :cleanup
if exist "%Work%\Ambient.wvb" goto :cleanup
set "FailureStep=compiler-negative-lock-digest"
call :expect_rejection_with_digest "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Minimum-Program.wv" "%Work%\Wrong-Digest.wvb" "4c5840af896924292a2ad3f3d5d986956211745a8e4a9bb60f0b45f10cecf9c0" "%SourceProfile%" || goto :cleanup
set "FailureStep=compiler-negative-profile-content"
copy /y "%SourceProfile%" "%Work%\Corrupt.wvsp" >nul || goto :cleanup
>>"%Work%\Corrupt.wvsp" echo x
call :expect_rejection_with_digest "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Minimum-Program.wv" "%Work%\Corrupt.wvb" "%SourceLockHash%" "%Work%\Corrupt.wvsp" || goto :cleanup
set "FailureStep=compiler-identity"
for %%F in ("%Work%\Minimum-A.wvb") do if not "%%~zF"=="221" goto :cleanup
certutil -hashfile "%Work%\Minimum-A.wvb" SHA256 | findstr /I /C:"25a18cf13d791db1e85fd6b237f89f21d4a0c7b9460b0a72db2da5e5deb205ae" >nul || goto :cleanup
echo PASS  language 1 front door phase=compiler-slice item=4/8

set "FailureStep=fixed-integer-compile-a"
echo START language 1 front door phase=fixed-integers item=5/8
"%Work%\Compiler.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Fixed-Integer-Program.wv" ^
    "%Work%\Fixed-Integer-A.wvb" >"%Work%\Fixed-Integer-A.out" 2>"%Work%\Fixed-Integer-A.err" || goto :cleanup
set "FailureStep=fixed-integer-compile-b"
"%Work%\Compiler.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Fixed-Integer-Program.wv" ^
    "%Work%\Fixed-Integer-B.wvb" >"%Work%\Fixed-Integer-B.out" 2>"%Work%\Fixed-Integer-B.err" || goto :cleanup
set "FailureStep=fixed-integer-determinism"
for %%F in ("%Work%\Fixed-Integer-A.err" "%Work%\Fixed-Integer-B.err") do if not "%%~zF"=="0" goto :cleanup
fc /b "%Work%\Fixed-Integer-A.out" "%Work%\Fixed-Integer-B.out" >nul || goto :cleanup
fc /b "%Work%\Fixed-Integer-A.wvb" "%Work%\Fixed-Integer-B.wvb" >nul || goto :cleanup

set "FailureStep=fixed-integer-trap-inputs"
for %%N in (Overflow Divide-By-Zero Invalid-Shift) do (
    "%Work%\Compiler.exe" ^
        --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
        --source-profile "%SourceProfile%" ^
        "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Fixed-Integer-%%N.wv" ^
        "%Work%\Fixed-Integer-%%N.wvb" >"%Work%\Fixed-Integer-%%N.out" 2>"%Work%\Fixed-Integer-%%N.err" || goto :cleanup
)
set "FailureStep=fixed-integer-source-rejections"
call :expect_rejection "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Fixed-Integer-Literal-Out-Of-Range.wv" "%Work%\Fixed-Integer-Literal-Out-Of-Range.wvb" || goto :cleanup
call :expect_rejection "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Fixed-Integer-Type-Mismatch.wv" "%Work%\Fixed-Integer-Type-Mismatch.wvb" || goto :cleanup
call :expect_rejection "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Fixed-Integer-Signed-Bitwise.wv" "%Work%\Fixed-Integer-Signed-Bitwise.wvb" || goto :cleanup
call :expect_rejection "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Fixed-Integer-Constant-Overflow.wv" "%Work%\Fixed-Integer-Constant-Overflow.wvb" || goto :cleanup

set "FailureStep=fixed-integer-verifier-build"
call "%Native%\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tools\Windvale-Compiler-Wvb-Verifier.wvproj" ^
    "%Work%\Verifier.wvb" >"%Work%\Verifier-Build.out" 2>"%Work%\Verifier-Build.err" || goto :cleanup
set "FailureStep=fixed-integer-verifier-package"
call "%Native%\Package-Hosted-Wvb.cmd" 2 ^
    "%Work%\Verifier.wvb" "%Work%\Verifier.exe" windows ^
    >"%Work%\Verifier-Package.out" 2>"%Work%\Verifier-Package.err" || goto :cleanup
set "FailureStep=fixed-integer-verifier"
for %%N in (A Overflow Divide-By-Zero Invalid-Shift) do (
    "%Work%\Verifier.exe" "%Work%\Fixed-Integer-%%N.wvb" ^
        >"%Work%\Verify-%%N.out" 2>"%Work%\Verify-%%N.err" || goto :cleanup
    findstr /c:"wvb status=Valid profile=compiler-aligned" "%Work%\Verify-%%N.out" >nul || goto :cleanup
)
set "FailureStep=fixed-integer-malformed"
node "%Native%\Verify-Language-1.0-Fixed-Integers.mjs" ^
    "%Work%\Verifier.exe" "%Work%\Fixed-Integer-A.wvb" ^
    "%Work%\Fixed-Integer-Malformed" || goto :cleanup

set "FailureStep=fixed-integer-execution"
call "%Native%\Run-Wvb.cmd" "%Work%\Fixed-Integer-A.wvb" ^
    >"%Work%\Fixed-Integer-Run.out" 2>"%Work%\Fixed-Integer-Run.err" || goto :cleanup
for %%F in ("%Work%\Fixed-Integer-Run.err") do if not "%%~zF"=="0" goto :cleanup
findstr /c:"Result: 42" "%Work%\Fixed-Integer-Run.out" >nul || goto :cleanup
call :expect_runtime_failure "%Work%\Fixed-Integer-Overflow.wvb" 3007 || goto :cleanup
call :expect_runtime_failure "%Work%\Fixed-Integer-Divide-By-Zero.wvb" 3032 || goto :cleanup
call :expect_runtime_failure "%Work%\Fixed-Integer-Invalid-Shift.wvb" 3033 || goto :cleanup

set "FailureStep=fixed-integer-runtime-oracle-build"
call "%Native%\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Wvb-Fixed-Integer-Runtime.wvproj" ^
    "%Work%\Fixed-Integer-Runtime.wvb" >nul || goto :cleanup
call "%Native%\Lower-Wvb-To-Wvo.cmd" ^
    "%Work%\Fixed-Integer-Runtime.wvb" "%Work%\Fixed-Integer-Runtime.wvo" >nul || goto :cleanup
call "%Native%\Check-Wvo.cmd" "%Work%\Fixed-Integer-Runtime.wvo" >nul || goto :cleanup
call "%Native%\Link-Wvo.cmd" 1048576 Main ^
    "%Work%\Fixed-Integer-Runtime.bin" "%Work%\Fixed-Integer-Runtime.wvo" ^
    >"%Work%\Fixed-Integer-Runtime.wvmap" || goto :cleanup
set "RuntimeAddress="
for /f "tokens=5 delims== " %%E in ('findstr /b /c:"entry name=Main address=" "%Work%\Fixed-Integer-Runtime.wvmap"') do set "RuntimeAddress=%%E"
if not defined RuntimeAddress goto :cleanup
set /a RuntimeEntry=RuntimeAddress-1048576
call "%Native%\Package-Console.cmd" windows-x64-console-v1 ^
    "%Work%\Fixed-Integer-Runtime.bin" %RuntimeEntry% ^
    "%Work%\Fixed-Integer-Runtime.exe" >nul || goto :cleanup
"%Work%\Fixed-Integer-Runtime.exe"
if not "%ERRORLEVEL%"=="42" goto :cleanup
for %%F in ("%Work%\Fixed-Integer-A.wvb") do echo INFO  language 1 fixed-integer wvb-bytes=%%~zF
echo PASS  language 1 front door phase=fixed-integers item=5/8

set "FailureStep=rune-compile-a"
echo START language 1 front door phase=runes item=6/8
"%Work%\Compiler.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Rune-Program.wv" ^
    "%Work%\Rune-A.wvb" >"%Work%\Rune-A.out" 2>"%Work%\Rune-A.err" || goto :cleanup
set "FailureStep=rune-compile-b"
"%Work%\Compiler.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Rune-Program.wv" ^
    "%Work%\Rune-B.wvb" >"%Work%\Rune-B.out" 2>"%Work%\Rune-B.err" || goto :cleanup
set "FailureStep=rune-determinism"
for %%F in ("%Work%\Rune-A.err" "%Work%\Rune-B.err") do if not "%%~zF"=="0" goto :cleanup
fc /b "%Work%\Rune-A.out" "%Work%\Rune-B.out" >nul || goto :cleanup
fc /b "%Work%\Rune-A.wvb" "%Work%\Rune-B.wvb" >nul || goto :cleanup

set "FailureStep=rune-source-rejections"
for %%N in (Empty Multiple Surrogate Out-Of-Range Invalid-Escape Unterminated Type-Mismatch Invalid-Operator) do (
    call :expect_rejection "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Rune-%%N.wv" "%Work%\Rune-%%N.wvb" || goto :cleanup
)

set "FailureStep=rune-verifier"
"%Work%\Verifier.exe" "%Work%\Rune-A.wvb" ^
    >"%Work%\Verify-Rune.out" 2>"%Work%\Verify-Rune.err" || goto :cleanup
findstr /c:"wvb status=Valid profile=compiler-aligned" "%Work%\Verify-Rune.out" >nul || goto :cleanup
set "FailureStep=rune-malformed"
node "%Native%\Verify-Language-1.0-Runes.mjs" ^
    "%Work%\Verifier.exe" "%Work%\Rune-A.wvb" ^
    "%Work%\Rune-Malformed" || goto :cleanup

set "FailureStep=rune-execution"
call "%Native%\Run-Wvb.cmd" "%Work%\Rune-A.wvb" ^
    >"%Work%\Rune-Run.out" 2>"%Work%\Rune-Run.err" || goto :cleanup
for %%F in ("%Work%\Rune-Run.err") do if not "%%~zF"=="0" goto :cleanup
findstr /x /c:"Result: 42" "%Work%\Rune-Run.out" >nul || goto :cleanup

set "FailureStep=rune-runtime-oracle-build"
call "%Native%\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Wvb-Rune-Runtime.wvproj" ^
    "%Work%\Rune-Runtime.wvb" >nul || goto :cleanup
call "%Native%\Lower-Wvb-To-Wvo.cmd" ^
    "%Work%\Rune-Runtime.wvb" "%Work%\Rune-Runtime.wvo" >nul || goto :cleanup
call "%Native%\Check-Wvo.cmd" "%Work%\Rune-Runtime.wvo" >nul || goto :cleanup
call "%Native%\Link-Wvo.cmd" 1048576 Main ^
    "%Work%\Rune-Runtime.bin" "%Work%\Rune-Runtime.wvo" ^
    >"%Work%\Rune-Runtime.wvmap" || goto :cleanup
set "RuntimeAddress="
for /f "tokens=5 delims== " %%E in ('findstr /b /c:"entry name=Main address=" "%Work%\Rune-Runtime.wvmap"') do set "RuntimeAddress=%%E"
if not defined RuntimeAddress goto :cleanup
set /a RuntimeEntry=RuntimeAddress-1048576
call "%Native%\Package-Console.cmd" windows-x64-console-v1 ^
    "%Work%\Rune-Runtime.bin" %RuntimeEntry% ^
    "%Work%\Rune-Runtime.exe" >nul || goto :cleanup
"%Work%\Rune-Runtime.exe"
if not "%ERRORLEVEL%"=="42" goto :cleanup
for %%F in ("%Work%\Rune-A.wvb") do set "RuneWvbBytes=%%~zF"
echo INFO  language 1 rune wvb-bytes=%RuneWvbBytes%
echo PASS  language 1 front door phase=runes item=6/8

set "FailureStep=floating-compile-a"
echo START language 1 front door phase=floating item=7/8
"%Work%\Compiler.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Floating-Program.wv" ^
    "%Work%\Floating-A.wvb" >"%Work%\Floating-A.out" 2>"%Work%\Floating-A.err" || goto :cleanup
set "FailureStep=floating-compile-b"
"%Work%\Compiler.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Floating-Program.wv" ^
    "%Work%\Floating-B.wvb" >"%Work%\Floating-B.out" 2>"%Work%\Floating-B.err" || goto :cleanup
set "FailureStep=floating-determinism"
for %%F in ("%Work%\Floating-A.err" "%Work%\Floating-B.err") do if not "%%~zF"=="0" goto :cleanup
fc /b "%Work%\Floating-A.out" "%Work%\Floating-B.out" >nul || goto :cleanup
fc /b "%Work%\Floating-A.wvb" "%Work%\Floating-B.wvb" >nul || goto :cleanup

set "FailureStep=floating-source-rejections"
for %%N in (Decimal-Literal Missing-Suffix Type-Mismatch Invalid-Operator) do (
    call :expect_rejection "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Floating-%%N.wv" "%Work%\Floating-%%N.wvb" || goto :cleanup
)

set "FailureStep=floating-verifier"
"%Work%\Verifier.exe" "%Work%\Floating-A.wvb" ^
    >"%Work%\Verify-Floating.out" 2>"%Work%\Verify-Floating.err" || goto :cleanup
findstr /c:"wvb status=Valid profile=compiler-aligned" "%Work%\Verify-Floating.out" >nul || goto :cleanup
set "FailureStep=floating-malformed"
node "%Native%\Verify-Language-1.0-Floating.mjs" ^
    "%Work%\Verifier.exe" "%Work%\Floating-A.wvb" ^
    "%Work%\Floating-Malformed" || goto :cleanup

set "FailureStep=floating-runner-build"
call "%Native%\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tools\Windvale-Wvb-Runner.wvproj" ^
    "%Work%\Floating-Runner.wvb" >nul || goto :cleanup
set "FailureStep=floating-runner-package"
call "%Native%\Package-Hosted-Wvb.cmd" 5 ^
    "%Work%\Floating-Runner.wvb" "%Work%\Floating-Runner.exe" windows ^
    >nul || goto :cleanup
set "FailureStep=floating-runner-execution"
"%Work%\Floating-Runner.exe" "%Work%\Floating-A.wvb" ^
    >"%Work%\Floating-Run.out" 2>"%Work%\Floating-Run.err" || goto :cleanup
set "FailureStep=floating-runner-diagnostic"
for %%F in ("%Work%\Floating-Run.err") do if not "%%~zF"=="0" goto :cleanup
set "FailureStep=floating-runner-result"
set "FloatingRunLine="
set /a FloatingRunLines=0
for /f "usebackq delims=" %%L in ("%Work%\Floating-Run.out") do (
    set "FloatingRunLine=%%L"
    set /a FloatingRunLines+=1 >nul
)
if not "%FloatingRunLines%"=="1" goto :cleanup
if not "%FloatingRunLine%"=="Result: 42" goto :cleanup

set "FailureStep=floating-runtime-oracle-build"
call "%Native%\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Wvb-Floating-Runtime.wvproj" ^
    "%Work%\Floating-Runtime.wvb" >nul || goto :cleanup
call "%Native%\Lower-Wvb-To-Wvo.cmd" ^
    "%Work%\Floating-Runtime.wvb" "%Work%\Floating-Runtime.wvo" >nul || goto :cleanup
call "%Native%\Check-Wvo.cmd" "%Work%\Floating-Runtime.wvo" >nul || goto :cleanup
call "%Native%\Link-Wvo.cmd" 1048576 Main ^
    "%Work%\Floating-Runtime.bin" "%Work%\Floating-Runtime.wvo" ^
    >"%Work%\Floating-Runtime.wvmap" || goto :cleanup
set "RuntimeAddress="
for /f "tokens=5 delims== " %%E in ('findstr /b /c:"entry name=Main address=" "%Work%\Floating-Runtime.wvmap"') do set "RuntimeAddress=%%E"
if not defined RuntimeAddress goto :cleanup
set /a RuntimeEntry=RuntimeAddress-1048576
call "%Native%\Package-Console.cmd" windows-x64-console-v1 ^
    "%Work%\Floating-Runtime.bin" %RuntimeEntry% ^
    "%Work%\Floating-Runtime.exe" >nul || goto :cleanup
"%Work%\Floating-Runtime.exe"
if not "%ERRORLEVEL%"=="42" goto :cleanup
for %%F in ("%Work%\Floating-A.wvb") do set "FloatingWvbBytes=%%~zF"
echo INFO  language 1 floating wvb-bytes=%FloatingWvbBytes%
echo PASS  language 1 front door phase=floating item=7/8

set "FailureStep=unit-never-compile-a"
echo START language 1 front door phase=unit-never item=8/8
"%Work%\Compiler.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Never-Control.wv" ^
    "%Work%\Never-A.wvb" >"%Work%\Never-A.out" 2>"%Work%\Never-A.err" || goto :cleanup
set "FailureStep=unit-never-compile-b"
"%Work%\Compiler.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Never-Control.wv" ^
    "%Work%\Never-B.wvb" >"%Work%\Never-B.out" 2>"%Work%\Never-B.err" || goto :cleanup
set "FailureStep=unit-never-determinism"
for %%F in ("%Work%\Never-A.err" "%Work%\Never-B.err") do if not "%%~zF"=="0" goto :cleanup
fc /b "%Work%\Never-A.out" "%Work%\Never-B.out" >nul || goto :cleanup
fc /b "%Work%\Never-A.wvb" "%Work%\Never-B.wvb" >nul || goto :cleanup

set "FailureStep=unit-never-source-rejections"
for %%N in (Fallthrough Return Parameter Unreachable) do (
    call :expect_rejection "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Never-%%N.wv" "%Work%\Never-%%N.wvb" || goto :cleanup
)

set "FailureStep=unit-never-verifier"
node "%Native%\Verify-Language-1.0-Unit-Never.mjs" ^
    "%Work%\Verifier.exe" "%Work%\Unit-A.wvb" "%Work%\Never-A.wvb" ^
    "%Work%\Unit-Never-Malformed" || goto :cleanup

set "FailureStep=unit-never-unit-execution"
"%Work%\Floating-Runner.exe" "%Work%\Unit-A.wvb" ^
    >"%Work%\Unit-Run.out" 2>"%Work%\Unit-Run.err" || goto :cleanup
for %%F in ("%Work%\Unit-Run.err") do if not "%%~zF"=="0" goto :cleanup
set "UnitRunLine="
set /a UnitRunLines=0
for /f "usebackq delims=" %%L in ("%Work%\Unit-Run.out") do (
    set "UnitRunLine=%%L"
    set /a UnitRunLines+=1 >nul
)
if not "%UnitRunLines%"=="1" goto :cleanup
if not "%UnitRunLine%"=="Result: 42" goto :cleanup

set "FailureStep=unit-never-never-execution"
"%Work%\Floating-Runner.exe" "%Work%\Never-A.wvb" ^
    >"%Work%\Never-Run.out" 2>"%Work%\Never-Run.err" || goto :cleanup
for %%F in ("%Work%\Never-Run.err") do if not "%%~zF"=="0" goto :cleanup
set "NeverRunLine="
set /a NeverRunLines=0
for /f "usebackq delims=" %%L in ("%Work%\Never-Run.out") do (
    set "NeverRunLine=%%L"
    set /a NeverRunLines+=1 >nul
)
if not "%NeverRunLines%"=="1" goto :cleanup
if not "%NeverRunLine%"=="Result: 42" goto :cleanup
for %%F in ("%Work%\Unit-A.wvb") do set "UnitWvbBytes=%%~zF"
for %%F in ("%Work%\Never-A.wvb") do set "NeverWvbBytes=%%~zF"
echo INFO  language 1 unit-never unit-wvb-bytes=%UnitWvbBytes% never-wvb-bytes=%NeverWvbBytes%
echo PASS  language 1 front door phase=unit-never item=8/8
set "Result=0"

:cleanup
for %%R in ("%Work%") do set "ResolvedWork=%%~fR"
echo(%ResolvedWork%| findstr /b /i /c:"%TEMP%\windvale-language-1-front-door-" >nul || exit /b 1
if not "%Result%"=="0" (
    >&2 echo FAIL  language 1 front door step=%FailureStep%
    if exist "%Work%\Segmented.err" type "%Work%\Segmented.err" >&2
    if exist "%Work%\Hosted.err" type "%Work%\Hosted.err" >&2
    if exist "%Work%\Compile-A.err" type "%Work%\Compile-A.err" >&2
    if exist "%Work%\Compile-B.err" type "%Work%\Compile-B.err" >&2
    if exist "%Work%\Unit-A.out" type "%Work%\Unit-A.out" >&2
    if exist "%Work%\Unit-A.err" type "%Work%\Unit-A.err" >&2
    if exist "%Work%\Unit-B.err" type "%Work%\Unit-B.err" >&2
    if exist "%Work%\Record-Update-A.out" type "%Work%\Record-Update-A.out" >&2
    if exist "%Work%\Record-Update-A.err" type "%Work%\Record-Update-A.err" >&2
    if exist "%Work%\Record-Update-B.err" type "%Work%\Record-Update-B.err" >&2
    if exist "%Work%\Record-Update.out" type "%Work%\Record-Update.out" >&2
    if exist "%Work%\Record-Update.err" type "%Work%\Record-Update.err" >&2
    if exist "%Work%\Rune-A.out" type "%Work%\Rune-A.out" >&2
    if exist "%Work%\Rune-A.err" type "%Work%\Rune-A.err" >&2
    if exist "%Work%\Rune-B.err" type "%Work%\Rune-B.err" >&2
    if exist "%Work%\Verify-Rune.out" type "%Work%\Verify-Rune.out" >&2
    if exist "%Work%\Verify-Rune.err" type "%Work%\Verify-Rune.err" >&2
    if exist "%Work%\Rune-Run.out" type "%Work%\Rune-Run.out" >&2
    if exist "%Work%\Rune-Run.err" type "%Work%\Rune-Run.err" >&2
    if exist "%Work%\Floating-A.out" type "%Work%\Floating-A.out" >&2
    if exist "%Work%\Floating-A.err" type "%Work%\Floating-A.err" >&2
    if exist "%Work%\Floating-B.err" type "%Work%\Floating-B.err" >&2
    if exist "%Work%\Verify-Floating.out" type "%Work%\Verify-Floating.out" >&2
    if exist "%Work%\Verify-Floating.err" type "%Work%\Verify-Floating.err" >&2
    if exist "%Work%\Floating-Run.out" type "%Work%\Floating-Run.out" >&2
    if exist "%Work%\Floating-Run.err" type "%Work%\Floating-Run.err" >&2
    if exist "%Work%\Never-A.out" type "%Work%\Never-A.out" >&2
    if exist "%Work%\Never-A.err" type "%Work%\Never-A.err" >&2
    if exist "%Work%\Never-B.err" type "%Work%\Never-B.err" >&2
    if exist "%Work%\Unit-Run.out" type "%Work%\Unit-Run.out" >&2
    if exist "%Work%\Unit-Run.err" type "%Work%\Unit-Run.err" >&2
    if exist "%Work%\Never-Run.out" type "%Work%\Never-Run.out" >&2
    if exist "%Work%\Never-Run.err" type "%Work%\Never-Run.err" >&2
    if exist "%Work%\Seed-Unit.out" type "%Work%\Seed-Unit.out" >&2
    if exist "%Work%\Seed-Unit.err" type "%Work%\Seed-Unit.err" >&2
    if exist "%Work%\Seed-Record-Update.out" type "%Work%\Seed-Record-Update.out" >&2
    if exist "%Work%\Seed-Record-Update.err" type "%Work%\Seed-Record-Update.err" >&2
    if exist "%Work%\Minimum.out" type "%Work%\Minimum.out" >&2
    if exist "%Work%\Minimum.err" type "%Work%\Minimum.err" >&2
    if exist "%Work%\Value-Front-End.out" type "%Work%\Value-Front-End.out" >&2
    if exist "%Work%\Value-Front-End.err" type "%Work%\Value-Front-End.err" >&2
)
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
if not "%Result%"=="0" exit /b %Result%
echo native language 1 front door status=Passed cases=96 frozen-inputs=250 source-fixtures=72 descriptor-cases=33 profile-cases=4 value-front-end-cases=23 compiler-cases=17 fixed-integer-cases=22 rune-cases=20 floating-cases=27 unit-never-cases=21 compiler-result=42 compiler-wvb-bytes=221 unit-wvb-bytes=%UnitWvbBytes% never-wvb-bytes=%NeverWvbBytes% record-update-wvb-bytes=1116 fixed-integer-wvb-bytes=5335 rune-wvb-bytes=%RuneWvbBytes% floating-wvb-bytes=%FloatingWvbBytes%
exit /b 0

:expect_rejection
call :expect_rejection_with_digest "%~f1" "%~f2" "%SourceLockHash%" "%SourceProfile%"
exit /b %ERRORLEVEL%

:expect_rejection_with_digest
if exist "%~f2" exit /b 1
"%Work%\Compiler.exe" --source-input-lock "%SourceLock%" "%~3" --source-profile "%~4" "%~f1" "%~f2" >"%~f2.out" 2>"%~f2.err"
if not errorlevel 1 exit /b 1
if exist "%~f2" exit /b 1
exit /b 0

:expect_runtime_failure
call "%Native%\Run-Wvb.cmd" "%~f1" >"%Work%\Runtime-%~2.out" 2>"%Work%\Runtime-%~2.err"
if not errorlevel 1 exit /b 1
findstr /b /c:"wvb run status=Failed code=%~2 " "%Work%\Runtime-%~2.err" >nul
exit /b %ERRORLEVEL%
