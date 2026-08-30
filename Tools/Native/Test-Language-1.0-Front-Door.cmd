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
set "BootstrapAnalyzerWvb=%RepositoryRoot%\Artifacts\Language-1.0-Target-Aware-Emission-Bootstrap\Wvb\wvanalyze.wvb"
set "BootstrapEmitterWvb=%RepositoryRoot%\Artifacts\Language-1.0-Target-Aware-Emission-Bootstrap\Wvb\wvemit.wvb"
set "BridgeEmitterWvb=%RepositoryRoot%\Artifacts\Language-1.0-Target-Aware-Emission-Bootstrap\Wvb\wvemit-wvir-1.9-bridge.wvb"
set "WINDVALE_SPLIT_COMPILER_ACTIVITY=0"
for /f "usebackq delims=" %%T in (`node -p "require('node:fs').realpathSync.native(process.argv[1])" "%TEMP%"`) do set "TemporaryRoot=%%T"
if not defined TemporaryRoot exit /b 1

:allocate
set "Work=%TemporaryRoot%\windvale-language-1-front-door-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "CanonicalWork="
for /f "usebackq delims=" %%W in (`node -p "require('node:fs').realpathSync.native(process.argv[1])" "%Work%"`) do set "CanonicalWork=%%W"
if not defined CanonicalWork (
    rmdir "%Work%" >nul 2>&1
    exit /b 1
)
set "Work=%CanonicalWork%"
set "CanonicalWork="
for /f "usebackq delims=" %%T in (`node -p "require('node:path').dirname(process.argv[1])" "%Work%"`) do set "TemporaryRoot=%%T"
if not defined TemporaryRoot exit /b 1
set "Result=1"
set "TargetDescriptor=%Work%\Target.wvtd"
node "%Native%\Write-Canonical-Language-1.0-Target-Descriptor.mjs" ^
    "%TargetDescriptor%" || goto :cleanup
set "FailureStep=frozen-fixtures"

echo START language 1 front door phase=frozen-fixtures item=1/13
node "%Native%\Verify-Language-1.0-Migration-Fixtures.mjs" || goto :cleanup
echo PASS  language 1 front door phase=frozen-fixtures item=1/13

set "FailureStep=descriptor"
echo START language 1 front door phase=descriptor item=2/13
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
echo PASS  language 1 front door phase=descriptor item=2/13

set "FailureStep=value-front-end"
echo START language 1 front door phase=value-front-end item=3/13
call "%Native%\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Language-1-Value-Front-End.wvproj" "%Work%\Value-Front-End.wvb" >nul || goto :cleanup
call "%Native%\Run-Wvb.cmd" "%Work%\Value-Front-End.wvb" >"%Work%\Value-Front-End.out" 2>"%Work%\Value-Front-End.err" || goto :cleanup
for %%F in ("%Work%\Value-Front-End.err") do if not "%%~zF"=="0" goto :cleanup
findstr /c:"Result: 42" "%Work%\Value-Front-End.out" >nul || goto :cleanup
call "%Native%\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Language-1-Generic-Declarations.wvproj" "%Work%\Generic-Declarations.wvb" >nul || goto :cleanup
call "%Native%\Run-Wvb.cmd" "%Work%\Generic-Declarations.wvb" >"%Work%\Generic-Declarations.out" 2>"%Work%\Generic-Declarations.err" || goto :cleanup
for %%F in ("%Work%\Generic-Declarations.err") do if not "%%~zF"=="0" goto :cleanup
findstr /c:"Result: 42" "%Work%\Generic-Declarations.out" >nul || goto :cleanup
call "%Native%\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Language-1-Generic-Calls.wvproj" "%Work%\Generic-Calls.wvb" >nul || goto :cleanup
set "FailureStep=value-front-end-generic-calls-native"
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 1 ^
    "%Work%\Generic-Calls.wvb" "%Work%\Generic-Calls.exe" ^
    --development-cache ^
    >"%Work%\Generic-Calls-Package.out" ^
    2>"%Work%\Generic-Calls-Package.err" || goto :cleanup
for %%F in ("%Work%\Generic-Calls-Package.err") do if not "%%~zF"=="0" goto :cleanup
"%Work%\Generic-Calls.exe" >"%Work%\Generic-Calls.out" 2>"%Work%\Generic-Calls.err"
set "GenericCallsResult=%ERRORLEVEL%"
if not "%GenericCallsResult%"=="42" goto :cleanup
for %%F in ("%Work%\Generic-Calls.out" "%Work%\Generic-Calls.err") do if not "%%~zF"=="0" goto :cleanup
set "FailureStep=value-front-end"
call "%Native%\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Language-1-Generic-Resolution.wvproj" "%Work%\Generic-Resolution.wvb" >nul || goto :cleanup
call "%Native%\Package-Hosted-Wvb.cmd" 1 "%Work%\Generic-Resolution.wvb" "%Work%\Generic-Resolution.exe" >"%Work%\Generic-Resolution-Package.out" 2>"%Work%\Generic-Resolution-Package.err" || goto :cleanup
for %%F in ("%Work%\Generic-Resolution-Package.err") do if not "%%~zF"=="0" goto :cleanup
"%Work%\Generic-Resolution.exe" >"%Work%\Generic-Resolution.out" 2>"%Work%\Generic-Resolution.err"
set "GenericResolutionResult=%ERRORLEVEL%"
if not "%GenericResolutionResult%"=="42" goto :cleanup
for %%F in ("%Work%\Generic-Resolution.out" "%Work%\Generic-Resolution.err") do if not "%%~zF"=="0" goto :cleanup
echo START language 1 front door step=generic-type-catalog
call "%Native%\Build-Wvb.cmd" "%RepositoryRoot%\Projects\Tests\Windvale-Native-Test-Language-1-Generic-Type-Catalog.wvproj" "%Work%\Generic-Type-Catalog.wvb" >nul || goto :cleanup
call "%Native%\Package-Hosted-Wvb.cmd" 1 "%Work%\Generic-Type-Catalog.wvb" "%Work%\Generic-Type-Catalog.exe" >"%Work%\Generic-Type-Catalog-Package.out" 2>"%Work%\Generic-Type-Catalog-Package.err" || goto :cleanup
for %%F in ("%Work%\Generic-Type-Catalog-Package.err") do if not "%%~zF"=="0" goto :cleanup
"%Work%\Generic-Type-Catalog.exe" >"%Work%\Generic-Type-Catalog.out" 2>"%Work%\Generic-Type-Catalog.err"
set "GenericTypeCatalogResult=%ERRORLEVEL%"
if not "%GenericTypeCatalogResult%"=="42" goto :cleanup
for %%F in ("%Work%\Generic-Type-Catalog.out" "%Work%\Generic-Type-Catalog.err") do if not "%%~zF"=="0" goto :cleanup
for %%F in ("%Work%\Generic-Type-Catalog.wvb") do set "GenericTypeCatalogWvbBytes=%%~zF"
echo PASS  language 1 front door step=generic-type-catalog wvb-bytes=%GenericTypeCatalogWvbBytes%
echo PASS  language 1 front door phase=value-front-end item=3/13

set "FailureStep=compiler-split-bootstrap-identities"
echo START language 1 front door phase=compiler-slice item=4/13
for %%F in ("%BootstrapAnalyzerWvb%") do if not "%%~zF"=="992412" goto :cleanup
certutil -hashfile "%BootstrapAnalyzerWvb%" SHA256 | findstr /I /C:"26ea9bccfe8c2763fb887a5a14c2f0a086a27265523c3df84187b361616f9120" >nul || goto :cleanup
for %%F in ("%BootstrapEmitterWvb%") do if not "%%~zF"=="895787" goto :cleanup
certutil -hashfile "%BootstrapEmitterWvb%" SHA256 | findstr /I /C:"ea8ade4774236a84208242a6e17d271077b9a4a94fb40c47ec487d43a97b2b94" >nul || goto :cleanup
for %%F in ("%BridgeEmitterWvb%") do if not "%%~zF"=="1146083" goto :cleanup
certutil -hashfile "%BridgeEmitterWvb%" SHA256 | findstr /I /C:"0d838b6d983320cf22b9094ef5a4692d6833f1834292863789577e034f6febdb" >nul || goto :cleanup
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 7 ^
    "%BootstrapAnalyzerWvb%" "%Work%\Bootstrap-Analyzer.exe" --development-cache || goto :cleanup
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 7 ^
    "%BootstrapEmitterWvb%" "%Work%\Bootstrap-Emitter.exe" --development-cache || goto :cleanup
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 7 ^
    "%BridgeEmitterWvb%" "%Work%\Bridge-Emitter.exe" --development-cache || goto :cleanup
node "%Native%\Write-Split-Compiler-Producer-Identity.mjs" ^
    analyzer "%Work%\Bootstrap-Analyzer.exe" "%Work%\Bootstrap-Analyzer.identity" || goto :cleanup
node "%Native%\Write-Split-Compiler-Producer-Identity.mjs" ^
    emitter "%Work%\Bootstrap-Emitter.exe" "%Work%\Bootstrap-Emitter.identity" || goto :cleanup
node "%Native%\Write-Split-Compiler-Producer-Identity.mjs" ^
    emitter "%Work%\Bridge-Emitter.exe" "%Work%\Bridge-Emitter.identity" || goto :cleanup
set "FailureStep=compiler-split-source-sets"
node "%Native%\Build-Cached-Split-Project-Wvb.mjs" ^
    "%RepositoryRoot%\Projects\Tools\Windvale-Compiler-Admission-Driver.wvproj" ^
    "%Work%\Admitter.wvb" ^
    "%Work%\Bootstrap-Analyzer.exe" "%Work%\Bootstrap-Analyzer.identity" ^
    "%Work%\Bootstrap-Emitter.exe" "%Work%\Bootstrap-Emitter.identity" || goto :cleanup
node "%Native%\Build-Cached-Split-Project-Wvb.mjs" ^
    "%RepositoryRoot%\Projects\Tools\Windvale-Compiler-Source-Authenticator.wvproj" ^
    "%Work%\Validator.wvb" ^
    "%Work%\Bootstrap-Analyzer.exe" "%Work%\Bootstrap-Analyzer.identity" ^
    "%Work%\Bootstrap-Emitter.exe" "%Work%\Bootstrap-Emitter.identity" || goto :cleanup
node "%Native%\Build-Cached-Split-Project-Wvb.mjs" ^
    "%RepositoryRoot%\Projects\Tools\Windvale-Compiler-Analysis-Driver.wvproj" ^
    "%Work%\Analyzer.wvb" ^
    "%Work%\Bootstrap-Analyzer.exe" "%Work%\Bootstrap-Analyzer.identity" ^
    "%Work%\Bootstrap-Emitter.exe" "%Work%\Bootstrap-Emitter.identity" || goto :cleanup
for %%F in ("%Work%\Admitter.wvb") do echo INFO  language 1 admitter wvb-bytes=%%~zF
certutil -hashfile "%Work%\Admitter.wvb" SHA256
for %%F in ("%Work%\Admitter.wvb") do if not "%%~zF"=="572926" goto :cleanup
certutil -hashfile "%Work%\Admitter.wvb" SHA256 | findstr /I /C:"a9c2e966b84420aaa64de89a232246a15b8fb859ba5ef737e853d2482d5f5831" >nul || goto :cleanup
for %%F in ("%Work%\Validator.wvb") do echo INFO  language 1 validator wvb-bytes=%%~zF
certutil -hashfile "%Work%\Validator.wvb" SHA256
for %%F in ("%Work%\Validator.wvb") do if not "%%~zF"=="91774" goto :cleanup
certutil -hashfile "%Work%\Validator.wvb" SHA256 | findstr /I /C:"88eec2e572e03cdd87de3bedc01c555da3a246fd2d160a62246da0d39331f580" >nul || goto :cleanup
for %%F in ("%Work%\Analyzer.wvb") do echo INFO  language 1 analyzer wvb-bytes=%%~zF
certutil -hashfile "%Work%\Analyzer.wvb" SHA256
for %%F in ("%Work%\Analyzer.wvb") do if not "%%~zF"=="1552090" goto :cleanup
certutil -hashfile "%Work%\Analyzer.wvb" SHA256 | findstr /I /C:"5baba39b96932eca26d694b537d380f9ee6dcd4683afc81c09a99ab3c3cb9c77" >nul || goto :cleanup
set "FailureStep=compiler-split-hosted-cache"
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 2 ^
    "%Work%\Admitter.wvb" "%Work%\Admitter.exe" --development-cache || goto :cleanup
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 7 ^
    "%Work%\Validator.wvb" "%Work%\Validator.exe" --development-cache || goto :cleanup
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 7 ^
    "%Work%\Analyzer.wvb" "%Work%\Analyzer.exe" --development-cache || goto :cleanup
node "%Native%\Write-Split-Compiler-Producer-Identity.mjs" ^
    analyzer "%Work%\Analyzer.exe" "%Work%\Analyzer.identity" || goto :cleanup
node "%Native%\Verify-Source-Wir-Incremental-Generics.mjs" ^
    "%Work%\Analyzer.exe" "%Work%" || goto :cleanup
set "FailureStep=compiler-generic-nominal-main-pipeline"
echo START language 1 front door step=generic-nominal-main-pipeline
"%Work%\Analyzer.exe" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Generic-Nominal-Main-Pipeline.wv" ^
    "%Work%\Generic-Nominal-Main.wvss" ^
    "%Work%\Generic-Nominal-Main.wvca" ^
    "%Work%\Generic-Nominal-Main.wvlb" ^
    "%Work%\Generic-Nominal-Main.wvir" ^
    >"%Work%\Generic-Nominal-Main.out" ^
    2>"%Work%\Generic-Nominal-Main.err" || goto :cleanup
