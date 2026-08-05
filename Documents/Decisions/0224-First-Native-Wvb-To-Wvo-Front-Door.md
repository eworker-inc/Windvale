# Decision 0224: First native WVB-to-WVO front door

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0213](0213-Stage0-Semantic-Freeze-And-Native-Front-Door.md)
- Builds on: [Decision 0223](0223-First-Native-Console-Application-Packager.md)
- Contract: [Windvale native WVB-to-WVO application](../../Specifications/Windvale-Native-Wvb-To-Wvo.md)

## Context

Windvale already owns a verified WVB-to-WVO lowerer for a bounded metered scalar, control-flow, and direct-call subset. Its small hosted shell remained under `Examples/Compiler` and executed only through Stage 0, so every native AOT path still entered the complete C# backend before linking or packaging.

The new native build driver, lowerer subset, linker candidate, and console-packager candidate are complementary. Packaging the lowerer is the missing tool boundary between a verified WVB and a WVO for the accepted subset; it does not imply that the Windvale lowerer already covers the complete compiler or hosted tool inventory.

## Decision

### Move the hosted shell to compiler ownership

Move the 35-line `Native-X64-Lowering-Tool.wv` shell from `Examples/Compiler` to `Compiler/Windvale` and update its existing Project 1 manifest. Keep `Native-X64-Lowering-Core.wv` as the single portable selector and verifier. The move changes no module identity or command behavior.

Replace the core's temporary `I32ˉformat`-to-decimal-to-`u32` conversion with canonical `Bytesˉfromˉi32ˉlittle` when emitting signed machine fields. The retained differential corpus must prove every WVO byte unchanged. This removes an irrelevant text-formatting service from the native tool rather than widening its application bundle to eleven entries.

### Add one fixed native WVB-to-WVO profile

Add `windows-x64-wvb-to-wvo-v1` and `linux-x64-wvb-to-wvo-v1` under `WVHN 1` metadata, profile 6 in the shared hosted-compiler family, profile flags 7, and outer container format 9. Require the canonical module identity, one exported `Main`, the exact six compiler-authority capabilities, nine exact fragment services, and startup-internal UTF-8 validation as the tenth application service.

Reuse the established compiler-authority startup and service leaves. The profile adds zero WVA or platform assembly. A focused writer reconstructs the module, fragment, service bundle, runtime state, metadata, startup, and complete PE/ELF package before publication.

### Pin one stable accepted-subset fixture

Add one portable return-42 fixture and Project 1 manifest. Its exact 174-byte WVB lowers to one independently verified 479-byte WVO. This fixed input/output pair remains useful after the C# differential oracle is archived.

### Review affected tests before execution

Add one target-discovery case and one native-package case after reviewing their complete assertions. The package case checks exact tool and container identities, exact service order, public current-host AOT construction, direct native lowering with no CLR mapping, deterministic repeated WVO output, independent WVO parsing, usage behavior, malformed-WVB rejection, and preservation of an existing output.

The source-core change also affects the existing shared-backend test. Update only its three directly derived module identities, then require the complete retained constant, arithmetic, comparison, control, loop, call, mixed-register, and malformed corpus to remain byte-identical to Stage 0.

### Defer broad qualification and composition

Run only the two new cases and the one directly affected shared-backend case locally. Do not run Standard, Qualification, or another GitHub loop. Before the final grouped gate, update from upstream, reconcile all retirement slices, regenerate current aggregate identities once, and run the complete Windows/Linux qualification.

Stage 0 still constructs this candidate and remains the complete backend outside the accepted subset. Native composition of build driver, lowerer, linker, packager, and test fixtures waits for pinned artifacts and the grouped source gate; this decision does not claim complete native-backend ownership.

## Consequences

- The accepted metered scalar/control/call subset now has a direct Windvale-authored Windows/Linux WVB-to-WVO candidate.
- The first complete native AOT chain has every required component as a source candidate, though no grouped promotion claim is made yet.
- Direct signed byte construction removes a text-formatting dependency while preserving every selected WVO byte.
- The large lowering core remains cohesive because selection, offset measurement, branch patching, and verification share exact invariants. It is not split into numbered fragments merely to reduce line count; future extraction requires a real owned interface.
- The general ABI-22 backend, descriptors, capabilities, data, relocations, complete compiler, JIT publication, and release automation remain open.

## Reconsideration triggers

Reconsider this profile if the accepted lowerer subset changes, WVO or ABI 22 changes, the tool needs authority outside the exact six capabilities, the shared startup prevents a narrower binding, or final dual-host evidence changes a candidate identity or retained WVO byte.
