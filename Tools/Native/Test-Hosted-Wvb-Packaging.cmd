@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Hosted-Wvb-Packaging.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Toolset=%RepositoryRoot%\Artifacts\Native-Hosted-Container-Toolset-Candidate"
set "OriginalTemp=%TEMP%"
:allocate
set "TestDirectory=%OriginalTemp%\windvale-native-hosted-package-test-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TestDirectory%" goto :allocate
mkdir "%TestDirectory%" || exit /b 1
set "TEMP=%TestDirectory%"
set "Result=1"

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" 1 ^
    "%Toolset%\Wvb\wvhostcontrol.wvb" "%TestDirectory%\Valid.exe" ^
    >"%TestDirectory%\Valid.out" 2>"%TestDirectory%\Valid.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Valid.err" "valid packaging wrote a diagnostic"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Valid.exe" 236032 2aa9fa9ea5d6d7c4860d65734de0fe538877700b9fcea5f6b8ca616e5cb2cd2f "valid package"
if errorlevel 1 goto :failed
fc /b "%TestDirectory%\Valid.exe" "%Toolset%\windows-x64\wvhostcontrol.exe" >nul
if errorlevel 1 (
    >&2 echo FAIL  hosted packaging: valid package differs from the candidate
    goto :failed
)
call :check_no_scratch
if errorlevel 1 goto :failed
echo PASS  hosted packaging exact Windows application

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" 1 ^
    "%Toolset%\Wvb\wvhostcontrol.wvb" "%TestDirectory%\Cross-Target.elf" linux ^
    >"%TestDirectory%\Cross-Target.out" 2>"%TestDirectory%\Cross-Target.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Cross-Target.err" "cross-target packaging wrote a diagnostic"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Cross-Target.elf" 237568 0bb6d21a928a880bfebdb522918d8aaa147bb3849f42b2c519bbdf2f7ca4a1d0 "cross-target Linux package"
if errorlevel 1 goto :failed
fc /b "%TestDirectory%\Cross-Target.elf" "%Toolset%\linux-x64\wvhostcontrol.elf" >nul
if errorlevel 1 (
    >&2 echo FAIL  hosted packaging: cross-target Linux package differs from the candidate
    goto :failed
)
call :check_no_scratch
if errorlevel 1 goto :failed
echo PASS  hosted packaging exact cross-target Linux application

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" 1 ^
    "%Toolset%\Wvb\wvhostverifierrequest.wvb" "%TestDirectory%\Verifier-Request.exe" ^
    >"%TestDirectory%\Verifier-Request.out" 2>"%TestDirectory%\Verifier-Request.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Verifier-Request.err" "verifier request packaging wrote a diagnostic"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Verifier-Request.exe" 201216 9676bdb292ebc08893bec8c677745a6e4676e3726808844205739174b26d2e61 "verifier request package"
if errorlevel 1 goto :failed
fc /b "%TestDirectory%\Verifier-Request.exe" "%Toolset%\windows-x64\wvhostverifierrequest.exe" >nul
if errorlevel 1 (
    >&2 echo FAIL  hosted packaging: verifier request package differs from the candidate
    goto :failed
)
call :check_no_scratch
if errorlevel 1 goto :failed
echo PASS  hosted packaging exact verifier request Windows application

call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" 1 ^
    "%Toolset%\Wvb\wvhostverifierrequest.wvb" "%TestDirectory%\Verifier-Request.elf" linux ^
    >"%TestDirectory%\Verifier-Request-Linux.out" 2>"%TestDirectory%\Verifier-Request-Linux.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Verifier-Request-Linux.err" "cross-target verifier request packaging wrote a diagnostic"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Verifier-Request.elf" 200704 f6b012988daf0c18ed6bb5ac8f2fd410d56aaba2f1559066442d40b118177e78 "cross-target verifier request package"
if errorlevel 1 goto :failed
fc /b "%TestDirectory%\Verifier-Request.elf" "%Toolset%\linux-x64\wvhostverifierrequest.elf" >nul
if errorlevel 1 (
    >&2 echo FAIL  hosted packaging: cross-target verifier request package differs from the candidate
    goto :failed
)
call :check_no_scratch
if errorlevel 1 goto :failed
echo PASS  hosted packaging exact cross-target verifier request Linux application

copy /y "%Toolset%\SHA256SUMS" "%TestDirectory%\Invalid.wvb" >nul || goto :failed
copy /y "%Toolset%\SHA256SUMS" "%TestDirectory%\Destination.exe" >nul || goto :failed
call "%RepositoryRoot%\Tools\Native\Package-Hosted-Wvb.cmd" 1 ^
    "%TestDirectory%\Invalid.wvb" "%TestDirectory%\Destination.exe" ^
    >"%TestDirectory%\Invalid.out" 2>"%TestDirectory%\Invalid.err"
if not errorlevel 1 (
    >&2 echo FAIL  hosted packaging: invalid WVB was accepted
    goto :failed
)
call :check_file "%TestDirectory%\Destination.exe" 6927 7f323dabafff6ef6c158ad1ad45c40474c60c282fda3baba3928b4d7cac8a2e4 "preserved destination"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Invalid.wvb" 6927 7f323dabafff6ef6c158ad1ad45c40474c60c282fda3baba3928b4d7cac8a2e4 "preserved input"
if errorlevel 1 goto :failed
call :check_no_scratch
if errorlevel 1 goto :failed
echo PASS  hosted packaging rejects invalid WVB and preserves resources
set "Result=0"
goto :cleanup

:check_file
if not exist "%~1" (
    >&2 echo FAIL  hosted packaging: missing %~4
    exit /b 1
)
for %%F in ("%~1") do if not "%%~zF"=="%~2" (
    >&2 echo FAIL  hosted packaging: %~4 byte length differs
    exit /b 1
)
certutil -hashfile "%~1" SHA256 | findstr /i /c:"%~3" >nul
if errorlevel 1 (
    >&2 echo FAIL  hosted packaging: %~4 digest differs
    exit /b 1
)
exit /b 0

:check_empty
for %%F in ("%~1") do if not "%%~zF"=="0" (
    >&2 echo FAIL  hosted packaging: %~2
    type "%~1" >&2
    exit /b 1
)
exit /b 0

:check_no_scratch
for /d %%D in ("%TestDirectory%\windvale-native-hosted-package-*") do if exist "%%~fD" (
    >&2 echo FAIL  hosted packaging: private package scratch remains
    exit /b 1
)
exit /b 0

:failed
set "Result=1"

:cleanup
set "TEMP=%OriginalTemp%"
for %%F in (Valid.exe Valid.out Valid.err Cross-Target.elf Cross-Target.out Cross-Target.err Verifier-Request.exe Verifier-Request.elf Verifier-Request.out Verifier-Request.err Verifier-Request-Linux.out Verifier-Request-Linux.err Invalid.wvb Destination.exe Invalid.out Invalid.err) do (
    if exist "%TestDirectory%\%%F" del /f /q "%TestDirectory%\%%F" >nul 2>nul
)
rmdir "%TestDirectory%" >nul 2>nul
if "%Result%"=="0" echo Tests: 5, Passed: 5, Failed: 0
exit /b %Result%