for %%F in ("%Work%\Generic-Nominal-Main.err") do if not "%%~zF"=="0" goto :cleanup
type "%Work%\Generic-Nominal-Main.out"
echo INFO  language 1 front door step=generic-nominal-main-pipeline analysis=Published
set "FailureStep=compiler-generic-nominal-function-body-analysis"
echo START language 1 front door step=generic-nominal-function-body
"%Work%\Analyzer.exe" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Generic-Nominal-Function-Body.wv" ^
    "%Work%\Generic-Nominal-Function-Body.wvss" ^
    "%Work%\Generic-Nominal-Function-Body.wvca" ^
    "%Work%\Generic-Nominal-Function-Body.wvlb" ^
    "%Work%\Generic-Nominal-Function-Body.wvir" ^
    >"%Work%\Generic-Nominal-Function-Body.out" ^
    2>"%Work%\Generic-Nominal-Function-Body.err" || goto :cleanup
for %%F in ("%Work%\Generic-Nominal-Function-Body.err") do if not "%%~zF"=="0" goto :cleanup
type "%Work%\Generic-Nominal-Function-Body.out"
echo INFO  language 1 front door step=generic-nominal-function-body analysis=Published
set "FailureStep=compiler-generic-nominal-declaration-dependency-analysis"
echo START language 1 front door step=generic-nominal-declaration-dependency
"%Work%\Analyzer.exe" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Generic-Nominal-Declaration-Dependency.wv" ^
    "%Work%\Generic-Nominal-Declaration-Dependency.wvss" ^
    "%Work%\Generic-Nominal-Declaration-Dependency.wvca" ^
    "%Work%\Generic-Nominal-Declaration-Dependency.wvlb" ^
    "%Work%\Generic-Nominal-Declaration-Dependency.wvir" ^
    >"%Work%\Generic-Nominal-Declaration-Dependency.out" ^
    2>"%Work%\Generic-Nominal-Declaration-Dependency.err" || goto :cleanup
for %%F in ("%Work%\Generic-Nominal-Declaration-Dependency.err") do if not "%%~zF"=="0" goto :cleanup
type "%Work%\Generic-Nominal-Declaration-Dependency.out"
call :expect_analysis_failure ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Generic-Nominal-Declaration-Cycle.wv" ^
    "Generic-Nominal-Declaration-Cycle" "Genericˉresolution" || goto :cleanup
echo INFO  language 1 front door step=generic-nominal-declaration-dependency analysis=Published cycle=Rejected
call :expect_analysis_failure ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Generic-Nominal-Missing-Field.wv" ^
    "Generic-Nominal-Missing-Field" "Missingˉrecordˉfield" || goto :cleanup
call :expect_analysis_failure ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Generic-Nominal-Duplicate-Field.wv" ^
    "Generic-Nominal-Duplicate-Field" "Duplicateˉrecordˉfield" || goto :cleanup
call :expect_analysis_failure ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Generic-Nominal-Field-Type-Mismatch.wv" ^
    "Generic-Nominal-Field-Type-Mismatch" "Typeˉmismatch" || goto :cleanup
call :expect_analysis_failure ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Generic-Nominal-Unknown-Field.wv" ^
    "Generic-Nominal-Unknown-Field" "Unknownˉfield" || goto :cleanup
echo PASS  language 1 front door step=generic-nominal-record-rejections cases=4
call :expect_analysis_failure ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Generic-Nominal-Function-Body-Type-Mismatch.wv" ^
    "Generic-Nominal-Function-Body-Type-Mismatch" "Typeˉmismatch" || goto :cleanup
call :expect_analysis_failure ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Generic-Nominal-Function-Body-Unknown-Field.wv" ^
    "Generic-Nominal-Function-Body-Unknown-Field" "Unknownˉfield" || goto :cleanup
call :expect_analysis_failure ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Generic-Nominal-Function-Body-Inference-Mismatch.wv" ^
    "Generic-Nominal-Function-Body-Inference-Mismatch" "Genericˉresolution" || goto :cleanup
echo PASS  language 1 front door step=generic-nominal-function-body-rejections cases=3
set "FailureStep=compiler-target-aware-emitter"
node "%Native%\Build-Cached-Split-Project-Wvb.mjs" ^
    "%RepositoryRoot%\Projects\Tools\Windvale-Compiler-Emission-Driver.wvproj" ^
    "%Work%\Emitter.wvb" ^
    "%Work%\Analyzer.exe" "%Work%\Analyzer.identity" ^
    "%Work%\Bridge-Emitter.exe" "%Work%\Bridge-Emitter.identity" || goto :cleanup
for %%F in ("%Work%\Emitter.wvb") do echo INFO  language 1 emitter wvb-bytes=%%~zF
certutil -hashfile "%Work%\Emitter.wvb" SHA256
for %%F in ("%Work%\Emitter.wvb") do if not "%%~zF"=="1556434" goto :cleanup
certutil -hashfile "%Work%\Emitter.wvb" SHA256 | findstr /I /C:"d16cc44f65a788a8c2dc45d423686dde095cac63e8f2fd8305d1246b29c168f9" >nul || goto :cleanup
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 7 ^
    "%Work%\Emitter.wvb" "%Work%\Emitter.exe" --development-cache || goto :cleanup
node "%Native%\Write-Split-Compiler-Producer-Identity.mjs" ^
    emitter "%Work%\Emitter.exe" "%Work%\Emitter.identity" || goto :cleanup
set "FailureStep=compiler-enum-backing-analysis"
echo START language 1 front door step=enum-backing
node "%Native%\Run-Authenticated-Source-Admission.mjs" ^
    "%Work%\Admitter.exe" "%Work%\Validator.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Enum-Backing-All.wv" ^
    "%Work%\Enum-Backing-All.wvss" ^
    >"%Work%\Enum-Backing-All-Admission.out" ^
    2>"%Work%\Enum-Backing-All-Admission.err" || goto :cleanup
for %%F in ("%Work%\Enum-Backing-All-Admission.err") do if not "%%~zF"=="0" goto :cleanup
call :analyze_authenticated ^
    "%Work%\Enum-Backing-All.wvss" "%Work%\Enum-Backing-All.wvss" ^
    "%Work%\Enum-Backing-All.wvca" "%Work%\Enum-Backing-All.wvlb" ^
    "%Work%\Enum-Backing-All.wvir" ^
    >"%Work%\Enum-Backing-All-Analysis.out" ^
    2>"%Work%\Enum-Backing-All-Analysis.err" || goto :cleanup
for %%F in ("%Work%\Enum-Backing-All-Analysis.err") do if not "%%~zF"=="0" goto :cleanup
findstr /b /c:"source analysis status=Published " ^
    "%Work%\Enum-Backing-All-Analysis.out" >nul || goto :cleanup
set "FailureStep=compiler-enum-backing-wvb-boundary"
"%Work%\Emitter.exe" ^
    "%Work%\Enum-Backing-All.wvss" "%Work%\Enum-Backing-All.wvca" ^
    "%Work%\Enum-Backing-All.wvlb" "%Work%\Enum-Backing-All.wvir" ^
    "%Work%\Enum-Backing-All.wvb" ^
    >"%Work%\Enum-Backing-All-Emission.out" ^
    2>"%Work%\Enum-Backing-All-Emission.err" || goto :cleanup
for %%F in ("%Work%\Enum-Backing-All-Emission.err") do if not "%%~zF"=="0" goto :cleanup
call "%Native%\Run-Wvb.cmd" "%Work%\Enum-Backing-All.wvb" ^
    >"%Work%\Enum-Backing-All-Run.out" ^
    2>"%Work%\Enum-Backing-All-Run.err" || goto :cleanup
for %%F in ("%Work%\Enum-Backing-All-Run.err") do if not "%%~zF"=="0" goto :cleanup
call :expect_result_42 "%Work%\Enum-Backing-All-Run.out" || goto :cleanup
for %%F in ("%Work%\Enum-Backing-All.wvb") do set "EnumDeadTypeWvbBytes=%%~zF"
if not "%EnumDeadTypeWvbBytes%"=="217" goto :cleanup
set "FailureStep=compiler-enum-u8-used-wvb-boundary"
node "%Native%\Run-Split-Compiler.mjs" ^
    "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Enum-U8-Used-Main.wv" ^
    "%Work%\Enum-U8-Used-Main-A.wvb" ^
    >"%Work%\Enum-U8-Used-Main-A.out" ^
    2>"%Work%\Enum-U8-Used-Main-A.err" || goto :cleanup
node "%Native%\Run-Split-Compiler.mjs" ^
    "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Enum-U8-Used-Main.wv" ^
    "%Work%\Enum-U8-Used-Main-B.wvb" ^
    >"%Work%\Enum-U8-Used-Main-B.out" ^
    2>"%Work%\Enum-U8-Used-Main-B.err" || goto :cleanup
for %%F in ("%Work%\Enum-U8-Used-Main-A.err" "%Work%\Enum-U8-Used-Main-B.err") do if not "%%~zF"=="0" goto :cleanup
fc /b "%Work%\Enum-U8-Used-Main-A.wvb" ^
    "%Work%\Enum-U8-Used-Main-B.wvb" >nul || goto :cleanup
for %%F in ("%Work%\Enum-U8-Used-Main-A.wvb") do set "EnumU8WvbBytes=%%~zF"
if not "%EnumU8WvbBytes%"=="415" goto :cleanup
certutil -hashfile "%Work%\Enum-U8-Used-Main-A.wvb" SHA256 | findstr /I /C:"961ba417955a523b9fc21e0b71df7a8d99613252b7450700dd4381aa94e825ed" >nul || goto :cleanup
set "FailureStep=compiler-enum-symbol-rejections"
call :expect_profiled_symbol_failure ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Enum-Backing-Duplicate-Signed.wv" ^
    "Enum-Backing-Duplicate-Signed" "Duplicateˉenumˉvalue" || goto :cleanup
call :expect_profiled_symbol_failure ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Enum-Backing-Mismatched-Suffix.wv" ^
    "Enum-Backing-Mismatched-Suffix" "Invalidˉenumˉvalue" || goto :cleanup
call :expect_profiled_symbol_failure ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Enum-Backing-Out-Of-Range.wv" ^
    "Enum-Backing-Out-Of-Range" "Invalidˉenumˉvalue" || goto :cleanup
call :expect_profiled_symbol_failure ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Enum-Backing-Unsigned-Negative.wv" ^
    "Enum-Backing-Unsigned-Negative" "Invalidˉenumˉvalue" || goto :cleanup
call :expect_profiled_symbol_failure ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Enum-Missing-Backing.wv" ^
    "Enum-Missing-Backing" "Missingˉenumˉbacking" || goto :cleanup
set "FailureStep=compiler-enum-i32"
node "%Native%\Run-Split-Compiler.mjs" ^
    "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Enum-I32-Negative-Main.wv" ^
    "%Work%\Enum-I32-Negative-Main.wvb" ^
    >"%Work%\Enum-I32-Negative-Main.out" ^
    2>"%Work%\Enum-I32-Negative-Main.err" || goto :cleanup
for %%F in ("%Work%\Enum-I32-Negative-Main.err") do if not "%%~zF"=="0" goto :cleanup
call "%Native%\Run-Wvb.cmd" "%Work%\Enum-I32-Negative-Main.wvb" ^
    >"%Work%\Enum-I32-Negative-Main-Run.out" ^
    2>"%Work%\Enum-I32-Negative-Main-Run.err" || goto :cleanup
for %%F in ("%Work%\Enum-I32-Negative-Main-Run.err") do if not "%%~zF"=="0" goto :cleanup
call :expect_result_42 "%Work%\Enum-I32-Negative-Main-Run.out" || goto :cleanup
for %%F in ("%Work%\Enum-I32-Negative-Main.wvb") do set "EnumI32WvbBytes=%%~zF"
if not "%EnumI32WvbBytes%"=="427" goto :cleanup
echo PASS  language 1 front door step=enum-backing cases=9 analysis=all-fixed-widths wvb=i32-only execution=42
set "FailureStep=compiler-borrow-call-semantics"
echo START language 1 front door step=borrow-call-semantics
"%Work%\Analyzer.exe" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Borrow-Call-Main-Pipeline.wv" ^
    "%Work%\Borrow-Call.wvss" "%Work%\Borrow-Call.wvca" ^
    "%Work%\Borrow-Call.wvlb" "%Work%\Borrow-Call.wvir" ^
    >"%Work%\Borrow-Call-Analysis.out" ^
    2>"%Work%\Borrow-Call-Analysis.err" || goto :cleanup
for %%F in ("%Work%\Borrow-Call-Analysis.err") do if not "%%~zF"=="0" goto :cleanup
"%Work%\Emitter.exe" ^
    "%Work%\Borrow-Call.wvss" "%Work%\Borrow-Call.wvca" ^
    "%Work%\Borrow-Call.wvlb" "%Work%\Borrow-Call.wvir" ^
    "%Work%\Borrow-Call.wvb" ^
    >"%Work%\Borrow-Call-Emission.out" ^
    2>"%Work%\Borrow-Call-Emission.err" || goto :cleanup
for %%F in ("%Work%\Borrow-Call-Emission.err") do if not "%%~zF"=="0" goto :cleanup
call "%Native%\Verify-Wvb.cmd" "%Work%\Borrow-Call.wvb" || goto :cleanup
set "FailureStep=compiler-borrow-call-webassembly"
node "%Native%\Run-WebAssembly-Scalar-Wvb.mjs" ^
    "%RepositoryRoot%\Artifacts\WebAssembly-Playground\Wvb-Scalar-Interpreter.wasm" ^
    "%Work%\Borrow-Call.wvb" 42 ^
    >"%Work%\Borrow-Call-Run.out" 2>"%Work%\Borrow-Call-Run.err" || goto :cleanup
