@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Hosted-Verifier-Publisher-File-Pipeline.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Construction=%RepositoryRoot%\Artifacts\Native-Hosted-Verifier-Publisher-Construction-Candidate"
set "PublisherTools=%Construction%\windows-x64"
set "VerifierCandidate=%RepositoryRoot%\Artifacts\Native-Hosted-Verifier-Application-Candidate\windows-x64-wvverify.exe"
set "OriginalTemp=%TEMP%"
:allocate
set "TestDirectory=%OriginalTemp%\windvale-publisher-file-test-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TestDirectory%" goto :allocate
mkdir "%TestDirectory%" || exit /b 1
set "TEMP=%TestDirectory%"
set /a Total=0
set /a Passed=0
set "Result=1"

set /a Total+=1
call :check_file "%Construction%\SHA256SUMS" 4420 430645441d930284089684ac125bfefc6d57d5cbd3e26612a951964767bcd6d5 "construction inventory"
if errorlevel 1 goto :failed
for /f "usebackq tokens=1,*" %%H in ("%Construction%\SHA256SUMS") do (
    call :check_digest "%Construction%\%%I" %%H "construction artifact"
    if errorlevel 1 goto :failed
)
call :pass "publisher construction inventory"

set /a Total+=1
call "%RepositoryRoot%\Tools\Native\Construct-Hosted-Verifier-Publisher.cmd" windows "%TestDirectory%\Publisher.exe" >"%TestDirectory%\Windows.out" 2>"%TestDirectory%\Windows.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Windows.err" "Windows construction wrote a diagnostic"
if errorlevel 1 goto :failed
findstr /x /c:"publisher construction status=Valid target=windows bytes=256000" "%TestDirectory%\Windows.out" >nul
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Publisher.exe" 256000 735320b5ff33419d685925044add6f254bf402c0d49fc575c77f6110fac705f6 "Windows publisher"
if errorlevel 1 goto :failed
call :check_no_private_scratch
if errorlevel 1 goto :failed
call :pass "exact Windows publisher construction"

set /a Total+=1
call "%RepositoryRoot%\Tools\Native\Construct-Hosted-Verifier-Publisher.cmd" linux "%TestDirectory%\Publisher.elf" >"%TestDirectory%\Linux.out" 2>"%TestDirectory%\Linux.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Linux.err" "Linux construction wrote a diagnostic"
if errorlevel 1 goto :failed
findstr /x /c:"publisher construction status=Valid target=linux bytes=254917" "%TestDirectory%\Linux.out" >nul
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Publisher.elf" 254917 de4f06f6d837eb58457a31b4757c3410e389ecc3c11fd79daf229dbdeb23e02a "Linux publisher"
if errorlevel 1 goto :failed
call :check_no_private_scratch
if errorlevel 1 goto :failed
call :pass "exact cross-target Linux publisher construction"

copy /y "%Construction%\SHA256SUMS" "%TestDirectory%\Invalid.wvsq" >nul || goto :failed
copy /y "%Construction%\SHA256SUMS" "%TestDirectory%\Sentinel.wvhv" >nul || goto :failed
set /a Total+=1
"%PublisherTools%\wvhostverifierpublisherbasemetadata.exe" 1 3001 "%TestDirectory%\Invalid.wvsq" "%TestDirectory%\Sentinel.wvhv" >"%TestDirectory%\Reject.out" 2>"%TestDirectory%\Reject.err"
if not "%ERRORLEVEL%"=="2" goto :failed
call :check_empty "%TestDirectory%\Reject.out" "metadata rejection wrote standard output"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Reject.err" "metadata rejection wrote a diagnostic"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Invalid.wvsq" 4420 430645441d930284089684ac125bfefc6d57d5cbd3e26612a951964767bcd6d5 "rejected metadata input"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Sentinel.wvhv" 4420 430645441d930284089684ac125bfefc6d57d5cbd3e26612a951964767bcd6d5 "preserved metadata destination"
if errorlevel 1 goto :failed
copy /y "%Construction%\SHA256SUMS" "%TestDirectory%\Sentinel.wvhr" >nul || goto :failed
"%PublisherTools%\wvhostverifierpublisherbaseruntime.exe" "%TestDirectory%\Invalid.wvsq" "%TestDirectory%\Sentinel.wvhr" >"%TestDirectory%\Reject.out" 2>"%TestDirectory%\Reject.err"
if not "%ERRORLEVEL%"=="2" goto :failed
call :check_empty "%TestDirectory%\Reject.out" "runtime rejection wrote standard output"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Reject.err" "runtime rejection wrote a diagnostic"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Sentinel.wvhr" 4420 430645441d930284089684ac125bfefc6d57d5cbd3e26612a951964767bcd6d5 "preserved runtime destination"
if errorlevel 1 goto :failed
call :pass "base tools reject malformed input and preserve destinations"

