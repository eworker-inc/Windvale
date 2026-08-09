# Decision 0058: Reproducible compiler bootstrap convergence

- Date: 2026-07-31
- Status: Qualified on Windows and Debian at `5c1654726f0288808d2c1d3918d65ec1c59b4635`

## Context

Decision 0055 made the complete typed-WVIR compiler workload fit below its unchanged four-billion-instruction gate. That cleared the entry condition for the actual bootstrap experiment: compile the complete Windvale compiler with Stage 0, run that Stage 1 compiler over the same explicit source inventory, independently verify the resulting Stage 2 module, and compare the two complete artifacts.

The first full Stage 1 attempt still exhausted the separate 8,000,000,000-instruction bootstrap ceiling after approximately 456 seconds and published no Stage 2 output. Per-function evidence showed that the next costs were structural rather than missing language semantics. Canonical WVB emission repeatedly reranked every symbol by rescanning the complete declaration directory, while each declaration read reconstructed its absolute source position by rescanning from the module header. Equality-only identifier paths also paid for forward ordinal comparisons across the long common `Compilerˉ` prefix.

The acceptance ceiling remains engineering pressure rather than a value to raise until a result appears. The compiler must retain its deterministic source, WVSS, symbol, typed-WVIR, WVB, verifier, and publication boundaries.

## Decision

Use three evidence-preserving optimizations and make full convergence a dedicated verifier:

- Add `Compilerˉsourceˉspansˉequal` for equality-only comparisons of already validated source spans. It rejects unequal lengths and scans equal-length spans from the end. Ordering continues to use the existing unsigned ordinal comparison.
- Build immutable entry-to-rank and rank-to-entry byte tables once for capabilities, data, records, enums, and functions. WVB capability, data, type, code, function, export, and call emission reuse those tables instead of repeatedly ranking the complete WVSD directory. The existing public rank helpers remain available as independent reference operations.
- After the complete source set and semantic evidence have succeeded, parse declarations for emission from their accepted byte offsets with relative coordinates. Absolute diagnostic positions remain the responsibility of the checked upstream source boundary; backend emission consumes accepted offsets and shapes and cannot publish bytes after an upstream failure.

The public `WVSS 1`, `WVSD 1`, `WVLB 1`, `WVIR 1`, and `WVB 1.6` formats, language semantics, canonical ordering, diagnostic identities, and five Stage 0 differential fixture outputs do not change.

The scripts now retained as `Tools/Recovery/Verify-Managed-Bootstrap.ps1` and
`Tools/Recovery/Verify-Managed-Bootstrap.sh` own the expensive proof outside the
Fast, Standard, and ordinary Qualification inner loop. They use one canonical
root plus eleven explicit dependencies, build Stage 1 with the C#
reference/recovery compiler, execute Stage 1 with only its six declared hosted
capabilities, independently verify Stage 2, and require complete Stage 1/Stage 2
byte equality. This decision's original default ceiling was 8,000,000,000
instructions; the retained recovery scripts now default to the later measured
48,000,000,000-instruction compiler envelope.

## Rejected alternatives

Merely raising the instruction ceiling was rejected because it would hide repeated work and weaken the measured gate.

A first equality-only experiment reduced a medium compiler workload by only about 17.7 million net instructions and still failed its four-billion ceiling. Equality remains useful, but it was not accepted as the bootstrap solution by itself.

Repeated `symbolˉrank`/`symbolˉbyˉrank` scans were rejected after profiling exposed their multiplicative cost. Nested order records were unavailable in the current Seed subset, so the accepted representation uses flat immutable byte fields rather than adding a language feature solely for this optimization.

The first order-table insertion implementation failed to advance its scan cursor after finding an insertion point. A bounded diagnostic run reached its ceiling without output; the loop was corrected before any candidate artifact was accepted.

Reduced or incomplete dependency inventories are useful performance probes but are not bootstrap evidence. Qualification uses the exact complete inventory embedded in the dedicated verifier.

## Consequences