for %%F in ("%Work%\Borrow-Call-Run.err") do if not "%%~zF"=="0" goto :cleanup
findstr /c:"webassembly scalar status=Valid result=42" "%Work%\Borrow-Call-Run.out" >nul || goto :cleanup
echo PASS  language 1 front door step=borrow-call-semantics item=execution result=42 engine=webassembly
set "FailureStep=compiler-borrow-call-semantics"
call :expect_analysis_failure ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Borrow-Missing-Explicit.wv" ^
    "Borrow-Missing-Explicit" "Invalidˉborrow" || goto :cleanup
call :expect_analysis_failure ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Borrow-Immutable-To-Mutable.wv" ^
    "Borrow-Immutable-To-Mutable" "Invalidˉborrow" || goto :cleanup
call :expect_analysis_failure ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Borrow-Mutable-From-Let.wv" ^
    "Borrow-Mutable-From-Let" "Invalidˉborrow" || goto :cleanup
call :expect_analysis_failure ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Borrow-Escape-Local.wv" ^
    "Borrow-Escape-Local" "Invalidˉborrow" || goto :cleanup
call :expect_analysis_failure ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Borrow-Return.wv" ^
    "Borrow-Return" "Invalidˉborrow" || goto :cleanup
call :expect_analysis_failure ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Borrow-Owned-Read-Through.wv" ^
    "Borrow-Owned-Read-Through" "Invalidˉborrow" || goto :cleanup
echo PASS  language 1 front door step=borrow-call-semantics item=direct-rejections cases=6
node "%Native%\Run-Authenticated-Source-Admission.mjs" ^
    "%Work%\Admitter.exe" "%Work%\Validator.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Borrow-Sequence-Read-Through.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Collections\Collections.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" ^
    "%Work%\Borrow-Sequence-Admitted.wvss" ^
    >"%Work%\Borrow-Sequence-Admission.out" ^
    2>"%Work%\Borrow-Sequence-Admission.err" || goto :cleanup
for %%F in ("%Work%\Borrow-Sequence-Admission.err") do if not "%%~zF"=="0" goto :cleanup
call :analyze_authenticated ^
    "%Work%\Borrow-Sequence-Admitted.wvss" ^
    "%Work%\Borrow-Sequence.wvss" "%Work%\Borrow-Sequence.wvca" ^
    "%Work%\Borrow-Sequence.wvlb" "%Work%\Borrow-Sequence.wvir" ^
    >"%Work%\Borrow-Sequence.out" 2>"%Work%\Borrow-Sequence.err" || goto :cleanup
for %%F in ("%Work%\Borrow-Sequence.err") do if not "%%~zF"=="0" goto :cleanup
findstr /c:"source analysis status=Published" "%Work%\Borrow-Sequence.out" >nul || goto :cleanup
echo PASS  language 1 front door step=borrow-call-semantics item=sequence ownership=Shared
node "%Native%\Run-Authenticated-Source-Admission.mjs" ^
    "%Work%\Admitter.exe" "%Work%\Validator.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Borrow-Vector-Owned-Read-Through.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Collections\Collections.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" ^
    "%Work%\Borrow-Vector-Admitted.wvss" ^
    >"%Work%\Borrow-Vector-Admission.out" ^
    2>"%Work%\Borrow-Vector-Admission.err" || goto :cleanup
for %%F in ("%Work%\Borrow-Vector-Admission.err") do if not "%%~zF"=="0" goto :cleanup
call :analyze_authenticated ^
    "%Work%\Borrow-Vector-Admitted.wvss" ^
    "%Work%\Borrow-Vector.wvss" "%Work%\Borrow-Vector.wvca" ^
    "%Work%\Borrow-Vector.wvlb" "%Work%\Borrow-Vector.wvir" ^
    >"%Work%\Borrow-Vector.out" 2>"%Work%\Borrow-Vector.err"
if not errorlevel 1 goto :cleanup
if errorlevel 2 goto :cleanup
for %%F in ("%Work%\Borrow-Vector.out") do if not "%%~zF"=="0" goto :cleanup
set "BorrowVectorLine="
set /a BorrowVectorLines=0
for /f "usebackq delims=" %%L in ("%Work%\Borrow-Vector.err") do (
    set "BorrowVectorLine=%%L"
    set /a BorrowVectorLines+=1 >nul
)
if not "%BorrowVectorLines%"=="1" goto :cleanup
node "%Native%\Verify-Source-Analysis-Diagnostic.mjs" ^
    "%Work%\Borrow-Vector.err" wir Invalid-borrow || goto :cleanup
if exist "%Work%\Borrow-Vector.wvir" goto :cleanup
echo PASS  language 1 front door step=borrow-call-semantics item=vector ownership=Owned rejection=Invalid-borrow
node "%Native%\Run-Authenticated-Source-Admission.mjs" ^
    "%Work%\Admitter.exe" "%Work%\Validator.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Memory-Budget-Type-Identity.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" ^
    "%Work%\Memory-Budget-Admitted.wvss" ^
    >"%Work%\Memory-Budget-Admission.out" ^
    2>"%Work%\Memory-Budget-Admission.err" || goto :cleanup
for %%F in ("%Work%\Memory-Budget-Admission.err") do if not "%%~zF"=="0" goto :cleanup
call :analyze_authenticated ^
    "%Work%\Memory-Budget-Admitted.wvss" ^
    "%Work%\Memory-Budget.wvss" "%Work%\Memory-Budget.wvca" ^
    "%Work%\Memory-Budget.wvlb" "%Work%\Memory-Budget.wvir" ^
    >"%Work%\Memory-Budget-Analysis.out" ^
    2>"%Work%\Memory-Budget-Analysis.err" || goto :cleanup
for %%F in ("%Work%\Memory-Budget-Analysis.err") do if not "%%~zF"=="0" goto :cleanup
findstr /c:"source analysis status=Published" "%Work%\Memory-Budget-Analysis.out" >nul || goto :cleanup
"%Work%\Emitter.exe" ^
    "%Work%\Memory-Budget.wvss" "%Work%\Memory-Budget.wvca" ^
    "%Work%\Memory-Budget.wvlb" "%Work%\Memory-Budget.wvir" ^
    "%Work%\Memory-Budget.wvb" ^
    >"%Work%\Memory-Budget-Emission.out" ^
    2>"%Work%\Memory-Budget-Emission.err"
if not errorlevel 1 goto :cleanup
if errorlevel 2 goto :cleanup
for %%F in ("%Work%\Memory-Budget-Emission.out") do if not "%%~zF"=="0" goto :cleanup
if exist "%Work%\Memory-Budget.wvb" goto :cleanup
set "MemoryBudgetEmissionLine="
set /a MemoryBudgetEmissionLines=0
for /f "usebackq delims=" %%L in ("%Work%\Memory-Budget-Emission.err") do (
    set "MemoryBudgetEmissionLine=%%L"
    set /a MemoryBudgetEmissionLines+=1 >nul
)
if not "%MemoryBudgetEmissionLines%"=="1" goto :cleanup
if not "%MemoryBudgetEmissionLine%"=="source emission status=Valid analysis-status=Valid wvb-status=Unsupportedˉshape function=1 operation=4 source-line=0" goto :cleanup
echo PASS  language 1 front door step=borrow-call-semantics item=memory-budget identity=Owned-WVIR wvb=Unsupported-shape
call :expect_profiled_analysis_failure_with_dependencies ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Memory-Budget-Owned-Read-Through.wv" ^
    "Memory-Budget-Owned-Read-Through" "Invalidˉborrow" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" || goto :cleanup
call :expect_profiled_symbol_failure_with_dependency ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Memory-Budget-Unqualified.wv" ^
    "Memory-Budget-Unqualified" "Unknownˉtype" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" || goto :cleanup
call :expect_profiled_symbol_failure_with_dependency ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Memory-Budget-Lookalike-Module.wv" ^
    "Memory-Budget-Lookalike-Module" "Unknownˉtype" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Foundation-Memory-Lookalike.wv" || goto :cleanup
echo PASS  language 1 front door step=borrow-call-semantics item=memory-rejections cases=3
set "FailureStep=compiler-memory-budget-entry"
echo START language 1 front door step=memory-budget-entry
node "%Native%\Run-Authenticated-Source-Admission.mjs" ^
    "%Work%\Admitter.exe" "%Work%\Validator.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Memory-Budget-Entry-Main.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" ^
    "%Work%\Memory-Budget-Entry-Admitted.wvss" ^
    >"%Work%\Memory-Budget-Entry-Admission.out" ^
    2>"%Work%\Memory-Budget-Entry-Admission.err" || goto :cleanup
for %%F in ("%Work%\Memory-Budget-Entry-Admission.err") do if not "%%~zF"=="0" goto :cleanup
call :analyze_authenticated ^
    "%Work%\Memory-Budget-Entry-Admitted.wvss" ^
    "%Work%\Memory-Budget-Entry.wvss" "%Work%\Memory-Budget-Entry.wvca" ^
    "%Work%\Memory-Budget-Entry.wvlb" "%Work%\Memory-Budget-Entry.wvir" ^
    >"%Work%\Memory-Budget-Entry-Analysis.out" ^
    2>"%Work%\Memory-Budget-Entry-Analysis.err" || goto :cleanup
for %%F in ("%Work%\Memory-Budget-Entry-Analysis.err") do if not "%%~zF"=="0" goto :cleanup
findstr /c:"source analysis status=Published" "%Work%\Memory-Budget-Entry-Analysis.out" >nul || goto :cleanup
for %%S in (A B) do (
    "%Work%\Emitter.exe" ^
        "%Work%\Memory-Budget-Entry.wvss" "%Work%\Memory-Budget-Entry.wvca" ^
        "%Work%\Memory-Budget-Entry.wvlb" "%Work%\Memory-Budget-Entry.wvir" ^
        "%Work%\Memory-Budget-Entry-%%S.wvb" ^
        >"%Work%\Memory-Budget-Entry-%%S.out" ^
        2>"%Work%\Memory-Budget-Entry-%%S.err" || goto :cleanup
)
for %%F in ("%Work%\Memory-Budget-Entry-A.err" "%Work%\Memory-Budget-Entry-B.err") do if not "%%~zF"=="0" goto :cleanup
fc /b "%Work%\Memory-Budget-Entry-A.wvb" "%Work%\Memory-Budget-Entry-B.wvb" >nul || goto :cleanup
for %%F in ("%Work%\Memory-Budget-Entry-A.wvb") do set "MemoryBudgetEntryWvbBytes=%%~zF"
if not "%MemoryBudgetEntryWvbBytes%"=="242" goto :cleanup
echo PASS  language 1 front door step=memory-budget-entry item=compile format=WVB-1.21 deterministic=1 wvb-bytes=%MemoryBudgetEntryWvbBytes%
echo PASS  language 1 front door step=borrow-call-semantics cases=14 execution=42 vector=Owned sequence=Shared memory-budget=Owned-WVIR
set "FailureStep=compiler-memory-budget-split"
echo START language 1 front door step=memory-budget-split
node "%Native%\Run-Authenticated-Source-Admission.mjs" ^
    "%Work%\Admitter.exe" "%Work%\Validator.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Memory-Budget-Split-Wir.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Values\Result.wv" ^
    "%Work%\Memory-Budget-Split-Admitted.wvss" ^
    >"%Work%\Memory-Budget-Split-Admission.out" ^
    2>"%Work%\Memory-Budget-Split-Admission.err" || goto :cleanup
for %%F in ("%Work%\Memory-Budget-Split-Admission.err") do if not "%%~zF"=="0" goto :cleanup
call :analyze_authenticated ^
    "%Work%\Memory-Budget-Split-Admitted.wvss" ^
    "%Work%\Memory-Budget-Split.wvss" ^
    "%Work%\Memory-Budget-Split.wvca" ^
    "%Work%\Memory-Budget-Split.wvlb" ^
    "%Work%\Memory-Budget-Split.wvir" ^
    >"%Work%\Memory-Budget-Split-Analysis.out" ^
    2>"%Work%\Memory-Budget-Split-Analysis.err" || goto :cleanup
for %%F in ("%Work%\Memory-Budget-Split-Analysis.err") do if not "%%~zF"=="0" goto :cleanup
findstr /b /c:"source analysis status=Published " ^
    "%Work%\Memory-Budget-Split-Analysis.out" >nul || goto :cleanup
mkdir "%Work%\Memory-Budget-Split-Malformed" || goto :cleanup
node "%Native%\Verify-Language-1.0-Memory-Budget-Split-Wir.mjs" ^
    "%Work%\Emitter.exe" ^
    "%Work%\Memory-Budget-Split.wvss" ^
    "%Work%\Memory-Budget-Split.wvca" ^
    "%Work%\Memory-Budget-Split.wvlb" ^
    "%Work%\Memory-Budget-Split.wvir" ^
    "%Work%\Memory-Budget-Split-Malformed" || goto :cleanup
call :expect_profiled_analysis_failure_with_dependencies ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Memory-Budget-Split-Immutable-Borrow.wv" ^
    "Memory-Budget-Split-Immutable-Borrow" "Invalidˉborrow" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Values\Result.wv" || goto :cleanup
call :expect_profiled_analysis_failure_with_dependencies ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Memory-Budget-Split-Wrong-Limit.wv" ^
    "Memory-Budget-Split-Wrong-Limit" "Invalidˉargument" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Values\Result.wv" || goto :cleanup
call :expect_profiled_analysis_failure_with_dependencies ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Memory-Budget-Split-Wrong-Result.wv" ^
    "Memory-Budget-Split-Wrong-Result" "Genericˉresolution" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Values\Result.wv" || goto :cleanup
call :expect_profiled_analysis_failure_with_dependencies ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Memory-Budget-Split-Wrong-Allocation-Failure.wv" ^
    "Memory-Budget-Split-Wrong-Allocation-Failure" "Genericˉresolution" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Foundation-Memory-Wrong-Allocation-Failure.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Values\Result.wv" || goto :cleanup
