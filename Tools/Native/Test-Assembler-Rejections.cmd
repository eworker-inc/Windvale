@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Assembler-Rejections.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
:allocate
set "TemporaryDirectory=%TEMP%\windvale-assembler-rejections-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TemporaryDirectory%" goto :allocate
mkdir "%TemporaryDirectory%" || exit /b 1

set "Oversized=%TemporaryDirectory%\Oversized.wva"
set "Sentinel=%TemporaryDirectory%\Sentinel.wvo"
set "Destination=%TemporaryDirectory%\Destination.wvo"
set "RunOutput=%TemporaryDirectory%\Run.out"
set "RunError=%TemporaryDirectory%\Run.err"
set "DecodeOutput=%TemporaryDirectory%\Decode.out"
set "DecodeError=%TemporaryDirectory%\Decode.err"
set "CreateOutput=%TemporaryDirectory%\Create.out"
set "CreateError=%TemporaryDirectory%\Create.err"
set /a Total=0
set /a Passed=0

certutil -f -decode "%RepositoryRoot%\Tests\Native\Wvo\Return-42.wvo.b64" "%Sentinel%" > "%DecodeOutput%" 2> "%DecodeError%"
if errorlevel 1 (
    >&2 echo The native assembler destination sentinel could not be decoded.
    goto :failed
)
for %%S in ("%DecodeError%") do if not "%%~zS"=="0" (
    >&2 echo The native assembler destination decoder wrote a diagnostic.
    type "%DecodeError%" >&2
    goto :failed
)
certutil -hashfile "%Sentinel%" SHA256 | findstr /I /C:"0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5" >nul
if errorlevel 1 (
    >&2 echo The native assembler destination sentinel identity differs.
    goto :failed
)

fsutil file createnew "%Oversized%" 1048577 > "%CreateOutput%" 2> "%CreateError%"
if errorlevel 1 (
    >&2 echo The native assembler oversized fixture could not be created.
    type "%CreateError%" >&2
    goto :failed
)
certutil -hashfile "%Oversized%" SHA256 | findstr /I /C:"2cb74edba754a81d121c9db6833704a8e7d417e5b13d1a19f4a52f007d644264" >nul
if errorlevel 1 (
    >&2 echo The native assembler oversized fixture identity differs.
    goto :failed
)

