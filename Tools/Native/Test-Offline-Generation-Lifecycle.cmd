@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Offline-Generation-Lifecycle.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "Work=%TEMP%\windvale-generation-lifecycle-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Result=1"

echo native offline generation lifecycle step=build-tools item=1/4 tools=2
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tools\Windvale-Installation-Activation-Planner.wvproj" ^
    "%Work%\Planner.wvb" || goto :cleanup
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects\Tools\Windvale-Installation-Command-Resolver.wvproj" ^
    "%Work%\Resolver.wvb" || goto :cleanup

echo native offline generation lifecycle step=package-tools item=2/4 target=windows-x64
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" 6 ^
    "%Work%\Planner.wvb" "%Work%\Planner.exe" windows || goto :cleanup
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" 6 ^
    "%Work%\Resolver.wvb" "%Work%\Resolver.exe" windows || goto :cleanup

echo native offline generation lifecycle step=verify-planner item=3/4 cases=12
node "%RepositoryRoot%\Tools\Package\Verify-Installation-Activation-Planner.mjs" ^
    "%Work%\Planner.exe" || goto :cleanup

echo native offline generation lifecycle step=compose-lifecycle item=4/4 cases=15
node "%RepositoryRoot%\Tools\Package\Verify-Offline-Generation-Lifecycle.mjs" ^
    "%Work%\Planner.exe" "%Work%\Resolver.exe" windows-x64 || goto :cleanup
echo native offline lifecycle composition status=Passed cases=27 planner=12 lifecycle=15 generations=2 activations=3 rollback=Verified uninstall=Verified preservation=2
set "Result=0"

:cleanup
for %%R in ("%Work%") do set "ResolvedWork=%%~fR"
echo(%ResolvedWork%| findstr /b /i /c:"%TEMP%\windvale-generation-lifecycle-" >nul || exit /b 1
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
exit /b %Result%