echo PASS  language 1 front door step=memory-budget-split cases=13 wvir=1.11 valid=1 wvb-boundary=1 malformed=7 source-rejections=4
set "FailureStep=compiler-vector-construct-reserved"
echo START language 1 front door step=vector-construct-reserved
node "%Native%\Run-Authenticated-Source-Admission.mjs" ^
    "%Work%\Admitter.exe" "%Work%\Validator.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Vector-Construct-Reserved-Wir.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Collections\Collections.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Values\Result.wv" ^
    "%Work%\Vector-Construct-Reserved-Admitted.wvss" ^
    >"%Work%\Vector-Construct-Reserved-Admission.out" ^
    2>"%Work%\Vector-Construct-Reserved-Admission.err" || goto :cleanup
for %%F in ("%Work%\Vector-Construct-Reserved-Admission.err") do if not "%%~zF"=="0" goto :cleanup
call :analyze_authenticated ^
    "%Work%\Vector-Construct-Reserved-Admitted.wvss" ^
    "%Work%\Vector-Construct-Reserved.wvss" ^
    "%Work%\Vector-Construct-Reserved.wvca" ^
    "%Work%\Vector-Construct-Reserved.wvlb" ^
    "%Work%\Vector-Construct-Reserved.wvir" ^
    >"%Work%\Vector-Construct-Reserved-Analysis.out" ^
    2>"%Work%\Vector-Construct-Reserved-Analysis.err" || goto :cleanup
for %%F in ("%Work%\Vector-Construct-Reserved-Analysis.err") do if not "%%~zF"=="0" goto :cleanup
findstr /b /c:"source analysis status=Published " ^
    "%Work%\Vector-Construct-Reserved-Analysis.out" >nul || goto :cleanup
mkdir "%Work%\Vector-Construct-Reserved-Malformed" || goto :cleanup
node "%Native%\Verify-Language-1.0-Vector-Construct-Reserved-Wir.mjs" ^
    "%Work%\Emitter.exe" ^
    "%Work%\Vector-Construct-Reserved.wvss" ^
    "%Work%\Vector-Construct-Reserved.wvca" ^
    "%Work%\Vector-Construct-Reserved.wvlb" ^
    "%Work%\Vector-Construct-Reserved.wvir" ^
    "%Work%\Vector-Construct-Reserved-Malformed" || goto :cleanup
echo START language 1 front door step=vector-construct-reserved item=source-rejection case=inferred
call :expect_profiled_analysis_failure_with_dependencies ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Vector-Construct-Reserved-Inferred.wv" ^
    "Vector-Construct-Reserved-Inferred" "Genericˉresolution" ^
    "%RepositoryRoot%\Libraries\Foundation\Collections\Collections.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Values\Result.wv" || goto :cleanup
echo START language 1 front door step=vector-construct-reserved item=source-rejection case=wrong-maximum
call :expect_profiled_analysis_failure_with_dependencies ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Vector-Construct-Reserved-Wrong-Maximum.wv" ^
    "Vector-Construct-Reserved-Wrong-Maximum" "Invalidˉargument" ^
    "%RepositoryRoot%\Libraries\Foundation\Collections\Collections.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Values\Result.wv" || goto :cleanup
echo START language 1 front door step=vector-construct-reserved item=source-rejection case=wrong-result
call :expect_profiled_analysis_failure_with_dependencies ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Vector-Construct-Reserved-Wrong-Result.wv" ^
    "Vector-Construct-Reserved-Wrong-Result" "Genericˉresolution" ^
    "%RepositoryRoot%\Libraries\Foundation\Collections\Collections.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Values\Result.wv" || goto :cleanup
echo START language 1 front door step=vector-construct-reserved item=source-rejection case=wrong-budget
call :expect_profiled_analysis_failure_with_dependencies ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Vector-Construct-Reserved-Wrong-Budget.wv" ^
    "Vector-Construct-Reserved-Wrong-Budget" "Invalidˉargument" ^
    "%RepositoryRoot%\Libraries\Foundation\Collections\Collections.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Values\Result.wv" || goto :cleanup
echo START language 1 front door step=vector-construct-reserved item=source-rejection case=wrong-allocation-failure
call :expect_profiled_analysis_failure_with_dependencies ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Vector-Construct-Reserved-Wrong-Allocation-Failure.wv" ^
    "Vector-Construct-Reserved-Wrong-Allocation-Failure" "Genericˉresolution" ^
    "%RepositoryRoot%\Libraries\Foundation\Collections\Collections.wv" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Foundation-Memory-Wrong-Allocation-Failure.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Values\Result.wv" || goto :cleanup
echo START language 1 front door step=vector-construct-reserved item=ownership-rejection case=use-after
node "%Native%\Run-Authenticated-Source-Admission.mjs" ^
    "%Work%\Admitter.exe" "%Work%\Validator.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Vector-Construct-Reserved-Use-After.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Collections\Collections.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Values\Result.wv" ^
    "%Work%\Vector-Construct-Reserved-Use-After-Admitted.wvss" ^
    >"%Work%\Vector-Construct-Reserved-Use-After-Admission.out" ^
    2>"%Work%\Vector-Construct-Reserved-Use-After-Admission.err" || goto :cleanup
for %%F in ("%Work%\Vector-Construct-Reserved-Use-After-Admission.err") do if not "%%~zF"=="0" goto :cleanup
echo PASS  language 1 front door step=vector-construct-reserved item=ownership-rejection case=use-after phase=admission
call :analyze_authenticated ^
    "%Work%\Vector-Construct-Reserved-Use-After-Admitted.wvss" ^
    "%Work%\Vector-Construct-Reserved-Use-After.wvss" ^
    "%Work%\Vector-Construct-Reserved-Use-After.wvca" ^
    "%Work%\Vector-Construct-Reserved-Use-After.wvlb" ^
    "%Work%\Vector-Construct-Reserved-Use-After.wvir" ^
    >"%Work%\Vector-Construct-Reserved-Use-After-Analysis.out" ^
    2>"%Work%\Vector-Construct-Reserved-Use-After-Analysis.err" || goto :cleanup
for %%F in ("%Work%\Vector-Construct-Reserved-Use-After-Analysis.err") do if not "%%~zF"=="0" goto :cleanup
echo PASS  language 1 front door step=vector-construct-reserved item=ownership-rejection case=use-after phase=analysis
"%Work%\Emitter.exe" ^
    "%Work%\Vector-Construct-Reserved-Use-After.wvss" ^
    "%Work%\Vector-Construct-Reserved-Use-After.wvca" ^
    "%Work%\Vector-Construct-Reserved-Use-After.wvlb" ^
    "%Work%\Vector-Construct-Reserved-Use-After.wvir" ^
    "%Work%\Vector-Construct-Reserved-Use-After.wvb" ^
    >"%Work%\Vector-Construct-Reserved-Use-After-Emission.out" ^
    2>"%Work%\Vector-Construct-Reserved-Use-After-Emission.err"
set "VectorUseAfterEmissionExit=%ERRORLEVEL%"
echo INFO  language 1 front door step=vector-construct-reserved item=ownership-rejection case=use-after phase=emission exit=%VectorUseAfterEmissionExit%
type "%Work%\Vector-Construct-Reserved-Use-After-Emission.err"
if not "%VectorUseAfterEmissionExit%"=="1" goto :cleanup
for %%F in ("%Work%\Vector-Construct-Reserved-Use-After-Emission.out") do if not "%%~zF"=="0" goto :cleanup
set "VectorUseAfterEmissionLine="
set /a VectorUseAfterEmissionLines=0
for /f "usebackq delims=" %%L in ("%Work%\Vector-Construct-Reserved-Use-After-Emission.err") do (
    set "VectorUseAfterEmissionLine=%%L"
    set /a VectorUseAfterEmissionLines+=1 >nul
)
if not "%VectorUseAfterEmissionLines%"=="1" goto :cleanup
if not "%VectorUseAfterEmissionLine%"=="source emission status=Invalidˉanalysis analysis-status=Invalidˉwir wvb-status=Sourceˉwir function=0 operation=0 source-line=0" goto :cleanup
if exist "%Work%\Vector-Construct-Reserved-Use-After.wvb" goto :cleanup
echo PASS  language 1 front door step=vector-construct-reserved item=ownership-rejection case=use-after phase=evidence
echo PASS  language 1 front door step=vector-construct-reserved cases=16 wvir=1.11 valid=1 wvb-boundary=1 malformed=8 source-rejections=5 ownership-rejections=1
set "FailureStep=compiler-generic-nominal-variant"
echo START language 1 front door step=generic-nominal-variant
node "%Native%\Run-Authenticated-Source-Admission.mjs" ^
    "%Work%\Admitter.exe" "%Work%\Validator.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Generic-Nominal-Variant.wv" ^
    "%Work%\Generic-Nominal-Variant.wvss" ^
    >"%Work%\Generic-Nominal-Variant-Admission.out" ^
    2>"%Work%\Generic-Nominal-Variant-Admission.err" || goto :cleanup
for %%F in ("%Work%\Generic-Nominal-Variant-Admission.err") do if not "%%~zF"=="0" goto :cleanup
call :analyze_authenticated ^
    "%Work%\Generic-Nominal-Variant.wvss" ^
    "%Work%\Generic-Nominal-Variant.wvss" ^
    "%Work%\Generic-Nominal-Variant.wvca" ^
    "%Work%\Generic-Nominal-Variant.wvlb" ^
    "%Work%\Generic-Nominal-Variant.wvir" ^
    >"%Work%\Generic-Nominal-Variant-Analysis.out" ^
    2>"%Work%\Generic-Nominal-Variant-Analysis.err" || goto :cleanup
for %%F in ("%Work%\Generic-Nominal-Variant-Analysis.err") do if not "%%~zF"=="0" goto :cleanup
"%Work%\Emitter.exe" ^
    "%Work%\Generic-Nominal-Variant.wvss" ^
    "%Work%\Generic-Nominal-Variant.wvca" ^
    "%Work%\Generic-Nominal-Variant.wvlb" ^
    "%Work%\Generic-Nominal-Variant.wvir" ^
    "%Work%\Generic-Nominal-Variant.wvb" ^
    >"%Work%\Generic-Nominal-Variant-Emission.out" ^
    2>"%Work%\Generic-Nominal-Variant-Emission.err" || goto :cleanup
for %%F in ("%Work%\Generic-Nominal-Variant-Emission.err") do if not "%%~zF"=="0" goto :cleanup
call :expect_profiled_analysis_failure ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Generic-Nominal-Variant-Type-Mismatch.wv" ^
    "Generic-Nominal-Variant-Type-Mismatch" "Typeˉmismatch" || goto :cleanup
call :expect_profiled_analysis_failure ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Generic-Nominal-Variant-Missing-Field.wv" ^
    "Generic-Nominal-Variant-Missing-Field" "Missingˉvariantˉfield" || goto :cleanup
call :expect_profiled_analysis_failure ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Generic-Nominal-Variant-Pattern-Type-Mismatch.wv" ^
    "Generic-Nominal-Variant-Pattern-Type-Mismatch" "Typeˉmismatch" || goto :cleanup
for %%F in ("%Work%\Generic-Nominal-Variant.wvb") do set "GenericNominalVariantWvbBytes=%%~zF"
echo INFO  language 1 front door step=generic-nominal-variant analysis=Published wvb-bytes=%GenericNominalVariantWvbBytes% rejections=3
set "FailureStep=compiler-generic-nominal-main-emission"
"%Work%\Emitter.exe" ^
    "%Work%\Generic-Nominal-Main.wvss" ^
    "%Work%\Generic-Nominal-Main.wvca" ^
    "%Work%\Generic-Nominal-Main.wvlb" ^
    "%Work%\Generic-Nominal-Main.wvir" ^
    "%Work%\Generic-Nominal-Main.wvb" ^
    >"%Work%\Generic-Nominal-Main-Emission.out" ^
    2>"%Work%\Generic-Nominal-Main-Emission.err" || goto :cleanup
for %%F in ("%Work%\Generic-Nominal-Main-Emission.err") do if not "%%~zF"=="0" goto :cleanup
type "%Work%\Generic-Nominal-Main-Emission.out"
call "%Native%\Verify-Wvb.cmd" "%Work%\Generic-Nominal-Main.wvb" || goto :cleanup
node "%Native%\Verify-Generic-Nominal-Main-Pipeline.mjs" ^
    "%Work%\Generic-Nominal-Main.wvss" ^
    "%Work%\Generic-Nominal-Main.wvca" ^
    "%Work%\Generic-Nominal-Main.wvlb" ^
    "%Work%\Generic-Nominal-Main.wvir" ^
    "%Work%\Generic-Nominal-Main.wvb" || goto :cleanup
call "%Native%\Run-Wvb.cmd" "%Work%\Generic-Nominal-Main.wvb" ^
    >"%Work%\Generic-Nominal-Main-Run.out" ^
    2>"%Work%\Generic-Nominal-Main-Run.err" || goto :cleanup
for %%F in ("%Work%\Generic-Nominal-Main-Run.err") do if not "%%~zF"=="0" goto :cleanup
call :expect_result_42 "%Work%\Generic-Nominal-Main-Run.out" || goto :cleanup
echo PASS  language 1 front door step=generic-nominal-main-pipeline cases=26 verification=compiler-aligned execution=42
set "FailureStep=compiler-generic-nominal-function-body-emission"
"%Work%\Emitter.exe" ^
    "%Work%\Generic-Nominal-Function-Body.wvss" ^
    "%Work%\Generic-Nominal-Function-Body.wvca" ^
    "%Work%\Generic-Nominal-Function-Body.wvlb" ^
    "%Work%\Generic-Nominal-Function-Body.wvir" ^
    "%Work%\Generic-Nominal-Function-Body.wvb" ^
    >"%Work%\Generic-Nominal-Function-Body-Emission.out" ^
    2>"%Work%\Generic-Nominal-Function-Body-Emission.err" || goto :cleanup
