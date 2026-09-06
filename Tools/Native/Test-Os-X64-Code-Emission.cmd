@echo off
setlocal EnableExtensions DisableDelayedExpansion

set "DevelopmentTarget="
set "DevelopmentAll="
set "DevelopmentCache="
if "%~1"=="" goto :arguments_ready
if /I "%~1"=="--development-all" goto :development_all
if /I not "%~1"=="--development-target" exit /b 64
if "%~2"=="" exit /b 64
if not "%~3"=="" exit /b 64
set "DevelopmentTarget=%~2"
set "DevelopmentCache=1"
echo(%DevelopmentTarget%| findstr /r /i /x "[a-z0-9][a-z0-9-]*" >nul || exit /b 64
goto :arguments_ready

:development_all
if not "%~2"=="" exit /b 64
set "DevelopmentAll=1"
set "DevelopmentCache=1"

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

set "BuildDriverSource=%RepositoryRoot%\Artifacts\Native-Front-Door\windows-x64\wvbuild.exe"
set "WvbPublisherSource=%RepositoryRoot%\Artifacts\Native-Front-Door\windows-x64\wvpublish.exe"
set "LowererSource=%RepositoryRoot%\Artifacts\Native-Wvb-To-Wvo-Candidate\Wvb-To-Wvo.exe"
set "WvoPublisherSource=%RepositoryRoot%\Artifacts\Native-Wvo-Publisher-Candidate\windows-x64-wvopublish.exe"
set "LinkerSource=%RepositoryRoot%\Artifacts\Native-Wv-Linker-Candidate\Wv-Linker.exe"
set "PackagerSource=%RepositoryRoot%\Artifacts\Native-Console-Packager-Candidate\Console-Packager.exe"
set "ConsolePublisherSource=%RepositoryRoot%\Artifacts\Native-Console-Application-Publisher-Candidate\windows-x64-wvappublish.exe"
set "CachedProjectBuilder=%RepositoryRoot%\Tools\Native\Build-Cached-Os-X64-Project-Wvbs.mjs"
set "WorkspacePath=%RepositoryRoot%\Windvale.wvws"
if not exist "%WorkspacePath%" (
    >&2 echo The native workspace marker is missing.
    exit /b 1
)
if defined DevelopmentCache if not exist "%CachedProjectBuilder%" (
    >&2 echo The native project-WVB checkpoint builder is missing.
    exit /b 1
)
fsutil reparsepoint query "%RepositoryRoot%" >nul 2>nul
if not errorlevel 1 (
    >&2 echo The native workspace root must not be a reparse point.
    exit /b 1
)
for /f "delims=" %%L in ('dir /a:l /s /b "%RepositoryRoot%" 2^>nul') do (
    >&2 echo The native workspace must not contain a reparse point: %%L
    exit /b 1
)
set "WorkspaceResource=%WorkspacePath:\=/%"
goto :allocate

:allocate
set "Work=%TEMP%\windvale-os-x64-code-emission-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
set "Result=1"
call :stage_toolchain
if errorlevel 1 (
    >&2 echo The Windows native OS x64 verification toolchain digest is invalid.
    goto :cleanup
)
if not defined DevelopmentCache goto :cache_ready
set "CacheTarget=all"
if defined DevelopmentTarget set "CacheTarget=%DevelopmentTarget%"
node "%CachedProjectBuilder%" "%TargetPlan%" "%Work%" ^
    "%BuildDriver%" "%CacheTarget%"
if not errorlevel 1 goto :cache_ready
set "Result=%ERRORLEVEL%"
>&2 echo The native OS x64 project-WVB checkpoint batch failed.
goto :cleanup

:cache_ready
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
) else if defined DevelopmentAll (
    echo native os x64 code emission development status=Passed target=all projects=56 cases=336 cross-host-images=Verified source-owned-bytes=33826 relocation-fields=569
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
set "CaseProjectPath=%RepositoryRoot%\%CaseProject%"
for %%P in ("%CaseProjectPath%") do set "CaseProjectPath=%%~fP"
set "CaseProjectResource=%CaseProjectPath:\=/%"
set "CandidateWvb=%Work%\%CaseArtifact%.candidate.wvb"
set "CandidateWvbResource=%CandidateWvb:\=/%"
if defined DevelopmentCache goto :build_ready
"%BuildDriver%" --workspace "%WorkspaceResource%" --project ^
    "%CaseProjectResource%" "%CandidateWvbResource%" >nul
if errorlevel 1 exit /b 1
:build_ready
if not exist "%CandidateWvb%" exit /b 1
"%WvbPublisher%" "%CandidateWvb%" "%Work%\%CaseArtifact%.wvb" >nul
if errorlevel 1 exit /b 1
call :verify "%Work%\%CaseArtifact%.wvb" "%CaseWvbBytes%" "%CaseWvbSha256%"
if errorlevel 1 exit /b 1
set "CandidateWvo=%Work%\%CaseArtifact%.candidate.wvo"
"%Lowerer%" "%Work%\%CaseArtifact%.wvb" "%CandidateWvo%" >nul
if errorlevel 1 exit /b 1
"%WvoPublisher%" "%CandidateWvo%" "%Work%\%CaseArtifact%.wvo" >nul
if errorlevel 1 exit /b 1
call :verify "%Work%\%CaseArtifact%.wvo" "%CaseWvoBytes%" "%CaseWvoSha256%"
if errorlevel 1 exit /b 1
"%Linker%" ^
    0 Main "%Work%\%CaseArtifact%.bin" "%Work%\%CaseArtifact%.wvo" >nul
if errorlevel 1 exit /b 1
call :verify "%Work%\%CaseArtifact%.bin" "%CaseBinBytes%" "%CaseBinSha256%"
if errorlevel 1 exit /b 1
set "CandidateExe=%Work%\%CaseArtifact%.candidate.exe"
"%Packager%" ^
    windows-x64-console-v1 "%Work%\%CaseArtifact%.bin" 0 ^
    "%CandidateExe%" >nul
if errorlevel 1 exit /b 1
"%ConsolePublisher%" "%CandidateExe%" "%Work%\%CaseArtifact%.exe" >nul
if errorlevel 1 exit /b 1
call "%Work%\%CaseArtifact%.exe" >nul
if not "%ERRORLEVEL%"=="%CaseExpectedExit%" exit /b 1
call :verify "%Work%\%CaseArtifact%.exe" "%CaseWindowsBytes%" "%CaseWindowsSha256%"
if errorlevel 1 exit /b 1
set "CandidateElf=%Work%\%CaseArtifact%.candidate.elf"
"%Packager%" ^
    linux-x64-console-v1 "%Work%\%CaseArtifact%.bin" 0 ^
    "%CandidateElf%" >nul
if errorlevel 1 exit /b 1
"%ConsolePublisher%" "%CandidateElf%" "%Work%\%CaseArtifact%.elf" >nul
if errorlevel 1 exit /b 1
call :verify "%Work%\%CaseArtifact%.elf" "%CaseLinuxBytes%" "%CaseLinuxSha256%"
exit /b %ERRORLEVEL%

:verify
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /i /x /c:"%~3" >nul
exit /b %ERRORLEVEL%

:verify_digest
if not exist "%~1" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /i /x /c:"%~2" >nul
exit /b %ERRORLEVEL%

:stage_toolchain
copy /b "%BuildDriverSource%" "%Work%\wvbuild.exe" >nul || exit /b 1
copy /b "%WvbPublisherSource%" "%Work%\wvpublish.exe" >nul || exit /b 1
copy /b "%LowererSource%" "%Work%\Wvb-To-Wvo.exe" >nul || exit /b 1
copy /b "%WvoPublisherSource%" "%Work%\wvopublish.exe" >nul || exit /b 1
copy /b "%LinkerSource%" "%Work%\Wv-Linker.exe" >nul || exit /b 1
copy /b "%PackagerSource%" "%Work%\Console-Packager.exe" >nul || exit /b 1
copy /b "%ConsolePublisherSource%" "%Work%\wvappublish.exe" >nul || exit /b 1
set "BuildDriver=%Work%\wvbuild.exe"
set "WvbPublisher=%Work%\wvpublish.exe"
set "Lowerer=%Work%\Wvb-To-Wvo.exe"
set "WvoPublisher=%Work%\wvopublish.exe"
set "Linker=%Work%\Wv-Linker.exe"
set "Packager=%Work%\Console-Packager.exe"
set "ConsolePublisher=%Work%\wvappublish.exe"
call :verify_digest "%BuildDriver%" 65602cd41bd929f9d698d9a4a74f683a8525b7dc2c903a5462e8b22fe1fe34ec || exit /b 1
call :verify_digest "%WvbPublisher%" b9fd1b11bc1e4a726e4a43b16830a9351fe573b30e547ba8d8f6660f688ed421 || exit /b 1
call :verify_digest "%Lowerer%" a46d73ada72fba9561e9db1fcfc5477bf19be2518ad9db2d8487184112923dfd || exit /b 1
call :verify_digest "%WvoPublisher%" 76f632ffa7998a6cce0386456fee98f02cbb5ec424d0d914a7e1f06ff3853910 || exit /b 1
call :verify_digest "%Linker%" f47a952867203fbff53abb131ea155b4fe9e14a8be153cc61c0ca5fd8e4a74e0 || exit /b 1
call :verify_digest "%Packager%" 0dddbe6cfd38c37e3fd5332567b3323480a5548a6fbeb41b6b50aed0e57ac3d2 || exit /b 1
call :verify_digest "%ConsolePublisher%" 0bafe84096859f4b88dc14be92c6cdc5336d791b7c5b0a332dccb76b913dd24e || exit /b 1
exit /b 0

:cleanup
if exist "%Work%\." rmdir /s /q "%Work%"
exit /b %Result%
