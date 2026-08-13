# Decision 0222: First native WVO read-only front door

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0213](0213-Stage0-Semantic-Freeze-And-Native-Front-Door.md)
- Builds on: [Decision 0221](0221-First-Native-Wv-Linker-Front-Door.md)
- Contract: [Windvale native WVO inspector](../../Specifications/Windvale-Native-Wvo-Inspector.md)

## Context

Windvale already owns complete WVO 1.0 structural admission and deterministic reporting, but the object core was still presented as a sample writer under `Examples/Foundation`. Normal `object-verify` and `object-inspect` commands therefore still entered C# even though the reusable semantics existed in Windvale source.

The existing hosted WVB inspector package already supplies the exact read-only capabilities and eleven native services this WVO tool needs. Creating another startup or copying object semantics into assembly would add a second implementation without adding authority or product value.

## Decision

### Move object semantics to their owning architecture area

Move the source to `Object-Model/Windvale/Wvo-Object-Core.wv` and add `Projects/Object-Model/Windvale-Wvo-Object.wvproj` with its exact byte-ordering and SHA-256 dependencies. Keep canonical sample construction only as an internal self-test. Remove the file-writing shell and its capability; the ordinary commands read, verify, and report an existing WVO.

Close the remaining verifier parity gaps for duplicate section/symbol names and complete inspection views. Successful `verify` and `inspect` output matches the independent C# Stage 0 commands byte for byte after platform newline normalization. Invalid input returns a deterministic structural status without partially reporting an object.

Add explicit zero `Invalid` enum members required by the current bounded native nominal-type rule. Existing serialized section, symbol, binding, and relocation values remain 1 through 4; WVO 1.0 bytes do not change.

### Reuse one existing read-only native profile

Add `windows-x64-wvo-inspector-v1` and `linux-x64-wvo-inspector-v1` as profile 6 in the shared hosted-verifier metadata and outer container format 4. Require the canonical module identity, one exported `Main`, the exact five read-only capabilities, and the existing eleven-service inspector bundle.

Reuse the current Windows/Linux inspector startup and service leaves. Add zero WVO or platform assembly. A focused WVO application writer performs module, entry, profile, bundle, runtime, and complete PE/ELF reconstruction checks.

### Review changed tests before execution

Replace the obsolete source test's sample-writer expectations before running it. The new test requires file-read authority, Windvale SHA-256, absence of file-write authority, exact successful verify/inspect reports, and the retained internal sample identity.

Add one focused native-package case. It reconstructs both packages, exercises the public current-host AOT target, runs self-test/verify/inspect/malformed/usage paths, and proves that the current-host process loads no .NET runtime. The existing C# hostile-object matrix remains the independent oracle; this slice does not duplicate it.

### Defer the broad gate once

During the active .NET-retirement goal, run only the named WVO source and native-package checks locally. Do not run Standard, Qualification, or repeat GitHub qualification for each candidate slice. Before the final broad gate, update from upstream, reconcile the complete retirement batch, review the affected test inventory, regenerate all current native identities once, and then run the full Windows/Linux qualification.

After that source gate, pin the paired applications and add digest-bound native launchers in a separate provenance commit. Only their exact containing commit passing both hosts moves ordinary object verification and inspection away from C#. C# remains named recovery/differential evidence until the complete Decision 0057 archive gate permits deletion.

## Consequences

- WVO validation and reporting now have one architecture-owned Windvale implementation and one explicit native candidate package.
- The normal candidate has no write authority and adds no platform assembly.
- Candidate tests compare with C# while it is available; promoted ordinary tests use fixed vectors, structural assertions, malformed outcomes, and pinned identities without needing a live C# result generator.
- The source remains one cohesive object-format module for now. The repository guidance favors reviewable files, but extraction will occur only when a real owned boundary appears, not as numbered fragments or line-count-only churn.
- No broad qualification or normal-front-door promotion is claimed by this candidate commit.

## Reconsideration triggers

Reconsider this profile if WVO 1.0 changes, inspection requires authority outside the exact five capabilities, the shared inspector startup prevents a narrower future binding, or the final dual-host gate changes any candidate identity or successful report.
