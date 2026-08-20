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
set "SourceLockHash=9e2ca572552ed52ed496142d18539f2f55fed2bbdfb1ec602f283b5d72386f3e"
set "BootstrapEmitterWvb=%RepositoryRoot%\Artifacts\Language-1.0-Target-Aware-Emission-Bootstrap\Wvb\wvemit.wvb"

:allocate
set "Work=%TEMP%\windvale-language-1-front-door-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Result=1"
set "FailureStep=frozen-fixtures"

echo START language 1 front door phase=frozen-fixtures item=1/11
node "%Native%\Verify-Language-1.0-Migration-Fixtures.mjs" || goto :cleanup
echo PASS  language 1 front door phase=frozen-fixtures item=1/11

set "FailureStep=descriptor"
echo START language 1 front door phase=descriptor item=2/11
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
echo PASS  language 1 front door phase=descriptor item=2/11

set "FailureStep=value-front-end"
echo START language 1 front door phase=value-front-end item=3/11
call "%Native%\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Language-1-Value-Front-End.wvproj" "%Work%\Value-Front-End.wvb" >nul || goto :cleanup
call "%Native%\Run-Wvb.cmd" "%Work%\Value-Front-End.wvb" >"%Work%\Value-Front-End.out" 2>"%Work%\Value-Front-End.err" || goto :cleanup
for %%F in ("%Work%\Value-Front-End.err") do if not "%%~zF"=="0" goto :cleanup
findstr /c:"Result: 42" "%Work%\Value-Front-End.out" >nul || goto :cleanup
call "%Native%\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Language-1-Generic-Declarations.wvproj" "%Work%\Generic-Declarations.wvb" >nul || goto :cleanup
call "%Native%\Run-Wvb.cmd" "%Work%\Generic-Declarations.wvb" >"%Work%\Generic-Declarations.out" 2>"%Work%\Generic-Declarations.err" || goto :cleanup
for %%F in ("%Work%\Generic-Declarations.err") do if not "%%~zF"=="0" goto :cleanup
findstr /c:"Result: 42" "%Work%\Generic-Declarations.out" >nul || goto :cleanup
call "%Native%\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Language-1-Generic-Calls.wvproj" "%Work%\Generic-Calls.wvb" >nul || goto :cleanup
call "%Native%\Run-Wvb.cmd" "%Work%\Generic-Calls.wvb" >"%Work%\Generic-Calls.out" 2>"%Work%\Generic-Calls.err" || goto :cleanup
for %%F in ("%Work%\Generic-Calls.err") do if not "%%~zF"=="0" goto :cleanup
findstr /c:"Result: 42" "%Work%\Generic-Calls.out" >nul || goto :cleanup
call "%Native%\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Language-1-Generic-Resolution.wvproj" "%Work%\Generic-Resolution.wvb" >nul || goto :cleanup
call "%Native%\Package-Hosted-Wvb.cmd" 1 "%Work%\Generic-Resolution.wvb" "%Work%\Generic-Resolution.exe" >"%Work%\Generic-Resolution-Package.out" 2>"%Work%\Generic-Resolution-Package.err" || goto :cleanup
for %%F in ("%Work%\Generic-Resolution-Package.err") do if not "%%~zF"=="0" goto :cleanup
"%Work%\Generic-Resolution.exe" >"%Work%\Generic-Resolution.out" 2>"%Work%\Generic-Resolution.err"
set "GenericResolutionResult=%ERRORLEVEL%"
if not "%GenericResolutionResult%"=="42" goto :cleanup
for %%F in ("%Work%\Generic-Resolution.out" "%Work%\Generic-Resolution.err") do if not "%%~zF"=="0" goto :cleanup
echo PASS  language 1 front door phase=value-front-end item=3/11

set "FailureStep=compiler-bootstrap-profile"
echo START language 1 front door phase=compiler-slice item=4/11
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 2 ^
    "%RepositoryRoot%\Artifacts\Native-Compiler-Reconstruction-Candidate\Wvb\Windvale-Compiler.wvb" ^
    "%Work%\Bootstrap-Compiler.exe" --development-cache ^
    >"%Work%\Bootstrap.out" 2>"%Work%\Bootstrap.err" || goto :cleanup
for %%F in ("%Work%\Bootstrap.err") do if not "%%~zF"=="0" goto :cleanup
type "%Work%\Bootstrap.out"
set "FailureStep=compiler-source-set"
call "%Native%\Compile-Compiler-Source-Set.cmd" ^
    "%Work%\Bootstrap-Compiler.exe" ^
    "%RepositoryRoot%" "%Work%\Compiler.wvb" ^
    >"%Work%\Segmented.out" 2>"%Work%\Segmented.err" || goto :cleanup
for %%F in ("%Work%\Segmented.err") do if not "%%~zF"=="0" goto :cleanup
type "%Work%\Segmented.out"
set "FailureStep=compiler-split-source-sets"
node "%Native%\Compile-Project-2-With-Compiler.mjs" ^
    "%Work%\Bootstrap-Compiler.exe" ^
    "%RepositoryRoot%\Projects\Tools\Windvale-Compiler-Admission-Driver.wvproj" ^
    "%Work%\Admitter.wvb" || goto :cleanup
node "%Native%\Compile-Project-2-With-Compiler.mjs" ^
    "%Work%\Bootstrap-Compiler.exe" ^
    "%RepositoryRoot%\Projects\Tools\Windvale-Compiler-Analysis-Driver.wvproj" ^
    "%Work%\Analyzer.wvb" || goto :cleanup