for %%F in ("%Work%\Generic-Nominal-Function-Body-Emission.err") do if not "%%~zF"=="0" goto :cleanup
type "%Work%\Generic-Nominal-Function-Body-Emission.out"
call "%Native%\Verify-Wvb.cmd" "%Work%\Generic-Nominal-Function-Body.wvb" || goto :cleanup
node "%Native%\Verify-Generic-Nominal-Function-Body.mjs" ^
    "%Work%\Generic-Nominal-Function-Body.wvss" ^
    "%Work%\Generic-Nominal-Function-Body.wvca" ^
    "%Work%\Generic-Nominal-Function-Body.wvlb" ^
    "%Work%\Generic-Nominal-Function-Body.wvir" ^
    "%Work%\Generic-Nominal-Function-Body.wvb" || goto :cleanup
call "%Native%\Run-Wvb.cmd" "%Work%\Generic-Nominal-Function-Body.wvb" ^
    >"%Work%\Generic-Nominal-Function-Body-Run.out" ^
    2>"%Work%\Generic-Nominal-Function-Body-Run.err" || goto :cleanup
for %%F in ("%Work%\Generic-Nominal-Function-Body-Run.err") do if not "%%~zF"=="0" goto :cleanup
call :expect_result_42 "%Work%\Generic-Nominal-Function-Body-Run.out" || goto :cleanup
echo PASS  language 1 front door step=generic-nominal-function-body cases=33 verification=compiler-aligned execution=42
set "FailureStep=compiler-generic-nominal-declaration-dependency-emission"
"%Work%\Emitter.exe" ^
    "%Work%\Generic-Nominal-Declaration-Dependency.wvss" ^
    "%Work%\Generic-Nominal-Declaration-Dependency.wvca" ^
    "%Work%\Generic-Nominal-Declaration-Dependency.wvlb" ^
    "%Work%\Generic-Nominal-Declaration-Dependency.wvir" ^
    "%Work%\Generic-Nominal-Declaration-Dependency.wvb" ^
    >"%Work%\Generic-Nominal-Declaration-Dependency-Emission.out" ^
    2>"%Work%\Generic-Nominal-Declaration-Dependency-Emission.err" || goto :cleanup
for %%F in ("%Work%\Generic-Nominal-Declaration-Dependency-Emission.err") do if not "%%~zF"=="0" goto :cleanup
type "%Work%\Generic-Nominal-Declaration-Dependency-Emission.out"
call "%Native%\Verify-Wvb.cmd" "%Work%\Generic-Nominal-Declaration-Dependency.wvb" || goto :cleanup
node "%Native%\Verify-Generic-Nominal-Declaration-Dependency.mjs" ^
    "%Work%\Generic-Nominal-Declaration-Dependency.wvss" ^
    "%Work%\Generic-Nominal-Declaration-Dependency.wvca" ^
    "%Work%\Generic-Nominal-Declaration-Dependency.wvlb" ^
    "%Work%\Generic-Nominal-Declaration-Dependency.wvir" ^
    "%Work%\Generic-Nominal-Declaration-Dependency.wvb" || goto :cleanup
call "%Native%\Run-Wvb.cmd" "%Work%\Generic-Nominal-Declaration-Dependency.wvb" ^
    >"%Work%\Generic-Nominal-Declaration-Dependency-Run.out" ^
    2>"%Work%\Generic-Nominal-Declaration-Dependency-Run.err" || goto :cleanup
for %%F in ("%Work%\Generic-Nominal-Declaration-Dependency-Run.err") do if not "%%~zF"=="0" goto :cleanup
call :expect_result_42 "%Work%\Generic-Nominal-Declaration-Dependency-Run.out" || goto :cleanup
echo PASS  language 1 front door step=generic-nominal-declaration-dependency cases=33 verification=compiler-aligned execution=42 cycle=Rejected
set "FailureStep=compiler-minimum-a"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Minimum-Program.wv" ^
    "%Work%\Minimum-A.wvb" >"%Work%\Compile-A.out" 2>"%Work%\Compile-A.err" || goto :cleanup
set "FailureStep=compiler-minimum-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Minimum-Program.wv" ^
    "%Work%\Minimum-B.wvb" >"%Work%\Compile-B.out" 2>"%Work%\Compile-B.err" || goto :cleanup
for %%F in ("%Work%\Compile-A.err" "%Work%\Compile-B.err") do if not "%%~zF"=="0" goto :cleanup
set "FailureStep=compiler-determinism"
call :compare_split_reports "%Work%\Compile-A.out" "%Work%\Compile-B.out" || goto :cleanup
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
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Unit-Control.wv" ^
    "%Work%\Unit-A.wvb" >"%Work%\Unit-A.out" 2>"%Work%\Unit-A.err" || goto :cleanup
set "FailureStep=compiler-unit-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Unit-Control.wv" ^
    "%Work%\Unit-B.wvb" >"%Work%\Unit-B.out" 2>"%Work%\Unit-B.err" || goto :cleanup
for %%F in ("%Work%\Unit-A.err" "%Work%\Unit-B.err") do if not "%%~zF"=="0" goto :cleanup
set "FailureStep=compiler-unit-determinism"
call :compare_split_reports "%Work%\Unit-A.out" "%Work%\Unit-B.out" || goto :cleanup
fc /b "%Work%\Unit-A.wvb" "%Work%\Unit-B.wvb" >nul || goto :cleanup
type "%Work%\Unit-A.out"
for %%F in ("%Work%\Unit-A.wvb") do echo INFO  language 1 unit wvb-bytes=%%~zF
set "FailureStep=compiler-record-update-a"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Record-Update.wv" ^
    "%Work%\Record-Update-A.wvb" >"%Work%\Record-Update-A.out" 2>"%Work%\Record-Update-A.err" || goto :cleanup
set "FailureStep=compiler-record-update-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Record-Update.wv" ^
    "%Work%\Record-Update-B.wvb" >"%Work%\Record-Update-B.out" 2>"%Work%\Record-Update-B.err" || goto :cleanup
for %%F in ("%Work%\Record-Update-A.err" "%Work%\Record-Update-B.err") do if not "%%~zF"=="0" goto :cleanup
set "FailureStep=compiler-record-update-determinism"
call :compare_split_reports "%Work%\Record-Update-A.out" "%Work%\Record-Update-B.out" || goto :cleanup
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
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Value-Control.wv" ^
    "%Work%\Value-If-A.wvb" >"%Work%\Value-If-A.out" 2>"%Work%\Value-If-A.err" || goto :cleanup
set "FailureStep=compiler-value-if-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Value-Control.wv" ^
    "%Work%\Value-If-B.wvb" >"%Work%\Value-If-B.out" 2>"%Work%\Value-If-B.err" || goto :cleanup
for %%F in ("%Work%\Value-If-A.err" "%Work%\Value-If-B.err") do if not "%%~zF"=="0" goto :cleanup
call :compare_split_reports "%Work%\Value-If-A.out" "%Work%\Value-If-B.out" || goto :cleanup
fc /b "%Work%\Value-If-A.wvb" "%Work%\Value-If-B.wvb" >nul || goto :cleanup
set "FailureStep=compiler-value-if-execution"
call "%Native%\Run-Wvb.cmd" "%Work%\Value-If-A.wvb" >"%Work%\Value-If.out" 2>"%Work%\Value-If.err" || goto :cleanup
for %%F in ("%Work%\Value-If.err") do if not "%%~zF"=="0" goto :cleanup
call :expect_result_42 "%Work%\Value-If.out" || goto :cleanup
set "FailureStep=compiler-value-if-lazy"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
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
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Value-Match.wv" ^
    "%Work%\Value-Match-A.wvb" >"%Work%\Value-Match-A.out" 2>"%Work%\Value-Match-A.err" || goto :cleanup
set "FailureStep=compiler-value-match-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Value-Match.wv" ^
    "%Work%\Value-Match-B.wvb" >"%Work%\Value-Match-B.out" 2>"%Work%\Value-Match-B.err" || goto :cleanup
for %%F in ("%Work%\Value-Match-A.err" "%Work%\Value-Match-B.err") do if not "%%~zF"=="0" goto :cleanup
call :compare_split_reports "%Work%\Value-Match-A.out" "%Work%\Value-Match-B.out" || goto :cleanup
fc /b "%Work%\Value-Match-A.wvb" "%Work%\Value-Match-B.wvb" >nul || goto :cleanup
set "FailureStep=compiler-value-match-execution"
call "%Native%\Run-Wvb.cmd" "%Work%\Value-Match-A.wvb" >"%Work%\Value-Match.out" 2>"%Work%\Value-Match.err" || goto :cleanup
for %%F in ("%Work%\Value-Match.err") do if not "%%~zF"=="0" goto :cleanup
call :expect_result_42 "%Work%\Value-Match.out" || goto :cleanup
set "FailureStep=compiler-value-match-lazy"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Value-Match-Lazy.wv" ^
    "%Work%\Value-Match-Lazy.wvb" >"%Work%\Value-Match-Lazy.out" 2>"%Work%\Value-Match-Lazy.err" || goto :cleanup
for %%F in ("%Work%\Value-Match-Lazy.err") do if not "%%~zF"=="0" goto :cleanup
call "%Native%\Run-Wvb.cmd" "%Work%\Value-Match-Lazy.wvb" >"%Work%\Value-Match-Lazy-Run.out" 2>"%Work%\Value-Match-Lazy-Run.err" || goto :cleanup
for %%F in ("%Work%\Value-Match-Lazy-Run.err") do if not "%%~zF"=="0" goto :cleanup
call :expect_result_42 "%Work%\Value-Match-Lazy-Run.out" || goto :cleanup
set "FailureStep=compiler-value-match-never-a"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Value-Match-Never.wv" ^
    "%Work%\Value-Match-Never-A.wvb" >"%Work%\Value-Match-Never-A.out" 2>"%Work%\Value-Match-Never-A.err" || goto :cleanup
set "FailureStep=compiler-value-match-never-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Value-Match-Never.wv" ^
    "%Work%\Value-Match-Never-B.wvb" >"%Work%\Value-Match-Never-B.out" 2>"%Work%\Value-Match-Never-B.err" || goto :cleanup
for %%F in ("%Work%\Value-Match-Never-A.err" "%Work%\Value-Match-Never-B.err") do if not "%%~zF"=="0" goto :cleanup
call :compare_split_reports "%Work%\Value-Match-Never-A.out" "%Work%\Value-Match-Never-B.out" || goto :cleanup
fc /b "%Work%\Value-Match-Never-A.wvb" "%Work%\Value-Match-Never-B.wvb" >nul || goto :cleanup
set "FailureStep=compiler-value-match-variant-a"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Value-Match-Variant.wv" ^
    "%Work%\Value-Match-Variant-A.wvb" >"%Work%\Value-Match-Variant-A.out" 2>"%Work%\Value-Match-Variant-A.err" || goto :cleanup
set "FailureStep=compiler-value-match-variant-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Value-Match-Variant.wv" ^
    "%Work%\Value-Match-Variant-B.wvb" >"%Work%\Value-Match-Variant-B.out" 2>"%Work%\Value-Match-Variant-B.err" || goto :cleanup
for %%F in ("%Work%\Value-Match-Variant-A.err" "%Work%\Value-Match-Variant-B.err") do if not "%%~zF"=="0" goto :cleanup
call :compare_split_reports "%Work%\Value-Match-Variant-A.out" "%Work%\Value-Match-Variant-B.out" || goto :cleanup
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
echo PASS  language 1 front door phase=compiler-slice item=4/13

set "FailureStep=fixed-integer-compile-a"
echo START language 1 front door phase=fixed-integers item=5/13
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Fixed-Integer-Program.wv" ^
    "%Work%\Fixed-Integer-A.wvb" >"%Work%\Fixed-Integer-A.out" 2>"%Work%\Fixed-Integer-A.err" || goto :cleanup
set "FailureStep=fixed-integer-compile-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Fixed-Integer-Program.wv" ^
    "%Work%\Fixed-Integer-B.wvb" >"%Work%\Fixed-Integer-B.out" 2>"%Work%\Fixed-Integer-B.err" || goto :cleanup
set "FailureStep=fixed-integer-determinism"
for %%F in ("%Work%\Fixed-Integer-A.err" "%Work%\Fixed-Integer-B.err") do if not "%%~zF"=="0" goto :cleanup
call :compare_split_reports "%Work%\Fixed-Integer-A.out" "%Work%\Fixed-Integer-B.out" || goto :cleanup
fc /b "%Work%\Fixed-Integer-A.wvb" "%Work%\Fixed-Integer-B.wvb" >nul || goto :cleanup

