@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Os-Services.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"

:allocate
set "TemporaryDirectory=%TEMP%\windvale-os-services-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1
set "Result=1"

for %%P in (
    Windvale-Os-Resource-Service-Core
    Windvale-Os-Resource-Service-Bridge
    Windvale-Os-Resource-Store-Service
    Windvale-Os-Directory-Service-Core
    Windvale-Os-Directory-Service-Bridge
    Windvale-Os-Directory-Snapshot
    Windvale-Os-Directory-Snapshot-Service
    Windvale-Os-Directory-Snapshot-Bridge
) do (
    call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
        "%RepositoryRoot%\Projects\Operating-System\%%P.wvproj" ^
        "%TemporaryDirectory%\%%P.wvb" >nul
    if errorlevel 1 (
        >&2 echo Native OS service project failed: %%P
        goto :cleanup
    )
)

for %%T in (
    Windvale-Native-Test-Os-Resource-Service
    Windvale-Native-Test-Os-Directory-Service
) do (
    call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
        "%RepositoryRoot%\Projects\Tests\%%T.wvproj" ^
        "%TemporaryDirectory%\%%T.wvb" >nul
    if errorlevel 1 (
        >&2 echo Native OS service behavior project failed: %%T
        goto :cleanup
    )
    call "%RepositoryRoot%\Tools\Native\Run-Wvb.cmd" ^
        "%TemporaryDirectory%\%%T.wvb" ^
        >"%TemporaryDirectory%\%%T.out" 2>"%TemporaryDirectory%\%%T.err"
    if errorlevel 1 (
        >&2 echo Native OS service behavior execution failed: %%T
        goto :cleanup
    )
    call :verify_result "%TemporaryDirectory%\%%T.out" "%%T"
    if errorlevel 1 goto :cleanup
    for %%E in ("%TemporaryDirectory%\%%T.err") do if not "%%~zE"=="0" (
        >&2 echo Native OS service behavior wrote a diagnostic: %%T
        goto :cleanup
    )
)

echo native os services status=Passed projects=8 behavior=2 cases=10
set "Result=0"

:cleanup
if exist "%TemporaryDirectory%\." (
    del /f /q "%TemporaryDirectory%\*" >nul 2>nul
    rmdir "%TemporaryDirectory%" >nul 2>nul
)
exit /b %Result%

:verify_result
set "ActualResult="
set /p "ActualResult=" <"%~1"
if not "%ActualResult%"=="Result: 42" (
    >&2 echo Native OS service behavior result differs: %~2
    type "%~1" >&2
    exit /b 1
)
for %%S in ("%~1") do if not "%%~zS"=="11" (
    >&2 echo Native OS service behavior output framing differs: %~2
    exit /b 1
)
exit /b 0
