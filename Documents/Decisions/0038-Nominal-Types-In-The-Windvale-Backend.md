# Decision 0038: Nominal types in the Windvale backend

- Date: 2026-07-31
- Status: Accepted and cross-host qualified at `f39ff73913177de9e0f03896074262001d4eee00`

## Context

The qualified primitive backend can emit static data and functions, but it rejects every record and enum declaration before code generation. That boundary prevents the Windvale-written compiler modules from becoming backend inputs: their phase summaries, tokens, declarations, bindings, WVIR records, and status contracts are all nominal values.

WVIR already carries canonical record and enum identities, field/member indices, nominal function/local shapes, and the six required operations. WVB 1.6 already defines the tagged Types section and matching runtime/verifier behavior. A new IR, bytecode version, or runtime representation would duplicate contracts that are already qualified.

## Decision

Extend `Compilerˉsourceˉwvb` to serialize the complete current Seed record and enum subset for one portable module.

- Emit Types entries in canonical WVB order: records by ordinal name, followed by enums by ordinal name.
- Preserve record field and enum member declaration order inside each type.
- Encode primitive field shapes directly and enum field shapes with their canonical nominal type index.
- Encode record and enum shapes in function parameters, results, locals, and compiler temporaries.
- Lower WVIR `Recordˉcreate`, `Recordˉfield`, `Enumˉconstant`, `Enumˉequal`, `Enumˉnotˉequal`, and `Enumˉname` to their existing WVB opcodes.
- Continue to use validated WVSD identities as the source of canonical nominal indices; do not introduce a second type directory or remapping format.
- Keep capabilities, hosted/system profiles, imports, and multi-module backend translation as explicit later boundaries.

The Stage 0 compiler remains the differential oracle. A deliberately interleaved fixture must exercise canonical type ordering, primitive and enum record fields, nominal parameters/results/locals/temporaries, every nominal WVIR operation, verifier acceptance, and runtime execution.

## Consequences

The backend can now express the nominal value model used throughout the compiler frontend. This removes a semantic blocker to self-hosting without changing WVB 1.6 or committing to nested records, mutable fields, a general heap, or a native ABI layout.

Nominal WVIR shapes are wider than primitive shapes: one kind byte plus a `u32` Types index. Function metadata sizing must therefore use encoded shapes rather than assuming one byte per local or parameter.

The remaining compiler closure still has two independent barriers: the one-module backend boundary and the measured repeated-body-traversal/source-envelope limits. Nominal support does not by itself claim bootstrap closure.

## Verification gate

The candidate must pass:

- the focused source-to-WVB conformance test with the primitive, data/text, and nominal fixtures;
- exact WVB byte equality with Stage 0 for all three fixtures;
- mandatory WVB verification and runtime execution, with the nominal fixture returning `11`;
- the complete Standard suite and native verifier on Windows; and
- exact-commit Debian qualification with matching normalized reports and byte-identical retrieved portable artifacts.

Exact commit `f39ff73913177de9e0f03896074262001d4eee00` passed every gate on Windows x64 and Debian Linux x64. Both hosts passed all 48 tests and the complete native verifier with zero-warning Release builds; their normalized contracts matched, and all 57 retrieved portable artifacts were byte-identical. The exact evidence is recorded in `Documents/Project/Seed-Verification-Evidence.md`.