set "FailureStep=compiler-split-hosted-cache"
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 2 ^
    "%Work%\Admitter.wvb" "%Work%\Admitter.exe" --development-cache || goto :cleanup
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 2 ^
    "%Work%\Analyzer.wvb" "%Work%\Analyzer.exe" --development-cache || goto :cleanup
node "%Native%\Write-Split-Compiler-Producer-Identity.mjs" ^
    analyzer "%Work%\Analyzer.exe" "%Work%\Analyzer.identity" || goto :cleanup
set "FailureStep=compiler-bootstrap-emitter-identity"
for %%F in ("%BootstrapEmitterWvb%") do if not "%%~zF"=="746557" goto :cleanup
certutil -hashfile "%BootstrapEmitterWvb%" SHA256 | findstr /I /C:"a0fe54283ed51e1940bae837eb11bfb2d72f16dd91d7eb7022e51730eb0c5805" >nul || goto :cleanup
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 2 ^
    "%BootstrapEmitterWvb%" "%Work%\Bootstrap-Emitter.exe" --development-cache || goto :cleanup
node "%Native%\Write-Split-Compiler-Producer-Identity.mjs" ^
    emitter "%Work%\Bootstrap-Emitter.exe" "%Work%\Bootstrap-Emitter.identity" || goto :cleanup
set "FailureStep=compiler-target-aware-emitter"
node "%Native%\Build-Cached-Split-Project-Wvb.mjs" ^
    "%RepositoryRoot%\Projects\Tools\Windvale-Compiler-Emission-Driver.wvproj" ^
    "%Work%\Emitter.wvb" ^
    "%Work%\Analyzer.exe" "%Work%\Analyzer.identity" ^
    "%Work%\Bootstrap-Emitter.exe" "%Work%\Bootstrap-Emitter.identity" || goto :cleanup
for %%F in ("%Work%\Emitter.wvb") do if not "%%~zF"=="838654" goto :cleanup
certutil -hashfile "%Work%\Emitter.wvb" SHA256 | findstr /I /C:"707c3aec27b481745ae599206960bc6f9c0be0053aaae73b359cd20cd2cc4876" >nul || goto :cleanup
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 2 ^
    "%Work%\Emitter.wvb" "%Work%\Emitter.exe" --development-cache || goto :cleanup
set "FailureStep=compiler-minimum-a"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Minimum-Program.wv" ^
    "%Work%\Minimum-A.wvb" >"%Work%\Compile-A.out" 2>"%Work%\Compile-A.err" || goto :cleanup
set "FailureStep=compiler-minimum-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
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
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Unit-Control.wv" ^
    "%Work%\Unit-A.wvb" >"%Work%\Unit-A.out" 2>"%Work%\Unit-A.err" || goto :cleanup
set "FailureStep=compiler-unit-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
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
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Record-Update.wv" ^
    "%Work%\Record-Update-A.wvb" >"%Work%\Record-Update-A.out" 2>"%Work%\Record-Update-A.err" || goto :cleanup
set "FailureStep=compiler-record-update-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
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
set "FailureStep=compiler-value-if-a"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Value-Control.wv" ^
    "%Work%\Value-If-A.wvb" >"%Work%\Value-If-A.out" 2>"%Work%\Value-If-A.err" || goto :cleanup
set "FailureStep=compiler-value-if-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Value-Control.wv" ^
    "%Work%\Value-If-B.wvb" >"%Work%\Value-If-B.out" 2>"%Work%\Value-If-B.err" || goto :cleanup
for %%F in ("%Work%\Value-If-A.err" "%Work%\Value-If-B.err") do if not "%%~zF"=="0" goto :cleanup
fc /b "%Work%\Value-If-A.out" "%Work%\Value-If-B.out" >nul || goto :cleanup
fc /b "%Work%\Value-If-A.wvb" "%Work%\Value-If-B.wvb" >nul || goto :cleanup
set "FailureStep=compiler-value-if-execution"
call "%Native%\Run-Wvb.cmd" "%Work%\Value-If-A.wvb" >"%Work%\Value-If.out" 2>"%Work%\Value-If.err" || goto :cleanup
for %%F in ("%Work%\Value-If.err") do if not "%%~zF"=="0" goto :cleanup
call :expect_result_42 "%Work%\Value-If.out" || goto :cleanup
set "FailureStep=compiler-value-if-lazy"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Value-If-Lazy.wv" ^
    "%Work%\Value-If-Lazy.wvb" >"%Work%\Value-If-Lazy.out" 2>"%Work%\Value-If-Lazy.err" || goto :cleanup
for %%F in ("%Work%\Value-If-Lazy.err") do if not "%%~zF"=="0" goto :cleanup
call "%Native%\Run-Wvb.cmd" "%Work%\Value-If-Lazy.wvb" >"%Work%\Value-If-Lazy-Run.out" 2>"%Work%\Value-If-Lazy-Run.err" || goto :cleanup
for %%F in ("%Work%\Value-If-Lazy-Run.err") do if not "%%~zF"=="0" goto :cleanup
call :expect_result_42 "%Work%\Value-If-Lazy-Run.out" || goto :cleanup
set "FailureStep=compiler-value-if-source-rejections"
for %%N in (Missing-Else Trailing-Semicolon Type-Mismatch Invalid-Condition) do (
    call :expect_rejection "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Value-If-%%N.wv" "%Work%\Value-If-%%N.wvb" || goto :cleanup
)
set "FailureStep=compiler-negative-seed-value-if"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    "%RepositoryRoot%\Tests\Fixtures\Source-Wvb\Invalid-Value-If.wv" ^
    "%Work%\Seed-Value-If.wvb" >"%Work%\Seed-Value-If.out" 2>"%Work%\Seed-Value-If.err"
