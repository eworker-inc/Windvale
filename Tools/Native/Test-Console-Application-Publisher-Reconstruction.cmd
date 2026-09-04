@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Console-Application-Publisher-Reconstruction.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Candidate=%RepositoryRoot%\Artifacts\Native-Console-Application-Publisher-Candidate"
set "RawLowerer=%RepositoryRoot%\Artifacts\Native-Wvb-To-Wvo-Candidate\Wvb-To-Wvo.exe"
set /a Tests=0
set /a Passed=0

call :check_file "%Candidate%\Console-Application-Publisher.wvb" 115107 e8121fb76c7cc39b159d53a3c28d1da8bc2d44968d630495c692a7761656923d
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Console-Application-Publisher.wvo" 1139440 259c7d746c3a217c32706bfd617cf66894066bd2e50850cbe5733ac3338e4952
if errorlevel 1 goto :failed
call :check_file "%Candidate%\windows-x64-wvappublish.exe" 1158656 0bafe84096859f4b88dc14be92c6cdc5336d791b7c5b0a332dccb76b913dd24e
if errorlevel 1 goto :failed
call :check_file "%Candidate%\linux-x64-wvappublish.elf" 1156085 e9b8771978c9fb06c3a8ecc55c7b9a3ba1acd24faa541dc669920c10ed792925
if errorlevel 1 goto :failed
call :pass "candidate inventory"

:allocate
set "TestDirectory=%TEMP%\windvale-console-application-publisher-reconstruction-test-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TestDirectory%" goto :allocate
mkdir "%TestDirectory%" || goto :failed

call "%RepositoryRoot%\Tools\Native\Construct-Console-Application-Publisher.cmd" ^
    >"%TestDirectory%\Usage.out" 2>"%TestDirectory%\Usage.err"
if not "%ERRORLEVEL%"=="64" goto :failed
for %%F in ("%TestDirectory%\Usage.out") do if not "%%~zF"=="0" goto :failed
>"%TestDirectory%\Usage.expected" echo Usage: Tools\Native\Construct-Console-Application-Publisher.cmd ^<windows^|linux^> ^<output.exe^|output.elf^>
fc /b "%TestDirectory%\Usage.err" "%TestDirectory%\Usage.expected" >nul
if errorlevel 1 goto :failed

call "%RepositoryRoot%\Tools\Native\Build-Wvb.cmd" ^
    "%RepositoryRoot%\Projects/Tools/Windvale-Console-Application-Publisher.wvproj" ^
    "%TestDirectory%\Console-Application-Publisher.wvb" ^
    >"%TestDirectory%\Build.out" 2>"%TestDirectory%\Build.err"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Build.out" 220 13da87bcbb1b57a085b06bcbff40a128581d3802157a23b6cd27e532671ea4b5
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Build.err"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Console-Application-Publisher.wvb" "%Candidate%\Console-Application-Publisher.wvb"
if errorlevel 1 goto :failed

call :check_file "%RawLowerer%" 10656768 0a0894901341d71ef09712fb63ed0a9f7ac2b93c64b357d123dd09674045cfda
if errorlevel 1 goto :failed
"%RawLowerer%" "%TestDirectory%\Console-Application-Publisher.wvb" ^
    "%TestDirectory%\Console-Application-Publisher.wvo" ^
    >"%TestDirectory%\Lower.out" 2>"%TestDirectory%\Lower.err"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Lower.out" 71 2beb32dcc9d4ec7c3c4ff27c0d8fe2261512e0cd9ec5fb02fd1c9da90fb7a36b
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Lower.err"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Console-Application-Publisher.wvo" "%Candidate%\Console-Application-Publisher.wvo"
if errorlevel 1 goto :failed

call "%RepositoryRoot%\Tools\Native\Construct-Console-Application-Publisher.cmd" ^
    windows "%TestDirectory%\Console-Application-Publisher.exe" ^
    >"%TestDirectory%\Windows.out" 2>"%TestDirectory%\Windows.err"
if errorlevel 1 goto :failed
>"%TestDirectory%\Windows.expected" echo console-application publisher construction status=Valid target=windows bytes=1158656
call :check_equal "%TestDirectory%\Windows.out" "%TestDirectory%\Windows.expected"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Windows.err"
if errorlevel 1 goto :failed

call "%RepositoryRoot%\Tools\Native\Construct-Console-Application-Publisher.cmd" ^
    linux "%TestDirectory%\Console-Application-Publisher.elf" ^
    >"%TestDirectory%\Linux.out" 2>"%TestDirectory%\Linux.err"
