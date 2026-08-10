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
set "PromoterCandidate=%RepositoryRoot%\Artifacts\Native-Hosted-Verifier-Publisher-Promoter-Candidate"
set "WvbPublisherCandidate=%RepositoryRoot%\Artifacts\Native-Wvb-Publisher-Candidate"
set "PublisherTools=%Construction%\windows-x64"
set "VerifierCandidate=%RepositoryRoot%\Artifacts\Native-Hosted-Verifier-Application-Candidate\windows-x64-wvverify.exe"
set "PortableWvbCandidate=%RepositoryRoot%\Artifacts\Byte-Construction.wvb"
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
call :check_file "%Construction%\SHA256SUMS" 5064 8b752fd2c1b5afed4935453ee4d1f520d8807d439d7ad339f5f71a5ca30c05b1 "construction inventory"
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
call :check_file "%TestDirectory%\Publisher-Application-Admission-Tool.wvb" 30778 b4e0a2ee04de6cfff0efc723c57031bf5cfcd6706e3156525ce2157c5f287d07 "native-built publisher admission WVB"
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
call :check_file "%TestDirectory%\Publisher-Application-Admission-Tool.wvo" 555690 88cc97665cfd0de14f2c9ac6c80dfd985edc508fccdc3d9b887da740cd034e23 "native-lowered publisher admission WVO"
if errorlevel 1 goto :failed
fc /b "%Construction%\Publisher-Application-Admission-Tool.wvo" "%TestDirectory%\Publisher-Application-Admission-Tool.wvo" >nul
if errorlevel 1 goto :failed
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Windvale-Native-Hosted-Verifier-Publisher-Promoter.wvproj" ^
    "%TestDirectory%\Publisher-Promoter.wvb" ^
    >"%TestDirectory%\Promoter-Build.out" 2>"%TestDirectory%\Promoter-Build.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Promoter-Build.err" "promoter source build wrote a diagnostic"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Publisher-Promoter.wvb" 41268 c0c7c88996ef837bc5a2ec3ceb1de61254b025fbd6504e4f3d7dc055c4140672 "native-built publisher promoter WVB"
if errorlevel 1 goto :failed
fc /b "%Construction%\Publisher-Promoter.wvb" "%TestDirectory%\Publisher-Promoter.wvb" >nul
if errorlevel 1 goto :failed
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" ^
    "%TestDirectory%\Publisher-Promoter.wvb" ^
    "%TestDirectory%\Publisher-Promoter.wvo" ^
    >"%TestDirectory%\Promoter-Lower.out" 2>"%TestDirectory%\Promoter-Lower.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Promoter-Lower.err" "promoter native lowering wrote a diagnostic"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Publisher-Promoter.wvo" 660123 ba5d9c5afde115fede472369d24c3d1fe466806de523773d2e445e6a9e004667 "native-lowered publisher promoter WVO"
if errorlevel 1 goto :failed
fc /b "%Construction%\Publisher-Promoter.wvo" "%TestDirectory%\Publisher-Promoter.wvo" >nul
if errorlevel 1 goto :failed
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%TestDirectory%\Publisher-Promoter.bin" "%TestDirectory%\Publisher-Promoter.wvo" >"%TestDirectory%\Promoter-Link.out" 2>"%TestDirectory%\Promoter-Link.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Promoter-Link.err" "promoter native link wrote a diagnostic"
if errorlevel 1 goto :failed
findstr /b /c:"entry name=Main address=1178" "%TestDirectory%\Promoter-Link.out" >nul
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Publisher-Promoter.bin" 658339 e06189a37c038a5237787ffd16fb53466df3d10519efd4129b219bd814f4def2 "linked publisher promoter fragment"
if errorlevel 1 goto :failed
call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Windvale-Wvb-Publisher.wvproj" ^
    "%TestDirectory%\Wvb-Publisher.wvb" ^
    >"%TestDirectory%\Wvb-Publisher-Build.out" 2>"%TestDirectory%\Wvb-Publisher-Build.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Wvb-Publisher-Build.err" "WVB publisher source build wrote a diagnostic"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Wvb-Publisher.wvb" 159770 8247539e0f4a5436b3902ec1fef33c6c39c231703de7bf505a6c65d66a764f96 "native-built WVB publisher WVB"
