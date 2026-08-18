# Decision 0761: Resolve the Language 1.0 retained-GUI findings

## Status

Accepted by the project owner on 2026-08-17 under the owner's instruction to
integrate the recommended correctness/completeness findings directly while
building the remaining Language 1.0 workloads. This decision refines
[Decision 0751](0751-Accept-Windvale-Language-1.0-Direction.md),
[Decision 0758](0758-Resolve-Language-1.0-Compiler-Front-End-Findings.md), and
[Decision 0760](0760-Resolve-Language-1.0-Concurrent-Service-Findings.md).

It accepts all seven findings from the retained-GUI paper bundle. It does not
freeze edition 1, implement a GUI/provider, require host threads, or select an
operating-system display architecture.

## Context

The seventh mandatory workload parses one exact package theme, owns retained
widgets in a typed arena and stable identity map, publishes immutable frames,
runs layout from a copied snapshot, receives bounded timer/input events, rejects
a stale child result, applies fresh layout on the owning path, and distinguishes
complete, rejected, and indeterminate display publication.

The candidate already had the required variants, records, maps, arenas, handles,
builders, package data, tasks, effects, capabilities, and operation context. The
first complete source exposed one missing Foundation surface: typed arena replace
and remove calls for mutation behavior already described by the semantic model.
Review also needed exact policy for event payloads, Core/Hosted separation,
stale results, identity reuse, and frame publication.

## Decision

### Complete typed-arena mutation

Accept:

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

Replacement validates before mutation, preserves the live slot generation,
returns the previous node on success, and returns the proposed node on failure.
Removal validates before mutation, returns the removed node, advances the slot
generation or retires it before wrap, and never lets a stale handle alias a new
node. Every failure leaves the arena unchanged.

### Use closed semantic event variants

Events are one closed variant whose cases carry only meaningful fields. Do not
encode every native event as a broad kind record, reflected object, integer bag,
class hierarchy, or hidden callback. Host providers normalize native input into
the versioned semantic cases before source receives it.

### Preserve the Core/Hosted boundary and narrow authority

Theme, state, layout, and rendering remain Core. Task/provider/application types
remain Hosted. Surface publication, semantic input batches, and monotonic timer
ticks are three independently approved version-1 capabilities with explicit
generation-bound endpoints and effects. There is no ambient `gui.all`, desktop,
event-loop, UI-thread, wall-clock, clipboard, filesystem, GPU, or process grant.

### Apply background results only on the owning path

A task may copy an immutable layout snapshot. Successful task completion does
not mutate or authorize mutation. The state owner checks layout generation,
shape, and every target handle under one exclusive borrow before the first
replacement. Stale results are ordinary typed outcomes and change no widget.

This requires no actor, channel, dispatcher keyword, lock, detached task, or
source-level thread. Libraries may wrap the pattern without weakening structured
task ownership or revalidation.

### Accept stable identity tombstones for this workload

The logical identity map may retain a removed identity mapped to its stale
generation-checked handle. Every read revalidates through the arena. The
tombstone prevents silent identity reuse and preserves exact stale evidence.

Do not add map removal solely to erase that evidence. A product that permits
identity reuse must define a separate atomic map/arena policy. Later workloads
may still justify general map removal independently.

### Require exact immutable-frame mutation outcomes

The complete frame becomes immutable before provider dispatch. Publication
returns either complete local acceptance with exact generation/sequence/bytes,
rejection proving zero progress and returning the input frame, or indeterminate
dispatch with no safe replay point. No path retries or refreshes automatically.

Provider acceptance does not imply physical scanout, GPU completion, remote
receipt, or human observation. This is a capability contract, not a GUI opcode.

### Keep the first theme as exact package bytes

The 36-byte resource has an exact identity, type, maximum, digest,
magic/version, separators, and lowercase-hex grammar. An explicit bounded parser
is sufficient and retains no duplicate payload. This workload does not justify
reflection serialization or a new source schema language; future schema
resources remain a separate measured decision.

## Consequences

The retained-GUI bundle becomes draft reviewed. Eight of eleven workloads are
now draft reviewed; workloads 8 through 10 remain.

Foundation gains one failure record and two arena operations but no grammar form,
address exposure, unchecked access, hidden allocation, object identity,
finalizer, tracing-GC dependency, or GUI-specific behavior. The paper GUI
contract gains named future capability/provider/launcher/verifier owners.

The exact reference uses four→three live widgets, four stable identities, five
events, one batch/timer/task, a 73,728-byte application budget, and two immutable
frame hashes. These are conformance inputs, not implementation performance
claims.

## Reconsideration triggers

Reconsider arena mutation only if implementation cannot return ownership and
preserve atomic failure without a different generic shape. Preserve checked
generation validation, stale non-aliasing, and unchanged state on failure.

Reconsider identity tombstones when a real consumer requires bounded identity
reuse; require an explicit atomic policy rather than implicit map cleanup.

Reconsider the GUI capability split only when a measured provider cannot bind
the three interfaces independently. Never replace them with ambient desktop or
hidden event-loop authority.

Reconsider frame outcomes only through an explicit idempotency/publication
protocol that defines safe replay. Indeterminate mutation never becomes an
implicit retry point.
