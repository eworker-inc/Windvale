# Windvale native compiler seed bootstrap

## Status and scope

This document defines the implemented candidate for rebuilding the accepted
Windvale compiler WVB from a documented native compiler seed without invoking
.NET. It advances the clean-bootstrap condition in Decision 0057; it does not
complete that condition until the seed is promoted by an independent Windows and
Linux qualification and a later accepted source state consumes that previous
release.

The version-1 bootstrap is intentionally narrower than a complete native toolchain
bootstrap. It rebuilds and independently publishes the canonical compiler WVB. It
does not yet rebuild the seed compiler PE/ELF, the complete native backend, every
developer tool, or the final Stage 0 recovery archive without .NET.

## Seed and source identities

`Artifacts/Native-Compiler-Seed/Manifest.json` binds one canonical WVB and paired
format-3 Windows/Linux compiler applications to byte lengths, SHA-256 identities,
source commit, source tree, target, and qualification provenance. `SHA256SUMS`
repeats the three artifact identities for external tools.

The seed compiler source is the qualified semantic-freeze tree at commit
`524e84afb6e5bab6bbd95ebc0b9eeaf886af834b`. Commit
`3824f39d0997e3d7ab523f7cc1fe0f4bd8288e35` remains the exact Stage 0 recovery
state for the last 921,900-byte convergence candidate. Decision 0491 repins the
ordinary bootstrap contract to the current explicit inventory in
`Projects/Examples/Windvale-Compiler.wvproj`. With Decision 0713's capability
catalog extension, the seed emits a transitional 947,975-byte Stage 1 WVB at
SHA-256
`c929d5123078272e33a3c32288c770d6c20c2abc8f8800a3e0a32b8bda5c2fcb`.
The launcher packages that private compiler and uses it once to emit the stable
923,818-byte Stage 2 WVB at SHA-256
`49b5cbf040de4bcb22c071a5da9a4fbad47f4f0658ef910957a67b52c07607c2`.
Decision 0494 retains the unqualified current WVB and paired reconstruction in
`Artifacts/Native-Compiler-Reconstruction-Candidate`; exact checkpoint
qualification remains pending on both hosts.

The bootstrap artifact root contains both `Native-Compiler-Seed` and the qualified
`Native-Front-Door` publisher inventory. This permits a released artifact bundle
to be copied outside the source checkout and admitted before use without
duplicating the publisher binaries in two checked-in inventories.

## Host launchers

Windows x64 uses `Tools/Native/Bootstrap-Compiler.cmd`; Linux x64 uses
`Tools/Native/Bootstrap-Compiler.sh`. PowerShell is not the native bootstrap host:
PowerShell 7 loads CoreCLR and therefore cannot provide no-.NET process evidence.
It remains suitable for the explicitly retained recovery lane.

Each launcher:

1. admits the exact seed WVB, current-host seed compiler, qualified native
   publisher, and compiler project manifest by length and SHA-256;
2. passes the fixed project inventory to the seed compiler in manifest order;
3. requires the exact transitional Stage 1 identity, packages it through the
   pinned segmented compiler profile, and repeats the fixed source invocation;
4. requires the exact fixed-point Stage 2 compiler WVB length and SHA-256; and
5. invokes the qualified native publisher, which repeats compiler-aligned
   verification and atomically replaces the requested output.

Any identity, compilation, verification, or publication failure preserves an
existing destination. The launchers remove both private generations and their
private compiler on success or failure. They do not parse project syntax,
discover sources, infer imports, or install host dependencies; the explicit
promotion uses the already pinned segmented native compiler packager.

## Bounded source-WVB product launchers

`Tools/Native/Build-Source-Compiler-Product.cmd` and `.sh` expose the admitted
compiler seed for the current source-WVB `core`, `demo`, and `tool` products.
This bounded route exists because the pinned generic native Project build-driver
artifact does not compile the current WVB core/tool closure, while the compiler
seed application directly reproduces all three exact products. It is a removable
normal-construction seam, not a replacement Project format or a new compiler.

Each launcher admits the seed and publisher inventory, requires the exact
selected repository-root manifest identity, passes the fixed source inventory
for that product, writes a process-private candidate, and delegates final
verification and atomic replacement to the qualified publisher. The tool variant
reuses `Projects/Examples/Windvale-Compiler.wvproj`; core and demo use their focused aggregates.
Invalid product, arity, or output-suffix usage returns 64. Any admission,
compilation, or publication failure preserves an existing destination.

The Windows launcher pins the exact selected compiler, publisher, compiler-WVB,
and manifest files. The Linux launcher verifies the complete seed and
front-door `SHA256SUMS` inventories before selecting the current-host
applications. Independent Linux execution and grouped qualification remain
required before this current product transfer is a cross-host claim. A rebuilt,
qualified generic Project driver that compiles this closure should replace the
bounded launchers.

## Recovery and promotion

`Tools/Recovery/Rebuild-Native-Compiler-Seed.ps1` and its Bash peer reconstruct the
three seed artifacts from exact Git archives. This is an explicit Stage 0 recovery
operation and deliberately invokes the pinned .NET SDK; it is not the native
bootstrap route. Reconstruction always targets a separate directory and must
reproduce the checked-in identities exactly.

Promotion requires the same copied-seed operation on Windows and pinned Linux,
exact compiler output agreement, no CLR/.NET mapping in the compiler and publisher
children, destination-preservation checks, malformed or altered seed rejection,
and an exact-commit qualification reference in the seed manifest. A later release
must then use the promoted previous seed before Decision 0057 condition 7 can be
called complete.

## Native self-convergence coordinator

The ordinary bootstrap verification entry points require more than the internal
seed-to-fixed-point promotion. `Tools/Native/Verify-Compiler-Convergence.cmd`
and `.sh` admit the native verifier, build and publish the fixed-point compiler
through the seed launchers, package that exact WVB through the segmented
compiler-WVB profile, execute it over the canonical project inventory,
independently verify the next generation, and require complete byte equality.

`Tools/Native/Compile-Compiler-Source-Set.cmd` and `.sh` own the exact ordered
source invocation shared by the seed and convergence routes. They do not discover
files, parse the project, or weaken the project-manifest identity check.

The coordinators are repinned to require identical 923,818-byte outputs at
SHA-256
`49b5cbf040de4bcb22c071a5da9a4fbad47f4f0658ef910957a67b52c07607c2`.
The current toolset and paired compiler applications are reconstructed as
unqualified candidates. Exact equality on Windows and Linux and promotion remain
part of the grouped retirement qualification; a local or focused pass does not
replace a named qualification decision.
