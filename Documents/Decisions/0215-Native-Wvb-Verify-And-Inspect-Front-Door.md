# Decision 0215: Native WVB verify and inspect front door

- Date: 2026-08-05
- Status: Qualified on Windows and Debian at `e2d9c52548fd782a57765b1a9635d8cbe009df20`; GitHub Verify run 30977962784
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0213](0213-Stage0-Semantic-Freeze-And-Native-Front-Door.md)
- Builds on: [Decision 0185](0185-Standalone-Compiler-Wvb-Verifier-Applications.md) and [Decision 0069](0069-Dynamic-Native-Text-And-Complete-Wvdump.md)
- Contract: [Native WVB read-only front door](../../Specifications/Windvale-Native-Wvb-Read-Only-Front-Door.md)

## Context

The ordinary project source-to-WVB path is native, but the documented next commands still entered `dotnet` to verify or inspect the resulting module. A raw compiler-aligned verifier already existed on Windows and Linux. The complete Windvale-written `wvdump` also already ran through native JIT and WVO/AOT, but no standalone least-authority package or pinned ordinary launcher exposed it.

The inspector is structural: it can safely decode and report a bounded module, but its acceptance does not prove executable semantic validity. Replacing both commands therefore requires composition, not substituting one parser for the verifier.

## Decision

### Promote two digest-bound native commands

Add platform launchers for exactly these operations:

```text
Verify-Wvb <module.wvb>
Inspect-Wvb <module.wvb>
```

Verification executes the existing Windvale-authored compiler-aligned verifier. Inspection first executes that verifier and only then executes the complete Windvale-authored `wvdump`. Both launchers verify the exact raw application digest before execution and invoke no PowerShell, .NET CLI, CLR host, or ambient package tool.

Check the verifier WVB/application pair and inspector WVB/application pair into the existing native-front-door inventory with byte counts, SHA-256 digests, source projects, and target identities. Preserve the qualified verifier bytes exactly.

### Add a distinct read-only inspector package profile

Extend `WVHV 1` with metadata profile `4` for `wvdump`. It has the same exact five capabilities as the verifier: console output, diagnostics, file input, process arguments, and argument count. Its service bundle adds only the capability-free UTF-8, enum-name, concatenation, quoting, and integer-format leaves used by the report. It excludes `file.write_bytes`, a file-output table, and output scratch space.

Reuse the verifier's container, runtime-data, host-import, and independent parser implementation through an explicit profile parameter. Do not copy the package implementation.

Use separate Windows and Linux WVA startup modules only for the unavoidable pre-`Main` platform boundary: capture bounded arguments, bind read-only file/output state, bind the exact service pointers, and transfer control to Windvale. No verification, decoding, or report semantics live in assembly. This glue is already independent of .NET and can later be retired when a Windvale-native launcher or service manager owns capability binding.

### Keep one inspector implementation

`Windvale-Wvb-Inspector.wvproj` points to the existing `Examples/Foundation/Wv-Dump-Core.wv`. Do not create a parallel tool source merely to make the package look separate. The current file is large; a later source-only refactor may split envelope/payload scanning, instruction decoding, and report composition by ownership if it preserves the exact WVB/report contract. File size is a maintainability signal, not a hard limit, and numbered part files are not an acceptable substitute for real ownership boundaries.

### Use stable native oracles

Normal front-door tests compare exact artifact identities, fixed report text, structural assertions, malformed-input outcomes, and absence of CLR modules/mappings. They do not require a live C# inspector to calculate expected output. The frozen Stage 0 implementation remains only for explicitly named differential, package-reconstruction, and recovery evidence.

## Qualified implementation

The verifier artifacts retain their exact qualified identities. The inspector artifacts are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Inspector WVB | 61,890 | `333fffcb26912aed969581d394bf0d3b8a093edfaafc565a43f8f700a8afb43d` |
| Windows inspector | 678,400 | `30f8c6cbb1555665063dfb70fa35f08d90818107298c6ab5b91f845814d22daa` |
| Linux inspector | 679,936 | `4f99dc43e1af4ad074cc15a38bfe44a433af9979985a600739780ac156a52791` |

The focused package test compiles the real inspector, verifies the exact eleven-service/no-file-output profile, reconstructs both WVA templates, independently parses both containers, checks the public AOT target, runs the current-host raw application on a fixed golden module, rejects malformed magic, and checks that no CLR/.NET runtime is loaded. The native-front-door test verifies all twelve manifest artifacts and exercises both ordinary launchers with valid and rejected input.

Exact implementation commit `e2d9c52548fd782a57765b1a9635d8cbe009df20`
passes GitHub [Verify run 30977962784](https://github.com/eworker-inc/Windvale/actions/runs/30977962784).
Windows and digest-pinned Debian 12 each pass all 101 Seed tests, all 39 OS
tests, and the complete native CLI gate. The focused package/front-door cases
pass in 535/987 milliseconds on Windows and 484/587 milliseconds on Linux.
Windows completes in 24m47s and Linux in 15m55s.

## Retirement boundary and next item

The qualified Windows/Linux run makes ordinary WVB verification and inspection
native. The C# CLI commands remain available only as recovery/differential tools
until the final archive gate.

This decision does not retire the Stage 0 test runner or packaging implementation. The next coherent item is native execution: select and execute a verified WVB through one bounded native Windows/Linux front door. Test orchestration follows after that execution boundary is stable so the project does not port the monolithic C# harness line for line.

## Reconsideration triggers

Reconsider this profile if:

- `wvdump` requires any mutating capability;
- the inspector report ceases to be deterministic across permanent hosts;
- the compiler-aligned verifier no longer admits the ordinary accepted WVB surface;
- a shared native capability launcher can replace the fixed startups without widening authority; or
- dual-host qualification changes any pinned portable or platform artifact identity.