set "FailureStep=fixed-integer-trap-inputs"
for %%N in (Overflow Divide-By-Zero Invalid-Shift) do (
    node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
        --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
        --source-profile "%SourceProfile%" ^
        --target-descriptor "%TargetDescriptor%" ^
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
set "FailureStep=memory-budget-entry-verifier"
node "%Native%\Verify-Language-1.0-Memory-Budget-Entry.mjs" ^
    "%Work%\Verifier.exe" "%Work%\Memory-Budget-Entry-A.wvb" ^
    "%Work%\Memory-Budget-Entry-Malformed" || goto :cleanup
echo PASS  language 1 front door step=memory-budget-entry item=verification valid=1 malformed=9
set "FailureStep=compiler-enum-i32-verifier"
"%Work%\Verifier.exe" "%Work%\Enum-I32-Negative-Main.wvb" ^
    >"%Work%\Verify-Enum-I32.out" 2>"%Work%\Verify-Enum-I32.err" || goto :cleanup
for %%F in ("%Work%\Verify-Enum-I32.err") do if not "%%~zF"=="0" goto :cleanup
findstr /c:"wvb status=Valid profile=compiler-aligned" ^
    "%Work%\Verify-Enum-I32.out" >nul || goto :cleanup
echo PASS  language 1 front door step=enum-backing-verifier cases=1 verification=current-native
set "FailureStep=compiler-generic-nominal-variant-verifier"
"%Work%\Verifier.exe" "%Work%\Generic-Nominal-Variant.wvb" ^
    >"%Work%\Verify-Generic-Nominal-Variant.out" ^
    2>"%Work%\Verify-Generic-Nominal-Variant.err" || goto :cleanup
for %%F in ("%Work%\Verify-Generic-Nominal-Variant.err") do if not "%%~zF"=="0" goto :cleanup
findstr /c:"wvb status=Valid profile=compiler-aligned" ^
    "%Work%\Verify-Generic-Nominal-Variant.out" >nul || goto :cleanup
node "%Native%\Verify-Generic-Nominal-Variant.mjs" ^
    "%Work%\Generic-Nominal-Variant.wvss" ^
    "%Work%\Generic-Nominal-Variant.wvca" ^
    "%Work%\Generic-Nominal-Variant.wvlb" ^
    "%Work%\Generic-Nominal-Variant.wvir" ^
    "%Work%\Generic-Nominal-Variant.wvb" || goto :cleanup
echo PASS  language 1 front door step=generic-nominal-variant verification=current-native cases=94
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
echo PASS  language 1 front door phase=fixed-integers item=5/13

set "FailureStep=rune-compile-a"
echo START language 1 front door phase=runes item=6/13
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Rune-Program.wv" ^
    "%Work%\Rune-A.wvb" >"%Work%\Rune-A.out" 2>"%Work%\Rune-A.err" || goto :cleanup
set "FailureStep=rune-compile-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Rune-Program.wv" ^
    "%Work%\Rune-B.wvb" >"%Work%\Rune-B.out" 2>"%Work%\Rune-B.err" || goto :cleanup
set "FailureStep=rune-determinism"
for %%F in ("%Work%\Rune-A.err" "%Work%\Rune-B.err") do if not "%%~zF"=="0" goto :cleanup
call :compare_split_reports "%Work%\Rune-A.out" "%Work%\Rune-B.out" || goto :cleanup
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
echo PASS  language 1 front door phase=runes item=6/13

set "FailureStep=floating-compile-a"
echo START language 1 front door phase=floating item=7/13
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Floating-Program.wv" ^
    "%Work%\Floating-A.wvb" >"%Work%\Floating-A.out" 2>"%Work%\Floating-A.err" || goto :cleanup
set "FailureStep=floating-compile-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Floating-Program.wv" ^
    "%Work%\Floating-B.wvb" >"%Work%\Floating-B.out" 2>"%Work%\Floating-B.err" || goto :cleanup
set "FailureStep=floating-determinism"
for %%F in ("%Work%\Floating-A.err" "%Work%\Floating-B.err") do if not "%%~zF"=="0" goto :cleanup
call :compare_split_reports "%Work%\Floating-A.out" "%Work%\Floating-B.out" || goto :cleanup
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
node "%Native%\Build-Cached-Split-Project-Wvb.mjs" ^
    "%RepositoryRoot%\Projects\Tools\Windvale-Wvb-Runner.wvproj" ^
    "%Work%\Floating-Runner.wvb" ^
    "%Work%\Analyzer.exe" "%Work%\Analyzer.identity" ^
    "%Work%\Emitter.exe" "%Work%\Emitter.identity" >nul || goto :cleanup
set "FailureStep=floating-runner-segmented-package"
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 5 ^
    "%Work%\Floating-Runner.wvb" "%Work%\Floating-Runner.exe" ^
    --development-cache >nul || goto :cleanup
set "FailureStep=compiler-closure-pipeline"
node "%Native%\Verify-Language-1.0-Closure-Compiler-Pipeline.mjs" ^
    "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    "%Work%\Verifier.exe" "%Work%\Floating-Runner.exe" ^
    "%SourceLock%" "%SourceLockHash%" "%SourceProfile%" ^
    "%TargetDescriptor%" "%Work%" ^
    || goto :cleanup
set "FailureStep=memory-budget-entry-runtime"
"%Work%\Floating-Runner.exe" "%Work%\Memory-Budget-Entry-A.wvb" ^
    >"%Work%\Memory-Budget-Entry-Run.out" ^
    2>"%Work%\Memory-Budget-Entry-Run.err" || goto :cleanup
for %%F in ("%Work%\Memory-Budget-Entry-Run.err") do if not "%%~zF"=="0" goto :cleanup
call :expect_result_42 "%Work%\Memory-Budget-Entry-Run.out" || goto :cleanup
echo PASS  language 1 front door step=memory-budget-entry item=runtime transfer=launcher-to-main release=deterministic result=42
set "FailureStep=compiler-enum-u8-runtime"
node "%Native%\Verify-Language-1.0-U8-Enums.mjs" ^
    "%Work%\Verifier.exe" "%Work%\Floating-Runner.exe" ^
    "%Work%\Enum-U8-Used-Main-A.wvb" ^
    "%Work%\Enum-U8-Malformed" || goto :cleanup
echo PASS  language 1 front door step=enum-u8 valid=1 malformed=9 version=1.22 result=42
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

set "FailureStep=compiler-generic-nominal-variant-execution"
"%Work%\Floating-Runner.exe" "%Work%\Generic-Nominal-Variant.wvb" ^
    >"%Work%\Generic-Nominal-Variant-Run.out" ^
    2>"%Work%\Generic-Nominal-Variant-Run.err" || goto :cleanup
for %%F in ("%Work%\Generic-Nominal-Variant-Run.err") do if not "%%~zF"=="0" goto :cleanup
call :expect_result_42 "%Work%\Generic-Nominal-Variant-Run.out" || goto :cleanup
echo PASS  language 1 front door step=generic-nominal-variant cases=97 verification=current-native execution=42 rejections=3

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
echo PASS  language 1 front door phase=floating item=7/13

set "FailureStep=fixed-array-compile-a"
echo START language 1 front door phase=fixed-arrays item=8/13
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Fixed-Array-Main-Pipeline.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Collections\Collections.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" ^
    "%Work%\Fixed-Array-A.wvb" ^
    >"%Work%\Fixed-Array-A.out" 2>"%Work%\Fixed-Array-A.err" || goto :cleanup
set "FailureStep=fixed-array-compile-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Fixed-Array-Main-Pipeline.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Collections\Collections.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" ^
    "%Work%\Fixed-Array-B.wvb" ^
    >"%Work%\Fixed-Array-B.out" 2>"%Work%\Fixed-Array-B.err" || goto :cleanup
set "FailureStep=fixed-array-determinism"
for %%F in ("%Work%\Fixed-Array-A.err" "%Work%\Fixed-Array-B.err") do if not "%%~zF"=="0" goto :cleanup
call :compare_split_reports "%Work%\Fixed-Array-A.out" "%Work%\Fixed-Array-B.out" || goto :cleanup
fc /b "%Work%\Fixed-Array-A.wvb" "%Work%\Fixed-Array-B.wvb" >nul || goto :cleanup
set "FailureStep=fixed-array-verifier-runtime"
node "%Native%\Verify-Language-1.0-Fixed-Arrays.mjs" ^
    "%Work%\Verifier.exe" "%Work%\Floating-Runner.exe" ^
    "%Work%\Fixed-Array-A.wvb" "%Work%" || goto :cleanup
for %%F in ("%Work%\Fixed-Array-A.wvb") do set "FixedArrayWvbBytes=%%~zF"
echo INFO  language 1 fixed-array wvb-bytes=%FixedArrayWvbBytes%
echo PASS  language 1 front door phase=fixed-arrays item=8/13

set "FailureStep=vector-sequence-types-compile"
echo START language 1 front door phase=vector-sequence-types item=9/13
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Vector-Sequence-Wvb-Types.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Collections\Collections.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" ^
    "%Work%\Vector-Sequence-Types.wvb" ^
    >"%Work%\Vector-Sequence-Types.out" ^
    2>"%Work%\Vector-Sequence-Types.err" || goto :cleanup
for %%F in ("%Work%\Vector-Sequence-Types.err") do if not "%%~zF"=="0" goto :cleanup
set "FailureStep=vector-sequence-types-verifier-runtime"
node "%Native%\Verify-Language-1.0-Vector-Sequence-Types.mjs" ^
    "%Work%\Verifier.exe" "%Work%\Floating-Runner.exe" ^
    "%Work%\Vector-Sequence-Types.wvb" "%Work%" || goto :cleanup
set "FailureStep=vector-sequence-runtime"
node "%Native%\Verify-Language-1.0-Vector-Sequence-Runtime.mjs" ^
    "%Work%\Verifier.exe" "%Work%\Floating-Runner.exe" ^
    "%Work%\Vector-Sequence-Types.wvb" "%Work%" || goto :cleanup
set "FailureStep=sequence-reads-compile"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Sequence-Read-Main-Pipeline.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Collections\Collections.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" ^
    "%Work%\Sequence-Read.wvb" ^
    >"%Work%\Sequence-Read.out" 2>"%Work%\Sequence-Read.err" || goto :cleanup
for %%F in ("%Work%\Sequence-Read.err") do if not "%%~zF"=="0" goto :cleanup
set "FailureStep=sequence-reads-verifier-runtime"
node "%Native%\Verify-Language-1.0-Sequence-Reads.mjs" ^
    "%Work%\Verifier.exe" "%Work%\Floating-Runner.exe" ^
    "%Work%\Sequence-Read.wvb" "%Work%" || goto :cleanup
set "FailureStep=vector-reads-freeze-compile"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Vector-Read-Freeze-Main-Pipeline.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Collections\Collections.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" ^
    "%Work%\Vector-Read-Freeze.wvb" ^
    >"%Work%\Vector-Read-Freeze.out" 2>"%Work%\Vector-Read-Freeze.err" || goto :cleanup
for %%F in ("%Work%\Vector-Read-Freeze.err") do if not "%%~zF"=="0" goto :cleanup
set "FailureStep=vector-reads-freeze-verifier-runtime"
node "%Native%\Verify-Language-1.0-Vector-Reads-Freeze.mjs" ^
    "%Work%\Verifier.exe" "%Work%\Floating-Runner.exe" ^
    "%Work%\Vector-Read-Freeze.wvb" "%Work%" || goto :cleanup
set "FailureStep=sequence-reads-rejections"
call :expect_profiled_analysis_failure_with_dependencies ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Sequence-Read-Wrong-Owner.wv" ^
    "Sequence-Read-Wrong-Owner" "Invalidˉcollection" ^
    "%RepositoryRoot%\Libraries\Foundation\Collections\Collections.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" || goto :cleanup
call :expect_profiled_analysis_failure_with_dependencies ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Sequence-Read-Wrong-Index.wv" ^
    "Sequence-Read-Wrong-Index" "Invalidˉargument" ^
    "%RepositoryRoot%\Libraries\Foundation\Collections\Collections.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" || goto :cleanup
call :expect_profiled_analysis_failure_with_dependencies ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Sequence-Read-Unsupported-Element.wv" ^
    "Sequence-Read-Unsupported-Element" "Invalidˉcollection" ^
    "%RepositoryRoot%\Libraries\Foundation\Collections\Collections.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" || goto :cleanup
call :expect_profiled_analysis_failure_with_dependencies ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Sequence-Read-Lookalike.wv" ^
    "Sequence-Read-Lookalike" "Invalidˉargument" ^
    "%RepositoryRoot%\Libraries\Foundation\Collections\Collections.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Sequence-Read-Lookalike-Module.wv" || goto :cleanup
set "FailureStep=vector-reads-freeze-rejections"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Vector-Freeze-Use-After.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Collections\Collections.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" ^
    "%Work%\Vector-Freeze-Use-After.wvb" ^
    >"%Work%\Vector-Freeze-Use-After.out" 2>"%Work%\Vector-Freeze-Use-After.err"
if not errorlevel 1 goto :cleanup
if exist "%Work%\Vector-Freeze-Use-After.wvb" goto :cleanup
set "VectorFreezeUseAfterLine="
set /a VectorFreezeUseAfterLines=0
for /f "usebackq delims=" %%L in ("%Work%\Vector-Freeze-Use-After.err") do (
    set "VectorFreezeUseAfterLine=%%L"
    set /a VectorFreezeUseAfterLines+=1 >nul
)
if not "%VectorFreezeUseAfterLines%"=="1" goto :cleanup
if not "%VectorFreezeUseAfterLine%"=="source emission status=Invalidˉanalysis analysis-status=Invalidˉwir wvb-status=Sourceˉwir function=0 operation=0 source-line=0" goto :cleanup
echo PASS  language 1 front door step=vector-reads-freeze-rejections item=1/8 case=use-after
call :expect_profiled_analysis_failure_with_dependencies ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Vector-Freeze-Wrong-Borrow.wv" ^
    "Vector-Freeze-Wrong-Borrow" "Invalidˉborrow" ^
    "%RepositoryRoot%\Libraries\Foundation\Collections\Collections.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" || goto :cleanup
echo PASS  language 1 front door step=vector-reads-freeze-rejections item=2/8 case=wrong-borrow
call :expect_profiled_analysis_failure_with_dependencies ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Vector-Read-Parameter.wv" ^
    "Vector-Read-Parameter" "Invalidˉcollection" ^
    "%RepositoryRoot%\Libraries\Foundation\Collections\Collections.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" || goto :cleanup
echo PASS  language 1 front door step=vector-reads-freeze-rejections item=3/8 case=parameter
call :expect_profiled_analysis_failure_with_dependencies ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Vector-Read-Unsupported-Element.wv" ^
    "Vector-Read-Unsupported-Element" "Invalidˉcollection" ^
    "%RepositoryRoot%\Libraries\Foundation\Collections\Collections.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" || goto :cleanup
echo PASS  language 1 front door step=vector-reads-freeze-rejections item=4/8 case=unsupported-element
call :expect_profiled_analysis_failure_with_dependencies ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Vector-Read-Wrong-Borrow.wv" ^
    "Vector-Read-Wrong-Borrow" "Invalidˉborrow" ^
    "%RepositoryRoot%\Libraries\Foundation\Collections\Collections.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" || goto :cleanup
echo PASS  language 1 front door step=vector-reads-freeze-rejections item=5/8 case=read-wrong-borrow
call :expect_profiled_analysis_failure_with_dependencies ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Vector-Freeze-Inferred-Result.wv" ^
    "Vector-Freeze-Inferred-Result" "Invalidˉcollection" ^
    "%RepositoryRoot%\Libraries\Foundation\Collections\Collections.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" || goto :cleanup
echo PASS  language 1 front door step=vector-reads-freeze-rejections item=6/8 case=inferred-result
call :expect_profiled_analysis_failure_with_dependencies ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Vector-Freeze-Mismatched-Result.wv" ^
    "Vector-Freeze-Mismatched-Result" "Invalidˉcollection" ^
    "%RepositoryRoot%\Libraries\Foundation\Collections\Collections.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" || goto :cleanup
echo PASS  language 1 front door step=vector-reads-freeze-rejections item=7/8 case=mismatched-result
call :expect_profiled_analysis_failure_with_dependencies ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Vector-Freeze-Mismatched-Argument.wv" ^
    "Vector-Freeze-Mismatched-Argument" "Invalidˉcollection" ^
    "%RepositoryRoot%\Libraries\Foundation\Collections\Collections.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Memory\Memory.wv" || goto :cleanup
echo PASS  language 1 front door step=vector-reads-freeze-rejections item=8/8 case=mismatched-argument
for %%F in ("%Work%\Vector-Sequence-Types.wvb") do set "VectorSequenceTypesWvbBytes=%%~zF"
for %%F in ("%Work%\Sequence-Read.wvb") do set "SequenceReadWvbBytes=%%~zF"
for %%F in ("%Work%\Vector-Read-Freeze.wvb") do set "VectorReadFreezeWvbBytes=%%~zF"
echo INFO  language 1 vector-sequence types wvb-bytes=%VectorSequenceTypesWvbBytes%
echo INFO  language 1 sequence reads wvb-bytes=%SequenceReadWvbBytes% cases=10
echo INFO  language 1 vector reads and freeze wvb-bytes=%VectorReadFreezeWvbBytes% cases=19
echo PASS  language 1 front door phase=vector-sequence-types item=9/13

set "FailureStep=unit-never-compile-a"
echo START language 1 front door phase=unit-never item=10/13
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Never-Control.wv" ^
    "%Work%\Never-A.wvb" >"%Work%\Never-A.out" 2>"%Work%\Never-A.err" || goto :cleanup
set "FailureStep=unit-never-compile-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Never-Control.wv" ^
    "%Work%\Never-B.wvb" >"%Work%\Never-B.out" 2>"%Work%\Never-B.err" || goto :cleanup
set "FailureStep=unit-never-determinism"
for %%F in ("%Work%\Never-A.err" "%Work%\Never-B.err") do if not "%%~zF"=="0" goto :cleanup
call :compare_split_reports "%Work%\Never-A.out" "%Work%\Never-B.out" || goto :cleanup
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
echo PASS  language 1 front door phase=unit-never item=10/13

set "FailureStep=multi-field-variant-compile-a"
echo START language 1 front door phase=multi-field-variants item=11/13
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Multi-Field-Variant.wv" ^
    "%Work%\Multi-Field-Variant-A.wvb" >"%Work%\Multi-Field-Variant-A.out" 2>"%Work%\Multi-Field-Variant-A.err" || goto :cleanup
set "FailureStep=multi-field-variant-compile-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Multi-Field-Variant.wv" ^
    "%Work%\Multi-Field-Variant-B.wvb" >"%Work%\Multi-Field-Variant-B.out" 2>"%Work%\Multi-Field-Variant-B.err" || goto :cleanup
set "FailureStep=multi-field-variant-determinism"
for %%F in ("%Work%\Multi-Field-Variant-A.err" "%Work%\Multi-Field-Variant-B.err") do if not "%%~zF"=="0" goto :cleanup
call :compare_split_reports "%Work%\Multi-Field-Variant-A.out" "%Work%\Multi-Field-Variant-B.out" || goto :cleanup
fc /b "%Work%\Multi-Field-Variant-A.wvb" "%Work%\Multi-Field-Variant-B.wvb" >nul || goto :cleanup

set "FailureStep=multi-field-variant-source-rejections"
for %%N in (Duplicate-Declaration Empty-Payload Missing-Field Duplicate-Field Unknown-Field Type-Mismatch Pattern-Missing-Field Pattern-Duplicate-Field Pattern-Unknown-Field) do (
    call :expect_rejection "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Multi-Field-Variant-%%N.wv" "%Work%\Multi-Field-Variant-%%N.wvb" || goto :cleanup
)

set "FailureStep=named-single-field-variant"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
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
echo PASS  language 1 front door phase=multi-field-variants item=11/13

set "FailureStep=typed-failure-compile-a"
echo START language 1 front door phase=typed-failure item=12/13
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Result-Try.wv" ^
    "%Work%\Result-Try-A.wvb" >"%Work%\Result-Try-A.out" 2>"%Work%\Result-Try-A.err" || goto :cleanup
set "FailureStep=typed-failure-compile-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Result-Try.wv" ^
    "%Work%\Result-Try-B.wvb" >"%Work%\Result-Try-B.out" 2>"%Work%\Result-Try-B.err" || goto :cleanup
set "FailureStep=typed-failure-determinism"
for %%F in ("%Work%\Result-Try-A.err" "%Work%\Result-Try-B.err") do if not "%%~zF"=="0" goto :cleanup
call :compare_split_reports "%Work%\Result-Try-A.out" "%Work%\Result-Try-B.out" || goto :cleanup
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
echo PASS  language 1 front door phase=typed-failure item=12/13

set "FailureStep=foundation-generics-compile-a"
echo START language 1 front door phase=foundation-generics item=13/13
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Foundation-Generic-Result.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Values\Option.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Values\Result.wv" ^
    "%Work%\Foundation-Generic-A.wvb" ^
    >"%Work%\Foundation-Generic-A.out" 2>"%Work%\Foundation-Generic-A.err" || goto :cleanup
set "FailureStep=foundation-generics-compile-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Foundation-Generic-Result.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Values\Option.wv" ^
    "%RepositoryRoot%\Libraries\Foundation\Values\Result.wv" ^
    "%Work%\Foundation-Generic-B.wvb" ^
    >"%Work%\Foundation-Generic-B.out" 2>"%Work%\Foundation-Generic-B.err" || goto :cleanup
set "FailureStep=foundation-generics-determinism"
for %%F in ("%Work%\Foundation-Generic-A.err" "%Work%\Foundation-Generic-B.err") do if not "%%~zF"=="0" goto :cleanup
call :compare_split_reports "%Work%\Foundation-Generic-A.out" "%Work%\Foundation-Generic-B.out" || goto :cleanup
fc /b "%Work%\Foundation-Generic-A.wvb" "%Work%\Foundation-Generic-B.wvb" >nul || goto :cleanup

set "FailureStep=foundation-generics-source-rejections"
for %%N in (Result-Wrong-Arity Result-Extra-Argument Result-Bare Result-Inferred-Construction Try-Wrong-Error) do (
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
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Generic-Multiple-Specializations-Executable.wv" ^
    "%Work%\Generic-Specializations-A.wvb" ^
    >"%Work%\Generic-Specializations-A.out" 2>"%Work%\Generic-Specializations-A.err" || goto :cleanup
set "FailureStep=generic-specializations-compile-b"
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Generic-Multiple-Specializations-Executable.wv" ^
    "%Work%\Generic-Specializations-B.wvb" ^
    >"%Work%\Generic-Specializations-B.out" 2>"%Work%\Generic-Specializations-B.err" || goto :cleanup
set "FailureStep=generic-specializations-determinism"
for %%F in ("%Work%\Generic-Specializations-A.err" "%Work%\Generic-Specializations-B.err") do if not "%%~zF"=="0" goto :cleanup
call :compare_split_reports "%Work%\Generic-Specializations-A.out" "%Work%\Generic-Specializations-B.out" || goto :cleanup
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
echo PASS  language 1 front door phase=foundation-generics item=13/13
set "Result=0"

:cleanup
for %%R in ("%Work%") do set "ResolvedWork=%%~fR"
echo(%ResolvedWork%| findstr /b /i /c:"%TemporaryRoot%\windvale-language-1-front-door-" >nul || exit /b 1
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
    if exist "%Work%\Fixed-Array-A.err" type "%Work%\Fixed-Array-A.err" >&2
    if exist "%Work%\Fixed-Array-B.err" type "%Work%\Fixed-Array-B.err" >&2
    if exist "%Work%\Vector-Sequence-Types.out" type "%Work%\Vector-Sequence-Types.out" >&2
    if exist "%Work%\Vector-Sequence-Types.err" type "%Work%\Vector-Sequence-Types.err" >&2
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
    if exist "%Work%\Generic-Type-Catalog-Package.out" type "%Work%\Generic-Type-Catalog-Package.out" >&2
    if exist "%Work%\Generic-Type-Catalog-Package.err" type "%Work%\Generic-Type-Catalog-Package.err" >&2
    if exist "%Work%\Generic-Type-Catalog.out" type "%Work%\Generic-Type-Catalog.out" >&2
    if exist "%Work%\Generic-Type-Catalog.err" type "%Work%\Generic-Type-Catalog.err" >&2
    if exist "%Work%\Generic-Nominal-Main.out" type "%Work%\Generic-Nominal-Main.out" >&2
    if exist "%Work%\Generic-Nominal-Main.err" type "%Work%\Generic-Nominal-Main.err" >&2
    if exist "%Work%\Generic-Wir-Package.out" type "%Work%\Generic-Wir-Package.out" >&2
    if exist "%Work%\Generic-Wir-Package.err" type "%Work%\Generic-Wir-Package.err" >&2
    if exist "%Work%\Generic-Wir.out" type "%Work%\Generic-Wir.out" >&2
    if exist "%Work%\Generic-Wir.err" type "%Work%\Generic-Wir.err" >&2
    if exist "%Work%\Vector-Construct-Reserved-Admission.out" type "%Work%\Vector-Construct-Reserved-Admission.out" >&2
    if exist "%Work%\Vector-Construct-Reserved-Admission.err" type "%Work%\Vector-Construct-Reserved-Admission.err" >&2
    if exist "%Work%\Vector-Construct-Reserved-Analysis.out" type "%Work%\Vector-Construct-Reserved-Analysis.out" >&2
    if exist "%Work%\Vector-Construct-Reserved-Analysis.err" type "%Work%\Vector-Construct-Reserved-Analysis.err" >&2
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
echo native language 1 front door status=Passed cases=482 frozen-inputs=251 source-fixtures=114 descriptor-cases=33 profile-cases=4 value-front-end-cases=39 generic-front-end-cases=4 generic-resolution-cases=1 generic-type-catalog-cases=1 incremental-generic-wir-cases=3 generic-specialization-cases=4 generic-nominal-pipeline-cases=26 generic-nominal-function-body-cases=33 generic-nominal-declaration-dependency-cases=33 generic-nominal-variant-cases=97 compiler-cases=36 closure-compiler-pipeline-cases=5 enum-cases=20 borrow-cases=14 memory-budget-entry-cases=12 memory-budget-split-cases=13 vector-construct-reserved-cases=16 fixed-integer-cases=22 rune-cases=20 floating-cases=27 fixed-array-cases=6 vector-sequence-type-cases=6 vector-sequence-runtime-cases=12 sequence-read-cases=10 vector-read-freeze-cases=19 unit-never-cases=21 multi-field-variant-cases=25 typed-failure-cases=5 foundation-generic-cases=6 compiler-result=42 compiler-wvb-bytes=221 memory-budget-entry-wvb-bytes=%MemoryBudgetEntryWvbBytes% enum-dead-type-wvb-bytes=%EnumDeadTypeWvbBytes% enum-u8-wvb-bytes=%EnumU8WvbBytes% generic-type-catalog-wvb-bytes=%GenericTypeCatalogWvbBytes% generic-nominal-variant-wvb-bytes=%GenericNominalVariantWvbBytes% value-if-wvb-bytes=%ValueIfWvbBytes% value-match-wvb-bytes=%ValueMatchWvbBytes% value-match-never-wvb-bytes=%ValueMatchNeverWvbBytes% unit-wvb-bytes=%UnitWvbBytes% never-wvb-bytes=%NeverWvbBytes% record-update-wvb-bytes=1116 enum-i32-wvb-bytes=%EnumI32WvbBytes% fixed-integer-wvb-bytes=5335 rune-wvb-bytes=%RuneWvbBytes% floating-wvb-bytes=%FloatingWvbBytes% fixed-array-wvb-bytes=%FixedArrayWvbBytes% vector-sequence-type-wvb-bytes=%VectorSequenceTypesWvbBytes% vector-sequence-runtime-wvb-bytes=1156 sequence-read-wvb-bytes=%SequenceReadWvbBytes% vector-read-freeze-wvb-bytes=%VectorReadFreezeWvbBytes% multi-field-variant-wvb-bytes=%MultiFieldVariantWvbBytes% typed-failure-wvb-bytes=%ResultTryWvbBytes% foundation-generic-wvb-bytes=%FoundationGenericWvbBytes% generic-specializations-wvb-bytes=%GenericSpecializationsWvbBytes%
exit /b 0

:analyze_authenticated
setlocal EnableDelayedExpansion
set "Admitted=%~f1"
set "AnalysisSource=%~f2"
set "RemoveAnalysisSource=0"
if /I "!Admitted!"=="!AnalysisSource!" (
    set "AnalysisSource=%~f2.republished"
    set "RemoveAnalysisSource=1"
)
"%Work%\Analyzer.exe" --internal-source-set ^
    "!Admitted!" "!AnalysisSource!" "%~f3" "%~f4" "%~f5"
set "AnalysisStatus=!ERRORLEVEL!"
if not "!AnalysisStatus!"=="0" exit /b !AnalysisStatus!
fc /b "!Admitted!" "!AnalysisSource!" >nul || exit /b 1
if "!RemoveAnalysisSource!"=="1" del /q "!AnalysisSource!" || exit /b 1
exit /b 0

:expect_profiled_emission_failure
setlocal EnableDelayedExpansion
node "%Native%\Run-Authenticated-Source-Admission.mjs" ^
    "%Work%\Admitter.exe" "%Work%\Validator.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%~f1" "%Work%\%~2.wvss" ^
    >"%Work%\%~2-admission.out" 2>"%Work%\%~2-admission.err" || exit /b 1
for %%F in ("%Work%\%~2-admission.err") do if not "%%~zF"=="0" exit /b 1
call :analyze_authenticated ^
    "%Work%\%~2.wvss" "%Work%\%~2.wvss" ^
    "%Work%\%~2.wvca" "%Work%\%~2.wvlb" "%Work%\%~2.wvir" ^
    >"%Work%\%~2-analysis.out" 2>"%Work%\%~2-analysis.err" || exit /b 1
for %%F in ("%Work%\%~2-analysis.err") do if not "%%~zF"=="0" exit /b 1
"%Work%\Emitter.exe" ^
    "%Work%\%~2.wvss" "%Work%\%~2.wvca" ^
    "%Work%\%~2.wvlb" "%Work%\%~2.wvir" "%Work%\%~2.wvb" ^
    >"%Work%\%~2-emission.out" 2>"%Work%\%~2-emission.err"
set "EmissionExit=!ERRORLEVEL!"
if not "!EmissionExit!"=="1" exit /b 1
for %%F in ("%Work%\%~2-emission.out") do if not "%%~zF"=="0" exit /b 1
if exist "%Work%\%~2.wvb" exit /b 1
set "EmissionLine="
set /a EmissionLines=0
for /f "usebackq delims=" %%L in ("%Work%\%~2-emission.err") do (
    set "EmissionLine=%%L"
    set /a EmissionLines+=1 >nul
)
if not "!EmissionLines!"=="1" exit /b 1
if not "!EmissionLine!"=="%~3" exit /b 1
exit /b 0

:expect_analysis_failure
setlocal EnableDelayedExpansion
"%Work%\Analyzer.exe" "%~f1" ^
    "%Work%\%~2.wvss" "%Work%\%~2.wvca" ^
    "%Work%\%~2.wvlb" "%Work%\%~2.wvir" ^
    >"%Work%\%~2.out" 2>"%Work%\%~2.err"
set "AnalysisExit=!ERRORLEVEL!"
if not "!AnalysisExit!"=="1" exit /b 1
for %%F in ("%Work%\%~2.out") do if not "%%~zF"=="0" exit /b 1
for %%F in ("%Work%\%~2.wvss" "%Work%\%~2.wvca" "%Work%\%~2.wvlb" "%Work%\%~2.wvir") do if exist "%%~fF" exit /b 1
set "AnalysisLine="
set /a AnalysisLines=0
for /f "usebackq delims=" %%L in ("%Work%\%~2.err") do (
    set "AnalysisLine=%%L"
    set /a AnalysisLines+=1 >nul
)
if not "!AnalysisLines!"=="1" exit /b 1
set "WirStatus=%~3"
set "WirStatus=!WirStatus:ˉ=-!"
node "%Native%\Verify-Source-Analysis-Diagnostic.mjs" "%Work%\%~2.err" wir "!WirStatus!" || exit /b 1
exit /b 0

:expect_profiled_analysis_failure
setlocal EnableDelayedExpansion
node "%Native%\Run-Authenticated-Source-Admission.mjs" ^
    "%Work%\Admitter.exe" "%Work%\Validator.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%~f1" "%Work%\%~2.wvss" ^
    >"%Work%\%~2-admission.out" 2>"%Work%\%~2-admission.err" || exit /b 1
for %%F in ("%Work%\%~2-admission.err") do if not "%%~zF"=="0" exit /b 1
call :analyze_authenticated ^
    "%Work%\%~2.wvss" "%Work%\%~2.wvss" ^
    "%Work%\%~2.wvca" "%Work%\%~2.wvlb" "%Work%\%~2.wvir" ^
    >"%Work%\%~2.out" 2>"%Work%\%~2.err"
set "AnalysisExit=!ERRORLEVEL!"
if not "!AnalysisExit!"=="1" exit /b 1
for %%F in ("%Work%\%~2.out") do if not "%%~zF"=="0" exit /b 1
for %%F in ("%Work%\%~2.wvca" "%Work%\%~2.wvlb" "%Work%\%~2.wvir") do if exist "%%~fF" exit /b 1
set "AnalysisLine="
set /a AnalysisLines=0
for /f "usebackq delims=" %%L in ("%Work%\%~2.err") do (
    set "AnalysisLine=%%L"
    set /a AnalysisLines+=1 >nul
)
if not "!AnalysisLines!"=="1" exit /b 1
set "WirStatus=%~3"
set "WirStatus=!WirStatus:ˉ=-!"
node "%Native%\Verify-Source-Analysis-Diagnostic.mjs" "%Work%\%~2.err" wir "!WirStatus!" || exit /b 1
exit /b 0

:expect_profiled_analysis_failure_with_dependencies
setlocal EnableDelayedExpansion
if "%~5"=="" (
    node "%Native%\Run-Authenticated-Source-Admission.mjs" ^
    "%Work%\Admitter.exe" "%Work%\Validator.exe" ^
        --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
        --source-profile "%SourceProfile%" ^
        --target-descriptor "%TargetDescriptor%" ^
        "%~f1" "%~f4" "%Work%\%~2.wvss" ^
        >"%Work%\%~2-admission.out" 2>"%Work%\%~2-admission.err" || exit /b 1
) else if "%~6"=="" (
    node "%Native%\Run-Authenticated-Source-Admission.mjs" ^
    "%Work%\Admitter.exe" "%Work%\Validator.exe" ^
        --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
        --source-profile "%SourceProfile%" ^
        --target-descriptor "%TargetDescriptor%" ^
        "%~f1" "%~f4" "%~f5" "%Work%\%~2.wvss" ^
        >"%Work%\%~2-admission.out" 2>"%Work%\%~2-admission.err" || exit /b 1
) else (
    node "%Native%\Run-Authenticated-Source-Admission.mjs" ^
    "%Work%\Admitter.exe" "%Work%\Validator.exe" ^
        --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
        --source-profile "%SourceProfile%" ^
        --target-descriptor "%TargetDescriptor%" ^
        "%~f1" "%~f4" "%~f5" "%~f6" "%Work%\%~2.wvss" ^
        >"%Work%\%~2-admission.out" 2>"%Work%\%~2-admission.err" || exit /b 1
)
for %%F in ("%Work%\%~2-admission.err") do if not "%%~zF"=="0" exit /b 1
call :analyze_authenticated ^
    "%Work%\%~2.wvss" "%Work%\%~2.wvss" ^
    "%Work%\%~2.wvca" "%Work%\%~2.wvlb" "%Work%\%~2.wvir" ^
    >"%Work%\%~2.out" 2>"%Work%\%~2.err"
set "AnalysisExit=!ERRORLEVEL!"
if not "!AnalysisExit!"=="1" exit /b 1
for %%F in ("%Work%\%~2.out") do if not "%%~zF"=="0" exit /b 1
for %%F in ("%Work%\%~2.wvca" "%Work%\%~2.wvlb" "%Work%\%~2.wvir") do if exist "%%~fF" exit /b 1
set "AnalysisLine="
set /a AnalysisLines=0
for /f "usebackq delims=" %%L in ("%Work%\%~2.err") do (
    set "AnalysisLine=%%L"
    set /a AnalysisLines+=1 >nul
)
if not "!AnalysisLines!"=="1" exit /b 1
set "WirStatus=%~3"
set "WirStatus=!WirStatus:ˉ=-!"
node "%Native%\Verify-Source-Analysis-Diagnostic.mjs" "%Work%\%~2.err" wir "!WirStatus!" || exit /b 1
exit /b 0

:expect_profiled_symbol_failure
setlocal EnableDelayedExpansion
node "%Native%\Run-Authenticated-Source-Admission.mjs" ^
    "%Work%\Admitter.exe" "%Work%\Validator.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%~f1" "%Work%\%~2.wvss" ^
    >"%Work%\%~2-admission.out" 2>"%Work%\%~2-admission.err" || exit /b 1
for %%F in ("%Work%\%~2-admission.err") do if not "%%~zF"=="0" exit /b 1
call :analyze_authenticated ^
    "%Work%\%~2.wvss" "%Work%\%~2.wvss" ^
    "%Work%\%~2.wvca" "%Work%\%~2.wvlb" "%Work%\%~2.wvir" ^
    >"%Work%\%~2.out" 2>"%Work%\%~2.err"
set "AnalysisExit=!ERRORLEVEL!"
if not "!AnalysisExit!"=="1" exit /b 1
for %%F in ("%Work%\%~2.out") do if not "%%~zF"=="0" exit /b 1
for %%F in ("%Work%\%~2.wvca" "%Work%\%~2.wvlb" "%Work%\%~2.wvir") do if exist "%%~fF" exit /b 1
set "AnalysisLine="
set /a AnalysisLines=0
for /f "usebackq delims=" %%L in ("%Work%\%~2.err") do (
    set "AnalysisLine=%%L"
    set /a AnalysisLines+=1 >nul
)
if not "!AnalysisLines!"=="1" exit /b 1
set "SymbolStatus=%~3"
set "SymbolStatus=!SymbolStatus:ˉ=-!"
node "%Native%\Verify-Source-Analysis-Diagnostic.mjs" "%Work%\%~2.err" symbols "!SymbolStatus!" || exit /b 1
exit /b 0

:expect_profiled_symbol_failure_with_dependency
setlocal EnableDelayedExpansion
node "%Native%\Run-Authenticated-Source-Admission.mjs" ^
    "%Work%\Admitter.exe" "%Work%\Validator.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%~f1" "%~f4" "%Work%\%~2.wvss" ^
    >"%Work%\%~2-admission.out" 2>"%Work%\%~2-admission.err" || exit /b 1
for %%F in ("%Work%\%~2-admission.err") do if not "%%~zF"=="0" exit /b 1
call :analyze_authenticated ^
    "%Work%\%~2.wvss" "%Work%\%~2.wvss" ^
    "%Work%\%~2.wvca" "%Work%\%~2.wvlb" "%Work%\%~2.wvir" ^
    >"%Work%\%~2.out" 2>"%Work%\%~2.err"
set "AnalysisExit=!ERRORLEVEL!"
if not "!AnalysisExit!"=="1" exit /b 1
for %%F in ("%Work%\%~2.out") do if not "%%~zF"=="0" exit /b 1
for %%F in ("%Work%\%~2.wvca" "%Work%\%~2.wvlb" "%Work%\%~2.wvir") do if exist "%%~fF" exit /b 1
set "AnalysisLine="
set /a AnalysisLines=0
for /f "usebackq delims=" %%L in ("%Work%\%~2.err") do (
    set "AnalysisLine=%%L"
    set /a AnalysisLines+=1 >nul
)
if not "!AnalysisLines!"=="1" exit /b 1
set "SymbolStatus=%~3"
set "SymbolStatus=!SymbolStatus:ˉ=-!"
node "%Native%\Verify-Source-Analysis-Diagnostic.mjs" "%Work%\%~2.err" symbols "!SymbolStatus!" || exit /b 1
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

:compare_split_reports
findstr /V /B /L /C:"INFO  split compiler active " "%~f1" >"%Work%\Deterministic-Left.out"
if errorlevel 2 exit /b 1
findstr /V /B /L /C:"INFO  split compiler active " "%~f2" >"%Work%\Deterministic-Right.out"
if errorlevel 2 exit /b 1
fc /b "%Work%\Deterministic-Left.out" "%Work%\Deterministic-Right.out" >nul
exit /b %ERRORLEVEL%

:expect_rejection
call :expect_rejection_with_digest "%~f1" "%~f2" "%SourceLockHash%" "%SourceProfile%"
exit /b %ERRORLEVEL%

:expect_foundation_generic_rejection
if exist "%~f2" exit /b 1
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" ^
    --source-input-lock "%SourceLock%" "%SourceLockHash%" ^
    --source-profile "%SourceProfile%" ^
    --target-descriptor "%TargetDescriptor%" ^
    "%~f1" "%RepositoryRoot%\Libraries\Foundation\Values\Result.wv" ^
    "%~f2" >"%~f2.out" 2>"%~f2.err"
if not errorlevel 1 exit /b 1
if exist "%~f2" exit /b 1
exit /b 0

:expect_rejection_with_digest
if exist "%~f2" exit /b 1
node "%Native%\Run-Split-Compiler.mjs" "%Work%\Admitter.exe" "%Work%\Validator.exe" "%Work%\Analyzer.exe" "%Work%\Emitter.exe" --source-input-lock "%SourceLock%" "%~3" --source-profile "%~4" --target-descriptor "%TargetDescriptor%" "%~f1" "%~f2" >"%~f2.out" 2>"%~f2.err"
if not errorlevel 1 exit /b 1
if exist "%~f2" exit /b 1
exit /b 0

:expect_runtime_failure
call "%Native%\Run-Wvb.cmd" "%~f1" >"%Work%\Runtime-%~2.out" 2>"%Work%\Runtime-%~2.err"
if not errorlevel 1 exit /b 1
findstr /b /c:"wvb run status=Failed code=%~2 " "%Work%\Runtime-%~2.err" >nul
exit /b %ERRORLEVEL%
