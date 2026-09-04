# Decision 0956: coalesce overlapping generic nominal development products

- Date: 2026-09-04
- Status: Implemented development candidate with focused Windows evidence;
  Linux execution and qualification adoption remain pending
- Extends: [Decision 0955](0955-Add-A-Bounded-Language-Front-Door-Development-Checkpoint.md)
- Preserves: all 108 cases, three attributed owner receipts, single-owner
  selection, the four independent qualification owners, and existing limits

## Context

Generic nominal type binding, layout, and materialization are separate owners,
but their project closures contain almost the same compiler modules. In the
paired qualification baseline they consumed 671,179 ms on Windows and 414,567
ms on Linux. A common compiler source change selects all three, so ordinary
development repeats nearly the same full build and native package three times.

The WVLB carrier overlaps too, but a four-way trial produced 604 functions,
966,154 code bytes, and a 1,171,385-byte unpublished module. Publication failed
at the existing bound. Enlarging a compiler product solely to merge tests would
hide the suite's scaling problem rather than solve it.

## Decision

1. Add one Project 2 development bundle that imports and invokes the existing
   type-binding, type-layout, and type-materialization self-test modules.
2. Preserve failure attribution with disjoint result ranges: binding retains
   its result, layout adds 64, and materialization adds 96. Success remains 42.
3. Use the bundle only when the changed-file plan selects at least two of the
   three owners. A one-owner edit retains its original focused product.
4. Let each selected owner execute the same immutable bundle and emit only its
   existing registry summary. The first request constructs the project and
   native application; later requests require exact project and package cache
   hits before executing again.
5. Plan the shared development work as 330 expected seconds with a 600-second
   maximum, replacing two or three separate 300/600-second owner allowances.
6. Leave every no-argument wrapper and all qualification registry rows
   unchanged. Do not add the WVLB carrier or raise a product limit.

## Evidence

The retained three-way project contains 489 functions and publishes a 971,313-
byte WVB without changing a compiler or lowerer limit. On Windows, its exact
project cache miss took 49,870 ms, its segmented native package miss took
240,024 ms, and execution took 2,298 ms. The complete run passed in 292,203 ms.

With both exact products present, the type-binding, type-layout, and type-
materialization development wrappers passed their original 59-, 21-, and
28-case summaries in 2,625, 2,795, and 2,572 ms. The three receipts completed in
8,017 ms. Exact inputs and outputs are retained in the
[focused evidence record](../Evidence/2026-09-04-Generic-Nominal-Development-Bundle.json).

## Consequences

- A common source edit can construct one product for 108 cases instead of three
  near-duplicate products.
- Warm development feedback for the three receipts is measured in seconds.
- Qualification remains conservative until paired-host evidence proves the
  merged product and the timing-baseline migration is designed explicitly.
- The carrier remains a visible independent cost and capacity boundary.

## Reconsideration triggers

Retire the special bundle if ordinary incremental compilation makes the three
independent products equally cheap. Adopt it for qualification only after both
hosts pass and the owner-timing baseline can represent the topology change.
Split it if any member acquires distinct profile, authority, process-isolation,
or mutable-state requirements.
