@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~1"=="" goto :usage
if /I "%~1"=="--task-environment" goto :task_environment
if not "%~3"=="" goto :usage
if not "%~2"=="" if not "%~2"=="--report-steps" goto :usage
set "Mode=ordinary"
set "Module=%~f1"
set "Option=%~2"
goto :validate_module

:task_environment
if "%~9"=="" goto :usage
set "Mode=task-environment"
set "Module=%~f2"
set "TaskContext=%~3"
set "TaskClock=%~4"
set "TaskDeadline=%~5"
set "TaskExpectedRuntime=%~6"
set "TaskAdmittedRuntime=%~7"
set "TaskObservationTick=%~8"
set "TaskObservedRuntime=%~9"
shift /8
if not "%~9"=="" goto :usage

:validate_module
for %%M in ("%Module%") do if /I not "%%~xM"==".wvb" (
    >&2 echo The native runner input must use the .wvb extension.
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Runner=%RepositoryRoot%\Artifacts\Native-Wvb-Runner-Candidate\windows-x64-wvrun.exe"

for %%F in ("%Runner%") do if not "%%~zF"=="10368512" (
    >&2 echo The Windows native WVB runner artifact size is invalid.
    exit /b 1
)
certutil -hashfile "%Runner%" SHA256 | findstr /I /C:"d5743801003ac0c43ce6b5b2b3c4bb195d8334f84f5a7f84c6e1edd04b8cf7a7" >nul
if errorlevel 1 (
    >&2 echo The Windows native WVB runner artifact digest is invalid.
    exit /b 1
)

if "%Mode%"=="task-environment" (
    "%Runner%" --task-environment "%Module%" "%TaskContext%" "%TaskClock%" "%TaskDeadline%" "%TaskExpectedRuntime%" "%TaskAdmittedRuntime%" "%TaskObservationTick%" "%TaskObservedRuntime%"
) else if "%Option%"=="" (
    "%Runner%" "%Module%"
) else (
    "%Runner%" "%Module%" --report-steps
)
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Run-Wvb.cmd ^<module.wvb^> [--report-steps]
>&2 echo        Tools\Native\Run-Wvb.cmd --task-environment ^<module.wvb^> ^<context-generation^> ^<clock-generation^> ^<deadline^> ^<expected-runtime-generation^> ^<admitted-runtime-generation^> ^<observation-tick^> ^<observed-runtime-generation^>
exit /b 64
