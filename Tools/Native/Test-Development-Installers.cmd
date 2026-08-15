@echo off
setlocal EnableExtensions DisableDelayedExpansion

if not "%~1"=="" (
    >&2 echo Usage: Tools\Native\Test-Development-Installers.cmd
    exit /b 64
)

set "RepositoryRoot=%~dp0..\.."
for %%R in ("%RepositoryRoot%") do set "RepositoryRoot=%%~fR"

:allocate
set "Work=%TEMP%\windvale-development-installers-%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%Work%" goto :allocate
mkdir "%Work%" || exit /b 1
mkdir "%Work%\First" "%Work%\Second" "%Work%\Corrupt" "%Work%\Tampered-Extract" "%Work%\Clean-Extract" || goto :cleanup
>"%Work%\sentinel.txt" echo preserve
set "Result=1"
set "Builder=%RepositoryRoot%\Tools\Release\Build-Development-Installers.mjs"
set "WindowsArchive=windvale-0.1.0-dev.1-windows-x64.zip"
set "LinuxArchive=windvale-0.1.0-dev.1-linux-x64.tar.gz"
set "PackageDirectory=windvale-0.1.0-dev.1-windows-x64"
set "Generation=0.1.0-dev.1-windows-x64-6147bfdb0c4b"

echo native development installer step=construct-candidates item=1/8 targets=2 attempts=2
node "%Builder%" build "%Work%\First" || goto :cleanup
node "%Builder%" build "%Work%\Second" || goto :cleanup

echo native development installer step=prove-reproducibility item=2/8
fc /b "%Work%\First\%WindowsArchive%" "%Work%\Second\%WindowsArchive%" >nul || goto :cleanup
fc /b "%Work%\First\%LinuxArchive%" "%Work%\Second\%LinuxArchive%" >nul || goto :cleanup
call :verify_file "%Work%\First\%WindowsArchive%" 38351998 2c2112bef12e89b0594e2510b5ea71318b4c9ff8979b35c7fa7c20ca8703a186 "Windows installer" || goto :cleanup
call :verify_file "%Work%\First\%LinuxArchive%" 38363012 cbeddb17e258307b6005f5746925c5a4c3d68affca6495308abc6578d9294850 "Linux installer" || goto :cleanup

echo native development installer step=verify-and-reject item=3/8
node "%Builder%" verify "%Work%\First\%WindowsArchive%" >nul || goto :cleanup
node "%Builder%" verify "%Work%\First\%LinuxArchive%" >nul || goto :cleanup
copy /b "%Work%\First\%WindowsArchive%" "%Work%\Corrupt\%WindowsArchive%" >nul || goto :cleanup
>>"%Work%\Corrupt\%WindowsArchive%" echo x
node "%Builder%" verify "%Work%\Corrupt\%WindowsArchive%" >nul 2>nul
if not errorlevel 1 goto :cleanup

echo native development installer step=extract-host-package item=4/8
pwsh -NoLogo -NoProfile -Command "Expand-Archive -LiteralPath '%Work%\First\%WindowsArchive%' -DestinationPath '%Work%\Tampered-Extract'" || goto :cleanup
pwsh -NoLogo -NoProfile -Command "Expand-Archive -LiteralPath '%Work%\First\%WindowsArchive%' -DestinationPath '%Work%\Clean-Extract'" || goto :cleanup

echo native development installer step=reject-tampered-package item=5/8
pwsh -NoLogo -NoProfile -Command "$p='%Work%\Tampered-Extract\%PackageDirectory%\bin\wvbuild.exe'; $s=[IO.File]::OpenWrite($p); try { $s.Position=$s.Length; $s.WriteByte(0) } finally { $s.Dispose() }" || goto :cleanup
pwsh -NoLogo -NoProfile -File "%Work%\Tampered-Extract\%PackageDirectory%\Install-Windvale.ps1" -InstallRoot "%Work%\Rejected-Install" >nul 2>nul
if not errorlevel 1 goto :cleanup
if exist "%Work%\Rejected-Install" goto :cleanup

echo native development installer step=install-and-run item=6/8 attempts=2
pwsh -NoLogo -NoProfile -File "%Work%\Clean-Extract\%PackageDirectory%\Install-Windvale.ps1" -InstallRoot "%Work%\Installed" || goto :cleanup
pwsh -NoLogo -NoProfile -File "%Work%\Clean-Extract\%PackageDirectory%\Install-Windvale.ps1" -InstallRoot "%Work%\Installed" || goto :cleanup
echo native development installer step=install-and-run check=version
cmd /d /c ""%Work%\Installed\bin\wv.cmd" version" >"%Work%\Version.txt" || goto :cleanup
pwsh -NoLogo -NoProfile -Command "$l=[IO.File]::ReadAllLines('%Work%\Version.txt'); if ($l.Count -lt 1 -or $l[0] -ne 'Windvale 0.1.0-dev.1') { exit 1 }" || goto :cleanup
echo native development installer step=install-and-run check=doctor
cmd /d /c ""%Work%\Installed\bin\wv.cmd" doctor" >nul || goto :cleanup
echo native development installer step=install-and-run check=wvverify
cmd /d /c ""%Work%\Installed\bin\wvverify.cmd" "%RepositoryRoot%\Artifacts\Native-Front-Door\Wvb\Wvb-Runner.wvb"" >"%Work%\Wvb-Verify.txt" || goto :cleanup
pwsh -NoLogo -NoProfile -Command "$l=[IO.File]::ReadAllLines('%Work%\Wvb-Verify.txt'); if ($l.Count -ne 1 -or $l[0] -ne 'wvb status=Valid profile=compiler-aligned') { exit 1 }" || goto :cleanup

echo native development installer step=detect-installed-tamper item=7/8
pwsh -NoLogo -NoProfile -Command "$r='%Work%\Installed\generations\%Generation%'; $p=Join-Path $r 'bin\wvbuild.exe'; $s=[IO.File]::OpenWrite($p); try { $s.Position=$s.Length; $s.WriteByte(0) } finally { $s.Dispose() }; $b=[IO.File]::ReadAllBytes($p); $h=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($b)).ToLowerInvariant(); $m=Join-Path $r 'Payload-Manifest.txt'; $l=[IO.File]::ReadAllLines($m); for ($i=0; $i -lt $l.Count; $i++) { if ($l[$i].EndsWith(' bin/wvbuild.exe')) { $l[$i]='file ' + $h + ' ' + $b.Length + ' 0755 bin/wvbuild.exe' } }; [IO.File]::WriteAllText($m, (($l -join [char]10) + [char]10), [Text.UTF8Encoding]::new($false))" || goto :cleanup
cmd /d /c ""%Work%\Installed\bin\wv.cmd" doctor" >nul 2>nul
set "DoctorResult=%ERRORLEVEL%"
echo native development installer step=detect-installed-tamper result=%DoctorResult%
if "%DoctorResult%"=="0" goto :cleanup

echo native development installer step=uninstall-preserve-external item=8/8
pwsh -NoLogo -NoProfile -File "%Work%\Clean-Extract\%PackageDirectory%\Uninstall-Windvale.ps1" -InstallRoot "%Work%\Installed" >nul || goto :cleanup
if exist "%Work%\Installed" goto :cleanup
if not exist "%Work%\sentinel.txt" goto :cleanup

echo native development installer status=Passed cases=8 archives=2 reproducible=Verified host-install=Verified
set "Result=0"

:cleanup
for %%R in ("%Work%") do set "ResolvedWork=%%~fR"
echo(%ResolvedWork%| findstr /b /i /c:"%TEMP%\windvale-development-installers-" >nul || exit /b 1
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