if errorlevel 1 goto :failed
>"%TestDirectory%\Linux.expected" echo console-application publisher construction status=Valid target=linux bytes=1156085
call :check_equal "%TestDirectory%\Linux.out" "%TestDirectory%\Linux.expected"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Linux.err"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Console-Application-Publisher.exe" "%Candidate%\windows-x64-wvappublish.exe"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Console-Application-Publisher.elf" "%Candidate%\linux-x64-wvappublish.elf"
if errorlevel 1 goto :failed
call :pass "exact native WVB, WVO, and paired publisher reconstruction"

set "Probe=%TestDirectory%\Aot-Probe"
mkdir "%Probe%" || goto :failed
call "%RepositoryRoot%\Tools\Native\Construct-Aot-Composition-Probe.cmd" "%Probe%" ^
    >"%TestDirectory%\Probe.out" 2>"%TestDirectory%\Probe.err"
if errorlevel 1 goto :failed
>"%TestDirectory%\Probe.expected" echo native AOT composition probe status=Complete artifacts=6
call :check_equal "%TestDirectory%\Probe.out" "%TestDirectory%\Probe.expected"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Probe.err"
if errorlevel 1 goto :failed
set "WindowsFixture=%Probe%\Return-42.exe"
set "LinuxFixture=%Probe%\Return-42.elf"

call :check_file "%WindowsFixture%" 2560 8f2c3389dafa40c0231a0f5aeead3db5570697d54874f324a81f84a2d5b16eb6
if errorlevel 1 goto :failed
call :check_file "%LinuxFixture%" 8304 fe525b84b9bf902677a5c7beb36872dfd72e7d6d0f12bfb5c95d491c4e1cd3f7
if errorlevel 1 goto :failed
copy /b "%WindowsFixture%" "%TestDirectory%\Subject.exe" >nul
if errorlevel 1 goto :failed
copy /b "%Candidate%\Console-Application-Publisher.wvb" "%TestDirectory%\Destination.exe" >nul
if errorlevel 1 goto :failed
"%TestDirectory%\Console-Application-Publisher.exe" ^
    "%TestDirectory%\Subject.exe" "%TestDirectory%\Destination.exe" ^
    >"%TestDirectory%\Publish.out" 2>"%TestDirectory%\Publish.err"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Publish.out" 117 19172b4e0501f1cc471b857f8fd55e51f1a6677c5296ab7f67b5618ee7d8018f
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Publish.err"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Subject.exe" "%TestDirectory%\Destination.exe"
if errorlevel 1 goto :failed

copy /b "%Candidate%\Console-Application-Publisher.wvb" "%TestDirectory%\Invalid.exe" >nul
if errorlevel 1 goto :failed
"%TestDirectory%\Console-Application-Publisher.exe" ^
    "%TestDirectory%\Invalid.exe" "%TestDirectory%\Destination.exe" ^
    >"%TestDirectory%\Reject.out" 2>"%TestDirectory%\Reject.err"
if not "%ERRORLEVEL%"=="1" goto :failed
call :check_empty "%TestDirectory%\Reject.out"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Reject.err" 54 39db034713225109f62c272db447d75cfe93ff0c259c8d9e5211f0df5c007e1f
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Invalid.exe" "%Candidate%\Console-Application-Publisher.wvb"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Destination.exe" "%WindowsFixture%"
if errorlevel 1 goto :failed
for /f "usebackq delims=" %%S in (`dir /b /a "%TestDirectory%\.wvpublish-*" 2^>nul`) do goto :failed
call :pass "current-host independent version-1 publication and rejected-input preservation"

node --check "%RepositoryRoot%\Tools\Native\Check-Console-Publication-Candidate.mjs"
if errorlevel 1 goto :failed

copy /b "%Candidate%\Console-Application-Publisher.wvb" "%TestDirectory%\Public-Windows.exe" >nul
if errorlevel 1 goto :failed
call "%RepositoryRoot%\Tools\Native\Publish-Console.cmd" ^
    "%WindowsFixture%" "%TestDirectory%\Public-Windows.exe" ^
    >"%TestDirectory%\Public-Windows.out" 2>"%TestDirectory%\Public-Windows.err"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Public-Windows.out" 117 19172b4e0501f1cc471b857f8fd55e51f1a6677c5296ab7f67b5618ee7d8018f
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Public-Windows.err"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Public-Windows.exe" "%WindowsFixture%"
if errorlevel 1 goto :failed

