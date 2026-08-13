# Exact compiler Stage 0 recovery

## Purpose and boundary

This runbook reconstructs the canonical compiler WVB and paired format-3 Windows/Linux compiler applications from a clean repository checkout while C#/.NET remains the explicit Stage 0 recovery implementation. It records enough source, dependency, command, and artifact identity to repeat or audit the reconstruction. It does not by itself retire .NET, replace the dual-host Qualification gate, or constitute the final archived recovery release required before Stage 0 leaves normal automation.

The recovery dependency closure is deliberately small:

- One exact Git commit and its complete tree.
- The .NET SDK selected by `global.json`; the currently qualified version is `10.0.302`.
- The `Tools/Windvale.Tool` project-reference closure. That closure has no external `PackageReference`; the optional browser playground packages are not dependencies of the compiler tool.
- The canonical twelve-source inventory in `Projects/Examples/Windvale-Compiler.wvproj`.

Record the output of `git rev-parse HEAD`, `git rev-parse HEAD^{tree}`, `dotnet --version`, and the final artifact hashes with every retained recovery copy.

Exact commit `db20fefaa3333b7b78392ba12141d1ae2b6bb0c2` is the first paired direct-execution baseline. GitHub [Verify run 30816153900](https://github.com/eworker-inc/Windvale/actions/runs/30816153900) reconstructs both containers from clean checkouts, runs the PE on Windows and ELF on digest-pinned Debian 12, and reports every identity below unchanged. Exact public/recovery commit `57d154c1f6758315692e35a47939d51702d5c96b` then passes isolated GitHub [Verify run 30819768981](https://github.com/eworker-inc/Windvale/actions/runs/30819768981), qualifying the public writers, `aot` route, shared atomic publisher, and this recovery boundary on both hosts.

## Clean-checkout reconstruction

Start from a checkout with no tracked modifications:

```powershell
git status --short
git rev-parse HEAD
git rev-parse 'HEAD^{tree}'
dotnet --version
```

`git status --short` must be empty and `dotnet --version` must report `10.0.302` for the current evidence. First reproduce bytecode compiler convergence through the explicitly retained managed recovery procedure:

```powershell
pwsh -NoProfile -File Tools/Recovery/Verify-Managed-Bootstrap.ps1
```

```sh
./Tools/Recovery/Verify-Managed-Bootstrap.sh
```

Then build the Stage 0 tool once and use the canonical project manifest for every recovered artifact:

```powershell
dotnet build Tools/Windvale.Tool/Windvale.Tool.csproj --configuration Release --nologo
New-Item -ItemType Directory -Force artifacts/Exact-Compiler-Recovery | Out-Null
$Tool = 'Tools/Windvale.Tool/bin/Release/net10.0/windvale.dll'
dotnet $Tool build Projects/Examples/Windvale-Compiler.wvproj `
  -o artifacts/Exact-Compiler-Recovery/Windvale-Compiler.wvb
dotnet $Tool aot artifacts/Exact-Compiler-Recovery/Windvale-Compiler.wvb `
  --target windows-x64-console-v3 `
  -o artifacts/Exact-Compiler-Recovery/Windvale-Compiler.exe
dotnet $Tool aot artifacts/Exact-Compiler-Recovery/Windvale-Compiler.wvb `
  --target linux-x64-console-v3 `
  -o artifacts/Exact-Compiler-Recovery/Windvale-Compiler.elf
```

```sh
dotnet build Tools/Windvale.Tool/Windvale.Tool.csproj --configuration Release --nologo
mkdir -p artifacts/Exact-Compiler-Recovery
tool=Tools/Windvale.Tool/bin/Release/net10.0/windvale.dll
dotnet "$tool" build Projects/Examples/Windvale-Compiler.wvproj \
  -o artifacts/Exact-Compiler-Recovery/Windvale-Compiler.wvb
dotnet "$tool" aot artifacts/Exact-Compiler-Recovery/Windvale-Compiler.wvb \
  --target windows-x64-console-v3 \
  -o artifacts/Exact-Compiler-Recovery/Windvale-Compiler.exe
dotnet "$tool" aot artifacts/Exact-Compiler-Recovery/Windvale-Compiler.wvb \
  --target linux-x64-console-v3 \
  -o artifacts/Exact-Compiler-Recovery/Windvale-Compiler.elf
```

The project build consumes the manifest order and strict UTF-8 source bytes once. Both AOT calls verify the same emitted WVB before lowering it and independently verify the complete format-3 container before the shared unique-sibling atomic publisher exposes it. On Linux, the ELF is published with exact mode `0755`.

## Required artifact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `Windvale-Compiler.wvb` | 599,868 | `9673bf3331763181f443ec67b7a513bc66daa718969f7f6b0d197a4186071066` |
| `Windvale-Compiler.exe` | 17,157,120 | `356bd9c6be1a927017e987728b479d105f9852c0c7aad1b8b9e93202ba64010f` |
| `Windvale-Compiler.elf` | 17,158,144 | `42f3f947cccca8e44c279afce1b6e944682dc440e0e9cda6546883898d951f31` |

Verify sizes and hashes before execution or transfer. A mismatch is a recovery failure; do not bless or normalize a new identity from this runbook.

```powershell
Get-ChildItem artifacts/Exact-Compiler-Recovery/Windvale-Compiler.* |
  Select-Object Name,Length,@{Name='SHA256';Expression={(Get-FileHash -Algorithm SHA256 -LiteralPath $_.FullName).Hash.ToLowerInvariant()}}
```

```sh
wc -c artifacts/Exact-Compiler-Recovery/Windvale-Compiler.*
sha256sum artifacts/Exact-Compiler-Recovery/Windvale-Compiler.*
```

## Direct Stage 2 and host evidence

The exact-compiler AOT qualification case owns direct execution so its source inventory, expected status line, WVB comparison, and .NET-module rejection cannot drift into a second suite:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1 `
  -Level Fast `
  -TestArea compiler `
  -TestFilter 'exact compiler AOT transport pressure' `
  -IncludeExtended `
  -FailFast
```

```sh
VERIFY_LEVEL=fast TEST_AREAS=compiler \
TEST_FILTER='exact compiler AOT transport pressure' INCLUDE_EXTENDED=1 FAIL_FAST=1 \
  ./Tools/Verify/Verify-Seed.sh
```

On the current host that case executes the named raw application over the canonical twelve-source inventory, requires process status zero and the exact 599,868-byte WVB above, and rejects CLR/.NET host or runtime mappings. The complete dual-host Qualification workflow remains the authority for paired Windows and digest-pinned Debian evidence.

## Archive provenance

Before Stage 0 is removed from normal automation, create the final recovery archive from the exact qualified commit rather than from a working-directory copy:

```powershell
git archive --format=tar --prefix=windvale-stage0/ HEAD `
  -o artifacts/Exact-Compiler-Recovery/Windvale-Stage0-Source.tar
Get-FileHash -Algorithm SHA256 `
  artifacts/Exact-Compiler-Recovery/Windvale-Stage0-Source.tar
```

```sh
git archive --format=tar --prefix=windvale-stage0/ HEAD \
  -o artifacts/Exact-Compiler-Recovery/Windvale-Stage0-Source.tar
sha256sum artifacts/Exact-Compiler-Recovery/Windvale-Stage0-Source.tar
```

Retain together: the commit and tree identities, source archive and digest, exact SDK identity and acquisition record, this runbook, the three recovered artifacts and digests, the Windows/Linux Qualification run URL, and the separately retrieved reports when the final retirement gate is performed. Do not archive local SDK installations, NuGet caches, build outputs other than the named recovery artifacts, credentials, or machine-specific configuration.
