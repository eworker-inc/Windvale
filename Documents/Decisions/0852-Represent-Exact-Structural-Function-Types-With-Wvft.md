# Decision 0852: Represent exact structural function types with WVFT

- Status: Accepted
- Date: 2026-08-25

## Context

Language 1.0 function values carry complete parameter/result types, transfer
modes, profile, effects, and unsafe/asynchronous status. Existing ordinary
source shapes cannot encode that structure. Reusing the WVGT generic-nominal
range would mix two catalogs with different identities and consumers, while a
runtime address would be nonportable, nondeterministic, and incapable of
proving effect or capture compatibility.

## Decision

1. Add the separate bounded `WVFT 1.0` compiler catalog for concrete structural
   function-type identity.
2. Define identity as flags, profile, result shape/mode, exact resolved language
   and capability effects, and every ordered parameter shape/mode.
3. Give at most 256 instances compiler-private shapes
   `0x80000100..0x800001ff`, immediately after WVGT's nonoverlapping private
   range. Neither range may enter WVB.
4. Permit ordinary nonzero shapes, WVGT identities, and earlier WVFT identities;
   reject self, forward, cyclic, and all other private shapes.
5. Retain the frozen 64-parameter function boundary plus 256-instance, 1 MiB
   retained-evidence, and 16 MiB estimated-growth limits.
6. Reuse an exact signature before applying new growth limits. Estimated bytes
   are accounting, not identity.
7. Validate exact lengths, aggregates, modes, flags, profiles, dependency order,
   duplicate identities, and trailing bytes before accepting or extending
   evidence.
8. Keep runtime representation separate. Source binding, captures, WVIR, WVB,
   verifier, runtime, and native-call integration remain connected Slice 6
   checkpoints and cannot treat WVFT completion as their completion.

## Evidence

The focused native self-test covers an empty catalog, exact admission and
accessors, identity reuse before excessive-estimate rejection, distinct flags,
profiles, effects, transfer modes, earlier nested references, forward-reference
rejection, invalid modes/profiles/shapes, the 64-parameter boundary, malformed
length/magic, duplicate identities, and checked aggregate growth.

The current fixture builds as a canonical WVB and executes through the same
hosted native packaging path as the existing generic-type catalog, returning
`42`. Exact artifact sizes and hashes are recorded by the maintained focused
owner when this Slice 6 batch is integrated; cross-host and broad integration
evidence remain deferred to that gate.

## Consequences

The compiler has one collision-free exact identity for function types without
adding runtime generics, ambient addresses, or a second semantic compiler. The
catalog can be threaded through source evidence incrementally while every
unconnected output continues to reject rather than erase its meaning.

The implementation uses bounded linear duplicate lookup. At 256 entries this
keeps the correctness oracle simple and deterministic. It should be replaced
only if representative compiler workloads show that catalog lookup materially
dominates compilation.

## Reconsideration triggers

Reconsider the private range or instance count only if representative complete
programs reach the bound. Version the format rather than weakening exact
effects, ordered transfer modes, or dependency ordering if future package
interfaces require independently shipped function-type evidence.
