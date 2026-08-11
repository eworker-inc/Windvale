@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Wv-Linker-Reconstruction.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "Candidate=%RepositoryRoot%\Artifacts\Native-Wv-Linker-Candidate"
set /a Tests=0
set /a Passed=0

call :check_file "%Candidate%\Wv-Linker.wvb" 135740 02f727a8ce2d6826c8414cada0933c7d5a54893ea061621d08147984c3d6f874
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Wv-Linker.wvo" 1786271 0141219773241e8780e2520f30ab8377914bf89a72f57da091871ac40d68a287
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Wv-Linker.bin" 1777781 d30e0c4dce7159bf98c546a0200e8b541797612ab67d6f21e3d8ee876af27480
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Wv-Linker.exe" 1796608 08744f3cacf71280ea757dcdf6509ee3770d5536b08e5b3984a438cb6123fb78
if errorlevel 1 goto :failed
call :check_file "%Candidate%\Wv-Linker.elf" 1798144 8a220bfd6c7ef684897583e728419ecd6d383c8e8cf40094edbcfb695e3d6d7a
if errorlevel 1 goto :failed
call :pass "candidate inventory"

call "%RepositoryRoot%\Tools\Native\Construct-Wv-Linker-Reconstruction.cmd" >nul 2>nul
if not "%ERRORLEVEL%"=="64" goto :failed

:allocate
set "TestDirectory=%TEMP%\windvale-wv-linker-reconstruction-test-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TestDirectory%" goto :allocate
mkdir "%TestDirectory%" || goto :failed

call "%RepositoryRoot%\Tools\Native\Construct-Wv-Linker-Reconstruction.cmd" "%TestDirectory%" ^
    >"%TestDirectory%\Construct.out" 2>"%TestDirectory%\Construct.err"
if errorlevel 1 goto :failed
>"%TestDirectory%\Construct.expected" echo native Wv-Linker reconstruction status=Complete artifacts=5
fc /b "%TestDirectory%\Construct.out" "%TestDirectory%\Construct.expected" >nul
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Construct.err") do if not "%%~zF"=="0" goto :failed
call :check_equal "%TestDirectory%\Wv-Linker.wvb" "%Candidate%\Wv-Linker.wvb"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Wv-Linker.wvo" "%Candidate%\Wv-Linker.wvo"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Wv-Linker.bin" "%Candidate%\Wv-Linker.bin"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Wv-Linker.exe" "%Candidate%\Wv-Linker.exe"
if errorlevel 1 goto :failed
call :check_equal "%TestDirectory%\Wv-Linker.elf" "%Candidate%\Wv-Linker.elf"
if errorlevel 1 goto :failed
call :pass "exact independent paired reconstruction"

set "Main=%TestDirectory%\Main.wvo"
set "Provider=%TestDirectory%\Provider.wvo"
call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" ^
    "%RepositoryRoot%\Examples\Assembler\Hello-Object.wva" "%Main%" ^
    >"%TestDirectory%\Main-Assemble.out" 2>"%TestDirectory%\Main-Assemble.err"
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Main-Assemble.err") do if not "%%~zF"=="0" goto :failed
call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" ^
    "%RepositoryRoot%\Examples\Linker\Console-Provider.wva" "%Provider%" ^
    >"%TestDirectory%\Provider-Assemble.out" 2>"%TestDirectory%\Provider-Assemble.err"
if errorlevel 1 goto :failed
for %%F in ("%TestDirectory%\Provider-Assemble.err") do if not "%%~zF"=="0" goto :failed
call :check_file "%Main%" 218 992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85
if errorlevel 1 goto :failed
call :check_file "%Provider%" 91 486134e34bb32abadd233d1c3303acd9c313aa69d3874cafdce0fcb61b6e72ab
if errorlevel 1 goto :failed
set "Application=%TestDirectory%\Wv-Linker.exe"
set "Image=%TestDirectory%\Application.bin"
set "Map=%TestDirectory%\Application.wvmap"
set "ApplicationError=%TestDirectory%\Application.err"
"%Application%" 0 Main "%Image%" "%Main%" "%Provider%" >"%Map%" 2>"%ApplicationError%"
if errorlevel 1 goto :failed
call :check_file "%Image%" 24 7612954be9dc08e12ab06510e6539a37ab797bc381ee8844908b5f7c475d16a5
if errorlevel 1 goto :failed
call :check_file "%Map%" 1644 df43f1b8381a7f5778bbb81a0d6b3fd589f0565603eef5296e2816146816ea97
if errorlevel 1 goto :failed
for %%F in ("%ApplicationError%") do if not "%%~zF"=="0" goto :failed

"%Application%" 0 Main >"%TestDirectory%\Usage.out" 2>"%TestDirectory%\Usage.err"
if not "%ERRORLEVEL%"=="64" goto :failed
for %%F in ("%TestDirectory%\Usage.out") do if not "%%~zF"=="0" goto :failed
call :check_file "%TestDirectory%\Usage.err" 85 c7a8e24b9be3d5a2678c5eb27bd88a39019694177fa970ece70dab92da2e8eee
if errorlevel 1 goto :failed
call :check_file "%Main%" 218 992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85
if errorlevel 1 goto :failed
call :check_file "%Provider%" 91 486134e34bb32abadd233d1c3303acd9c313aa69d3874cafdce0fcb61b6e72ab
if errorlevel 1 goto :failed
call :pass "current-host link, usage, and input preservation"

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

:pass
set /a Tests+=1
set /a Passed+=1
echo PASS  %~1
exit /b 0

:cleanup
if not defined TestDirectory exit /b 0
for %%R in ("%TestDirectory%") do set "ResolvedTestDirectory=%%~fR"
echo(%ResolvedTestDirectory%| findstr /b /i /c:"%TEMP%\windvale-wv-linker-reconstruction-test-" >nul || exit /b 1
if exist "%ResolvedTestDirectory%\." rmdir /s /q "%ResolvedTestDirectory%"
exit /b 0

:failed
call :cleanup >nul 2>nul
set /a Tests+=1
set /a Failed=Tests-Passed
echo FAIL  Wv-Linker reconstruction
echo Tests: %Tests%, Passed: %Passed%, Failed: %Failed%
exit /b 1