if not errorlevel 1 goto :cleanup
if exist "%Work%\Seed-Value-If.wvb" goto :cleanup
for %%F in ("%Work%\Value-If-A.wvb") do set "ValueIfWvbBytes=%%~zF"
set "FailureStep=compiler-value-match-a"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Value-Match.wv" ^
    "%Work%\Value-Match-A.wvb" >"%Work%\Value-Match-A.out" 2>"%Work%\Value-Match-A.err" || goto :cleanup
set "FailureStep=compiler-value-match-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Value-Match.wv" ^
    "%Work%\Value-Match-B.wvb" >"%Work%\Value-Match-B.out" 2>"%Work%\Value-Match-B.err" || goto :cleanup
for %%F in ("%Work%\Value-Match-A.err" "%Work%\Value-Match-B.err") do if not "%%~zF"=="0" goto :cleanup
fc /b "%Work%\Value-Match-A.out" "%Work%\Value-Match-B.out" >nul || goto :cleanup
fc /b "%Work%\Value-Match-A.wvb" "%Work%\Value-Match-B.wvb" >nul || goto :cleanup
set "FailureStep=compiler-value-match-execution"
call "%Native%\Run-Wvb.cmd" "%Work%\Value-Match-A.wvb" >"%Work%\Value-Match.out" 2>"%Work%\Value-Match.err" || goto :cleanup
for %%F in ("%Work%\Value-Match.err") do if not "%%~zF"=="0" goto :cleanup
call :expect_result_42 "%Work%\Value-Match.out" || goto :cleanup
set "FailureStep=compiler-value-match-lazy"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Value-Match-Lazy.wv" ^
    "%Work%\Value-Match-Lazy.wvb" >"%Work%\Value-Match-Lazy.out" 2>"%Work%\Value-Match-Lazy.err" || goto :cleanup
for %%F in ("%Work%\Value-Match-Lazy.err") do if not "%%~zF"=="0" goto :cleanup
call "%Native%\Run-Wvb.cmd" "%Work%\Value-Match-Lazy.wvb" >"%Work%\Value-Match-Lazy-Run.out" 2>"%Work%\Value-Match-Lazy-Run.err" || goto :cleanup
for %%F in ("%Work%\Value-Match-Lazy-Run.err") do if not "%%~zF"=="0" goto :cleanup
call :expect_result_42 "%Work%\Value-Match-Lazy-Run.out" || goto :cleanup
set "FailureStep=compiler-value-match-never-a"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Value-Match-Never.wv" ^
    "%Work%\Value-Match-Never-A.wvb" >"%Work%\Value-Match-Never-A.out" 2>"%Work%\Value-Match-Never-A.err" || goto :cleanup
set "FailureStep=compiler-value-match-never-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Value-Match-Never.wv" ^
    "%Work%\Value-Match-Never-B.wvb" >"%Work%\Value-Match-Never-B.out" 2>"%Work%\Value-Match-Never-B.err" || goto :cleanup
for %%F in ("%Work%\Value-Match-Never-A.err" "%Work%\Value-Match-Never-B.err") do if not "%%~zF"=="0" goto :cleanup
fc /b "%Work%\Value-Match-Never-A.out" "%Work%\Value-Match-Never-B.out" >nul || goto :cleanup
fc /b "%Work%\Value-Match-Never-A.wvb" "%Work%\Value-Match-Never-B.wvb" >nul || goto :cleanup
set "FailureStep=compiler-value-match-variant-a"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Value-Match-Variant.wv" ^
    "%Work%\Value-Match-Variant-A.wvb" >"%Work%\Value-Match-Variant-A.out" 2>"%Work%\Value-Match-Variant-A.err" || goto :cleanup
set "FailureStep=compiler-value-match-variant-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Value-Match-Variant.wv" ^
    "%Work%\Value-Match-Variant-B.wvb" >"%Work%\Value-Match-Variant-B.out" 2>"%Work%\Value-Match-Variant-B.err" || goto :cleanup
for %%F in ("%Work%\Value-Match-Variant-A.err" "%Work%\Value-Match-Variant-B.err") do if not "%%~zF"=="0" goto :cleanup
fc /b "%Work%\Value-Match-Variant-A.out" "%Work%\Value-Match-Variant-B.out" >nul || goto :cleanup
fc /b "%Work%\Value-Match-Variant-A.wvb" "%Work%\Value-Match-Variant-B.wvb" >nul || goto :cleanup
set "FailureStep=compiler-value-match-source-rejections"
for %%N in (Missing-Case Trailing-Semicolon Type-Mismatch) do (
    call :expect_rejection "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Value-Match-%%N.wv" "%Work%\Value-Match-%%N.wvb" || goto :cleanup
)
set "FailureStep=compiler-negative-seed-value-match"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    "%RepositoryRoot%\Tests\Fixtures\Source-Wvb\Invalid-Value-Match.wv" ^
    "%Work%\Seed-Value-Match.wvb" >"%Work%\Seed-Value-Match.out" 2>"%Work%\Seed-Value-Match.err"
