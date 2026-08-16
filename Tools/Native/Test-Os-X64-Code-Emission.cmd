@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "DevelopmentTarget="
if "%~1"=="" goto :arguments_ready
if /I not "%~1"=="--development-target" exit /b 64
if "%~2"=="" exit /b 64
if not "%~3"=="" exit /b 64
set "DevelopmentTarget=%~2"
echo(%DevelopmentTarget%| findstr /r /i /x "[a-z0-9][a-z0-9-]*" >nul || exit /b 64

:arguments_ready
set "RepositoryRoot=%~dp0\..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "TargetPlan=%RepositoryRoot%\Tests\Native\Os-X64-Code-Emission-Development-Targets.txt"
if not exist "%TargetPlan%" (
    >&2 echo Missing OS x64 code-emission target manifest.
    exit /b 1
)
set "TargetHeader="
for /f "usebackq delims=" %%H in ("%TargetPlan%") do (
    set "TargetHeader=%%H"
    goto :target_header_read
)

:target_header_read
if not "%TargetHeader%"=="windvale-os-x64-code-emission-development-targets 2" (
    >&2 echo Invalid OS x64 code-emission target manifest.
    exit /b 1
)

:allocate
set "Work=%TEMP%\windvale-os-x64-code-emission-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Result=1"
set "TotalProjects=0"
set "Selected=0"

for /f "usebackq skip=1 tokens=1-14,* delims=|" %%A in ("%TargetPlan%") do (
    set "CaseTarget=%%A"
    set "CaseProject=%%B"
    set "CaseArtifact=%%C"
    set "CaseExpectedExit=%%D"
    set "CaseWvbBytes=%%E"
    set "CaseWvbSha256=%%F"
    set "CaseWvoBytes=%%G"
    set "CaseWvoSha256=%%H"
    set "CaseBinBytes=%%I"
    set "CaseBinSha256=%%J"
    set "CaseWindowsBytes=%%K"
    set "CaseWindowsSha256=%%L"
    set "CaseLinuxBytes=%%M"
    set "CaseLinuxSha256=%%N"
    set "CaseInputs=%%O"
    call :consider_case
    if errorlevel 1 goto :manifest_failed
)

if not "%TotalProjects%"=="56" (
    >&2 echo Invalid OS x64 code-emission target count.
    goto :cleanup
)
if defined DevelopmentTarget if not "%Selected%"=="1" (
    >&2 echo Unknown OS x64 code-emission development target: %DevelopmentTarget%
    set "Result=64"
    goto :cleanup
)
if defined DevelopmentTarget (
    echo native os x64 code emission development status=Passed target=%DevelopmentTarget% projects=1 cases=6 cross-host-images=Verified
) else (
    echo native os x64 code emission status=Passed projects=56 cases=336 local-results=50/51/52/53/54/55/56/57/58/59/60/61/62/63/64/65/66/67/68/69/70/71/72/73/74/75/76/77/78/79/80/81/82/83/84/85/86/87/88/89/90/91/92/93/94/95/96/97/98/99/100/101/102/103/104/105 cross-host-images=Verified source-owned-bytes=33826 relocation-fields=569
)
set "Result=0"
goto :cleanup

:manifest_failed
>&2 echo Invalid OS x64 code-emission target manifest entry: %CaseTarget%
goto :cleanup

:consider_case
set /a TotalProjects+=1 >nul
set /a ManifestExpectedExit=49+TotalProjects >nul
if not "%CaseExpectedExit%"=="%ManifestExpectedExit%" exit /b 1
if defined DevelopmentTarget if /I not "%DevelopmentTarget%"=="%CaseTarget%" exit /b 0
if "%CaseTarget%"=="" exit /b 1
if "%CaseProject%"=="" exit /b 1
if "%CaseArtifact%"=="" exit /b 1
if "%CaseInputs%"=="" exit /b 1
if not exist "%RepositoryRoot%\%CaseProject%" exit /b 1
for %%P in ("%CaseInputs:|=" "%") do if not exist "%RepositoryRoot%\%%~P" exit /b 1
set /a Selected+=1 >nul
echo step=%CaseTarget% item=%TotalProjects%/56
call :run_case
exit /b %ERRORLEVEL%

:run_case
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\%CaseProject%" "%Work%\%CaseArtifact%.wvb" >nul
if errorlevel 1 exit /b 1
call :verify "%Work%\%CaseArtifact%.wvb" "%CaseWvbBytes%" "%CaseWvbSha256%"
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" ^
    "%Work%\%CaseArtifact%.wvb" "%Work%\%CaseArtifact%.wvo" >nul
if errorlevel 1 exit /b 1
call :verify "%Work%\%CaseArtifact%.wvo" "%CaseWvoBytes%" "%CaseWvoSha256%"
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" ^
    0 Main "%Work%\%CaseArtifact%.bin" "%Work%\%CaseArtifact%.wvo" >nul
if errorlevel 1 exit /b 1
call :verify "%Work%\%CaseArtifact%.bin" "%CaseBinBytes%" "%CaseBinSha256%"
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" ^
    windows-x64-console-v1 "%Work%\%CaseArtifact%.bin" 0 ^
    "%Work%\%CaseArtifact%.exe" >nul
if errorlevel 1 exit /b 1
call "%Work%\%CaseArtifact%.exe" >nul
if not "%ERRORLEVEL%"=="%CaseExpectedExit%" exit /b 1
call :verify "%Work%\%CaseArtifact%.exe" "%CaseWindowsBytes%" "%CaseWindowsSha256%"
if errorlevel 1 exit /b 1
call "%RepositoryRoot%\Tools\Native\Package-Console.cmd" ^
    linux-x64-console-v1 "%Work%\%CaseArtifact%.bin" 0 ^
    "%Work%\%CaseArtifact%.elf" >nul
if errorlevel 1 exit /b 1
call :verify "%Work%\%CaseArtifact%.elf" "%CaseLinuxBytes%" "%CaseLinuxSha256%"
exit /b %ERRORLEVEL%

:verify
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /i /x /c:"%~3" >nul
exit /b %ERRORLEVEL%

:cleanup
if exist "%Work%\." rmdir /s /q "%Work%"
exit /b %Result%
