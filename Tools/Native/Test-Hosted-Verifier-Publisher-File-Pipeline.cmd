@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Hosted-Verifier-Publisher-File-Pipeline.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Construction=%RepositoryRoot%\Artifacts\Native-Hosted-Verifier-Publisher-Construction-Candidate"
set "AdmissionCandidate=%RepositoryRoot%\Artifacts\Native-Hosted-Verifier-Publisher-Admission-Candidate"
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
set "Phase=initialization"

set /a Total+=1
call :check_file "%Construction%\SHA256SUMS" 4634 aa8002e8689fa910f316466e908631b62b829fb5bf7dd3ed3675d10106ce21b8 "construction inventory"
if errorlevel 1 goto :failed
for /f "usebackq tokens=1,*" %%H in ("%Construction%\SHA256SUMS") do (
    call :check_digest "%Construction%\%%I" %%H "construction artifact"
    if errorlevel 1 goto :failed
)
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Windvale-Native-Hosted-Verifier-Publisher-Application-Tool.wvproj" ^
    "%TestDirectory%\Publisher-Application-Admission-Tool.wvb" ^
    >"%TestDirectory%\Admission-Build.out" 2>"%TestDirectory%\Admission-Build.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Admission-Build.err" "admission source build wrote a diagnostic"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Publisher-Application-Admission-Tool.wvb" 30778 c6ba933fa0ea1068f02235f75ed251655b10b43d64f8984d22b548f01608af0d "native-built publisher admission WVB"
if errorlevel 1 goto :failed
fc /b "%Construction%\Publisher-Application-Admission-Tool.wvb" "%TestDirectory%\Publisher-Application-Admission-Tool.wvb" >nul
if errorlevel 1 goto :failed
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" ^
    "%TestDirectory%\Publisher-Application-Admission-Tool.wvb" ^
    "%TestDirectory%\Publisher-Application-Admission-Tool.wvo" ^
    >"%TestDirectory%\Admission-Lower.out" 2>"%TestDirectory%\Admission-Lower.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Admission-Lower.err" "admission native lowering wrote a diagnostic"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Publisher-Application-Admission-Tool.wvo" 555690 722d819152d8415487c1cf111474fd11dd0ab89a863e33ab84c865a2e3e13771 "native-lowered publisher admission WVO"
if errorlevel 1 goto :failed
fc /b "%Construction%\Publisher-Application-Admission-Tool.wvo" "%TestDirectory%\Publisher-Application-Admission-Tool.wvo" >nul
if errorlevel 1 goto :failed
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

set /a Total+=1
call "%RepositoryRoot%\Tools\Native\Construct-Hosted-Verifier-Publisher-Admitter.cmd" windows "%TestDirectory%\Admitter.exe" >"%TestDirectory%\Admitter-Windows.out" 2>"%TestDirectory%\Admitter-Windows.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Admitter-Windows.err" "Windows admitter construction wrote a diagnostic"
if errorlevel 1 goto :failed
findstr /x /c:"publisher admitter construction status=Valid target=windows bytes=570368" "%TestDirectory%\Admitter-Windows.out" >nul
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Admitter.exe" 570368 7f58a5e321d1b4baa16ba673b3e0e1c21c9acd040cba92dae0f180d629c63e6b "Windows publisher admitter"
if errorlevel 1 goto :failed
fc /b "%AdmissionCandidate%\windows-x64-wvhostverifierpublisheradmit.exe" "%TestDirectory%\Admitter.exe" >nul
if errorlevel 1 goto :failed
call :check_no_private_scratch
if errorlevel 1 goto :failed
call :pass "exact Windows publisher-admitter construction"

