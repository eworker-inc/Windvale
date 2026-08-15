@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Installation-Command-Resolution.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Work=%TEMP%\windvale-command-resolution-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :eof
mkdir "%Work%" || exit /b 1
set "Result=1"

echo native installation command resolution step=build item=1/3
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tools\Windvale-Installation-Command-Resolver.wvproj" ^
    "%Work%\Resolver.wvb" || goto :cleanup
call :verify_file "%Work%\Resolver.wvb" 60732 521cd77ee53f20cec3157208e4f0b9c93841c212dcabec88f4e7cbc6a9229679 || goto :cleanup

echo native installation command resolution step=package item=2/3 target=windows-x64
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" 6 ^
    "%Work%\Resolver.wvb" "%Work%\Resolver.exe" windows || goto :cleanup

echo native installation command resolution step=resolve item=3/3 cases=8
node "%RepositoryRoot%\Tools\Package\Verify-Installation-Command-Resolver.mjs" ^
    "%Work%\Resolver.exe" windows-x64 || goto :cleanup
set "Result=0"

:cleanup
for %%R in ("%Work%") do set "ResolvedWork=%%~fR"
echo(%ResolvedWork%| findstr /b /i /c:"%TEMP%\windvale-command-resolution-" >nul || exit /b 1
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
exit /b %Result%

:verify_file
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /i /c:"%~3" >nul || exit /b 1
exit /b 0