if not errorlevel 1 goto :cleanup
if exist "%Work%\Seed-Value-Match.wvb" goto :cleanup
for %%F in ("%Work%\Value-Match-A.wvb") do set "ValueMatchWvbBytes=%%~zF"
for %%F in ("%Work%\Value-Match-Never-A.wvb") do set "ValueMatchNeverWvbBytes=%%~zF"
set "FailureStep=compiler-negative-unit-return-value"
call :expect_rejection "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Unit-Return-Value.wv" "%Work%\Unit-Return-Value.wvb" || goto :cleanup
set "FailureStep=compiler-negative-nonunit-return"
call :expect_rejection "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Unit-Return-From-I32.wv" "%Work%\Unit-Return-From-I32.wvb" || goto :cleanup
set "FailureStep=compiler-negative-seed-unit"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
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
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
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
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Minimum-Program.wv" "%Work%\Ambient.wvb" >"%Work%\Ambient.out" 2>"%Work%\Ambient.err"
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
echo PASS  language 1 front door phase=compiler-slice item=4/11

set "FailureStep=fixed-integer-compile-a"
echo START language 1 front door phase=fixed-integers item=5/11
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Fixed-Integer-Program.wv" ^
    "%Work%\Fixed-Integer-A.wvb" >"%Work%\Fixed-Integer-A.out" 2>"%Work%\Fixed-Integer-A.err" || goto :cleanup
set "FailureStep=fixed-integer-compile-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
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
    node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
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
echo PASS  language 1 front door phase=fixed-integers item=5/11

set "FailureStep=rune-compile-a"
echo START language 1 front door phase=runes item=6/11
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Rune-Program.wv" ^
    "%Work%\Rune-A.wvb" >"%Work%\Rune-A.out" 2>"%Work%\Rune-A.err" || goto :cleanup
set "FailureStep=rune-compile-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
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

set "FailureStep=rune-execution-run"
call "%Native%\Run-Wvb.cmd" "%Work%\Rune-A.wvb" ^
    >"%Work%\Rune-Run.out" 2>"%Work%\Rune-Run.err" || goto :cleanup
set "FailureStep=rune-execution-diagnostic"
for %%F in ("%Work%\Rune-Run.err") do if not "%%~zF"=="0" goto :cleanup
set "FailureStep=rune-execution-result"
call :expect_result_42 "%Work%\Rune-Run.out" || goto :cleanup

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
echo PASS  language 1 front door phase=runes item=6/11

set "FailureStep=floating-compile-a"
echo START language 1 front door phase=floating item=7/11
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Floating-Program.wv" ^
    "%Work%\Floating-A.wvb" >"%Work%\Floating-A.out" 2>"%Work%\Floating-A.err" || goto :cleanup
set "FailureStep=floating-compile-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
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
echo PASS  language 1 front door phase=floating item=7/11

set "FailureStep=unit-never-compile-a"
echo START language 1 front door phase=unit-never item=8/11
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Never-Control.wv" ^
    "%Work%\Never-A.wvb" >"%Work%\Never-A.out" 2>"%Work%\Never-A.err" || goto :cleanup
set "FailureStep=unit-never-compile-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
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
echo PASS  language 1 front door phase=unit-never item=8/11

set "FailureStep=multi-field-variant-compile-a"
echo START language 1 front door phase=multi-field-variants item=9/11
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Multi-Field-Variant.wv" ^
    "%Work%\Multi-Field-Variant-A.wvb" >"%Work%\Multi-Field-Variant-A.out" 2>"%Work%\Multi-Field-Variant-A.err" || goto :cleanup
set "FailureStep=multi-field-variant-compile-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Multi-Field-Variant.wv" ^
    "%Work%\Multi-Field-Variant-B.wvb" >"%Work%\Multi-Field-Variant-B.out" 2>"%Work%\Multi-Field-Variant-B.err" || goto :cleanup
set "FailureStep=multi-field-variant-determinism"
for %%F in ("%Work%\Multi-Field-Variant-A.err" "%Work%\Multi-Field-Variant-B.err") do if not "%%~zF"=="0" goto :cleanup
fc /b "%Work%\Multi-Field-Variant-A.out" "%Work%\Multi-Field-Variant-B.out" >nul || goto :cleanup
fc /b "%Work%\Multi-Field-Variant-A.wvb" "%Work%\Multi-Field-Variant-B.wvb" >nul || goto :cleanup

set "FailureStep=multi-field-variant-source-rejections"
for %%N in (Duplicate-Declaration Empty-Payload Missing-Field Duplicate-Field Unknown-Field Type-Mismatch Pattern-Missing-Field Pattern-Duplicate-Field Pattern-Unknown-Field) do (
    call :expect_rejection "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Multi-Field-Variant-%%N.wv" "%Work%\Multi-Field-Variant-%%N.wvb" || goto :cleanup
)

set "FailureStep=named-single-field-variant"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Named-Variant-Field.wv" ^
    "%Work%\Named-Variant-Field.wvb" >"%Work%\Named-Variant-Field.out" 2>"%Work%\Named-Variant-Field.err" || goto :cleanup
for %%F in ("%Work%\Named-Variant-Field.err") do if not "%%~zF"=="0" goto :cleanup

set "FailureStep=multi-field-variant-verifier"
node "%Native%\Verify-Language-1.0-Multi-Field-Variants.mjs" ^
    "%Work%\Verifier.exe" "%Work%\Multi-Field-Variant-A.wvb" "%Work%\Named-Variant-Field.wvb" ^
    "%Work%\Multi-Field-Variant-Malformed" || goto :cleanup
