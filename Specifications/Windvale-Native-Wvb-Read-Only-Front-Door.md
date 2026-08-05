# Native WVB read-only front door

## Status and scope

This contract defines the ordinary Windows and Linux commands for semantic verification and deterministic inspection of canonical WVB without loading .NET. The first implementation is checked in as a dual-host candidate; cross-host qualification must be recorded before the Stage 0 CLI variants become recovery-only commands.

The commands are:

```text
Tools\Native\Verify-Wvb.cmd <module.wvb>
Tools\Native\Inspect-Wvb.cmd <module.wvb>
./Tools/Native/Verify-Wvb.sh <module.wvb>
./Tools/Native/Inspect-Wvb.sh <module.wvb>
```

Each command accepts exactly one `.wvb` path. Invalid invocation returns `64`. The launchers use only operating-system command facilities and raw PE/ELF applications; they do not invoke PowerShell, the .NET CLI, a CLR host, or an ambient package manager.

## Verification and inspection

`Verify-Wvb` verifies the selected platform's pinned `wvverify` digest and executes that exact Windvale-authored compiler-aligned verifier. Acceptance returns `0` and writes:

```text
wvb status=Valid profile=compiler-aligned
```

Rejection returns `1` and writes the stable failed phase to standard error.

`Inspect-Wvb` verifies both the `wvverify` and `wvdump` digests. It first runs `wvverify`; rejection stops the command without invoking the inspector. After semantic acceptance, it executes the exact Windvale-authored inspector and emits the [`wvdump 1`](Wv-Dump-Report.md) report. This composition is mandatory because structural inspection is not semantic execution admission.

The report is checked against fixed contract text and artifact hashes. The normal result does not depend on running the C# inspector or comparing against a live .NET oracle. The frozen Stage 0 path remains useful for explicitly named differential and recovery tests.

## Pinned inventory

`Artifacts/Native-Front-Door/Manifest.json` and `SHA256SUMS` bind twelve artifacts: the existing build driver and publisher WVB/applications plus these six read-only-tool artifacts:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `Wvb/Compiler-Wvb-Verifier.wvb` | 125,721 | `259db7fc70679153982ca70843cf002e87b786d04ebeb0eafb628207f44c723f` |
| `Wvb/Wvb-Inspector.wvb` | 61,890 | `333fffcb26912aed969581d394bf0d3b8a093edfaafc565a43f8f700a8afb43d` |
| `windows-x64/wvverify.exe` | 1,007,104 | `f15422397ad890909f481f131f945e25651c858695ba5ce58b2a7305b34647f0` |
| `windows-x64/wvdump.exe` | 678,400 | `30f8c6cbb1555665063dfb70fa35f08d90818107298c6ab5b91f845814d22daa` |
| `linux-x64/wvverify.elf` | 1,007,616 | `dd98cd8f42ee8237b030d96dd1305e23843f92ae7dfd92469a67579e2cbe718a` |
| `linux-x64/wvdump.elf` | 679,936 | `4f99dc43e1af4ad074cc15a38bfe44a433af9979985a600739780ac156a52791` |

The verifier artifacts retain their previously qualified identities. The inspector WVB reuses the existing complete Windvale `Wv-Dump-Core.wv` implementation rather than introducing a second parser or report generator.

## Recovery boundary

Stage 0 currently remains responsible for reconstructing the packages, independently parsing their containers, and running repository qualification. After this front door qualifies, ordinary documentation and developer use select the native commands; `dotnet ... verify` and `dotnet ... inspect` remain only in an explicit recovery/differential lane.

This slice does not retire .NET from native lowering, package construction, general execution, test orchestration, assembly, linking, release production, or final recovery. Those are separate inventory items under [Decision 0057](../Documents/Decisions/0057-Windvale-Native-Execution-And-Dotnet-Retirement.md).
