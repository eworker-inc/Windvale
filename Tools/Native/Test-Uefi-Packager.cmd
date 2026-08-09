@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Uefi-Packager.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"
set "LinkerArtifacts=%RepositoryRoot%\Artifacts\Native-Linker-Candidate"
set "PackagerArtifacts=%RepositoryRoot%\Artifacts\Native-Uefi-Packager-Candidate"
call :check_file "%LinkerArtifacts%\Main.wvo" 218 992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85 "main WVO"
if errorlevel 1 exit /b 1
call :check_file "%LinkerArtifacts%\Provider.wvo" 91 486134e34bb32abadd233d1c3303acd9c313aa69d3874cafdce0fcb61b6e72ab "provider WVO"
if errorlevel 1 exit /b 1

:allocate
set "TestDirectory=%TEMP%\windvale-native-uefi-packager-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%TestDirectory%" goto :allocate
mkdir "%TestDirectory%" || exit /b 1
set "Result=1"

call "%RepositoryRoot%\Tools\Native\Link-Wvo.cmd" 0 Main ^
    "%TestDirectory%\Native.bin" "%LinkerArtifacts%\Main.wvo" ^
    "%LinkerArtifacts%\Provider.wvo" ^
    >"%TestDirectory%\Link.out" 2>"%TestDirectory%\Link.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Link.err" "native linking wrote a diagnostic"
if errorlevel 1 goto :failed
findstr /c:"entry name=Main address=0" "%TestDirectory%\Link.out" >nul
if errorlevel 1 goto :entry_report_failed
call :check_file "%TestDirectory%\Native.bin" 24 7612954be9dc08e12ab06510e6539a37ab797bc381ee8844908b5f7c475d16a5 "native linked image"
if errorlevel 1 goto :failed

call "%RepositoryRoot%\Tools\Native\Package-Uefi.cmd" ^
    "%TestDirectory%\Native.bin" 0 "%TestDirectory%\Application.efi" ^
    >"%TestDirectory%\Package.out" 2>"%TestDirectory%\Package.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Package.err" "valid packaging wrote a diagnostic"
if errorlevel 1 goto :failed
findstr /c:"uefi-package status=Valid native-image-bytes=24 entry-offset=0 application-bytes=1536" "%TestDirectory%\Package.out" >nul
if errorlevel 1 goto :valid_report_failed
call :check_file "%TestDirectory%\Application.efi" 1536 7d30fd4d220a2d578b0ce3da4cbb6006175f012268b7d3a08e80543e7e388b09 "canonical UEFI application"
if errorlevel 1 goto :failed
echo PASS  UEFI packaging composes native link output

call "%RepositoryRoot%\Tools\Native\Package-Uefi.cmd" ^
    "%TestDirectory%\Native.bin" 0 "%TestDirectory%\Application-Again.efi" ^
    >"%TestDirectory%\Repeat.out" 2>"%TestDirectory%\Repeat.err"
if errorlevel 1 goto :failed
call :check_empty "%TestDirectory%\Repeat.err" "repeat packaging wrote a diagnostic"
if errorlevel 1 goto :failed
fc /b "%TestDirectory%\Application.efi" "%TestDirectory%\Application-Again.efi" >nul
if errorlevel 1 goto :repeat_failed
echo PASS  UEFI packaging is deterministic

copy /y "%PackagerArtifacts%\Uefi-Packager.wvb" "%TestDirectory%\Rejected.efi" >nul || goto :failed
call "%RepositoryRoot%\Tools\Native\Package-Uefi.cmd" ^
    "%TestDirectory%\Native.bin" 24 "%TestDirectory%\Rejected.efi" ^
    >"%TestDirectory%\Invalid.out" 2>"%TestDirectory%\Invalid.err"
set "InvalidResult=%ERRORLEVEL%"
if not "%InvalidResult%"=="2" goto :invalid_result_failed
call :check_empty "%TestDirectory%\Invalid.out" "invalid packaging wrote standard output"
if errorlevel 1 goto :failed
findstr /c:"entry-offset=24 application-bytes=0" "%TestDirectory%\Invalid.err" >nul
if errorlevel 1 goto :invalid_report_failed
call :check_file "%TestDirectory%\Rejected.efi" 25999 063f95f53e39390c76bcf31fbf7bdc87eed6194388101fadc4d60ee41b2802e4 "preserved destination"
if errorlevel 1 goto :failed
echo PASS  UEFI packaging rejects invalid entry and preserves output
set "Result=0"
call :cleanup_files
echo Tests: 3, Passed: 3, Failed: 0
exit /b 0

:entry_report_failed
>&2 echo FAIL  UEFI packaging: native linker did not report entry zero
goto :failed

:valid_report_failed
>&2 echo FAIL  UEFI packaging: valid report differs
goto :failed

:repeat_failed
>&2 echo FAIL  UEFI packaging: repeated output differs
goto :failed

:invalid_result_failed
>&2 echo FAIL  UEFI packaging: invalid entry returned %InvalidResult%
goto :failed

:invalid_report_failed
>&2 echo FAIL  UEFI packaging: invalid-entry report differs
goto :failed

:check_file
if not exist "%~1" (
    >&2 echo FAIL  UEFI packaging: missing %~4
    exit /b 1
)
for %%F in ("%~1") do if not "%%~zF"=="%~2" (
    >&2 echo FAIL  UEFI packaging: %~4 length differs
    exit /b 1
)
certutil -hashfile "%~1" SHA256 | findstr /i /c:"%~3" >nul
if errorlevel 1 (
    >&2 echo FAIL  UEFI packaging: %~4 digest differs
    exit /b 1
)
exit /b 0

:check_empty
for %%F in ("%~1") do if not "%%~zF"=="0" (
    >&2 echo FAIL  UEFI packaging: %~2
    type "%~1" >&2
    exit /b 1
)
exit /b 0

:failed
set "Result=1"
call :cleanup_files
exit /b 1

:cleanup_files
for %%F in (Native.bin Link.out Link.err Application.efi Package.out Package.err Application-Again.efi Repeat.out Repeat.err Rejected.efi Invalid.out Invalid.err) do (
    if exist "%TestDirectory%\%%F" del /f /q "%TestDirectory%\%%F" >nul 2>nul
)
rmdir "%TestDirectory%" >nul 2>nul
exit /b 0
