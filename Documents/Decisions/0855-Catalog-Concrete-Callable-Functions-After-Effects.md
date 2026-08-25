# Decision 0855: Catalog concrete callable functions after effects

- Status: Accepted
- Date: 2026-08-25

## Context

WVFT 1.0 can represent an exact structural function type, and WVEF 1.0 can
resolve exact transitive effects, but no phase connected real source function
declarations to those identities. Guessing a type before effect closure would
make the identity incomplete. Treating generic templates, legacy `void`, or
borrowed results as ordinary callable values would erase information that the
current WVFT and runtime do not represent.

The first implementation also exposed a development-scale boundary. Adding the
complete 183-function WVIR implementation to the new phase's compiler closure
pushed source analysis beyond its retained evidence limit. Re-running the full
semantic WVIR verifier in every downstream phase would duplicate work and make
compiler/test builds larger without strengthening the already validated
immutable phase result.

## Decision

1. Run callable-type cataloging only after exact WVEF analysis.
2. Publish compiler-private WVCF 1.0 with one disposition per prepared WVIR
   function entry and one embedded, independently valid WVFT catalog.
3. Classify non-functions, unspecialized generic templates, legacy zero-result
   functions, borrowed-result functions, and concrete callable functions
   explicitly. Do not guess or erase an unsupported representation.
4. Give every concrete callable its exact source parameter modes,
   binding-derived shapes, result shape, flags, module profile, and transitive
   effect masks. Reuse the first equal WVFT identity deterministically.
5. Require ascending first use of every WVFT instance and reject unreferenced
   catalog identities.
6. Bound WVCF to 87,380 function entries and 2 MiB retained evidence while
   retaining WVFT's 256-instance, 64-parameter, and 16 MiB estimated-output
   limits.
7. Consume prepared immutable WVIR evidence through a checked structural view.
   Keep full operation-level semantic validation in the preceding WVIR owner;
   do not import or rerun that complete implementation here.
8. Parse at most 64 module profiles once per analysis and reuse them for every
   function in that module.
9. Keep WVCF and WVFT out of WVB. Runtime callable values, captures, indirect
   calls, verification, and execution require a later explicit representation.

## Consequences

The compiler now has a deterministic post-effect answer for every named source
function: either one exact WVFT identity or an exact reason it cannot yet be a
callable value. Equal signatures share type identity without sharing function
identity. Generic templates, legacy `void`, and borrowed results remain visible
rather than being silently accepted under incomplete semantics.

The prepared-phase boundary reduces redundant verification and keeps the
standalone compiler closure within its existing evidence envelope. Its local
WIR checks protect every read but do not replace the full WVIR semantic
verifier. Any external or serialized input that has not passed the preceding
phase remains inadmissible.

The focused callable test reuses the existing effect-semantics executable
instead of adding a sixth compiler-sized native package. It covers edition-1
concrete, generic-template, and borrowed-result functions, a separate legacy
WVSS v1 `void` function, equal-signature reuse, exact private shape assignment,
directory validation, and truncated safe access.

This decision still does not make function values executable. Callable-aware
WVIR and WVB, noncapturing named-function values, closure environments, escape
and move enforcement, indirect-call verification, and runtime/native execution
remain later Slice 6 checkpoints.

## Evidence

The standalone callable compiler closure builds through the maintained native
front door as 548 functions and 977,399 WVB bytes. The catalog probe is folded
into the existing effect-semantics executable rather than packaged as a sixth
compiler-sized application. The focused Windows owner passes all 31 cases:

```text
native language 1 callable semantics status=Passed cases=31 result=42 modules=5 wvb-bytes=3243802 evidence-sha256=f58c7cfe1b856f462b059f0416675732b821b085d4ca2c073f0e0dcd31b9b52f
```

The registry remains 113 owners and advances to 5,473 cases. Its 17,993
LF-only bytes have SHA-256
`400967ff5b8c15b085b8efac2ba27cbb95393f8bc3213f2eab0645a1ed97d77e`.
Independent Linux execution and repository-wide Qualification remain separate
claims.

## Reconsideration triggers

Reconsider the dispositions when Language 1.0 selects exact borrowed-return or
legacy migration semantics for runtime function values. Reconsider the
prepared structural view only if immutable phase evidence can be forged across
the internal compiler boundary. Any replacement must retain bounded reads,
deterministic identity, exact effects and modes, and must not reintroduce
redundant whole-phase verification.