set /a Total+=1
"%PublisherTools%\wvhostverifierpublisherbasemetadata.exe" 1 3001 "%TestDirectory%\Invalid.wvsq" "%TestDirectory%\Invalid.wvsq" >"%TestDirectory%\Alias.out" 2>"%TestDirectory%\Alias.err"
if not "%ERRORLEVEL%"=="64" goto :failed
"%PublisherTools%\wvhostverifierpublisherbaseruntime.exe" "%TestDirectory%\Invalid.wvsq" "%TestDirectory%\Invalid.wvsq" >>"%TestDirectory%\Alias.out" 2>>"%TestDirectory%\Alias.err"
if not "%ERRORLEVEL%"=="64" goto :failed
call :check_empty "%TestDirectory%\Alias.out" "alias rejection wrote standard output"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Alias.err" "alias rejection wrote a diagnostic"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Invalid.wvsq" 4420 430645441d930284089684ac125bfefc6d57d5cbd3e26612a951964767bcd6d5 "preserved alias input"
if errorlevel 1 goto :failed
call :pass "base tools reject exact path aliases"

set /a Total+=1
call :check_file "%VerifierCandidate%" 1004032 aea110110300870cd4f8e3dfcae98de24d90678dd33bfc8584351f58028ff34a "Windows verifier candidate"
if errorlevel 1 goto :failed
"%TestDirectory%\Publisher.exe" "%VerifierCandidate%" "%TestDirectory%\Installed.exe" >"%TestDirectory%\Execute.out" 2>"%TestDirectory%\Execute.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Execute.err" "constructed publisher execution wrote a diagnostic"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Installed.exe" 1004032 aea110110300870cd4f8e3dfcae98de24d90678dd33bfc8584351f58028ff34a "installed verifier"
if errorlevel 1 goto :failed
fc /b "%VerifierCandidate%" "%TestDirectory%\Installed.exe" >nul
if errorlevel 1 goto :failed
call :check_no_private_scratch
if errorlevel 1 goto :failed
call :pass "constructed current-host publisher execution"
set "Result=0"
goto :cleanup

:pass
set /a Passed+=1
echo PASS  %~1
exit /b 0

:check_file
if not exist "%~1" (
    >&2 echo FAIL  hosted-verifier publisher files: missing %~4
    exit /b 1
)
for %%F in ("%~1") do if not "%%~zF"=="%~2" (
    >&2 echo FAIL  hosted-verifier publisher files: %~4 byte length differs
    exit /b 1
)
call :check_digest "%~1" %~3 "%~4"
exit /b %ERRORLEVEL%

:check_digest
certutil -hashfile "%~1" SHA256 | findstr /i /c:"%~2" >nul
if errorlevel 1 (
    >&2 echo FAIL  hosted-verifier publisher files: %~3 digest differs
    exit /b 1
)
exit /b 0

:check_empty
for %%F in ("%~1") do if not "%%~zF"=="0" (
    >&2 echo FAIL  hosted-verifier publisher files: %~2
    type "%~1" >&2
    exit /b 1
)
exit /b 0

:check_no_private_scratch
for /d %%D in ("%TestDirectory%\windvale-hosted-verifier-publisher-*") do if exist "%%~fD" (
    >&2 echo FAIL  hosted-verifier publisher files: construction scratch remains
    exit /b 1
)
for %%F in ("%TestDirectory%\.wvpublish-*") do if exist "%%~fF" (
    >&2 echo FAIL  hosted-verifier publisher files: publication scratch remains
    exit /b 1
)
exit /b 0

:failed
set "Result=1"
for %%F in (Windows.err Linux.err Reject.err Alias.err Execute.err) do if exist "%TestDirectory%\%%F" (
    for %%S in ("%TestDirectory%\%%F") do if not "%%~zS"=="0" type "%%~fS" >&2
)

:cleanup
set "TEMP=%OriginalTemp%"
del /f /q "%TestDirectory%\*" >nul 2>nul
rmdir "%TestDirectory%" >nul 2>nul
if "%Result%"=="0" echo Tests: %Total%, Passed: %Passed%, Failed: 0
if "%Result%"=="0" exit /b 0
set /a Failed=Total-Passed
>&2 echo Tests: %Total%, Passed: %Passed%, Failed: %Failed%
exit /b 1