On the medium Source Graph compiler closure, the original qualified compiler exceeded 4,000,000,000 instructions. Precomputed order tables reduced the candidate to 2,755,343,813 instructions, and removal of redundant declaration-position rescans reduced it further to 2,258,073,874. Every successful version produced the same independently verified 210,509-byte module with SHA-256 `2d2fa7ae2cca012834fb340253a551f9332a764200bb8f6449158b8dad4b30b2` as Stage 0.

The canonical bootstrap inventory contains 12 modules and 677,073 source bytes. Stage 0 produces a verified 599,868-byte Stage 1 compiler. Stage 1 compiles the same inventory successfully in 6,700,562,174 VM instructions and reports:

```text
source wvb status=Valid functions=328 code-bytes=481356 module-bytes=599868
Result: 0
Instructions: 6700562174
```

The independently verified Stage 2 module is byte-for-byte identical to Stage 1. Both have SHA-256 `9673bf3331763181f443ec67b7a513bc66daa718969f7f6b0d197a4186071066`.

This is bytecode compiler self-reproduction: a Windvale-written compiler running as verified Windvale bytecode compiles its own complete source inventory to the canonical artifact. It does not claim native compiler execution, a Windvale-native VM, retirement of C#/.NET from the normal workflow, or execution inside Windvale OS. The C# runtime remains the current host and independent verifier for this proof; Decision 0057's later native-retirement gate governs when that implementation leaves normal automation and becomes archived recovery/provenance evidence.

The real closure fits comfortably inside WVSS 1's 4 MiB envelope. Parity with Stage 0's 16 MiB aggregate source limit is therefore no longer a prerequisite for this bootstrap milestone, though it remains an explicit future limit decision.

## Verification

Before exact-commit qualification, the candidate passed focused source-set, canonical-WVB, and golden-contract tests. Windows Qualification then completed in 427.3 seconds with a 210.018-second 48-test suite; the dedicated bootstrap verifier completed in 386.8 seconds. The zero-warning build, all tests, complete native CLI verifier, independent Stage 2 verification, and exact Stage 1/Stage 2 comparison passed.

Exact candidate commit `5c1654726f0288808d2c1d3918d65ec1c59b4635`, tree `b00666be6a8b31d8868934e66751d8c6d8011cc7`, was archived as `windvale-bootstrap-5c1654726f02.tar.gz`, 2,764,889 bytes with SHA-256 `b8c7cc0193b149d5fab84b60c1d751f74f495a77090994fea6a57e4524b73051`. The same digest was verified after transfer, and the executable Linux verifier was checked before running from `/tmp/windvale-bootstrap-5c1654726f02-20260731` on the isolated E-Worker Debian QA host. The host ran Debian GNU/Linux 12 x64 with .NET SDK `10.0.302`; both reports identify .NET `10.0.10`.

Debian Qualification completed in 442.4 seconds with a 223.751-second 48-test suite; the dedicated bootstrap verifier completed in 423.3 seconds. It reproduced the exact 6,700,562,174 instruction count, 599,868-byte compiler size, and SHA-256 recorded above. The 15,563-byte Windows report has SHA-256 `6780bd68cfcf7dacea10d34f5a4b9d7eeb6cdc2c2c4a70cf055c6830429330f`; the 15,473-byte Debian report has SHA-256 `1fec4c222425f2737a7f41c415b1841f88aab0a8e7458b40d4e7d170e1d9c35d`. Their normalized contracts match.

All 61 directly retrieved portable artifacts, totaling 7,752,612 bytes, matched Windows byte for byte. The Debian evidence bundle is 2,293,322 bytes with SHA-256 `ce0555ded58b2dd1abcd3c4301dca7b45461efb0fd3d842d31fbb19d5cccdbbc`. The separately retrieved Debian Stage 2 compiler also has the canonical bootstrap SHA-256. After retrieval and comparison, the resolved exact QA directory, source archive, and evidence bundle were removed and confirmed absent. This qualifies bytecode compiler self-reproduction and roadmap Phase 8 without claiming any Decision 0057 native-runtime or .NET-retirement gate.
