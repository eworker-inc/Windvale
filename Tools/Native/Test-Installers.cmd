@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Installers.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"

:allocate
set "Work=%TEMP%\windvale-installers-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
mkdir "%Work%\First-Development" "%Work%\Second-Development" "%Work%\First-Release" "%Work%\Second-Release" "%Work%\Corrupt" "%Work%\Development-Extract" "%Work%\Tampered-Extract" "%Work%\Clean-Extract" || goto :cleanup
>"%Work%\sentinel.txt" echo preserve
set "Result=1"
set "Builder=%RepositoryRoot%\Tools\Release\Build-Installers.mjs"
set "ReleaseInput=Distribution/Installers/Windvale-Release-Installer.json"
set "DevelopmentWindowsArchive=windvale-0.2.0-dev.1-windows-x64.zip"
set "DevelopmentLinuxArchive=windvale-0.2.0-dev.1-linux-x64.tar.gz"
set "DevelopmentPackageDirectory=windvale-0.2.0-dev.1-windows-x64"
set "DevelopmentPayload=1f86c542450c0ce92ad886363ec2ef1112431e71314ff5115166db6fad31dfc7"
set "WindowsArchive=windvale-0.1.0-windows-x64.zip"
set "LinuxArchive=windvale-0.1.0-linux-x64.tar.gz"
set "PackageDirectory=windvale-0.1.0-windows-x64"
set "Generation=0.1.0-windows-x64-639a04bcca88"

echo native installer step=construct-candidates item=1/8 channels=2 targets=2 attempts=2
node "%Builder%" build "%Work%\First-Development" || goto :cleanup
node "%Builder%" build "%Work%\Second-Development" || goto :cleanup
node "%Builder%" build "%Work%\First-Release" "%ReleaseInput%" || goto :cleanup
node "%Builder%" build "%Work%\Second-Release" "%ReleaseInput%" || goto :cleanup

echo native installer step=prove-reproducibility item=2/8
fc /b "%Work%\First-Development\%DevelopmentWindowsArchive%" "%Work%\Second-Development\%DevelopmentWindowsArchive%" >nul || goto :cleanup
fc /b "%Work%\First-Development\%DevelopmentLinuxArchive%" "%Work%\Second-Development\%DevelopmentLinuxArchive%" >nul || goto :cleanup
fc /b "%Work%\First-Release\%WindowsArchive%" "%Work%\Second-Release\%WindowsArchive%" >nul || goto :cleanup
fc /b "%Work%\First-Release\%LinuxArchive%" "%Work%\Second-Release\%LinuxArchive%" >nul || goto :cleanup
call :verify_file "%Work%\First-Development\%DevelopmentWindowsArchive%" 4659946 88b67397575768eec4027fe2f6118354b8e117875c0563c3ff90561c74b0216c "Windows development installer" || goto :cleanup
call :verify_file "%Work%\First-Development\%DevelopmentLinuxArchive%" 4653399 eef1401522b829d1c0de76e37d914fa3215a5bdea31dfa66513a65e38e5a438f "Linux development installer" || goto :cleanup
call :verify_file "%Work%\First-Release\%WindowsArchive%" 38823943 a04156e699a9156584195c402d3fe41b90683378f3099b8b6ee9fad74088b2c4 "Windows release installer" || goto :cleanup
call :verify_file "%Work%\First-Release\%LinuxArchive%" 38835111 77b317a44c4d8408d1804b8c645108bd9517926e897747e606cef12a7adee23b "Linux release installer" || goto :cleanup

echo native installer step=verify-and-reject item=3/8 channel=stable
node "%Builder%" verify "%Work%\First-Release\%WindowsArchive%" "%ReleaseInput%" >nul || goto :cleanup
node "%Builder%" verify "%Work%\First-Release\%LinuxArchive%" "%ReleaseInput%" >nul || goto :cleanup
copy /b "%Work%\First-Release\%WindowsArchive%" "%Work%\Corrupt\%WindowsArchive%" >nul || goto :cleanup
>>"%Work%\Corrupt\%WindowsArchive%" echo x
node "%Builder%" verify "%Work%\Corrupt\%WindowsArchive%" "%ReleaseInput%" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native installer step=extract-host-packages item=4/8 channels=2
pwsh -NoLogo -NoProfile -Command "Expand-Archive -LiteralPath '%Work%\First-Development\%DevelopmentWindowsArchive%' -DestinationPath '%Work%\Development-Extract'" || goto :cleanup
pwsh -NoLogo -NoProfile -File "%Work%\Development-Extract\%DevelopmentPackageDirectory%\bin\wv-verify-installation.ps1" -Root "%Work%\Development-Extract\%DevelopmentPackageDirectory%" -ExpectedTarget windows-x64 -ExpectedPayload "%DevelopmentPayload%" >nul || goto :cleanup
pwsh -NoLogo -NoProfile -Command "Expand-Archive -LiteralPath '%Work%\First-Release\%WindowsArchive%' -DestinationPath '%Work%\Tampered-Extract'" || goto :cleanup
pwsh -NoLogo -NoProfile -Command "Expand-Archive -LiteralPath '%Work%\First-Release\%WindowsArchive%' -DestinationPath '%Work%\Clean-Extract'" || goto :cleanup

