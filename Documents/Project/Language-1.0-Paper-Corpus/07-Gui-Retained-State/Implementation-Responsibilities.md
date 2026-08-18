# Workload 7 implementation responsibilities

## Rule

This matrix assigns each paper dependency without treating missing GUI provider
code as a reason for new language syntax. It is an implementation plan after
source freeze, not a claim that edition 1 or these providers exist today.

## Responsibility matrix

| Boundary | Future owner | Required work | Special compiler feature? |
| --- | --- | --- | --- |
| Closed event variants, records, matches, loops, named calls | Language parser/type checker/lowering | Type and lower exact event payloads, state transitions, layout/render loops, and exhaustive outcomes. | Existing edition-1 forms. |
| Core/Hosted dependency direction | Module/profile analysis | Permit Hosted application→Core libraries; reject Core→Hosted task/provider imports. | Existing profile rule. |
| Explicit closure capture and task ownership | Ownership/effects/async lowering | Copy snapshot/maxima, return closure on spawn rejection, consume handle once, join on all exits. | Existing structured-task contract under Decision 0760. |
| Operation-context propagation | Foundation operation/task and runtime | Derive one child context and pass it to timer/input/display suspension points. | Lifetime evidence; no new syntax. |
| Arena replacement/removal | Foundation collections/runtime | Implement exact prevalidation, ownership-return, stable replacement generation, removal generation advance/retirement, and bounded failure. | Ordinary generic calls and affine-state analysis. |
| Ordered identity map | Foundation collections/runtime | Preserve canonical rank lookup, duplicate rejection, Copy handle read-through, and tombstone validation. | No new operation required for this policy. |
| Theme package data | Package builder/loader | Bind exact bytes/maximum/digest/type and reject before source start. | Existing typed content reference. |
| Theme decoder | Core Foundation bytes/compiler | Execute exact checked indexing/ASCII hex without allocation or locale. | No. |
| Immutable RGBA frame builder | Core Foundation bytes/runtime | Reserve exact capacity, append/freeze deterministically, preserve budget accounting and byte identity. | No GUI-specific lowering. |
| GUI capability catalog | Capability specification/runtime binding | Publish the three exact version-1 signature sets/endpoints/limits/failure families. | Module-bound roots plus explicit endpoint arguments. |
| Surface provider | Windows/Linux/Windvale adapters | Validate frame geometry/generation/sequence, implement complete/rejected/indeterminate publication and bounded teardown. | Host adapter; no source semantic change. |
| Input provider | Same host owners | Normalize native events to closed variants, bound immutable batches, enforce exact sequence/generation. | Host adapter. |
| Timer provider | Same host owners | Bind admitted monotonic tick source to operation context and exact sequence/generation. | Host adapter. |
| Launcher/package profile | Package/runtime launch | Select `Run`, create three rights-limited endpoints/parent context, transfer exact root budget and limits. | Named launcher metadata, no `Main` keyword/global allocator. |
| Diagnostics | Compiler/runtime/provider | Preserve phase/span, owner/borrow/capture, identity/slot/generation, expected/observed provider generation, and exact bounds. | Bounded records. |
| Verification | Focused Language 1.0/collection/GUI owners | Execute 54 rejected cases, two exact frame hashes, schedule permutations, cleanup, malformed provider outcomes, and paired-host differential evidence. | No unfiltered qualification substitute. |

## Candidate Foundation additions

The workload uses these exact generic shapes:

```text
export record Arenaˉreplaceˉfailure<T> {
    Error: Collectionˉfailure;
    Value: T;
}

export fn Arenaˉreplace<T>(
    Arena: borrow mut Arena<T>,
    Handle: borrow Handle<T>,
    Value: T,
) -> Result<T, Arenaˉreplaceˉfailure<T>> effects();

export fn Arenaˉremove<T>(
    Arena: borrow mut Arena<T>,
    Handle: borrow Handle<T>,
) -> Result<T, Collectionˉfailure> effects();
```

Replacement returns the old node on success and the proposed input on failure.
It preserves the live slot generation. Removal returns the old node on success,
advances the generation before reuse, and retires before wrap. All failure paths
leave the arena unchanged.

These calls complete the mutation behavior already required by the typed-arena
semantic model. They do not add address exposure, unchecked lookup, hidden
allocation, object identity, finalizers, or tracing GC.

## Planned verification owners

| Planning label | Initial bounded evidence |
| --- | --- |
| `language-1-paper-gui` | Parse/type/profile/effect/capture/task/apply-result cases and paper identities. |
| `foundation-arena-mutation` | Replace/remove success, every invalid handle family, ownership return, generation advance/retirement, prevalidation atomicity. |
| `package-data-gui-theme` | Exact binding/hash/length/type plus every decoder byte boundary. |
| `gui-reference-provider` | Input/timer/surface valid transcript, limits, generations, complete/rejected/indeterminate outcomes, teardown. |
| `gui-frame-differential` | Exact initial/final RGBA bytes and SHA-256 on Windows/Linux plus future Windvale provider. |
| `gui-schedule-differential` | Child before/during/after provider awaits with identical stale/final layout and frame results. |

These are planning labels, not current registry additions. Add an executable
owner only when its implementation boundary exists.

## Implementation sequence after source freeze

1. Implement/verify arena replace/remove over the simple correctness oracle.
2. Compile the Core theme/state/layout/render path and compare exact frame bytes.
3. Add a deterministic in-memory GUI provider implementing all three capability
   interfaces and the reference transcript.
4. Compile the Hosted application, task/context path, and schedule differential.
5. Add Windows and Linux adapters while retaining the in-memory provider as the
   semantic oracle.
6. Bind a Windvale OS console/display/input/timer provider when those product
   services exist; do not change source semantics to match native APIs.
7. Record tokens, phase times, WIR blocks/operations, WVB/native bytes, runtime
   time, peak memory, event/frame queues, cancellation latency, and teardown work.

Widgets, accessibility trees, font shaping, GPU composition, animation,
multi-window policy, clipboard, drag/drop, IME, and localized text are later
libraries/capabilities/workloads. They do not enter 1.0 invisibly through this
small retained-state proof.
