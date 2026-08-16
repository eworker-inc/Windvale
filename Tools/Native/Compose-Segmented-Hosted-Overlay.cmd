@echo off
setlocal EnableExtensions DisableDelayedExpansion

if "%~5"=="" goto :usage
if not "%~6"=="" goto :usage
if /I not "%~x2"==".wvli" goto :usage
if /I not "%~x3"==".wvo" goto :usage
if /I not "%~x4"==".wvo" goto :usage

pwsh -NoProfile -File "%~dp0Compose-Segmented-Hosted-Overlay.ps1" ^
    "%~1" "%~f2" "%~f3" "%~f4" "%~5"
exit /b %ERRORLEVEL%

:usage
>&2 echo Usage: Tools\Native\Compose-Segmented-Hosted-Overlay.cmd ^<application-chunk-prefix^> ^<application.wvli^> ^<common-provider.wvo^> ^<platform-provider.wvo^> ^<output-chunk-prefix^>
exit /b 64
