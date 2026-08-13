# Decision 0277: Native WebAssembly compiler regeneration

- Status: Accepted with exact Windows-local execution evidence
- Date: 2026-08-06
- Scope: portable browser-compiler WVB regeneration
- Builds on: [Decision 0169](0169-Public-Format3-Compiler-Targets.md), [Decision 0213](0213-Stage0-Semantic-Freeze-And-Native-Front-Door.md), [Decision 0266](0266-Pinned-WebAssembly-Playground-Package.md), and [Decision 0275](0275-Normal-Browser-Native-Playground.md)
- Retains: WVB 1.11, ABI 22, `WVHA 1`, the 48-billion-instruction compiler ceiling, the 128 MiB hosted dynamic arena, the canonical compiler semantics, and Stage 0 as an explicit recovery route

## Context

The normal browser playground no longer starts .NET, but its 919,577-byte portable compiler WVB still carried `stage0-recovery-baseline` provenance. The ordinary pinned native build driver could not replace that provenance: two bounded current-host attempts against `Projects/Compiler/Windvale-Compiler-Memory.wvproj` ran for about 50.5 seconds, exited with status 1, emitted no Windvale diagnostic, and published no candidate. The project has only thirteen sources and 1,099,733 aggregate source bytes, so it is below the build driver's 63-source and 4 MiB `WVSS 1` limits. The observed boundary is the combined build-driver/compiler/in-process-verifier execution envelope, not project admission.

The already-qualified format-3 hosted compiler profile is smaller at the relevant boundary. It owns project-independent source compilation and the same six explicit capabilities, but does not retain project parsing and a second in-process verifier inside the compiler's dynamic arena. A launcher can parse the small version-1 project manifest outside the guest, pass its ordered immutable sources to this native compiler, and preserve publication safety by using the separately pinned native WVB publisher.

## Decision

- Pin the exact current hosted compiler WVB and paired `windows-x64-console-v3` / `linux-x64-console-v3` packages under `Artifacts/WebAssembly-Native-Compiler`.
- Make `Tools/WebAssembly/Build-Compiler-Wvb.mjs` the normal regeneration route for `Projects/Compiler/Windvale-Compiler-Memory.wvproj`. It supports only Windows x64 and Linux x64, validates the package format and platform, verifies the complete selected compiler and native publisher identities before execution, strictly reads the version-1 project inventory, and rejects paths outside the repository.
- Let the native compiler write only a unique temporary candidate. Pass that candidate to the separately pinned Windvale-native publisher for verifier-admitted atomic replacement. `--check` publishes only inside a unique temporary directory and requires byte-for-byte equality with the checked-in browser compiler.
- Keep Stage 0 out of the normal launcher. `Tools/Recovery/Rebuild-WebAssembly-Native-Compiler.ps1` is the explicit recovery-only route that reconstructs the WVB and both native containers through the feature-frozen C# bootstrap, then requires every byte count and SHA-256 identity to match the pinned inventory.
- Change the portable browser compiler's production provenance from `stage0-recovery-baseline` to `pinned-native-source-compiler`. Retain the independently documented Stage 0 reconstruction path rather than deleting bootstrap evidence.
- Do not claim that this slice regenerates the interpreter WebAssembly module without Stage 0. Applying the Windvale-authored WVB-to-WebAssembly backend remains the next artifact-production seam.

## Exact evidence

Stage 0 recovery constructs the 921,900-byte hosted compiler WVB at SHA-256 `fd96bd567d08a18107a9b149560ce9f2e38b49454250e934a4375f465d132556`. Its 27,547,648-byte Windows package has SHA-256 `cba2f0754fac93cd9e79f4d87340a95cddd6491d17ec4d1ae169194a22a14c21`; its 27,549,696-byte Linux package has SHA-256 `67a04374c611c7880f916639e63462af818ed807b8cf0feda9423a11a02ddbbe`.

On the measured Windows host, the standalone Windows package reproduces its own 921,900-byte WVB byte for byte in 45.279 seconds. It separately compiles `Projects/Compiler/Windvale-Compiler-Memory.wvproj` in 45.905 seconds and publishes the exact 919,577-byte browser compiler at SHA-256 `2bf84dc2a8cbb80c52ec7fb6cb2e29eef27def1707f398a276c61063d73df06e`. Both native runs report valid source-WVB summaries and exit zero. The package construction itself takes 8.877 seconds for Windows and 8.610 seconds for Linux on that host.

These are exact current-host bootstrap and execution measurements, not a new dual-host qualification claim. The underlying format-3 contracts retain their earlier cross-host qualification. Independent Linux execution of these exact current package identities remains useful promotion evidence.

## Consequences

The normal website artifact-production path can now regenerate, verify, and atomically publish its portable source compiler WVB without starting .NET. Website build and deployment still consume only the three small browser package artifacts; the native compiler packages are development/toolchain inputs and are not copied into the site.

The failed general build-driver route remains valuable for ordinary projects and is not weakened or replaced globally. WebAssembly compiler regeneration uses a narrower application whose measured ownership boundary fits the current compiler. The additional pinned PE/ELF bytes are deliberate bootstrap artifacts with explicit source WVB, target, hashes, reconstruction, and reconsideration rules.

## Reconsideration triggers

Revisit this decision if:

- either native compiler package fails to reproduce the exact browser compiler on its target host;
- the source project outgrows 64 modules, the 4 MiB source-set contract, 128 MiB dynamic arena, 48-billion-instruction ceiling, or 64 MiB native bundle limit;
- a qualified Windvale-native package constructor can reproduce the format-3 containers without Stage 0;
- project parsing moves behind a smaller Windvale-native manifest adapter without restoring the failed combined envelope; or
- the normal regeneration route, website build, or deployment starts .NET.