set "FailureStep=multi-field-variant-runtime-execution"
"%Work%\Floating-Runner.exe" "%Work%\Multi-Field-Variant-A.wvb" ^
    >"%Work%\Multi-Field-Variant-Run.out" 2>"%Work%\Multi-Field-Variant-Run.err" || goto :cleanup
set "FailureStep=multi-field-variant-runtime-diagnostic"
for %%F in ("%Work%\Multi-Field-Variant-Run.err") do if not "%%~zF"=="0" goto :cleanup
set "FailureStep=multi-field-variant-runtime-result"
call :expect_result_42 "%Work%\Multi-Field-Variant-Run.out" || goto :cleanup
set "FailureStep=value-match-variant-runtime"
"%Work%\Floating-Runner.exe" "%Work%\Value-Match-Variant-A.wvb" ^
    >"%Work%\Value-Match-Variant-Run.out" 2>"%Work%\Value-Match-Variant-Run.err" || goto :cleanup
for %%F in ("%Work%\Value-Match-Variant-Run.err") do if not "%%~zF"=="0" goto :cleanup
call :expect_result_42 "%Work%\Value-Match-Variant-Run.out" || goto :cleanup
set "FailureStep=value-match-never-runtime"
"%Work%\Floating-Runner.exe" "%Work%\Value-Match-Never-A.wvb" ^
    >"%Work%\Value-Match-Never-Run.out" 2>"%Work%\Value-Match-Never-Run.err" || goto :cleanup
for %%F in ("%Work%\Value-Match-Never-Run.err") do if not "%%~zF"=="0" goto :cleanup
call :expect_result_42 "%Work%\Value-Match-Never-Run.out" || goto :cleanup
set "FailureStep=named-single-field-variant-runtime"
"%Work%\Floating-Runner.exe" "%Work%\Named-Variant-Field.wvb" ^
    >"%Work%\Named-Variant-Field-Run.out" 2>"%Work%\Named-Variant-Field-Run.err" || goto :cleanup
for %%F in ("%Work%\Named-Variant-Field-Run.err") do if not "%%~zF"=="0" goto :cleanup
call :expect_result_42 "%Work%\Named-Variant-Field-Run.out" || goto :cleanup
set "FailureStep=multi-field-variant-runtime-mismatch"
"%Work%\Floating-Runner.exe" ^
    "%Work%\Multi-Field-Variant-Malformed\runtime-case-mismatch.wvb" ^
    >"%Work%\Multi-Field-Variant-Mismatch.out" ^
    2>"%Work%\Multi-Field-Variant-Mismatch.err"
if not errorlevel 1 goto :cleanup
findstr /b /c:"wvb run status=Failed code=3017 " ^
    "%Work%\Multi-Field-Variant-Mismatch.err" >nul || goto :cleanup
set "FailureStep=variant-runtime-pressure-build"
call "%Native%\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Wvb-Variant-Runtime-Pressure.wvproj" ^
    "%Work%\Variant-Runtime-Pressure.wvb" >nul || goto :cleanup
set "FailureStep=variant-runtime-pressure-execution"
"%Work%\Floating-Runner.exe" "%Work%\Variant-Runtime-Pressure.wvb" ^
    >"%Work%\Variant-Runtime-Pressure.out" ^
    2>"%Work%\Variant-Runtime-Pressure.err" || goto :cleanup
for %%F in ("%Work%\Variant-Runtime-Pressure.err") do if not "%%~zF"=="0" goto :cleanup
call :expect_result_42 "%Work%\Variant-Runtime-Pressure.out" || goto :cleanup
for %%F in ("%Work%\Multi-Field-Variant-A.wvb") do set "MultiFieldVariantWvbBytes=%%~zF"
echo INFO  language 1 multi-field-variants wvb-bytes=%MultiFieldVariantWvbBytes%
echo PASS  language 1 front door phase=multi-field-variants item=9/11

set "FailureStep=typed-failure-compile-a"
echo START language 1 front door phase=typed-failure item=10/11
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Result-Try.wv" ^
    "%Work%\Result-Try-A.wvb" >"%Work%\Result-Try-A.out" 2>"%Work%\Result-Try-A.err" || goto :cleanup
set "FailureStep=typed-failure-compile-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Result-Try.wv" ^
    "%Work%\Result-Try-B.wvb" >"%Work%\Result-Try-B.out" 2>"%Work%\Result-Try-B.err" || goto :cleanup
set "FailureStep=typed-failure-determinism"
for %%F in ("%Work%\Result-Try-A.err" "%Work%\Result-Try-B.err") do if not "%%~zF"=="0" goto :cleanup
fc /b "%Work%\Result-Try-A.out" "%Work%\Result-Try-B.out" >nul || goto :cleanup
fc /b "%Work%\Result-Try-A.wvb" "%Work%\Result-Try-B.wvb" >nul || goto :cleanup

set "FailureStep=typed-failure-source-rejections"
for %%N in (Lookalike Wrong-Value-Field Extra-Case Scalar) do (
    call :expect_rejection "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Result-Try-%%N.wv" "%Work%\Result-Try-%%N.wvb" || goto :cleanup
)

set "FailureStep=typed-failure-verifier"
"%Work%\Verifier.exe" "%Work%\Result-Try-A.wvb" ^
    >"%Work%\Verify-Result-Try.out" 2>"%Work%\Verify-Result-Try.err" || goto :cleanup