if errorlevel 1 goto :failed
fc /b "%Construction%\Wvb-Publisher.wvb" "%TestDirectory%\Wvb-Publisher.wvb" >nul
if errorlevel 1 goto :failed
call "%RepositoryRoot%\Tools\Native\Lower-Wvb-To-Wvo.cmd" ^
    "%TestDirectory%\Wvb-Publisher.wvb" ^
    "%TestDirectory%\Wvb-Publisher.wvo" ^
    >"%TestDirectory%\Wvb-Publisher-Lower.out" 2>"%TestDirectory%\Wvb-Publisher-Lower.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Wvb-Publisher-Lower.err" "WVB publisher native lowering wrote a diagnostic"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Wvb-Publisher.wvo" 1319377 edc49bbae0bfd16a38db4a08d9a6e636edfac35828e1c6b050c45d85d5e1f9e3 "native-lowered WVB publisher WVO"
if errorlevel 1 goto :failed
fc /b "%Construction%\Wvb-Publisher.wvo" "%TestDirectory%\Wvb-Publisher.wvo" >nul
if errorlevel 1 goto :failed
call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main "%TestDirectory%\Wvb-Publisher.bin" "%TestDirectory%\Wvb-Publisher.wvo" >"%TestDirectory%\Wvb-Publisher-Link.out" 2>"%TestDirectory%\Wvb-Publisher-Link.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Wvb-Publisher-Link.err" "WVB publisher native link wrote a diagnostic"
if errorlevel 1 goto :failed
findstr /b /c:"entry name=Main address=0" "%TestDirectory%\Wvb-Publisher-Link.out" >nul
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Wvb-Publisher.bin" 1317613 9003479563a043bb69113be43100289f653f6772356c48a17098c1c6700f5271 "linked WVB publisher fragment"
if errorlevel 1 goto :failed
call :pass "publisher construction inventory"

set /a Total+=1
call "%RepositoryRoot%\Tools\Native\Construct-Hosted-Verifier-Publisher.cmd" windows "%TestDirectory%\Publisher.exe" >"%TestDirectory%\Windows.out" 2>"%TestDirectory%\Windows.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Windows.err" "Windows construction wrote a diagnostic"
if errorlevel 1 goto :failed
findstr /x /c:"publisher construction status=Valid target=windows bytes=256000" "%TestDirectory%\Windows.out" >nul
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Publisher.exe" 256000 17cb5c4228e8448693b17f1b73695fd0ecfd03d7ada922794a5bf3bd7594fc96 "Windows publisher"
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
call :check_file "%TestDirectory%\Publisher.elf" 254917 babe721a573e29f89ec095c35677880077ff465d4e2129063f6742cd47591a97 "Linux publisher"
if errorlevel 1 goto :failed
call :check_no_private_scratch
if errorlevel 1 goto :failed
call :pass "exact cross-target Linux publisher construction"

set /a Total+=1
call "%RepositoryRoot%\Tools\Native\Construct-Hosted-Verifier-Publisher-Promoter.cmd" windows "%TestDirectory%\Promoter.exe" >"%TestDirectory%\Promoter-Windows.out" 2>"%TestDirectory%\Promoter-Windows.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Promoter-Windows.err" "Windows promoter construction wrote a diagnostic"
if errorlevel 1 goto :failed
findstr /x /c:"publisher promoter construction status=Valid target=windows bytes=681472" "%TestDirectory%\Promoter-Windows.out" >nul
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Promoter.exe" 681472 598bd2de8247abd19d931efa1edcc8323adef7f56da51da1d41256933667eb23 "Windows publisher promoter"
if errorlevel 1 goto :failed
fc /b "%PromoterCandidate%\windows-x64-wvhostverifierpublisherinstall.exe" "%TestDirectory%\Promoter.exe" >nul
if errorlevel 1 goto :failed
call :check_no_private_scratch
if errorlevel 1 goto :failed
call :pass "exact Windows publisher-promoter construction"

