# Decision 0957: represent immutable Foundation payload borrows in candidate WVB 1.39

- Date: 2026-09-04
- Status: Implemented as a local Windows source-publication candidate; complete
  verification, execution, direct borrowed-payload call-parameter identity,
  Linux reproduction, and cross-host qualification remain pending
- Extends: [Decision 0942](0942-Advance-The-Frozen-Source-Identity-For-Foreign-And-Payload-Borrowing.md)
- Preserves: qualified Language 1.0 Slice 8, earlier WVB versions, owner
  lifetime, exact generic identity, and narrower consumer version boundaries

## Context

The Libraries 1.0 Option/Result checkpoint already lowered immutable payload
borrows through typed WVIR 1.33/1.34 operation `191`. The source validator
requires one direct named owner, freezes it through function exit, propagates
non-owning payload provenance, and rejects escape, duplication, mutation,
consuming use, and serialization. WVB emission previously stopped before this
operation, so the accepted source behavior had no versioned publication form.

The representation must distinguish the borrowed Option view from its owner and
the extracted borrowed payload from an owned value without publishing a pointer,
host handle, or implicit ownership transfer. It must also leave WVB 1.11 through
1.38 byte-identical for unaffected source.

## Decision

1. Candidate WVB 1.39 adds 13-byte opcode `E1 foundation.value.borrow`. Its
   immediates are the direct owner slot, exact canonical `Option<borrow T>` Types
   index, and projection `1` for `Option.Present`, `2` for `Result.Valid`, or `3`
   for `Result.Failure`.
2. `E1` consumes no stack value and produces an ephemeral borrowed variant view
   encoded with existing shape `29`. Zero and projections above three reject.
3. New recursive shape `37` wraps the exact planned payload shape for
   compiler-generated temporary and non-parameter local metadata. It is a
   non-owning identity, not a pointer or independently storable owner.
4. A bounded per-function plan freezes every named owner, propagates borrowed
   provenance through the generated load, case-test, payload-projection, and
   store path, preserves temporary identity, and suppresses ownership-taking
   loads for those values. It uses no global compiler state.
5. The writer selects minor 39 only when reachable operation `191` is emitted.
   Unaffected modules keep their prior lowest required minor and bytes.
6. The first independent reader is structural and bounded to 16 MiB. It is not
   the complete compiler-aligned verifier and grants no execution admission.
7. Shape `37` is not yet written into the corresponding callee parameter when a
   borrowed payload crosses a direct helper call. No complete verifier or
   runtime may admit WVB 1.39 until that identity and its lifetime rules are
   represented and checked.

## Evidence

`Foundation-Value-Payload-Borrow-Wvb.wv` covers all three projections with a
nominal record and `u32` payload. The source is compiled twice. The independent
reader requires byte-identical output, canonical WVB 1.39 with seven sections,
three `E1` instructions in projection order `1,2,3`, shape-`29` borrowed Option
views, shape-`37` payload locals, exact Option/Result relationships, and bounded
code geometry. It rejects six mutations: prior minor, unknown opcode, zero and
unknown projections, out-of-range owner, and out-of-range Option type.

The exact focused Windows command
`Invoke-WindvaleTests.ps1 -Owner language-1-memory-budget-split-execution -AllowLongRun`
passed all 184 cases in one attempt. The owner reported 502,292 ms and the
wrapper completed in 502,659 ms. The 12 new cases are source-publication and
structural-reader evidence; the database suites, complete WVB verifier, runtime,
native lowerer, WebAssembly targets, packages, OS consumers, and Linux were not
run for this candidate.

A later experiment attempted to infer shape-`37` call parameters with a global
fixed-point relation. Building the enlarged compiler exhausted the bounded text
arena and exited with process result `66` before any case executed. That
experiment was removed. It does not invalidate the earlier passing bounded
candidate and is not part of this decision.

## Consequences

- Direct-owner immutable Option/Result payload borrows now have an explicit,
  deterministic candidate WVB publication form.
- The publication keeps ownership and payload provenance visible without
  defining a target representation or expanding runtime authority.
- The next compiler chunk is narrow: encode cross-call borrowed-payload
  parameter identity, teach the complete verifier shape `37` and opcode `E1`,
  then add bounded scalar execution and Linux reproduction.
- `Borrowˉmut`, consuming `Take`, `Map`, broader Foundation APIs, and consumer
  migration remain separate Libraries 1.0 work.

## Reconsideration triggers

Revise the candidate if complete verification requires a different recursive
shape, if cross-call identity cannot be represented without global inference,
or if runtime implementation reveals an observable ownership or target-layout
dependency. Do not widen compiler memory limits merely to retain the discarded
global fixed-point experiment.
