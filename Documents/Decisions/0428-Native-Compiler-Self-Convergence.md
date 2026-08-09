# Decision 0428: Native compiler self-convergence

- Status: Implemented candidate; Windows exact convergence passes
- Date: 2026-08-09
- Advances: [Decision 0427](0427-Native-Normal-Bootstrap-Verification.md)
- Contract: [Native compiler seed bootstrap](../../Specifications/Windvale-Native-Compiler-Seed-Bootstrap.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

Decision 0427 moved the ordinary bootstrap entry point from the managed Stage 0
convergence harness to the digest-bound native compiler seed. That route rebuilt
and published Stage 1 without .NET, but stopped before proving that the newly
packaged Stage 1 compiler could reproduce itself. The managed recovery harness
remained the only Stage 1 to Stage 2 byte-equality proof.

The native compiler-scale hosted pipeline can now package and execute the full
compiler without raising its qualified bounds. The ordinary bootstrap verifier
can therefore answer both bootstrap questions in one native route: whether the
accepted previous seed builds the current compiler and whether that resulting
compiler reproduces the same WVB.

## Decision

Add paired `Tools/Native/Verify-Compiler-Convergence.cmd` and `.sh` coordinators.
Each coordinator:

1. admits the qualified native verifier;
2. builds Stage 1 through the versioned native compiler seed;
3. packages Stage 1 through the segmented compiler-WVB profile;
4. executes that newly packaged Stage 1 compiler over the exact project source
   inventory to produce Stage 2;
5. independently verifies the pinned Stage 2 identity; and
6. requires complete Stage 1 and Stage 2 byte equality.

Keep the exact ordered thirteen-source invocation in paired focused
`Compile-Compiler-Source-Set` launchers shared by seed bootstrap and convergence.
The ordinary `Tools/Verify/Verify-Bootstrap.cmd` and `.sh` wrappers now call the
convergence coordinator. The managed Stage 0 proof stays under `Tools/Recovery`
as frozen recovery and differential evidence.

## Evidence and consequences

The Windows coordinator completed in 341.3 seconds. Both stages were 921,900
bytes with SHA-256
`fd96bd567d08a18107a9b149560ce9f2e38b49454250e934a4375f465d132556`.
Each verified module contained 419 functions and 760,855 code bytes. Native
packaging admitted nine fixed-service resources, transported their source
geometry as 17 chunks, emitted seven service-bundle segments, transported the
compiler source set as 12 chunks, and emitted seven final application segments.
The newly packaged Stage 1 executable produced Stage 2; the native verifier
accepted it and the final byte comparison passed.

An altered verifier is rejected before convergence scratch is created, and the
successful run leaves no private convergence directory. No compiler, transport,
resource, or instruction bound was raised for this proof.

The Linux coordinator implements the same contract. Its execution, independent
paired-host promotion, later-release seed consumption, and the final grouped
retirement gate remain deliberately pending for the goal-end qualification.

## Reconsideration triggers

Do not duplicate the compiler source order in future coordinators; update the
project inventory and its shared source-set launcher together. Retain the managed
recovery proof until the final digest-bound Stage 0 recovery release and complete
Decision 0057 gate permit managed-source deletion.
