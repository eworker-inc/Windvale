@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Bounded-Parallel-Task-Scheduler.cmd
    exit /b 64
)

node "%~dp0Test-Bounded-Parallel-Task-Scheduler.mjs"
exit /b %ERRORLEVEL%
