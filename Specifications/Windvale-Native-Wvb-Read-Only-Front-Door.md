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

Decision 0520 makes the pinned host WvDump application own its no-argument
self-test, canonical Sum report, and deterministic bad-magic report inside both
broad Seed scripts. The helper checks the application identity before direct
execution and rechecks both input digests afterward. Reference-runtime
capability refusal and missing/empty-resource diagnostics remain separate
managed contracts.

## Pinned inventory

`Artifacts/Native-Front-Door/Manifest.json` and `SHA256SUMS` bind twelve artifacts: the existing build driver and publisher WVB/applications plus these six read-only-tool artifacts:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `Wvb/Compiler-Wvb-Verifier.wvb` | 125,721 | `259db7fc70679153982ca70843cf002e87b786d04ebeb0eafb628207f44c723f` |
| `Wvb/Wvb-Inspector.wvb` | 76,527 | `293be3267ff95f9272e96684e036a5647abc060f2bc87a9e654beac7140af753` |
| `windows-x64/wvverify.exe` | 1,007,104 | `f15422397ad890909f481f131f945e25651c858695ba5ce58b2a7305b34647f0` |
| `windows-x64/wvdump.exe` | 795,136 | `61512dae2941607b93da7d29dd59f973c690f0fec3ba24f772f2101c87ed5381` |
| `linux-x64/wvverify.elf` | 1,007,616 | `dd98cd8f42ee8237b030d96dd1305e23843f92ae7dfd92469a67579e2cbe718a` |
| `linux-x64/wvdump.elf` | 794,624 | `d3215e8345bf5cd9f3265b8421cf57d456ae605c5493fcc215a3e11daab44627` |

The verifier artifacts retain their previously qualified identities. The inspector WVB reuses the existing complete Windvale `Wv-Dump-Core.wv` implementation rather than introducing a second parser or report generator.

## Recovery boundary

Stage 0 currently remains responsible for reconstructing the packages, independently parsing their containers, and running repository qualification. After this front door qualifies, ordinary documentation and developer use select the native commands; `dotnet ... verify` and `dotnet ... inspect` remain only in an explicit recovery/differential lane.

Current Stage 0 application-writer tests do not duplicate the pinned application
digests above. They reconstruct each host application twice, require byte
equality, independently verify its profile and bundle, compare it with current
CLI AOT output, execute accepted and rejected current-host inputs, and prove no
CLR module or mapping was loaded. Pinned product identity and current recovery
writer determinism are intentionally distinct evidence.

This slice does not retire .NET from native lowering, package construction, general execution, test orchestration, assembly, linking, release production, or final recovery. Those are separate inventory items under [Decision 0057](../Documents/Decisions/0057-Windvale-Native-Execution-And-Dotnet-Retirement.md).
