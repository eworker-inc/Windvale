@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Compiler-Source-Sentinel.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Native=%RepositoryRoot%\Tools\Native"
for %%R in ("%TEMP%") do set "TemporaryRoot=%%~fR"
:allocate
set "Work=%TemporaryRoot%\windvale-compiler-source-sentinel-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Result=1"
set "FailureStep=compiler"

echo START compiler source sentinel phase=compiler item=1/5
call "%Native%\Build-Cached-Segmented-Project.cmd" ^
    "%RepositoryRoot%\Projects\Examples\Windvale-Compiler.wvproj" ^
    "%RepositoryRoot%\Artifacts\Native-Compiler-Reconstruction-Candidate\windows-x64\wvbuild.exe" ^
    "%Work%\Compiler.wvb" "%Work%\Compiler-Image" "%Work%\Compiler.wvli" ^
    >"%Work%\Segmented.out" 2>"%Work%\Segmented.err" || goto :cleanup
for %%F in ("%Work%\Segmented.err") do if not "%%~zF"=="0" goto :cleanup
type "%Work%\Segmented.out"
set "CompilerEntry="
set "CompilerFragments="
for /f "tokens=10,12 delims== " %%E in ('findstr /b /c:"native segmented project cache status=" "%Work%\Segmented.out"') do (
    set "CompilerEntry=%%E"
    set "CompilerFragments=%%F"
)
if not defined CompilerEntry goto :cleanup
if not defined CompilerFragments goto :cleanup
call "%Native%\Build-Cached-Hosted-Application.cmd" 1 ^
    "%Work%\Compiler.wvb" "%Work%\Compiler-Image" ^
    %CompilerFragments% %CompilerEntry% "%Work%\Compiler.exe" windows || goto :cleanup
echo PASS  compiler source sentinel phase=compiler item=1/5

set "FailureStep=compile"
echo START compiler source sentinel phase=compile item=2/5
"%Work%\Compiler.exe" ^
    "%RepositoryRoot%\Tests\Fixtures\Source-Wvb\Function-Only.wv" ^
    "%Work%\Sentinel-A.wvb" >"%Work%\Compile-A.out" 2>"%Work%\Compile-A.err" || goto :cleanup
for %%F in ("%Work%\Compile-A.err") do if not "%%~zF"=="0" goto :cleanup
type "%Work%\Compile-A.out"
echo PASS  compiler source sentinel phase=compile item=2/5

set "FailureStep=determinism"
echo START compiler source sentinel phase=determinism item=3/5
"%Work%\Compiler.exe" ^
    "%RepositoryRoot%\Tests\Fixtures\Source-Wvb\Function-Only.wv" ^
    "%Work%\Sentinel-B.wvb" >"%Work%\Compile-B.out" 2>"%Work%\Compile-B.err" || goto :cleanup
for %%F in ("%Work%\Compile-B.err") do if not "%%~zF"=="0" goto :cleanup
fc /b "%Work%\Compile-A.out" "%Work%\Compile-B.out" >nul || goto :cleanup
fc /b "%Work%\Sentinel-A.wvb" "%Work%\Sentinel-B.wvb" >nul || goto :cleanup
for %%F in ("%Work%\Sentinel-A.wvb") do set "WvbBytes=%%~zF"
echo PASS  compiler source sentinel phase=determinism item=3/5 bytes=%WvbBytes%

set "FailureStep=verification"
echo START compiler source sentinel phase=verification item=4/5
call "%Native%\Verify-Wvb.cmd" "%Work%\Sentinel-A.wvb" || goto :cleanup
echo PASS  compiler source sentinel phase=verification item=4/5

set "FailureStep=execution"
echo START compiler source sentinel phase=execution item=5/5
call "%Native%\Run-Wvb.cmd" "%Work%\Sentinel-A.wvb" ^
    >"%Work%\Run.out" 2>"%Work%\Run.err" || goto :cleanup
for %%F in ("%Work%\Run.err") do if not "%%~zF"=="0" goto :cleanup
set "RunLine="
set /a RunLines=0
for /f "usebackq delims=" %%L in ("%Work%\Run.out") do (
    set "RunLine=%%L"
    set /a RunLines+=1
)
if not "%RunLines%"=="1" goto :cleanup
if not "%RunLine%"=="Result: 6" goto :cleanup
type "%Work%\Run.out"
echo PASS  compiler source sentinel phase=execution item=5/5
set "Result=0"

:cleanup
for %%R in ("%Work%") do set "ResolvedWork=%%~fR"
echo(%ResolvedWork%| findstr /b /i /c:"%TemporaryRoot%\windvale-compiler-source-sentinel-" >nul || exit /b 1
if not "%Result%"=="0" (
    >&2 echo FAIL  compiler source sentinel step=%FailureStep%
    if exist "%Work%\Segmented.err" type "%Work%\Segmented.err" >&2
    if exist "%Work%\Compile-A.err" type "%Work%\Compile-A.err" >&2
    if exist "%Work%\Compile-B.err" type "%Work%\Compile-B.err" >&2
    if exist "%Work%\Run.out" type "%Work%\Run.out" >&2
    if exist "%Work%\Run.err" type "%Work%\Run.err" >&2
)
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
if not "%Result%"=="0" exit /b %Result%
echo native compiler source sentinel status=Passed cases=5 source-functions=4 result=6
exit /b 0
