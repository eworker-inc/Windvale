@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Seed-Native-Console-Aot.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"

:allocate
set "OutputDirectory=%TEMP%\windvale-seed-native-console-aot-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%OutputDirectory%" goto :allocate
mkdir "%OutputDirectory%" || exit /b 1
set "BuildOutput=%OutputDirectory%\Build.out"
set "BuildError=%OutputDirectory%\Build.err"

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Examples\Seed\Sum-Data.wvproj" ^
    "%OutputDirectory%\Sum-Data.wvb" > "%BuildOutput%" 2> "%BuildError%"
set "Result=%ERRORLEVEL%"
if not "%Result%"=="0" (
    type "%BuildOutput%" >&2
    type "%BuildError%" >&2
    call :cleanup
    exit /b %Result%
)
for %%S in ("%BuildError%") do if not "%%~zS"=="0" (
    >&2 echo The native Seed console AOT input build wrote standard error.
    type "%BuildError%" >&2
    call :cleanup
    exit /b 1
)

pwsh -NoProfile -File "%RepositoryRoot%\Tools\Verify\Verify-Seed-Native-Console-Aot.ps1" -OutputDirectory "%OutputDirectory%"
set "Result=%ERRORLEVEL%"
if not "%Result%"=="0" (
    call :cleanup
    exit /b %Result%
)

call :cleanup
echo Tests: 1, Passed: 1, Failed: 0
exit /b 0

:cleanup
if exist "%OutputDirectory%" rmdir /s /q "%OutputDirectory%" >nul 2>nul
exit /b 0
