# Workload 7 review findings

## Status

First-author review is complete. The project owner authorized direct acceptance
of the recommended correctness/completeness findings on 2026-08-17; all seven
findings below are accepted under
[Decision 0761](../../../Decisions/0761-Resolve-Language-1.0-Retained-Gui-Findings.md).
They are normative-candidate/source-freeze inputs, not implementation or final
source-freeze claims.

## Pressure matrix

| Required pressure | Evidence | Standing |
| --- | --- | :---: |
| Event variants | Five closed cases carry only their meaningful payload. | Pass |
| Bounded package data | One 36-byte exact theme with maximum/digest/parser. | Pass |
| Owned mutable state / immutable publication | One exclusive retained aggregate; two frozen RGBA frames. | Pass |
| Maps/arenas and generation handles | Ordered stable identity map plus arena validation, replace, remove, tombstone. | Pass with accepted Foundation completion |
| Closure capture / background work | One copied snapshot/maxima; one scoped handle; parent-only apply. | Pass |
| Explicit GUI authority | Separate surface, input-batch, and timer capabilities/endpoints/effects. | Pass |
| Bounds and failure | Exact memory/event/task/render/provider ceilings and 54 rejected cases. | Pass |
| No class/GC/global event loop | Records/variants/arena/scope remain readable and acyclic in ownership. | Pass |

## Finding 1: complete typed-arena mutation

The candidate arena semantics already said removal returns the node and advances
its generation, but the accepted signature surface exposed only construction,
insert, validate, borrow, length, and freeze. Retained state also needs to replace
a widget without changing its handle.

Accept exact `Arenaˉreplace` and `Arenaˉremove` signatures with these rules:

- replacement validates before mutation, preserves generation, returns the old
  node on success, and returns the proposed input on failure;
- removal validates before mutation, returns the old node, advances generation
  or retires before wrap, and never aliases a stale handle; and
- every failure leaves the arena unchanged.

This completes Foundation collection behavior. It is not a pointer API, object
model, hidden allocation, or GUI-specific primitive.

## Finding 2: events should be closed variants

An early draft used one event-kind enum plus a record containing coordinates and
dimensions for every case. That permits meaningless states such as a close event
with resize payload and weakens exhaustiveness.

Accept a closed `Event` variant with pointer coordinates only on
`Pointerˉpressed`, dimensions only on `Resize`, and payload-free request/remove/
close cases. Existing variant/match syntax is sufficient; no inheritance,
visitor protocol, tagged object, reflection, or special event syntax is needed.

## Finding 3: keep pure GUI logic Core and authority Hosted

Theme parsing, retained collections, layout, and deterministic pixel construction
depend only on Core contracts. Task failures and GUI endpoints are Hosted.

Accept the split between `Retainedˉguiˉtypes` and
`Retainedˉguiˉhostˉtypes`, and keep display, input, and timer as three independent
capabilities. A broad `gui.all` grant, implicit UI-thread global, ambient event
loop, or Core→Hosted import would obscure authority and portability.

## Finding 4: task completion never commits state by itself

The child owns a Copy layout snapshot. The parent owns all mutation and applies
the result only after checking layout generation, status shape, and every target
handle under one exclusive state borrow. The reference intentionally makes the
child result stale and proves it changes nothing.

Accept this as the GUI background-work rule for 1.0. The workload needs no actor,
channel, observable property, dispatcher keyword, detached task, lock, or
source-level thread. Later GUI libraries may wrap this pattern without weakening
scope ownership or revalidation.

## Finding 5: stable identity tombstones are a valid 1.0 policy

Removing status leaves logical identity 4 mapped to the old generation-checked
handle. Every read revalidates through the arena, so this produces exact stale
evidence rather than unsafe aliasing. It also prevents silent identity reuse.

Accept this policy for the workload and do not add `Mapˉremove` merely to erase
the evidence. Products that require identity reuse must specify a separate
atomic arena/map policy. The general map contract may gain removal from a later
set/package workload on its own evidence, not as speculative GUI convenience.

## Finding 6: immutable frame publication needs exact mutation outcomes

The renderer freezes complete RGBA8 bytes before capability dispatch. Surface
publication is still an external mutation and must distinguish:

- complete local-provider acceptance with exact bytes/generation/sequence;
- rejection proving zero publication and returning the input frame; and
- indeterminate dispatch returning no safe replay point.

Accept this paper capability shape. It follows the existing external-mutation
rules and adds no graphics opcode or implicit retry. “Accepted” does not claim
physical scanout, GPU completion, remote receipt, or human observation.

## Finding 7: typed bytes are sufficient for the first theme resource

The 36-byte theme has an exact resource identity, type, maximum, digest,
magic/version, separators, and lowercase-hex grammar. A tiny explicit decoder is
clearer than reflection or a new general schema language for this workload.

Accept `package data ...: bytes` here. This does not reject future typed schema
resources; it says workload 7 supplies no evidence that they belong in the 1.0
source grammar.

## Quantitative record

| Measure | Recorded value |
| --- | --- |
| Source | 8 modules; 2,004 lines / 65,944 UTF-8 bytes; 74 top-level declarations; largest module 729 lines. |
| Package data | 1 object / 36 bytes / one exact SHA-256. |
| Retained state | 4 initial widgets, 3 final live widgets, 4 stable identities. |
| Events/tasks | 5 events, 1 batch, 1 timer tick, 1 child/handle/outcome. |
| Frames | 7,680 and 12,288 bytes with exact cross-target hashes. |
| Memory | 73,728-byte application root split into five exact children. |
| Failure surface | 54 named compile/package/state/task/render/provider cases. |
| New general surface | 1 failure record and 2 typed-arena functions; no grammar form. |

Implementation must record actual tokens, compiler phase time/memory, generic
instances, WIR blocks/operations, WVB/native bytes, execution/peak memory,
provider queue maxima, frame work, cancellation latency, and teardown work. The
paper values are bounds and exact fixtures, not performance measurements.

## Owner resolution

The owner accepted all seven recommendations. Workload 7 is draft reviewed. The
Foundation candidate now carries exact arena replace/remove semantics; the GUI
capability and package contracts remain paper source-freeze inputs with named
implementation owners. No current compiler, GUI provider, operating-system
console, native display stack, or frozen Language 1.0 claim follows.
