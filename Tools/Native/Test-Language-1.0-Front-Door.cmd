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

echo START language 1 front door phase=frozen-fixtures item=1/4
node "%Native%\Verify-Language-1.0-Migration-Fixtures.mjs" || goto :cleanup
echo PASS  language 1 front door phase=frozen-fixtures item=1/4

set "FailureStep=descriptor"
echo START language 1 front door phase=descriptor item=2/4
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
echo PASS  language 1 front door phase=descriptor item=2/4

set "FailureStep=value-front-end"
echo START language 1 front door phase=value-front-end item=3/4
call "%Native%\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Language-1-Value-Front-End.wvproj" "%Work%\Value-Front-End.wvb" >nul || goto :cleanup
call "%Native%\Run-Wvb.cmd" "%Work%\Value-Front-End.wvb" >"%Work%\Value-Front-End.out" 2>"%Work%\Value-Front-End.err" || goto :cleanup
for %%F in ("%Work%\Value-Front-End.err") do if not "%%~zF"=="0" goto :cleanup
findstr /c:"Result: 42" "%Work%\Value-Front-End.out" >nul || goto :cleanup
echo PASS  language 1 front door phase=value-front-end item=3/4

set "FailureStep=compiler-segmented-cache"
echo START language 1 front door phase=compiler-slice item=4/4
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
echo PASS  language 1 front door phase=compiler-slice item=4/4
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
    if exist "%Work%\Seed-Unit.out" type "%Work%\Seed-Unit.out" >&2
    if exist "%Work%\Seed-Unit.err" type "%Work%\Seed-Unit.err" >&2
    if exist "%Work%\Minimum.out" type "%Work%\Minimum.out" >&2
    if exist "%Work%\Minimum.err" type "%Work%\Minimum.err" >&2
    if exist "%Work%\Value-Front-End.out" type "%Work%\Value-Front-End.out" >&2
    if exist "%Work%\Value-Front-End.err" type "%Work%\Value-Front-End.err" >&2
)
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
if not "%Result%"=="0" exit /b %Result%
echo native language 1 front door status=Passed cases=17 frozen-inputs=250 source-fixtures=72 descriptor-cases=33 profile-cases=4 value-front-end-cases=23 compiler-cases=12 compiler-result=42 compiler-wvb-bytes=221 unit-wvb-bytes=356
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
