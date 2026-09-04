@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Echo-Command-Launch.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Native=%RepositoryRoot%\Tools\Native"
:allocate
set "Work=%TEMP%\windvale-echo-command-launch-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Result=1"

echo native echo command launch step=build-tools item=1/3
call "%Native%\Build-Current-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tools\Windvale-Installation-Command-Resolver.wvproj" ^
    "%Work%\Resolver.wvb" || goto :cleanup
call "%Native%\Package-Hosted-Wvb.cmd" 6 "%Work%\Resolver.wvb" ^
    "%Work%\Resolver.exe" windows || goto :cleanup
call "%Native%\Build-Current-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tools\Windvale-Package-Bundle-Writer.wvproj" ^
    "%Work%\Writer.wvb" || goto :cleanup
call :verify_file "%Work%\Writer.wvb" 613470 ce17913d57ffab710abc296b1bbbdfc0b25dc3978b1259f3190673fdd9e3e7b1 || goto :cleanup
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 6 "%Work%\Writer.wvb" ^
    "%Work%\Writer.exe" || goto :cleanup
call "%Native%\Build-Current-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tools\Windvale-Package-Bundle-Verifier.wvproj" ^
    "%Work%\Verifier.wvb" || goto :cleanup
call :verify_file "%Work%\Verifier.wvb" 632763 cb8c959e44b24aa380f2a0f6b838d371ed2815d51c586e3e96a36190f52319c7 || goto :cleanup
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 6 "%Work%\Verifier.wvb" ^
    "%Work%\Verifier.exe" || goto :cleanup

echo native echo command launch step=construct-package item=2/3
call "%Native%\Build-Echo-Package.cmd" ^
    "%RepositoryRoot%\Distribution\Applications\Echo\Windvale-Echo.wvpack" ^
    "%RepositoryRoot%\Distribution\Applications\Echo\Windvale-Echo.wvlock" ^
    "%Work%\Echo.wvb" || goto :cleanup
node -e "const fs=require('node:fs');const x=fs.readFileSync(process.argv[1],'utf8').replaceAll('\r\n','\n');if(x.includes('\r')||x.includes('\0'))process.exit(1);fs.writeFileSync(process.argv[2],x);" ^
    "%RepositoryRoot%\LICENSE.md" "%Work%\LICENSE.md" || goto :cleanup
"%Work%\Writer.exe" ^
    "%RepositoryRoot%\Distribution\Applications\Echo\Windvale-Echo.wvpack" ^
    "%RepositoryRoot%\Distribution\Applications\Echo\Windvale-Echo.wvlock" ^
    "%Work%\Echo.wvb" "%Work%\LICENSE.md" ^
    "%RepositoryRoot%\Distribution\Applications\Echo\Windvale-Echo.wvprov" ^
    "%Work%\Echo.wvbundle" >nul || goto :cleanup
node -e "const fs=require('node:fs'),c=require('node:crypto'),p=process.argv[1],b=fs.readFileSync(p),h=c.createHash('sha256').update(b).digest('hex');if(b.length!==17009||h!=='a649a98c6d6f8dd2873f1b5097f74f613f7e3422929ef8838a7f6522bc464a0e')process.exit(1);" ^
    "%Work%\Echo.wvbundle" || goto :cleanup
"%Work%\Verifier.exe" "%Work%\Echo.wvbundle" >"%Work%\Bundle.txt" || goto :cleanup
node -e "const fs=require('node:fs'),x=fs.readFileSync(process.argv[1],'utf8').replaceAll('\r\n','\n');if(x!=='bundle status=Valid bytes=17009 package=windvale.echo version=0.1.0 target=hosted-wvb-v1 items=3 blobs=5 sha256=a649a98c6d6f8dd2873f1b5097f74f613f7e3422929ef8838a7f6522bc464a0e\n')process.exit(1);" ^
    "%Work%\Bundle.txt" || goto :cleanup

echo native echo command launch step=package-and-dispatch item=3/3 cases=10
call "%Native%\Package-Hosted-Wvb.cmd" 6 "%Work%\Echo.wvb" ^
    "%Work%\Echo.exe" windows || goto :cleanup
node "%RepositoryRoot%\Tools\Package\Verify-Echo-Command-Launch.mjs" ^
    "%Work%\Resolver.exe" windows-x64 "%Work%\Echo.wvbundle" ^
    "%Work%\Echo.exe" || goto :cleanup
set "Result=0"

:cleanup
for %%R in ("%Work%") do set "ResolvedWork=%%~fR"
echo(%ResolvedWork%| findstr /b /i /c:"%TEMP%\windvale-echo-command-launch-" >nul || exit /b 1
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
exit /b %Result%

:verify_file
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~3" >nul
exit /b %ERRORLEVEL%
