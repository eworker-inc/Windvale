# Windvale installer runbook

This runbook builds and installs deterministic Windvale Installer 1 archives.
Use the `0.1.0-dev.1` channel for local development. Use stable `0.1.0` archives
only as exact inputs to the signed preview release ceremony.

## Build both targets

Create an empty output directory, then build the development artifacts from the
repository root:

```powershell
New-Item -ItemType Directory -Path C:\Temp\windvale-installers
node Tools/Release/Build-Installers.mjs build C:\Temp\windvale-installers
```

For the release-labeled artifacts, use a different empty directory and name the
stable input explicitly:

```powershell
New-Item -ItemType Directory -Path C:\Temp\windvale-release-installers
node Tools/Release/Build-Installers.mjs build C:\Temp\windvale-release-installers Distribution/Installers/Windvale-Release-Installer.json
```

The same commands work on Linux. Each build emits a Windows ZIP and Linux
tarball on either host. The bounded progress and final report name the channel,
target, byte length, archive SHA-256, payload identity, and generation.

Verify an artifact against the matching input envelope with:

```text
node Tools/Release/Build-Installers.mjs verify <development-artifact>
node Tools/Release/Build-Installers.mjs verify <stable-artifact> Distribution/Installers/Windvale-Release-Installer.json
```

Before distributing a stable archive, verify the complete release directory
through the independently obtained root as described by
[Preview-Release-Ceremony.md](Preview-Release-Ceremony.md). The stable filename
alone is not authenticity evidence.

## Install on Windows x64

Extract `windvale-0.1.0-windows-x64.zip`, open PowerShell in its top-level
directory, and run:

```powershell
.\Install-Windvale.ps1 -AddToPath
```

Omit `-AddToPath` to leave the user environment unchanged. Open a new terminal
after a PATH change, then inspect the installation:

```powershell
wv version
wv tools
wv doctor
```

Uninstall with the installed or matching extracted script:

```powershell
& "$env:LOCALAPPDATA\Windvale\Uninstall-Windvale.ps1"
```

## Install on Linux x64

Extract and run:

```sh
tar -xzf windvale-0.1.0-linux-x64.tar.gz
cd windvale-0.1.0-linux-x64
./install.sh
wv version
wv tools
wv doctor
```

The default command links are placed in `${XDG_BIN_HOME}` or
`${HOME}/.local/bin`. Use `--no-links` to publish only installation-local
command shims, or `--root` and `--bin-dir` for an explicit per-user policy.

Uninstall with:

```sh
~/.local/share/windvale/uninstall.sh
```

Use the corresponding XDG data location when `XDG_DATA_HOME` is set.

## Current boundary

Installer 1 verifies bytes and modes, installs one immutable generation,
detects later tampering, and removes only its recorded installation. It has no
updater, rollback manager, registry integration, machine-wide mode, or embedded
trust root. Release Envelope 1 supplies distribution authenticity for the
stable archives; the project-owner ceremony, exact-state qualification, and
immutable tag still gate official publication.
