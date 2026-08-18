@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Language-1.0-Front-Door.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Native=%RepositoryRoot%\Tools\Native"

:allocate
set "Work=%TEMP%\windvale-language-1-front-door-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Result=1"
set "FailureStep=frozen-fixtures"

echo START language 1 front door phase=frozen-fixtures item=1/3
node "%Native%\Verify-Language-1.0-Migration-Fixtures.mjs" || goto :cleanup
echo PASS  language 1 front door phase=frozen-fixtures item=1/3

set "FailureStep=descriptor"
echo START language 1 front door phase=descriptor item=2/3
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
echo PASS  language 1 front door phase=descriptor item=2/3

set "FailureStep=compiler-segmented-cache"
echo START language 1 front door phase=compiler-slice item=3/3
call "%Native%\Build-Cached-Segmented-Project.cmd" ^
    "%RepositoryRoot%\Projects\Examples\Windvale-Compiler.wvproj" ^
    "%RepositoryRoot%\Artifacts\Native-Front-Door\windows-x64\wvbuild.exe" ^
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
    "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Minimum-Program.wv" ^
    "%Work%\Minimum-A.wvb" >"%Work%\Compile-A.out" 2>"%Work%\Compile-A.err" || goto :cleanup
set "FailureStep=compiler-minimum-b"
"%Work%\Compiler.exe" ^
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
set "FailureStep=compiler-negative-unsupported-profile"
call :expect_rejection "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Unsupported-Source-Profile.wv" "%Work%\Unsupported.wvb" || goto :cleanup
set "FailureStep=compiler-negative-missing-profile"
call :expect_rejection "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Missing-Edition-Profile.wv" "%Work%\Missing-Profile.wvb" || goto :cleanup
set "FailureStep=compiler-negative-descriptorless"
call :expect_rejection "%RepositoryRoot%\Tests\Fixtures\Language-1.0\Descriptorless-Edition-Header.wv" "%Work%\Descriptorless.wvb" || goto :cleanup
set "FailureStep=compiler-identity"
for %%F in ("%Work%\Minimum-A.wvb") do if not "%%~zF"=="221" goto :cleanup
certutil -hashfile "%Work%\Minimum-A.wvb" SHA256 | findstr /I /C:"2f080e3bb2b43b3da2da1d3c9aea4b7d3e3e3a23432cc39ed189c553da4e1d2a" >nul || goto :cleanup
echo PASS  language 1 front door phase=compiler-slice item=3/3
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
    if exist "%Work%\Minimum.out" type "%Work%\Minimum.out" >&2
    if exist "%Work%\Minimum.err" type "%Work%\Minimum.err" >&2
)
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
if not "%Result%"=="0" exit /b %Result%
echo native language 1 front door status=Passed cases=7 frozen-inputs=250 source-fixtures=72 descriptor-cases=37 compiler-cases=4 compiler-result=42 compiler-wvb-bytes=221
exit /b 0

:expect_rejection
if exist "%~f2" exit /b 1
"%Work%\Compiler.exe" "%~f1" "%~f2" >"%~f2.out" 2>"%~f2.err"
if not errorlevel 1 exit /b 1
if exist "%~f2" exit /b 1
exit /b 0