findstr /c:"wvb status=Valid profile=compiler-aligned" "%Work%\Verify-Result-Try.out" >nul || goto :cleanup

set "FailureStep=typed-failure-runtime"
"%Work%\Floating-Runner.exe" "%Work%\Result-Try-A.wvb" ^
    >"%Work%\Result-Try-Run.out" 2>"%Work%\Result-Try-Run.err" || goto :cleanup
for %%F in ("%Work%\Result-Try-Run.err") do if not "%%~zF"=="0" goto :cleanup
call :expect_result_42 "%Work%\Result-Try-Run.out" || goto :cleanup
for %%F in ("%Work%\Result-Try-A.wvb") do set "ResultTryWvbBytes=%%~zF"
echo INFO  language 1 typed-failure wvb-bytes=%ResultTryWvbBytes%
echo PASS  language 1 front door phase=typed-failure item=10/11

set "FailureStep=foundation-generics-compile-a"
echo START language 1 front door phase=foundation-generics item=11/11
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Foundation-Generic-Result.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Values\Option.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Values\Result.wv" ^
    "%Work%\Foundation-Generic-A.wvb" ^
    >"%Work%\Foundation-Generic-A.out" 2>"%Work%\Foundation-Generic-A.err" || goto :cleanup
set "FailureStep=foundation-generics-compile-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Foundation-Generic-Result.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Values\Option.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Values\Result.wv" ^
    "%Work%\Foundation-Generic-B.wvb" ^
    >"%Work%\Foundation-Generic-B.out" 2>"%Work%\Foundation-Generic-B.err" || goto :cleanup
set "FailureStep=foundation-generics-determinism"
for %%F in ("%Work%\Foundation-Generic-A.err" "%Work%\Foundation-Generic-B.err") do if not "%%~zF"=="0" goto :cleanup
fc /b "%Work%\Foundation-Generic-A.out" "%Work%\Foundation-Generic-B.out" >nul || goto :cleanup
fc /b "%Work%\Foundation-Generic-A.wvb" "%Work%\Foundation-Generic-B.wvb" >nul || goto :cleanup

set "FailureStep=foundation-generics-source-rejections"
for %%N in (Result-Wrong-Arity Result-Extra-Argument Result-Bare Try-Wrong-Error) do (
    call :expect_foundation_generic_rejection ^
        "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Foundation-Generic-%%N.wv" ^
        "%Work%\Foundation-Generic-%%N.wvb" || goto :cleanup
)

set "FailureStep=foundation-generics-verifier"
"%Work%\Verifier.exe" "%Work%\Foundation-Generic-A.wvb" ^
    >"%Work%\Verify-Foundation-Generic.out" ^
    2>"%Work%\Verify-Foundation-Generic.err" || goto :cleanup
findstr /c:"wvb status=Valid profile=compiler-aligned" ^
    "%Work%\Verify-Foundation-Generic.out" >nul || goto :cleanup

set "FailureStep=foundation-generics-runtime"
"%Work%\Floating-Runner.exe" "%Work%\Foundation-Generic-A.wvb" ^
    >"%Work%\Foundation-Generic-Run.out" ^
    2>"%Work%\Foundation-Generic-Run.err" || goto :cleanup
for %%F in ("%Work%\Foundation-Generic-Run.err") do if not "%%~zF"=="0" goto :cleanup
call :expect_result_42 "%Work%\Foundation-Generic-Run.out" || goto :cleanup
for %%F in ("%Work%\Foundation-Generic-A.wvb") do set "FoundationGenericWvbBytes=%%~zF"
echo INFO  language 1 foundation-generics wvb-bytes=%FoundationGenericWvbBytes%

set "FailureStep=generic-specializations-compile-a"
echo START language 1 front door step=generic-specializations
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Generic-Multiple-Specializations.wv" ^
    "%Work%\Generic-Specializations-A.wvb" ^
    >"%Work%\Generic-Specializations-A.out" 2>"%Work%\Generic-Specializations-A.err" || goto :cleanup
set "FailureStep=generic-specializations-compile-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Generic-Multiple-Specializations.wv" ^
    "%Work%\Generic-Specializations-B.wvb" ^
    >"%Work%\Generic-Specializations-B.out" 2>"%Work%\Generic-Specializations-B.err" || goto :cleanup
set "FailureStep=generic-specializations-determinism"
for %%F in ("%Work%\Generic-Specializations-A.err" "%Work%\Generic-Specializations-B.err") do if not "%%~zF"=="0" goto :cleanup
fc /b "%Work%\Generic-Specializations-A.out" "%Work%\Generic-Specializations-B.out" >nul || goto :cleanup
fc /b "%Work%\Generic-Specializations-A.wvb" "%Work%\Generic-Specializations-B.wvb" >nul || goto :cleanup
set "FailureStep=generic-specializations-verifier"
"%Work%\Verifier.exe" "%Work%\Generic-Specializations-A.wvb" ^
    >"%Work%\Verify-Generic-Specializations.out" ^
    2>"%Work%\Verify-Generic-Specializations.err" || goto :cleanup
findstr /c:"wvb status=Valid profile=compiler-aligned" ^
    "%Work%\Verify-Generic-Specializations.out" >nul || goto :cleanup
set "FailureStep=generic-specializations-runtime"
"%Work%\Floating-Runner.exe" "%Work%\Generic-Specializations-A.wvb" ^
    >"%Work%\Generic-Specializations-Run.out" ^
    2>"%Work%\Generic-Specializations-Run.err" || goto :cleanup