set /a Total+=1
call "%RepositoryRoot%\Tools\Native\Construct-Hosted-Verifier-Publisher-Admitter.cmd" linux "%TestDirectory%\Admitter.elf" >"%TestDirectory%\Admitter-Linux.out" 2>"%TestDirectory%\Admitter-Linux.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Admitter-Linux.err" "Linux admitter construction wrote a diagnostic"
if errorlevel 1 goto :failed
findstr /x /c:"publisher admitter construction status=Valid target=linux bytes=569344" "%TestDirectory%\Admitter-Linux.out" >nul
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Admitter.elf" 569344 9bfe16fa751e21a32847f5534eff7de18ba74cfe5b714c63fb6a6589d30d7cad "Linux publisher admitter"
if errorlevel 1 goto :failed
fc /b "%AdmissionCandidate%\linux-x64-wvhostverifierpublisheradmit.elf" "%TestDirectory%\Admitter.elf" >nul
if errorlevel 1 goto :failed
call :check_no_private_scratch
if errorlevel 1 goto :failed
call :pass "exact Linux publisher-admitter construction"

set /a Total+=1
set "Phase=current-host Windows publisher admission"
call "%RepositoryRoot%\Tools\Native\Admit-Hosted-Verifier-Publisher.cmd" windows "%TestDirectory%\Publisher.exe" >"%TestDirectory%\Admit-Windows.out" 2>"%TestDirectory%\Admit-Windows.err"
if errorlevel 1 goto :failed
set "Phase=current-host Windows publisher admission output"
call :check_file "%TestDirectory%\Admit-Windows.out" 58 449d559e4d7f203e2f9d99cffb28144c171559c65344b3cd9335c34ee4be9708 "Windows publisher admission output"
if errorlevel 1 goto :failed
set "Phase=current-host Windows publisher admission diagnostic"
call :check_empty "%TestDirectory%\Admit-Windows.err" "Windows publisher admission wrote a diagnostic"
if errorlevel 1 goto :failed
set "Phase=current-host Linux publisher admission"
call "%RepositoryRoot%\Tools\Native\Admit-Hosted-Verifier-Publisher.cmd" linux "%TestDirectory%\Publisher.elf" >"%TestDirectory%\Admit-Linux.out" 2>"%TestDirectory%\Admit-Linux.err"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Admit-Linux.out" 58 449d559e4d7f203e2f9d99cffb28144c171559c65344b3cd9335c34ee4be9708 "Linux publisher admission output"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Admit-Linux.err" "Linux publisher admission wrote a diagnostic"
if errorlevel 1 goto :failed
set "Phase=publisher target-swap rejection"
call "%RepositoryRoot%\Tools\Native\Admit-Hosted-Verifier-Publisher.cmd" linux "%TestDirectory%\Publisher.exe" >"%TestDirectory%\Admit-Swap.out" 2>"%TestDirectory%\Admit-Swap.err"
if not "%ERRORLEVEL%"=="2" goto :failed
call :check_file "%TestDirectory%\Admit-Swap.err" 61 ffadaf98e0978439eb19a97ccfe2d4c06f810b8c9926d5193eb4827f3c126b89 "target-swap rejection diagnostic"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Admit-Swap.out" "target-swap rejection wrote standard output"
if errorlevel 1 goto :failed
set "Phase=wrong-digest publisher creation"
fsutil file createnew "%TestDirectory%\Wrong-Digest.exe" 256000 >nul 2>nul
if errorlevel 1 goto :failed
set "Phase=wrong-digest publisher rejection"
call "%RepositoryRoot%\Tools\Native\Admit-Hosted-Verifier-Publisher.cmd" windows "%TestDirectory%\Wrong-Digest.exe" >"%TestDirectory%\Admit-Corrupt.out" 2>"%TestDirectory%\Admit-Corrupt.err"
if not "%ERRORLEVEL%"=="2" goto :failed
call :check_empty "%TestDirectory%\Admit-Corrupt.out" "wrong-digest rejection wrote standard output"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Admit-Corrupt.err" 61 ffadaf98e0978439eb19a97ccfe2d4c06f810b8c9926d5193eb4827f3c126b89 "wrong-digest rejection diagnostic"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Wrong-Digest.exe" 256000 24a046dc04fefdb652e4077b41162490b344a4dd45f918505477f84c592f3070 "preserved wrong-digest publisher"
if errorlevel 1 goto :failed
set "Phase=invalid publisher target rejection"
call "%RepositoryRoot%\Tools\Native\Admit-Hosted-Verifier-Publisher.cmd" other "%TestDirectory%\Publisher.exe" >"%TestDirectory%\Admit-Usage.out" 2>"%TestDirectory%\Admit-Usage.err"
if not "%ERRORLEVEL%"=="64" goto :failed
call :check_empty "%TestDirectory%\Admit-Usage.out" "invalid-target usage wrote standard output"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Admit-Usage.err" 103 6c0d4ead9db1e4edfd4f5b85ea1b4f8b2245825c58e2227e286e47faa7857d84 "invalid-target usage diagnostic"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Publisher.exe" 256000 735320b5ff33419d685925044add6f254bf402c0d49fc575c77f6110fac705f6 "preserved Windows publisher subject"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Publisher.elf" 254917 de4f06f6d837eb58457a31b4757c3410e389ecc3c11fd79daf229dbdeb23e02a "preserved Linux publisher subject"
if errorlevel 1 goto :failed
call :check_no_private_scratch
if errorlevel 1 goto :failed
call :pass "current-host publisher admission matrix"