set /a Total+=1
call "%RepositoryRoot%\Tools\Native\Construct-Hosted-Verifier-Publisher-Promoter.cmd" linux "%TestDirectory%\Promoter.elf" >"%TestDirectory%\Promoter-Linux.out" 2>"%TestDirectory%\Promoter-Linux.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Promoter-Linux.err" "Linux promoter construction wrote a diagnostic"
if errorlevel 1 goto :failed
findstr /x /c:"publisher promoter construction status=Valid target=linux bytes=680901" "%TestDirectory%\Promoter-Linux.out" >nul
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Promoter.elf" 680901 422332fb4f2824ae558bf93adadb6470597399d07810f5428f71aa4d971a4f58 "Linux publisher promoter"
if errorlevel 1 goto :failed
fc /b "%PromoterCandidate%\linux-x64-wvhostverifierpublisherinstall.elf" "%TestDirectory%\Promoter.elf" >nul
if errorlevel 1 goto :failed
call :check_no_private_scratch
if errorlevel 1 goto :failed
call :pass "exact cross-target Linux publisher-promoter construction"

set /a Total+=1
call "%RepositoryRoot%\Tools\Native\Construct-Wvb-Publisher.cmd" windows "%TestDirectory%\Wvb-Publisher.exe" >"%TestDirectory%\Wvb-Publisher-Windows.out" 2>"%TestDirectory%\Wvb-Publisher-Windows.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Wvb-Publisher-Windows.err" "Windows WVB publisher construction wrote a diagnostic"
if errorlevel 1 goto :failed
findstr /x /c:"WVB publisher construction status=Valid target=windows bytes=1340928" "%TestDirectory%\Wvb-Publisher-Windows.out" >nul
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Wvb-Publisher.exe" 1340928 71794a6a254ccfd652ffe3bad556c32f86e2d9210a5a3099bad576f97476a8f3 "Windows WVB publisher"
if errorlevel 1 goto :failed
fc /b "%WvbPublisherCandidate%\windows-x64-wvpublish.exe" "%TestDirectory%\Wvb-Publisher.exe" >nul
if errorlevel 1 goto :failed
call :check_no_private_scratch
if errorlevel 1 goto :failed
call :pass "exact Windows WVB-publisher construction"

set /a Total+=1
call "%RepositoryRoot%\Tools\Native\Construct-Wvb-Publisher.cmd" linux "%TestDirectory%\Wvb-Publisher.elf" >"%TestDirectory%\Wvb-Publisher-Linux.out" 2>"%TestDirectory%\Wvb-Publisher-Linux.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Wvb-Publisher-Linux.err" "Linux WVB publisher construction wrote a diagnostic"
if errorlevel 1 goto :failed
findstr /x /c:"WVB publisher construction status=Valid target=linux bytes=1340357" "%TestDirectory%\Wvb-Publisher-Linux.out" >nul
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Wvb-Publisher.elf" 1340357 7f2dbfaecf2734c5afdbd6e2e54263a5a74038b8a498eeb1e155ee71788b630c "Linux WVB publisher"
if errorlevel 1 goto :failed
fc /b "%WvbPublisherCandidate%\linux-x64-wvpublish.elf" "%TestDirectory%\Wvb-Publisher.elf" >nul
if errorlevel 1 goto :failed
call :check_no_private_scratch
if errorlevel 1 goto :failed
call :pass "exact cross-target Linux WVB-publisher construction"

