# Decision 0427: Native normal bootstrap verification

- Status: Implemented candidate; Windows native seed path previously passes
- Date: 2026-08-09
- Advances: [Decision 0213](0213-Stage0-Semantic-Freeze-And-Native-Front-Door.md)
- Contract: [Native compiler seed bootstrap](../../Specifications/Windvale-Native-Compiler-Seed-Bootstrap.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

The ordinary bootstrap verifier still built the Stage 0 C# tool, compiled the
current compiler to Stage 1, ran Stage 1 in the managed reference runtime to
produce Stage 2, and compared both WVB files. That is valuable independent
convergence evidence, but it kept `dotnet` in a named normal verification path
after the digest-bound native compiler seed could already rebuild and publish
the accepted compiler WVB on both permanent hosts.

The two proofs answer different questions. Native seed bootstrap proves that a
documented prior native release can rebuild the current compiler without a
managed product dependency. Managed Stage 1/Stage 2 convergence proves bytecode
self-reproduction through the simple reference runtime. Retaining the latter
does not require treating it as the ordinary bootstrap route.

## Decision

Make `Tools/Verify/Verify-Bootstrap.cmd` and `.sh` the ordinary bootstrap
verification entry points. They delegate only to the digest-bound native seed
launchers, rebuild the exact current compiler inventory, require the pinned
921,900-byte WVB identity, and publish through the qualified native publisher.

Move the former PowerShell and shell convergence procedures to
`Tools/Recovery/Verify-Managed-Bootstrap.ps1` and `.sh`. Their names, output
paths, and messages explicitly identify managed recovery. Preserve their Stage
0 build, Stage 1 execution, independent Stage 2 verification, and byte-equality
checks; do not port those steps line for line into another normal harness.

Keep the small host wrappers focused. The Windows normal route is a command file
because PowerShell itself loads CoreCLR and cannot provide no-.NET process
evidence. The Linux route remains a POSIX shell script.

## Evidence and consequences

The underlying Windows native seed bootstrap already passes from both the
checked-in artifact root and a copied seed root, produces the exact accepted
compiler WVB, rejects an altered seed, preserves an existing destination, and
does not load a managed runtime in its compiler or publisher children. This
change reassigns entry-point ownership; it does not change compiler semantics,
artifact identities, source inventory, or the native seed implementation.

The direct managed-entry inventory retains the two convergence scripts as
recovery-only entries and removes them from the verification lane. The normal
wrapper contracts, inventory classification, documentation paths, and recovery
paths receive focused static verification in this slice. The expensive native
compiler rebuild is not repeated because its implementation and admitted
inputs are unchanged and its latest passing result is reused.

[Decision 0428](0428-Native-Compiler-Self-Convergence.md) adds the all-native
Stage 1 → Stage 2 proof. Windows now packages and executes the newly rebuilt
Stage 1 compiler through the compiler-scale pipeline and requires exact Stage 2
equality. Linux execution, paired-host promotion, later-release seed consumption,
and the final grouped retirement gate remain open.

## Reconsideration triggers

Replace the recovery convergence scripts only when an independently qualified
native Stage 1 → Stage 2 proof provides at least the same exact inventory,
output, failure, and cross-host evidence. Do not delete the managed recovery
proof before the final Stage 0 recovery release and Decision 0057 gate are
complete.
