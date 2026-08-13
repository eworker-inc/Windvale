# Decision 0223: First native console-application packager

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0213](0213-Stage0-Semantic-Freeze-And-Native-Front-Door.md)
- Builds on: [Decision 0221](0221-First-Native-Wv-Linker-Front-Door.md)
- Contract: [Windvale native console packager](../../Specifications/Windvale-Native-Console-Packager.md)

## Context

Windvale already owns the exact layout, sparse construction recipe, and portable verification of the capability-free version-1 Windows PE and Linux ELF console applications. The final recipe materialization still occurred in the C# target adapters. That left a .NET operation between a native linker image and a directly executable host application even though no remaining PE or ELF semantics needed to live there.

The existing hosted-compiler startup already provides the bounded arguments, input, output, diagnostics, formatting, and byte-construction services needed by a packager. A new platform startup or a second PE/ELF implementation would add machinery without moving product ownership.

## Decision

### Compose construction, materialization, and verification in Windvale

Add `Projects/Linker/Windvale-Console-Application-Packager.wvproj`. Its hosted root parses an exact target and decimal entry offset, reads one raw native image, constructs the existing 32-byte `WVCQ 1` request, invokes the existing portable sparse constructor, validates every recipe field and segment, and materializes canonical zero gaps plus literal and native spans into one completed application value.

Before its single output call, the application runs the existing portable console-application verifier and requires exact target, native-image bytes, and entry-offset recovery. Invalid target, malformed entry, invalid recipe, oversized complete application, verification disagreement, or runtime failure produces no application write. This source adds no PE/ELF layout constants outside the existing construction and verification modules.

The current native byte-value boundary restricts this first materializer to completed applications no larger than 4,194,304 bytes. This is narrower than the existing sparse recipe's theoretical maximum and is reported as an explicit `Applicationˉlimit` result rather than silently increasing the runtime limit.

### Add one fixed native packager profile

Add `windows-x64-console-packager-v1` and `linux-x64-console-packager-v1` under `WVHP 1` metadata, profile 5 in the shared hosted-compiler family, profile flags 6, and outer container format 8. Require the canonical module identity, one exported `Main`, six exact capabilities, nine exact fragment services, and the existing startup-internal UTF-8 service as the tenth application service.

Reuse the current compiler-authority startup and service leaves. The profile adds zero WVA or platform assembly. A focused application writer reconstructs the module, fragment, profile, bundle, runtime state, and complete PE/ELF package before publication.

### Review the focused behavior test before execution

Add one regular package case after reviewing it against the new boundary. It reconstructs both candidate containers, exercises the public current-host AOT target and self-test, packages one raw `mov eax, 42; ret` image into both version-1 formats from the same host tool, verifies exact native bytes and entry through the independent C# PE/ELF parsers, executes the current-host result, proves deterministic repetition, rejects malformed input without changing an existing output, and inspects the packager process for CLR/.NET mappings.

The C# comparison is transition evidence while the recovery implementation exists. Promoted ordinary tests will retain fixed raw images, exact container identities, structural recovery, malformed outcomes, and direct execution without requiring a live C# result generator.

### Defer broad qualification and promotion

Run only the two named packager checks locally during this slice. Do not run Standard, Qualification, or another GitHub qualification loop. Before the final grouped gate, update from upstream, reconcile the retirement batch, regenerate current identities once, review the complete affected test inventory, and then run the full Windows/Linux qualification.

The new profile is still constructed through Stage 0 until exact native packager artifacts are pinned and promoted. This slice removes .NET from the raw-image-to-version-1-application operation itself; it does not yet remove Stage 0 from candidate construction, complete backend lowering, repository automation, release production, or recovery.

## Consequences

- The standard native linker can now feed a Windvale-written application materializer without duplicating PE or ELF construction logic.
- Version-1 layout, construction, and verification remain in their existing focused modules; the new source owns only orchestration and bounded materialization.
- The candidate uses the established ten-service startup and adds no platform assembly.
- The one-value materializer limit is explicit. Supporting the final 8,304 bytes of the theoretical maximum requires a later segmented output or streaming publication contract.
- No broad qualification, ordinary-front-door cutover, or .NET retirement is claimed by this candidate commit.

## Reconsideration triggers

Reconsider this profile if the version-1 console contracts change, the packager needs authority outside the exact six capabilities, completed maximum-size applications must be materialized before segmented output exists, the shared startup prevents a narrower binding, or the final dual-host gate changes a candidate identity.