copy /y "%Construction%\SHA256SUMS" "%TestDirectory%\Invalid.wvsq" >nul || goto :failed
copy /y "%Construction%\SHA256SUMS" "%TestDirectory%\Sentinel.wvhv" >nul || goto :failed
set /a Total+=1
"%PublisherTools%\wvhostverifierpublisherbasemetadata.exe" 1 3001 "%TestDirectory%\Invalid.wvsq" "%TestDirectory%\Sentinel.wvhv" >"%TestDirectory%\Reject.out" 2>"%TestDirectory%\Reject.err"
if not "%ERRORLEVEL%"=="2" goto :failed
call :check_empty "%TestDirectory%\Reject.out" "metadata rejection wrote standard output"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Reject.err" "metadata rejection wrote a diagnostic"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Invalid.wvsq" 4634 aa8002e8689fa910f316466e908631b62b829fb5bf7dd3ed3675d10106ce21b8 "rejected metadata input"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Sentinel.wvhv" 4634 aa8002e8689fa910f316466e908631b62b829fb5bf7dd3ed3675d10106ce21b8 "preserved metadata destination"
if errorlevel 1 goto :failed
copy /y "%Construction%\SHA256SUMS" "%TestDirectory%\Sentinel.wvhr" >nul || goto :failed
"%PublisherTools%\wvhostverifierpublisherbaseruntime.exe" "%TestDirectory%\Invalid.wvsq" "%TestDirectory%\Sentinel.wvhr" >"%TestDirectory%\Reject.out" 2>"%TestDirectory%\Reject.err"
if not "%ERRORLEVEL%"=="2" goto :failed
call :check_empty "%TestDirectory%\Reject.out" "runtime rejection wrote standard output"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Reject.err" "runtime rejection wrote a diagnostic"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Sentinel.wvhr" 4634 aa8002e8689fa910f316466e908631b62b829fb5bf7dd3ed3675d10106ce21b8 "preserved runtime destination"
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
call :check_file "%TestDirectory%\Invalid.wvsq" 4634 aa8002e8689fa910f316466e908631b62b829fb5bf7dd3ed3675d10106ce21b8 "preserved alias input"
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
>&2 echo FAIL  hosted-verifier publisher files: %Phase%
for %%F in (Admission-Build.err Admission-Lower.err Windows.err Linux.err Admitter-Windows.err Admitter-Linux.err Admit-Windows.out Admit-Windows.err Admit-Linux.out Admit-Linux.err Admit-Swap.out Admit-Swap.err Admit-Corrupt.out Admit-Corrupt.err Admit-Usage.out Admit-Usage.err Reject.err Alias.err Execute.err) do if exist "%TestDirectory%\%%F" (
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