copy /b "%Candidate%\Console-Application-Publisher.wvb" "%TestDirectory%\Public-Linux.elf" >nul
if errorlevel 1 goto :failed
call "%RepositoryRoot%\Tools\Native\Publish-Console.cmd" ^
    "%LinuxFixture%" "%TestDirectory%\Public-Linux.elf" ^
    >"%TestDirectory%\Public-Linux.out" 2>"%TestDirectory%\Public-Linux.err"
if errorlevel 1 goto :failed
call :check_file "%TestDirectory%\Public-Linux.out" 117 3f37b8e32972391192e21fc581500909e0ea74629744e0c248589f3bc0f24d7c
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Public-Linux.err"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Public-Linux.elf" "%LinuxFixture%"
if errorlevel 1 goto :failed

copy /b "%WindowsFixture%" "%TestDirectory%\Wrong-Windows.elf" >nul
if errorlevel 1 goto :failed
copy /b "%Candidate%\Console-Application-Publisher.wvb" "%TestDirectory%\Wrong-Windows-Destination.elf" >nul
if errorlevel 1 goto :failed
call "%RepositoryRoot%\Tools\Native\Publish-Console.cmd" ^
    "%TestDirectory%\Wrong-Windows.elf" "%TestDirectory%\Wrong-Windows-Destination.elf" ^
    >"%TestDirectory%\Wrong-Windows.out" 2>"%TestDirectory%\Wrong-Windows.err"
if not "%ERRORLEVEL%"=="1" goto :failed
call :check_empty "%TestDirectory%\Wrong-Windows.out"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Wrong-Windows.err" "%TestDirectory%\Reject.err"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Wrong-Windows-Destination.elf" "%Candidate%\Console-Application-Publisher.wvb"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Wrong-Windows.elf" "%WindowsFixture%"
if errorlevel 1 goto :failed

copy /b "%LinuxFixture%" "%TestDirectory%\Wrong-Linux.exe" >nul
if errorlevel 1 goto :failed
copy /b "%Candidate%\Console-Application-Publisher.wvb" "%TestDirectory%\Wrong-Linux-Destination.exe" >nul
if errorlevel 1 goto :failed
call "%RepositoryRoot%\Tools\Native\Publish-Console.cmd" ^
    "%TestDirectory%\Wrong-Linux.exe" "%TestDirectory%\Wrong-Linux-Destination.exe" ^
    >"%TestDirectory%\Wrong-Linux.out" 2>"%TestDirectory%\Wrong-Linux.err"
if not "%ERRORLEVEL%"=="1" goto :failed
call :check_empty "%TestDirectory%\Wrong-Linux.out"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Wrong-Linux.err" "%TestDirectory%\Reject.err"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Wrong-Linux-Destination.exe" "%Candidate%\Console-Application-Publisher.wvb"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Wrong-Linux.exe" "%LinuxFixture%"
if errorlevel 1 goto :failed
for /f "usebackq delims=" %%S in (`dir /b /a "%TestDirectory%\.wvpublish-*" 2^>nul`) do goto :failed
call :pass "public front-door target preflight and cross-target publication"

call :cleanup
echo Tests: %Tests%, Passed: %Passed%, Failed: 0
exit /b 0

:check_file
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="%~2" exit /b 1
certutil -hashfile "%~1" SHA256 | findstr /I /C:"%~3" >nul
exit /b %ERRORLEVEL%

:check_equal
if not exist "%~1" exit /b 1
if not exist "%~2" exit /b 1
fc /b "%~1" "%~2" >nul
exit /b %ERRORLEVEL%

:check_empty
if not exist "%~1" exit /b 1
for %%F in ("%~1") do if not "%%~zF"=="0" exit /b 1
exit /b 0

:pass
set /a Tests+=1
set /a Passed+=1
echo PASS  %~1
exit /b 0

:cleanup
if not defined TestDirectory exit /b 0
for %%R in ("%TestDirectory%") do set "ResolvedTestDirectory=%%~fR"
echo(%ResolvedTestDirectory%| findstr /b /i /c:"%TEMP%\windvale-console-application-publisher-reconstruction-test-" >nul || exit /b 1
if exist "%ResolvedTestDirectory%\." rmdir /s /q "%ResolvedTestDirectory%"
exit /b 0

:failed
call :cleanup >nul 2>nul
set /a Tests+=1
set /a Failed=Tests-Passed
echo FAIL  console-application publisher reconstruction
echo Tests: %Tests%, Passed: %Passed%, Failed: %Failed%
exit /b 1
