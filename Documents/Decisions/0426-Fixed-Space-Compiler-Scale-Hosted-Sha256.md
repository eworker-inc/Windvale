# Decision 0426: Fixed-space compiler-scale hosted SHA-256

- Status: Implemented candidate; Windows hosted compiler packaging passes
- Date: 2026-08-09
- Advances: [Decision 0425](0425-Compiler-Scale-Native-Wvo-Resource-Staging.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

Decision 0425 staged and linked the complete compiler image through native
processes, but the hosted metadata-request process returned bounded status 1
while hashing the first 27 MiB identity region. The same streaming SHA-256 path
passed its 4 MiB fixture. Instrumented native execution completed chunks zero
through three and stopped inside chunk four, isolating the failure to cumulative
compression work rather than resource acquisition, manifest geometry, or the
compiler bytes.

The first scalar ring schedule removed transient byte construction but repeatedly
passed sixteen words through dynamic lookup helpers. That shape exhausted the
existing 48,000,000,000-instruction hosted-tool budget. Raising the budget would
have hidden inefficient portable code and weakened an already explicit bound.

## Decision

Keep the hosted instruction budget, the 64 MiB streaming input bound, the
4 MiB byte-value bound, and all serialized formats unchanged.

Use a fixed 64-word SHA-256 schedule expressed as scalar values. Expand each word
once, execute the compression rounds in four focused groups of sixteen, and
return one frame-owned working-state record between groups. Process every
contiguous complete-block range in one compression call. Add a range update to
the streaming owner so an identity region can consume the intersecting portion
of an immutable resource without first constructing a whole-resource slice.

Keep the compression implementation cohesive in
`Foundation/Sha256-Compression.wv`. A proposed cross-module record boundary was
valid under Stage 0 but rejected by the current Windvale-native source-binding
closure; retaining one 393-line focused file preserves the native build path and
follows the repository guidance not to split code merely to reduce line count.

The C# application writers remain exact Stage 0 recovery and differential
identity owners. They do not implement hashing, select the schedule, or execute
the normal native hosted pipeline.

## Evidence and consequences

The three affected modules and paired Stage 0 recovery containers are:

| Tool | WVB bytes | WVB SHA-256 | Windows bytes | Windows SHA-256 | Linux bytes | Linux SHA-256 |
| --- | ---: | --- | ---: | --- | ---: | --- |
| Metadata request | 63,278 | `55edb3633ee13f4ed7b02781e469c2d0325d8a0a8e274658a3bb06cc580bac04` | 1,052,672 | `4d1d5c114f9b022e594dd7d4abef2408143f9de60e4fa4bb00810316b5557366` | 1,052,672 | `8a4fb176439e2b71f98c244a98c04deec7985453038f3b2813de6fd6e179d4dd` |
| Streaming evidence | 48,364 | `95a112cc469c7667e8158cd57770a806501ede1bdea9a82a797b770b9e59dea4` | 914,432 | `16719d10c539c8950b620c7eee73e23d82a915b5e395977da9eabdf88e18e9a9` | 913,408 | `e0383452a56712748a17ffbe1f780817c338bf1900d3ef914706604ce592b6ea` |
| Final source set | 81,502 | `d8cb87c7c8b1da83572d13ff92c4555c16b19f44c1d649c5b5cb35f9e9fd60ce` | 1,280,512 | `a84dbdc7f96eafaab2ed17b076897338cfc86271be4ffddf4bef627d17d12083` | 1,282,048 | `872cca3fa39763a58b2183f9cf145d60c666d57fe0c7cd5984070cd55e1b6786` |

Their focused final-state tests pass metadata construction, a streaming region
that crosses the 4 MiB value boundary, final source-set reconstruction,
malformed-input preservation, exact Windows/Linux container identities, and
native Project 1 WVB reconstruction.

The promoted candidate toolset keeps 19 commands and a 5,426-byte
`SHA256SUMS` inventory. Its new inventory SHA-256 is
`35a48a3ed0080b5537dd38bdd6ccb3867794ac3a6f3d71c22f4afeaaa59f3e41`.

One Windows native hosted-packaging run reuses Decision 0425's preserved seven
compiler-image fragments. It admits 17 fragment/service resources, produces
seven service-bundle segments, emits twelve final source chunks, constructs
seven final application segments, and completes in about 149 seconds. The
result is the exact 27,467,776-byte compiler seed application with SHA-256
`344940f66b26b516b8b4e10a712a6b2c01cbff95aa7ff18aac0789ba9197f970`.
That reconstructed compiler turns `Examples/Seed/Sum-Data.wv` into the exact
494-byte WVB with SHA-256
`76b4fa3c4c0cc37e6f1350e8191ccd78c6272224f146ef9816b5f987114c15df`,
byte-for-byte matching the qualified native front door.

This closes the compiler-scale Windows metadata and hosted-composition blocker.
The Linux applications above are exact Stage 0 recovery constructions, not Linux
execution evidence. Linux native execution of the complete wrapper, candidate
promotion, and the final grouped dual-host qualification remain open.

## Reconsideration triggers

Revisit the explicit scalar schedule if the language gains a bounded mutable
fixed-width value with equivalent portable semantics and native support. Revisit
the four groups only if a measured compiler changes the native record-value or
instruction envelope. Do not increase the hosted instruction budget merely to
accommodate avoidable hashing overhead.