echo native installer step=reject-tampered-package item=5/8 channel=stable
pwsh -NoLogo -NoProfile -Command "$p='%Work%\Tampered-Extract\%PackageDirectory%\bin\wvbuild.exe'; $s=[IO.File]::OpenWrite($p); try { $s.Position=$s.Length; $s.WriteByte(0) } finally { $s.Dispose() }" || goto :cleanup
pwsh -NoLogo -NoProfile -File "%Work%\Tampered-Extract\%PackageDirectory%\Install-Windvale.ps1" -InstallRoot "%Work%\Rejected-Install" >nul 2>nul
if not errorlevel 1 goto :cleanup
if exist "%Work%\Rejected-Install" goto :cleanup

echo native installer step=install-and-run item=6/8 channel=stable attempts=2
pwsh -NoLogo -NoProfile -File "%Work%\Clean-Extract\%PackageDirectory%\Install-Windvale.ps1" -InstallRoot "%Work%\Installed" || goto :cleanup
pwsh -NoLogo -NoProfile -File "%Work%\Clean-Extract\%PackageDirectory%\Install-Windvale.ps1" -InstallRoot "%Work%\Installed" || goto :cleanup
echo native installer step=install-and-run check=version
cmd /d /c ""%Work%\Installed\bin\wv.cmd" version" >"%Work%\Version.txt" || goto :cleanup
pwsh -NoLogo -NoProfile -Command "$l=[IO.File]::ReadAllLines('%Work%\Version.txt'); if ($l.Count -lt 1 -or $l[0] -ne 'Windvale 0.1.0') { exit 1 }" || goto :cleanup
echo native installer step=install-and-run check=doctor
cmd /d /c ""%Work%\Installed\bin\wv.cmd" doctor" >nul || goto :cleanup
echo native installer step=install-and-run check=wvverify
cmd /d /c ""%Work%\Installed\bin\wvverify.cmd" "%RepositoryRoot%\Artifacts\Native-Front-Door\Wvb\Wvb-Runner.wvb"" >"%Work%\Wvb-Verify.txt" || goto :cleanup
pwsh -NoLogo -NoProfile -Command "$l=[IO.File]::ReadAllLines('%Work%\Wvb-Verify.txt'); if ($l.Count -ne 1 -or $l[0] -ne 'wvb status=Valid profile=compiler-aligned') { exit 1 }" || goto :cleanup
echo native installer step=install-and-run check=scripting
call "%Work%\Installed\bin\wv.cmd" run "%RepositoryRoot%\Tests\Fixtures\Scripting\Arguments-And-Output.wv" -flag "snow day" >"%Work%\Script.out" 2>"%Work%\Script.err"
if not "%ERRORLEVEL%"=="7" goto :cleanup
pwsh -NoLogo -NoProfile -Command "$o=[IO.File]::ReadAllText('%Work%\Script.out'); $e=[IO.File]::ReadAllLines('%Work%\Script.err'); if ($o -cne ('first=-flag'+[char]10) -or $e.Count -ne 1 -or $e[0] -cne 'second=snow day') { exit 1 }" || goto :cleanup

echo native installer step=detect-installed-tamper item=7/8 channel=stable
pwsh -NoLogo -NoProfile -Command "$r='%Work%\Installed\generations\%Generation%'; $p=Join-Path $r 'bin\wvbuild.exe'; $s=[IO.File]::OpenWrite($p); try { $s.Position=$s.Length; $s.WriteByte(0) } finally { $s.Dispose() }; $b=[IO.File]::ReadAllBytes($p); $h=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($b)).ToLowerInvariant(); $m=Join-Path $r 'Payload-Manifest.txt'; $l=[IO.File]::ReadAllLines($m); for ($i=0; $i -lt $l.Count; $i++) { if ($l[$i].EndsWith(' bin/wvbuild.exe')) { $l[$i]='file ' + $h + ' ' + $b.Length + ' 0755 bin/wvbuild.exe' } }; [IO.File]::WriteAllText($m, (($l -join [char]10) + [char]10), [Text.UTF8Encoding]::new($false))" || goto :cleanup
cmd /d /c ""%Work%\Installed\bin\wv.cmd" doctor" >nul 2>nul
set "DoctorResult=%ERRORLEVEL%"
echo native installer step=detect-installed-tamper result=%DoctorResult%
if "%DoctorResult%"=="0" goto :cleanup

echo native installer step=uninstall-preserve-external item=8/8 channel=stable
pwsh -NoLogo -NoProfile -File "%Work%\Clean-Extract\%PackageDirectory%\Uninstall-Windvale.ps1" -InstallRoot "%Work%\Installed" >nul || goto :cleanup
if exist "%Work%\Installed" goto :cleanup
if not exist "%Work%\sentinel.txt" goto :cleanup

echo native installer status=Passed cases=8 channels=2 archives=4 reproducible=Verified host-install=Verified
set "Result=0"

:cleanup
for %%R in ("%Work%") do set "ResolvedWork=%%~fR"
echo(%ResolvedWork%| findstr /b /i /c:"%TEMP%\windvale-installers-" >nul || exit /b 1
if exist "%ResolvedWork%\." rmdir /s /q "%ResolvedWork%"
exit /b %Result%

:verify_file
if not exist "%~1" (
    >&2 echo Missing %~4: %~1
    exit /b 1
)
for %%F in ("%~1") do if not "%%~zF"=="%~2" (
    >&2 echo Invalid byte length for %~4.
    exit /b 1
)
certutil -hashfile "%~1" SHA256 | findstr /i /c:"%~3" >nul
if errorlevel 1 (
    >&2 echo Invalid SHA-256 for %~4.
    exit /b 1
)
exit /b 0