set /a Total+=1
call "%RepositoryRoot%\Tools\Native\Construct-Hosted-Verifier-Publisher-Admitter.cmd" windows "%TestDirectory%\Admitter.exe" >"%TestDirectory%\Admitter-Windows.out" 2>"%TestDirectory%\Admitter-Windows.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Admitter-Windows.err" "Windows admitter construction wrote a diagnostic"
if errorlevel 1 goto :failed
findstr /x /c:"publisher admitter construction status=Valid target=windows bytes=570368" "%TestDirectory%\Admitter-Windows.out" >nul
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Admitter.exe" 570368 4742ee299759728be1b72fed3d3b42620c21b10f77aed12cf150c1549b177b53 "Windows publisher admitter"
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
call :check_file "%TestDirectory%\Admitter.elf" 569344 b03788fad58ce071788b2f30945ed1dc0992559bb04b6cad04e719ff1114dc0a "Linux publisher admitter"
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
call :check_file "%TestDirectory%\Publisher.exe" 256000 17cb5c4228e8448693b17f1b73695fd0ecfd03d7ada922794a5bf3bd7594fc96 "preserved Windows publisher subject"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Publisher.elf" 254917 babe721a573e29f89ec095c35677880077ff465d4e2129063f6742cd47591a97 "preserved Linux publisher subject"
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
call :check_file "%TestDirectory%\Invalid.wvsq" 5064 8b752fd2c1b5afed4935453ee4d1f520d8807d439d7ad339f5f71a5ca30c05b1 "rejected metadata input"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Sentinel.wvhv" 5064 8b752fd2c1b5afed4935453ee4d1f520d8807d439d7ad339f5f71a5ca30c05b1 "preserved metadata destination"
if errorlevel 1 goto :failed
copy /y "%Construction%\SHA256SUMS" "%TestDirectory%\Sentinel.wvhr" >nul || goto :failed
"%PublisherTools%\wvhostverifierpublisherbaseruntime.exe" "%TestDirectory%\Invalid.wvsq" "%TestDirectory%\Sentinel.wvhr" >"%TestDirectory%\Reject.out" 2>"%TestDirectory%\Reject.err"
if not "%ERRORLEVEL%"=="2" goto :failed
call :check_empty "%TestDirectory%\Reject.out" "runtime rejection wrote standard output"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Reject.err" "runtime rejection wrote a diagnostic"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Sentinel.wvhr" 5064 8b752fd2c1b5afed4935453ee4d1f520d8807d439d7ad339f5f71a5ca30c05b1 "preserved runtime destination"
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
call :check_file "%TestDirectory%\Invalid.wvsq" 5064 8b752fd2c1b5afed4935453ee4d1f520d8807d439d7ad339f5f71a5ca30c05b1 "preserved alias input"
if errorlevel 1 goto :failed
call :pass "base tools reject exact path aliases"

set /a Total+=1
call "%RepositoryRoot%\Tools\Native\Install-Hosted-Verifier-Publisher.cmd" "%TestDirectory%\Publisher.exe" "%TestDirectory%\Installed-Publisher.exe" >"%TestDirectory%\Install-Publisher-Windows.out" 2>"%TestDirectory%\Install-Publisher-Windows.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Install-Publisher-Windows.err" "Windows publisher installation wrote a diagnostic"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Install-Publisher-Windows.out" 117 734463bece6f4670d21a7dba7c0a6e865073d36e43990ba3027650263d3e5b8b "Windows publisher installation report"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Installed-Publisher.exe" 256000 17cb5c4228e8448693b17f1b73695fd0ecfd03d7ada922794a5bf3bd7594fc96 "installed Windows publisher"
if errorlevel 1 goto :failed
fc /b "%TestDirectory%\Publisher.exe" "%TestDirectory%\Installed-Publisher.exe" >nul
if errorlevel 1 goto :failed
call "%RepositoryRoot%\Tools\Native\Install-Hosted-Verifier-Publisher.cmd" "%TestDirectory%\Publisher.elf" "%TestDirectory%\Installed-Publisher.elf" >"%TestDirectory%\Install-Publisher-Linux.out" 2>"%TestDirectory%\Install-Publisher-Linux.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Install-Publisher-Linux.err" "Linux publisher installation wrote a diagnostic"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Install-Publisher-Linux.out" 117 72e2786587919e14f707b437166f86ae18dca24869249c8c541b9836d6ace397 "Linux publisher installation report"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Installed-Publisher.elf" 254917 babe721a573e29f89ec095c35677880077ff465d4e2129063f6742cd47591a97 "installed Linux publisher"
if errorlevel 1 goto :failed
fc /b "%TestDirectory%\Publisher.elf" "%TestDirectory%\Installed-Publisher.elf" >nul
if errorlevel 1 goto :failed
call :check_no_private_scratch
if errorlevel 1 goto :failed
call :pass "current-host promoter installs both exact publishers"