call :run_case "wva1001" "%RepositoryRoot%\Tests\Native\Wva-Rejections\Bad-Header.wva" "a0c401f0ff8df946469bc46a2a8e6aeeea17ac1335267d377c5636f2ada31376" "4cfa4a4e82f3f03d8447865354e4c6f4d433680dadf3ce5c074e708c79a4de31"
if errorlevel 1 goto :failed
call :run_case "wva1002" "%RepositoryRoot%\Tests\Native\Wva-Rejections\Late-Symbol.wva" "e80f74ddb1daa2e52b731d70f01c2bef21910b70a5a5b3a83baafbf290bb35dd" "8642b0a6d4d2ac84a8e5be5d8d6009bdbc945082c954c5a8e15359494c212d58"
if errorlevel 1 goto :failed
call :run_case "wva1003" "%RepositoryRoot%\Tests\Native\Wva-Rejections\Short-Symbol.wva" "05db14bde97f50b4373bac9d1d4432aceb84d67cd040f059a6ff275ace41de88" "b627119175c5b48c0ea1e7ad8566e61df57c467ebe2de91d92cf456131f8a53a"
if errorlevel 1 goto :failed
call :run_case "wva1004" "%RepositoryRoot%\Tests\Native\Wva-Rejections\Bad-Machine-Name.wva" "13dcbcc9a1882d238c220f5ce91a9407e86e5ab558b2742a5454accd596cf694" "909f464d645ede6ec49f119c933573638eff80525d1b1c49738b90c96cfcc27c"
if errorlevel 1 goto :failed
call :run_case "wva1005" "%RepositoryRoot%\Tests\Native\Wva-Rejections\Bad-Alignment.wva" "f7e2b5e7adc5e782289ba6d9e5f2f1505d7352ea2d79e8ce30af44d677633bdc" "d46c03051b79c8af12274df50737cc7963b77a6aa4404282558c052b07e94b65"
if errorlevel 1 goto :failed
call :run_case "wva1006" "%RepositoryRoot%\Tests\Native\Wva-Rejections\Noncanonical-Symbols.wva" "ddce08026e091ef40e43f770557e510660c99a3beba4eea4d840d66c5616c9e8" "9c2270a866c3383ea43020bea7693d8d0ae87aae06fe86d46a35d30146e1a4ec"
if errorlevel 1 goto :failed
call :run_case "wva1007" "%RepositoryRoot%\Tests\Native\Wva-Rejections\Wrong-Symbol-Section.wva" "490ee170d0899f724f2c51c326ebfb6b90b540d95f4663813d20ef2969fae9ac" "440fb3e5eaf8153ee771d926274393e58a4689a14824c5a1846317bf819053d1"
if errorlevel 1 goto :failed
call :run_case "wva1008" "%RepositoryRoot%\Tests\Native\Wva-Rejections\Wrong-Statement-Section.wva" "e10d6cfc9568bfb13cc3281953eff68d9fc988521b5ca726adff59eb8e63a267" "9715af284f22626fd002ea4465185bfecf609be0fe378febbb93765d9736344a"
if errorlevel 1 goto :failed
call :run_case "wva1009" "%RepositoryRoot%\Tests\Native\Wva-Rejections\Missing-Call-Target.wva" "d5f86e0b5c975edaff2b82bfd7b48c5f8fcb1fd7a0ac49ea2601ca41a4f7d1ec" "d0d7a09622b8cc73cf2a4b87863f1b0cfe7c20f3da343f01764092472f6f1fd8"
if errorlevel 1 goto :failed
call :run_case "wva1010" "%RepositoryRoot%\Tests\Native\Wva-Rejections\Unclosed-Definition.wva" "bfefa2b17caad9c1966854ff0f23dc0e73647db9ad8cbea9c3f1c882002c6030" "ce6ea19735ebbbfa18725b7b600c12be4a240e68b7f6e5aec061722278969af4"
if errorlevel 1 goto :failed
call :run_case "wva1011" "%Oversized%" "2cb74edba754a81d121c9db6833704a8e7d417e5b13d1a19f4a52f007d644264" "0637a77d191b3e749c5779bcd069859f330314be167647d6db05bb96eb8d483c"
if errorlevel 1 goto :failed

call :cleanup
echo Tests: %Total%, Passed: %Passed%, Failed: 0
exit /b 0

:run_case
set /a Total+=1
certutil -hashfile "%~2" SHA256 | findstr /I /C:"%~3" >nul
if errorlevel 1 (
    >&2 echo FAIL  %~1: native assembler input identity differs
    exit /b 1
)
copy /y "%Sentinel%" "%Destination%" >nul
if errorlevel 1 (
    >&2 echo FAIL  %~1: destination could not be created
    exit /b 1
)
call "%RepositoryRoot%\Tools\Native\Assemble-Wva.cmd" "%~2" "%Destination%" > "%RunOutput%" 2> "%RunError%"
set "RunExit=%ERRORLEVEL%"
if not "%RunExit%"=="2" (
    >&2 echo FAIL  %~1: native assembler exit differs
    exit /b 1
)
for %%S in ("%RunOutput%") do if not "%%~zS"=="0" (
    >&2 echo FAIL  %~1: rejected assembly wrote standard output
    type "%RunOutput%" >&2
    exit /b 1
)
certutil -hashfile "%RunError%" SHA256 | findstr /I /C:"%~4" >nul
if errorlevel 1 (
    >&2 echo FAIL  %~1: native assembler report differs
    type "%RunError%" >&2
    exit /b 1
)
certutil -hashfile "%Destination%" SHA256 | findstr /I /C:"0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5" >nul
if errorlevel 1 (
    >&2 echo FAIL  %~1: rejected assembly changed the destination
    exit /b 1
)
del /f /q "%Destination%" "%RunOutput%" "%RunError%" >nul 2>nul
set /a Passed+=1
echo PASS  %~1
exit /b 0

:failed
set /a Failed=Total-Passed
call :cleanup
>&2 echo Tests: %Total%, Passed: %Passed%, Failed: %Failed%
exit /b 1

:cleanup
for %%F in (Oversized.wva Sentinel.wvo Destination.wvo Run.out Run.err Decode.out Decode.err Create.out Create.err) do (
    if exist "%TemporaryDirectory%\%%F" del /f /q "%TemporaryDirectory%\%%F" >nul 2>nul
)
rmdir "%TemporaryDirectory%" >nul 2>nul
exit /b 0