for %%F in ("%Work%\Generic-Specializations-Run.err") do if not "%%~zF"=="0" goto :cleanup
call :expect_result_42 "%Work%\Generic-Specializations-Run.out" || goto :cleanup
for %%F in ("%Work%\Generic-Specializations-A.wvb") do set "GenericSpecializationsWvbBytes=%%~zF"
echo PASS  language 1 front door step=generic-specializations wvb-bytes=%GenericSpecializationsWvbBytes%
echo PASS  language 1 front door phase=foundation-generics item=11/11
set "Result=0"

:cleanup
for %%R in ("%Work%") do set "ResolvedWork=%%~fR"
echo(%ResolvedWork%| findstr /b /i /c:"%TEMP%\windvale-language-1-front-door-" >nul || exit /b 1
if not "%Result%"=="0" (
    >&2 echo FAIL  language 1 front door step=%FailureStep%
    if exist "%Work%\Bootstrap.err" type "%Work%\Bootstrap.err" >&2
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
    if exist "%Work%\Multi-Field-Variant-A.out" type "%Work%\Multi-Field-Variant-A.out" >&2
    if exist "%Work%\Multi-Field-Variant-A.err" type "%Work%\Multi-Field-Variant-A.err" >&2
    if exist "%Work%\Multi-Field-Variant-B.err" type "%Work%\Multi-Field-Variant-B.err" >&2
    if exist "%Work%\Named-Variant-Field.out" type "%Work%\Named-Variant-Field.out" >&2
    if exist "%Work%\Named-Variant-Field.err" type "%Work%\Named-Variant-Field.err" >&2
    if exist "%Work%\Multi-Field-Variant-Run.out" type "%Work%\Multi-Field-Variant-Run.out" >&2
    if exist "%Work%\Multi-Field-Variant-Run.err" type "%Work%\Multi-Field-Variant-Run.err" >&2
    if exist "%Work%\Named-Variant-Field-Run.out" type "%Work%\Named-Variant-Field-Run.out" >&2
    if exist "%Work%\Named-Variant-Field-Run.err" type "%Work%\Named-Variant-Field-Run.err" >&2
    if exist "%Work%\Multi-Field-Variant-Mismatch.out" type "%Work%\Multi-Field-Variant-Mismatch.out" >&2
    if exist "%Work%\Multi-Field-Variant-Mismatch.err" type "%Work%\Multi-Field-Variant-Mismatch.err" >&2
    if exist "%Work%\Variant-Runtime-Pressure.out" type "%Work%\Variant-Runtime-Pressure.out" >&2
    if exist "%Work%\Variant-Runtime-Pressure.err" type "%Work%\Variant-Runtime-Pressure.err" >&2
    if exist "%Work%\Result-Try-A.out" type "%Work%\Result-Try-A.out" >&2
    if exist "%Work%\Result-Try-A.err" type "%Work%\Result-Try-A.err" >&2
    if exist "%Work%\Result-Try-B.err" type "%Work%\Result-Try-B.err" >&2
    if exist "%Work%\Verify-Result-Try.out" type "%Work%\Verify-Result-Try.out" >&2
    if exist "%Work%\Verify-Result-Try.err" type "%Work%\Verify-Result-Try.err" >&2
    if exist "%Work%\Result-Try-Run.out" type "%Work%\Result-Try-Run.out" >&2
    if exist "%Work%\Result-Try-Run.err" type "%Work%\Result-Try-Run.err" >&2
    if exist "%Work%\Foundation-Generic-A.out" type "%Work%\Foundation-Generic-A.out" >&2
    if exist "%Work%\Foundation-Generic-A.err" type "%Work%\Foundation-Generic-A.err" >&2
    if exist "%Work%\Foundation-Generic-B.err" type "%Work%\Foundation-Generic-B.err" >&2
    if exist "%Work%\Verify-Foundation-Generic.out" type "%Work%\Verify-Foundation-Generic.out" >&2
    if exist "%Work%\Verify-Foundation-Generic.err" type "%Work%\Verify-Foundation-Generic.err" >&2
    if exist "%Work%\Foundation-Generic-Run.out" type "%Work%\Foundation-Generic-Run.out" >&2
    if exist "%Work%\Foundation-Generic-Run.err" type "%Work%\Foundation-Generic-Run.err" >&2
    if exist "%Work%\Generic-Specializations-A.out" type "%Work%\Generic-Specializations-A.out" >&2
    if exist "%Work%\Generic-Specializations-A.err" type "%Work%\Generic-Specializations-A.err" >&2
    if exist "%Work%\Generic-Specializations-B.err" type "%Work%\Generic-Specializations-B.err" >&2
    if exist "%Work%\Verify-Generic-Specializations.out" type "%Work%\Verify-Generic-Specializations.out" >&2
    if exist "%Work%\Verify-Generic-Specializations.err" type "%Work%\Verify-Generic-Specializations.err" >&2
    if exist "%Work%\Generic-Specializations-Run.out" type "%Work%\Generic-Specializations-Run.out" >&2
    if exist "%Work%\Generic-Specializations-Run.err" type "%Work%\Generic-Specializations-Run.err" >&2
    if exist "%Work%\Seed-Unit.out" type "%Work%\Seed-Unit.out" >&2
    if exist "%Work%\Seed-Unit.err" type "%Work%\Seed-Unit.err" >&2
    if exist "%Work%\Seed-Record-Update.out" type "%Work%\Seed-Record-Update.out" >&2
    if exist "%Work%\Seed-Record-Update.err" type "%Work%\Seed-Record-Update.err" >&2
    if exist "%Work%\Minimum.out" type "%Work%\Minimum.out" >&2
    if exist "%Work%\Minimum.err" type "%Work%\Minimum.err" >&2
    if exist "%Work%\Value-Front-End.out" type "%Work%\Value-Front-End.out" >&2
    if exist "%Work%\Value-Front-End.err" type "%Work%\Value-Front-End.err" >&2
    if exist "%Work%\Generic-Declarations.out" type "%Work%\Generic-Declarations.out" >&2
    if exist "%Work%\Generic-Declarations.err" type "%Work%\Generic-Declarations.err" >&2
    if exist "%Work%\Generic-Calls.out" type "%Work%\Generic-Calls.out" >&2
    if exist "%Work%\Generic-Calls.err" type "%Work%\Generic-Calls.err" >&2
    if exist "%Work%\Generic-Resolution-Package.out" type "%Work%\Generic-Resolution-Package.out" >&2
    if exist "%Work%\Generic-Resolution-Package.err" type "%Work%\Generic-Resolution-Package.err" >&2
    if exist "%Work%\Generic-Resolution.out" type "%Work%\Generic-Resolution.out" >&2
    if exist "%Work%\Generic-Resolution.err" type "%Work%\Generic-Resolution.err" >&2
    if exist "%Work%\Value-Match-A.err" type "%Work%\Value-Match-A.err" >&2
    if exist "%Work%\Value-Match-Lazy.err" type "%Work%\Value-Match-Lazy.err" >&2
    if exist "%Work%\Value-Match-Lazy-Run.err" type "%Work%\Value-Match-Lazy-Run.err" >&2
    if exist "%Work%\Value-Match-Never-A.err" type "%Work%\Value-Match-Never-A.err" >&2
    if exist "%Work%\Value-Match-Never-Run.err" type "%Work%\Value-Match-Never-Run.err" >&2
    if exist "%Work%\Value-Match-Variant-A.err" type "%Work%\Value-Match-Variant-A.err" >&2
    if exist "%Work%\Value-Match-Variant-Run.err" type "%Work%\Value-Match-Variant-Run.err" >&2
)
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
if not "%Result%"=="0" exit /b %Result%
echo native language 1 front door status=Passed cases=155 frozen-inputs=251 source-fixtures=72 descriptor-cases=33 profile-cases=4 value-front-end-cases=39 generic-front-end-cases=4 generic-resolution-cases=1 generic-specialization-cases=4 compiler-cases=32 fixed-integer-cases=22 rune-cases=20 floating-cases=27 unit-never-cases=21 multi-field-variant-cases=25 typed-failure-cases=5 foundation-generic-cases=5 compiler-result=42 compiler-wvb-bytes=221 value-if-wvb-bytes=%ValueIfWvbBytes% value-match-wvb-bytes=%ValueMatchWvbBytes% value-match-never-wvb-bytes=%ValueMatchNeverWvbBytes% unit-wvb-bytes=%UnitWvbBytes% never-wvb-bytes=%NeverWvbBytes% record-update-wvb-bytes=1116 fixed-integer-wvb-bytes=5335 rune-wvb-bytes=%RuneWvbBytes% floating-wvb-bytes=%FloatingWvbBytes% multi-field-variant-wvb-bytes=%MultiFieldVariantWvbBytes% typed-failure-wvb-bytes=%ResultTryWvbBytes% foundation-generic-wvb-bytes=%FoundationGenericWvbBytes% generic-specializations-wvb-bytes=%GenericSpecializationsWvbBytes%
exit /b 0

:expect_result_42
setlocal EnableDelayedExpansion
set "ResultLine="
set /a ResultLines=0
for /f "usebackq delims=" %%L in ("%~f1") do (
    set "ResultLine=%%L"
    set /a ResultLines+=1 >nul
)
if not "!ResultLines!"=="1" exit /b 1
if not "!ResultLine!"=="Result: 42" exit /b 1
exit /b 0

:expect_rejection
call :expect_rejection_with_digest "%~f1" "%~f2" "%SourceLockHash%" "%SourceProfile%"
exit /b %ERRORLEVEL%

:expect_foundation_generic_rejection
if exist "%~f2" exit /b 1
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    "%~f1" "%RepositoryRoot%\Libraries\Foundation\Values\Result.wv" ^
    "%~f2" >"%~f2.out" 2>"%~f2.err"
if not errorlevel 1 exit /b 1
if exist "%~f2" exit /b 1
exit /b 0

:expect_rejection_with_digest
if exist "%~f2" exit /b 1
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" --source-input-lock "%SourceLock%" "%~3" --source-profile "%~4" "%~f1" "%~f2" >"%~f2.out" 2>"%~f2.err"
if not errorlevel 1 exit /b 1
if exist "%~f2" exit /b 1
exit /b 0

:expect_runtime_failure
call "%Native%\Run-Wvb.cmd" "%~f1" >"%Work%\Runtime-%~2.out" 2>"%Work%\Runtime-%~2.err"
if not errorlevel 1 exit /b 1
findstr /b /c:"wvb run status=Failed code=%~2 " "%Work%\Runtime-%~2.err" >nul
exit /b %ERRORLEVEL%