set /a Total+=1
call :check_file "%VerifierCandidate%" 1004032 aea110110300870cd4f8e3dfcae98de24d90678dd33bfc8584351f58028ff34a "Windows verifier candidate"
if errorlevel 1 goto :failed
"%TestDirectory%\Installed-Publisher.exe" "%VerifierCandidate%" "%TestDirectory%\Installed.exe" >"%TestDirectory%\Execute.out" 2>"%TestDirectory%\Execute.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Execute.err" "constructed publisher execution wrote a diagnostic"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Installed.exe" 1004032 aea110110300870cd4f8e3dfcae98de24d90678dd33bfc8584351f58028ff34a "installed verifier"
if errorlevel 1 goto :failed
fc /b "%VerifierCandidate%" "%TestDirectory%\Installed.exe" >nul
if errorlevel 1 goto :failed
call :check_no_private_scratch
if errorlevel 1 goto :failed
call :pass "promoted current-host publisher execution"

set /a Total+=1
set "Phase=current-host WVB publisher execution"
"%WvbPublisherCandidate%\windows-x64-wvpublish.exe" "%PortableWvbCandidate%" "%TestDirectory%\Published-Portable.wvb" >"%TestDirectory%\Wvb-Publisher-Execute.out" 2>"%TestDirectory%\Wvb-Publisher-Execute.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Wvb-Publisher-Execute.err" "WVB publisher execution wrote a diagnostic"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Wvb-Publisher-Execute.out" 117 6e988c238fb917825f93e21b147567e04e256be0ec1e4df9c8dc07e19e4fa32e "WVB publisher completion report"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Published-Portable.wvb" 2001 3be0d06b8f4e7745dd9ffd9f325804d69ce524ac7ff6341b1e7b38037f6dd6f8 "published portable WVB"
if errorlevel 1 goto :failed
fc /b "%PortableWvbCandidate%" "%TestDirectory%\Published-Portable.wvb" >nul
if errorlevel 1 goto :failed
call :check_no_private_scratch
if errorlevel 1 goto :failed
call :pass "current-host WVB publisher execution"
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
for %%F in (Admission-Build.err Admission-Lower.err Promoter-Build.err Promoter-Lower.err Promoter-Link.err Windows.err Linux.err Promoter-Windows.err Promoter-Linux.err Wvb-Publisher-Windows.err Wvb-Publisher-Linux.err Admitter-Windows.err Admitter-Linux.err Admit-Windows.out Admit-Windows.err Admit-Linux.out Admit-Linux.err Admit-Swap.out Admit-Swap.err Admit-Corrupt.out Admit-Corrupt.err Admit-Usage.out Admit-Usage.err Install-Publisher-Windows.err Install-Publisher-Linux.err Reject.err Alias.err Execute.err Wvb-Publisher-Execute.out Wvb-Publisher-Execute.err) do if exist "%TestDirectory%\%%F" (
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
