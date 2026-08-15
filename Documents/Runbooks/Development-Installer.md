# Windvale development installer runbook

This runbook builds and installs the unsigned `0.1.0-dev.1` Milestone 3
development artifacts. They are not the Windvale `v0.1.0` release.

## Build both artifacts

Create an empty output directory, then run from the repository root:

```powershell
New-Item -ItemType Directory -Path C:\Temp\windvale-installers
node Tools/Release/Build-Development-Installers.mjs build C:\Temp\windvale-installers
```

On Linux, the same builder command accepts an existing empty directory. It emits
both the Windows ZIP and Linux tarball on either host. The final report includes
their byte lengths, SHA-256 identities, payload identities, and generation names.

Verify one artifact against the checked-in input envelope with:

```text
node Tools/Release/Build-Development-Installers.mjs verify <artifact>
```

## Install on Windows x64

Extract `windvale-0.1.0-dev.1-windows-x64.zip`, open PowerShell in its top-level
directory, and run:

```powershell
.\Install-Windvale.ps1 -AddToPath
```

Omit `-AddToPath` to leave the user environment unchanged. The installer prints
the exact per-user `bin` directory in either case. Open a new terminal after a
PATH change, then inspect the installation:

```powershell
wv version
wv tools
wv doctor
```

Uninstall with the installed script or the matching extracted script:

```powershell
& "$env:LOCALAPPDATA\Windvale\Uninstall-Windvale.ps1"
```

## Install on Linux x64

Extract and run:

```sh
tar -xzf windvale-0.1.0-dev.1-linux-x64.tar.gz
cd windvale-0.1.0-dev.1-linux-x64
./install.sh
wv version
wv tools
wv doctor
```

The default command links are placed in `${XDG_BIN_HOME}` or
`${HOME}/.local/bin`. The installer reports when that directory is not currently
on PATH. Use `--no-links` to publish only the installation-local command shims,
or `--root` and `--bin-dir` for an explicit per-user policy.

Uninstall with:

```sh
~/.local/share/windvale/uninstall.sh
```

Use the corresponding XDG data location when `XDG_DATA_HOME` is set.

## Current boundary

The development installer verifies bytes and modes, installs one immutable
generation, detects later tampering, and removes only its recorded installation.
It is unsigned and has no updater, rollback, registry, application package,
capability approval, or release trust root. Do not redistribute it as an official
release until the remaining Milestone 3 gates are complete.
