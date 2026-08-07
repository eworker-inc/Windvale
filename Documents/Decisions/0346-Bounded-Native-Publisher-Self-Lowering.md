# Decision 0346: Bounded native publisher self-lowering

- Status: Accepted local implementation; Linux execution and grouped qualification pending
- Date: 2026-08-07
- Advances: [Decision 0345](0345-Verifier-Scale-Native-Staged-Wvo-Publication.md)
- Uses: [Decision 0282](0282-Bounded-Native-Function-Batches.md), [Decision 0300](0300-Native-Staged-Wvo-Producer-Publisher-Composition.md), and [Decision 0308](0308-Native-Wvo-Publication.md)

## Context

Decision 0345 proved verifier-scale native staged WVO production and
publication, but lowering the publisher itself exhausted the unchanged
128 MiB native text arena before producing output. The accepted source and
object limits were not the cause. The lowerer retained too much aggregate
function and record-planning state while traversing the complete hosted
publisher closure.

The scratch-record planner also assigned result identities independently in
each basic block and then combined those identities into one function-wide
interference graph. Equal numeric identities from unrelated blocks therefore
became false lifetime relationships. After correcting that representation,
exact Stage 0 comparison exposed one ABI-layout difference: borrowed
descriptor return cells must precede record backing, while hidden record
return storage remains after scratch backing.

## Decision

- Extend the existing immutable-plan batch API with explicit maximum function
  count and maximum grouped-function byte limits. Preserve the original API as
  a byte-compatible delegating entry point.
- Let a function larger than the grouping threshold occupy one batch by
  itself when it still fits the artifact ceiling. Never split one function or
  weaken the existing output bounds.
- Use at most 16 functions and 64 KiB of grouped-function payload per staged
  publication batch. Clamp the grouping threshold to the caller's artifact
  payload limit.
- Keep scratch record-result identities unique across the function, but plan
  their physical field offsets independently per basic block because scratch
  values may not cross block boundaries. Retain a compact block-offset table
  that maps the function-wide evidence to each block-local allocation.
- Version the private record-storage evidence as version 2 and validate its
  persistent offsets, scratch offsets, block offsets, counts, and lengths
  before emission.
- Match ABI 22's exact hidden-result layout: a borrowed text or bytes result
  cell immediately follows ordinary local and value cells, before persistent
  and scratch record backing; a record-return pointer follows scratch backing.
- Emit relocation bytes directly and extract cohesive descriptor-call and
  record-storage routines into their owning focused modules. File size remains
  guidance rather than a reason to create numbered fragments or obscure an
  invariant.

## Evidence

The exact native inputs and Stage 0 comparison object are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Producer WVB | 421,544 | `4d8fcda41a013768a10a2919d06658d9c37fc66d8acbf51bad839c5ef4d13fc6` |
| Publisher WVB | 440,994 | `6ef23e0db58ecd788ca97218428dc7a131662f90f5875f7644f76592a7664acc` |
| Stage 0 publisher WVO | 6,449,889 | `050cf0f189501b1f3f433aff1dd7b8e125fd8e4f58da2ad58258e4b9327c0148` |

The deterministic host applications are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Windows producer | 6,170,624 | `c18253d135f15195cad32ccf6f7243711bfa959a44696b388475165406216adb` |
| Linux producer | 6,172,672 | `e38bc7b4128afc829de112098c5844d3c3fc159d11d09d6e97ef2f79d19845d7` |
| Windows publisher | 6,458,368 | `07ae86a7ed2922f117e3d314b7110812ab2721f8205e86f52c659f23deb84aa8` |
| Linux publisher | 6,455,017 | `9ea96ff5977b18c3dc97329601941bc892cc728bf0fb4da747f61dc8f36577ad` |

After test review, the focused extended Windows contract passes 1/1 in
39.417 test seconds after a zero-warning Release build. It executes the native
producer and publisher without loading .NET, admits multiple bounded chunks
within the 62-chunk resource ceiling, independently verifies the result,
requires the exact Stage 0 WVO length and digest above, and cleans temporary
state atomically on failure. The focused bounded-batch compatibility and limit
contract passes 1/1 in 1.779 test seconds after a zero-warning build.

Linux packages are constructed and pinned but are not executed by this local
evidence. Development, Standard, Qualification, promotion, and the grouped
end-of-goal gate were deliberately not run.

## Consequences

The current-host native publisher can now lower and publish itself into the
exact Stage 0 WVO without a .NET child and without widening the native arena.
The measured Decision 0345 lifetime boundary is closed for this slice.

This does not retire Stage 0 or promote the publisher to the ordinary route.
Linux execution, native replacement of the host-package constructors,
remaining backend subset gaps, cross-host promotion, and the complete
Decision 0057 retirement gate remain open.

## Reconsideration triggers

Revisit the grouping limits only if a measured successor demonstrates a
smaller coherent bound or an admitted individual function no longer fits the
ordinary artifact ceiling. Rebuild and requalify every pinned identity if the
lowering, frame, record-storage, ABI, object, runtime-service, or publication
contract changes.
