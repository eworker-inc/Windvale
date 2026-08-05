# Decision 0221: First native Windvale linker front door

- Date: 2026-08-05
- Status: Implemented candidate; dual-host source qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0213](0213-Stage0-Semantic-Freeze-And-Native-Front-Door.md)
- Builds on: [Decision 0220](0220-First-Native-Wva-Assembler-Front-Door.md)
- Contract: [Windvale native linker](../../Specifications/Windvale-Native-Wv-Linker.md)

## Context

Windvale already owns the complete standard WVO-to-flat-image linker, including independent reconstruction, canonical maps, deterministic failures, and publish-after-success behavior. Normal linking still enters the C# CLI, so the product semantics exist in Windvale while the ordinary executable front door remains a .NET bootstrap dependency.

The linker initially used the semantic SHA-256 opcode and exact `console.write`. The current x64 subset does not lower that digest opcode, and the qualified shared startup exposes line output. The map also needs signed decimal rendering, but adding another service would require a new startup/service-table variant.

## Decision

### Compose one exact Windvale linker project

Add `Windvale-Wv-Linker.wvproj` with the linker core as root and its exact machine-contract, byte-ordering, decimal-parsing, byte-construction, and SHA-256 dependencies. Use the Windvale Foundation SHA implementation for input and image identities. Preserve the complete canonical map bytes; use `console.write_line` by removing the map's already-final LF before the call.

Format raw two's-complement map values through a small Windvale helper over `U32ˉformat`. Cover zero, signed maximum, signed minimum, and minus one in the embedded self-test. Do not add `I32ˉformat`, another startup template, or linker semantics in assembly.

### Add one fixed native linker profile

Add `windows-x64-wv-linker-v1` and `linux-x64-wv-linker-v1` under distinct `WVHL 1` metadata and outer container format 7. Require the canonical module identity, one exported `Main`, the exact six capabilities, and the exact ten services specified by the contract.

Reuse the existing compiler-authority startup and ten-service runtime layout. The profile adds zero WVA or platform assembly. A focused linker-owned application writer performs the module, fragment, profile, bundle, and complete PE/ELF reconstruction checks.

### Review behavior tests before running them

Update the existing linker tests before execution. The regular source test now expects `console.write_line`, the Windvale SHA function, absence of the unsupported digest opcode, and the new module identity. Its embedded self-test covers the source-level signed formatter. The retained extended differential case continues to compare every canonical image and map byte across aligned, unaligned, reordered, complete-section, limit, and failure scenarios.

Add one regular native-package case rather than duplicating that extended matrix. It reconstructs both platform packages, exercises the public current-host AOT target, runs the raw self-test and canonical two-object link, compares the entire image and map with Stage 0 during candidate qualification, checks signed map output, proves rejected WVO preserves an existing output, and inspects loaded modules or mappings for .NET.

### Keep qualification and retirement separate

The Stage 0 CLI constructs both candidates and remains the normal `windvale link` path for now. After one exact commit passes the source evidence on Windows and Linux, pin both platform applications and add digest-bound `Link-Wvo.cmd` and `.sh` launchers in a separate provenance commit. Only after that exact artifact commit passes both hosts does ordinary WVO linking move to the native launcher.

The C# linker remains reachable solely through named recovery and differential paths after cutover. The complete Decision 0057 gate still controls final deletion and archival; the UEFI target adapter is outside this standard flat-image profile.

## Consequences

- WVO validation, resolution, layout, relocation, image reconstruction, map construction, and publication remain one Windvale implementation.
- The additional machine-specific source is zero; the linker reuses the established startup and service boundary.
- Candidate execution exposed and corrected a general native record-frame rule: even a dead record definition writes its fixed backing, so its destination must not overlap another record local live across that store. A focused native regression now owns that invariant.
- Exact Stage 0 comparison remains candidate evidence, not a permanent normal-test requirement. After promotion, stable WVO/image/map vectors, independent parsers, malformed input, and deterministic artifact identities can own ordinary coverage.
- The 2,700-line linker source is not split mechanically during this boundary change. Future work should extract only cohesive, explicitly owned modules where real interfaces exist, consistent with the repository's focused-source guidance.

## Reconsideration triggers

Reconsider this profile if Windvale Linking 1 changes, the linker requires a capability outside the exact six-entry set, the shared startup prevents a narrower future binding, or dual-host evidence changes any candidate identity.
