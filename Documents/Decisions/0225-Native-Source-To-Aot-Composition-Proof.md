# Decision 0225: Native source-to-AOT composition proof

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0213](0213-Stage0-Semantic-Freeze-And-Native-Front-Door.md)
- Builds on: [Decision 0224](0224-First-Native-Wvb-To-Wvo-Front-Door.md)
- Contract: [Native source-to-AOT composition](../../Specifications/Windvale-Native-Source-To-Aot-Composition.md)

## Context

The native source builder, WVB-to-WVO lowerer, standard flat linker, and version-1 console packager had focused evidence in isolation. That did not prove their exact file and process contracts compose into one directly executable product. Promoting binaries or adding an ordinary launcher before that proof would make the grouped retirement gate diagnose integration and qualification problems at the same time.

## Decision

### Compose existing processes without a new coordinator

Add one current-host conformance case over the existing return-42 Project 1 fixture. It invokes the already-qualified native source-to-WVB launcher, then current-source native WVB-to-WVO, linker, and console-packager applications as separate processes. It finally executes the produced PE or ELF and requires process result 42.

Do not add a combined in-process Windvale tool yet. The linker and packager currently expose process-oriented `Main` contracts, and extracting internal APIs solely for this proof would widen the slice. A future coordinator requires a real ownership and failure contract rather than a large source merge or numbered fragments.

### Retain fixed evidence independent of Stage 0 behavior

Pin the complete accepted-subset chain:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Return-42 WVB | 174 | `7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31` |
| Return-42 WVO | 479 | `0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5` |
| Canonical link map | 630 | `857710249807d2fed4da847729d0244f08ccdc70156c043fdaa0516de394e2dc` |
| Linked native image | 406 | `7c05565142850adab1d63d999479977a23ef50c7264c03ee55ce5b323df26408` |
| Windows x64 console application | 2,560 | `8f2c3389dafa40c0231a0f5aeead3db5570697d54874f324a81f84a2d5b16eb6` |
| Linux x64 console application | 8,304 | `fe525b84b9bf902677a5c7beb36872dfd72e7d6d0f12bfb5c95d491c4e1cd3f7` |

The test also parses the WVB, WVO, and current-host application structurally, requires exported `Main` at native offset zero, recovers the exact linked bytes from the container, and observes no named .NET runtime mapping in the lowerer, linker, packager, or result process. These fixed identities remain portable to the future native test plan after the C# harness becomes recovery-only.

### Keep promotion and broad verification grouped

This proof does not update `Artifacts/Native-Front-Door`, add digest-bound AOT launchers, or claim Linux execution from a Windows run. Review the new test before execution and run only that focused case locally. Before the final broad gate, update from upstream, reconcile the accumulated retirement slices, regenerate aggregate identities once, and run the complete Windows/Linux qualification from one source state.

## Consequences

- The accepted scalar/control/direct-call subset now has one demonstrated source-to-executable native chain on the current host.
- Integration uses stable process and file contracts, so failure ownership remains visible at each phase.
- Stage 0 still constructs the three unpromoted tool candidates for this test; it is setup and recovery evidence, not a child-process dependency of the exercised chain.
- Complete backend coverage, pinned artifact promotion, ordinary AOT launchers, native release automation, broader native test orchestration, and the final .NET archive remain open.

## Reconsideration triggers

Reconsider the multi-process shape when linker and packager logic expose cohesive portable APIs, when process startup dominates ordinary builds, when additional native targets require one transactional coordinator, or when the grouped dual-host gate changes any pinned identity.
