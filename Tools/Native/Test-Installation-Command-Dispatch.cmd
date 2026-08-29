@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Installation-Command-Dispatch.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Native=%RepositoryRoot%\Tools\Native"
:allocate
set "Work=%TEMP%\windvale-command-dispatch-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Result=1"

echo native installation command dispatch step=build-selection-and-bundle-tools item=1/7
call "%Native%\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tools\Windvale-Installation-Command-Resolver.wvproj" ^
    "%Work%\Resolver.wvb" || goto :cleanup
call "%Native%\Package-Hosted-Wvb.cmd" 6 "%Work%\Resolver.wvb" "%Work%\Resolver.exe" windows || goto :cleanup
call "%Native%\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tools\Windvale-Package-Bundle-Writer.wvproj" ^
    "%Work%\Writer.wvb" || goto :cleanup
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 6 "%Work%\Writer.wvb" "%Work%\Writer.exe" || goto :cleanup

echo native installation command dispatch step=build-package-payloads item=2/7 packages=2
call "%Native%\Build-Wvdb-Query-Package.cmd" ^
    "%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvpack" ^
    "%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvlock" ^
    "%Work%\Wvdb-Query.wvb" || goto :cleanup
call "%Native%\Build-Wvb-Inspector-Package.cmd" ^
    "%RepositoryRoot%\Distribution\Applications\Wvb-Inspector\Windvale-Wvb-Inspector.wvpack" ^
    "%RepositoryRoot%\Distribution\Applications\Wvb-Inspector\Windvale-Wvb-Inspector.wvlock" ^
    "%Work%\Wvb-Inspector.wvb" || goto :cleanup

echo native installation command dispatch step=construct-exact-bundles item=3/7 packages=2
node -e "const fs=require('node:fs');const x=fs.readFileSync(process.argv[1],'utf8').replaceAll('\r\n','\n');if(x.includes('\r')||x.includes('\0'))process.exit(1);fs.writeFileSync(process.argv[2],x);" "%RepositoryRoot%\LICENSE.md" "%Work%\LICENSE.md" || goto :cleanup
"%Work%\Writer.exe" ^
    "%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvpack" ^
    "%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvlock" ^
    "%Work%\Wvdb-Query.wvb" "%Work%\LICENSE.md" ^
    "%RepositoryRoot%\Distribution\Applications\Wvdb-Query\Windvale-Wvdb-Query.wvprov" ^
    "%Work%\Wvdb-Query.wvbundle" >nul || goto :cleanup
"%Work%\Writer.exe" ^
    "%RepositoryRoot%\Distribution\Applications\Wvb-Inspector\Windvale-Wvb-Inspector.wvpack" ^
    "%RepositoryRoot%\Distribution\Applications\Wvb-Inspector\Windvale-Wvb-Inspector.wvlock" ^
    "%Work%\Wvb-Inspector.wvb" "%Work%\LICENSE.md" ^
    "%RepositoryRoot%\Distribution\Applications\Wvb-Inspector\Windvale-Wvb-Inspector.wvprov" ^
    "%Work%\Wvb-Inspector.wvbundle" >nul || goto :cleanup

echo native installation command dispatch step=lower-wvdb-host item=4/7
call "%Native%\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Compiler\Windvale-Native-X64-Lowering-Tool.wvproj" ^
    "%Work%\Lowerer.wvb" || goto :cleanup
call "%Native%\Package-Segmented-Compiler-Wvb.cmd" 6 "%Work%\Lowerer.wvb" "%Work%\Lowerer.exe" || goto :cleanup
"%Work%\Lowerer.exe" "%Work%\Wvdb-Query.wvb" "%Work%\Wvdb-Query.wvo" >nul || goto :cleanup

echo native installation command dispatch step=bind-rights-reduced-provider item=5/7 target=windows-x64
call "%Native%\Assemble-Wva.cmd" "%RepositoryRoot%\Runtime\Native\X64-Read-Only-Directory-Host.wva" "%Work%\Directory-Host.wvo" || goto :cleanup
call "%Native%\Assemble-Wva.cmd" "%RepositoryRoot%\Runtime\Native\Windows-X64-Read-Only-Directory.wva" "%Work%\Directory-Windows.wvo" || goto :cleanup
call "%Native%\Link-Wvo.cmd" 0 Directory_host_entry "%Work%\Windows-Image.chunk-0" ^
    "%Work%\Wvdb-Query.wvo" "%Work%\Directory-Host.wvo" "%Work%\Directory-Windows.wvo" >"%Work%\Link.txt" || goto :cleanup

echo native installation command dispatch step=package-exact-host item=6/7 target=windows-x64
call "%Native%\Build-Cached-Hosted-Application.cmd" 6 "%Work%\Wvdb-Query.wvb" ^
    "%Work%\Windows-Image" 1 233760 "%Work%\Wvdb-Query.exe" windows || goto :cleanup

echo native installation command dispatch step=dispatch-and-reject item=7/7 cases=9 executions=2
node "%RepositoryRoot%\Tools\Package\Verify-Installation-Command-Dispatcher.mjs" ^
    "%Work%\Resolver.exe" windows-x64 "%Work%\Wvb-Inspector.wvbundle" ^
    "%Work%\Wvdb-Query.wvbundle" "%Work%\Wvdb-Query.exe" || goto :cleanup
set "Result=0"

:cleanup
for %%R in ("%Work%") do set "ResolvedWork=%%~fR"
echo(%ResolvedWork%| findstr /b /i /c:"%TEMP%\windvale-command-dispatch-" >nul || exit /b 1
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
exit /b %Result%
