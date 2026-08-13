@echo off
setlocal
if "%WINDVALE_CONTAINMENT_ARGUMENT_COUNT%"=="1" goto one_argument
if "%WINDVALE_CONTAINMENT_ARGUMENT_COUNT%"=="2" goto two_arguments
exit /b 64

:one_argument
"%WINDVALE_CONTAINMENT_CHILD%" "%WINDVALE_CONTAINMENT_ARGUMENT_0%"
set "WindvaleChildExit=%errorlevel%"
goto report

:two_arguments
"%WINDVALE_CONTAINMENT_CHILD%" "%WINDVALE_CONTAINMENT_ARGUMENT_0%" "%WINDVALE_CONTAINMENT_ARGUMENT_1%"
set "WindvaleChildExit=%errorlevel%"

:report
echo windvale-child-exit=%WindvaleChildExit%
exit /b 0
