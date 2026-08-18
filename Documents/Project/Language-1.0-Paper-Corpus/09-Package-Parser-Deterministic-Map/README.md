# Language 1.0 paper workload 9: package parser and deterministic map

## Status

Draft reviewed after the project owner accepted all six findings on 2026-08-17
under
[Decision 0763](../../../Decisions/0763-Resolve-Language-1.0-Package-Parser-Findings.md).
This is paper Language 1.0 source. Current Seed tools do not accept it, and it
does not implement package parsing, deterministic collections, package-data
deduplication, or freeze edition 1.

## Result

Seven Core modules express one bounded deterministic package audit that:

1. reads a 63-byte `WVPACK1` manifest and a deliberately shuffled 111-byte
   `WVLOCK1` lock from typed package data;
2. rejects both inputs by byte maximum before constructing collections;
3. validates lower-ASCII package identities and strict whole-value u64 versions;
4. builds ordered sets for dependencies and an ordered map for four packages;
5. freezes those collections and validates the manifest against the lock;
6. rejects unknown dependencies and computes a bounded, lexical-tie
   topological order without recursion;
7. renders packages and dependencies only by canonical rank; and
8. produces one exact 160-byte report with SHA-256
   `a9df168004784b0b1af30bb2c563d9ae166bd3a38dceb388b731b8d72dcba2b7`.

Two `bytes` declarations bind the same 53-byte notice content. The canonical
package has four declaration references but only three distinct content
objects and 227 unique payload bytes. Source observes equal immutable values,
not storage identity.

## Source modules

| Module | Responsibility |
| --- | --- |
| `Packageˉgraphˉpackage` | Four typed package-data declarations. |
| `Packageˉgraphˉtypes` | Limits, identities, parsed values, failures, result. |
| `Packageˉgraphˉordering` | One ordinal scalar order for package identity. |
| `Packageˉgraphˉparser` | Bounded line/word parsing, validation, set/map construction. |
| `Packageˉgraphˉgraph` | Reference validation and bounded deterministic topology. |
| `Packageˉgraphˉreport` | Explicit version-1 canonical serializer and literal pressure. |
| `Packageˉgraphˉapplication` | Limit validation, budget split, orchestration, publication. |

Every module is Core and targets Windows, Linux, and Windvale. There is no
filesystem capability, runtime path, reflection, hash map, locale, recursion,
task, FFI call, unsafe block, or host package parser.

## Evidence index

- [format contract](Format-Contract.md)
- [package and execution plan](Package-Plan.md)
- [semantic review](Semantic-Review.md)
- [rejected and boundary cases](Rejected-Cases.md)
- [expected outcomes](Expected-Outcomes.md)
- [implementation responsibilities](Implementation-Responsibilities.md)
- [review findings](Review-Findings.md)

## Acceptance answer

Language 1.0 can express a readable bounded parser and canonical dependency
report without reflection, implicit allocation, host maps, randomized hashing,
or serialization magic. Exact immutable map/set publication and observation
complete the required Foundation surface; the source grammar needs no new form.

## Nonclaims

This is not the Windvale product package format, dependency solver, semantic
version range language, registry protocol, signature format, installer, or
dynamic module loader. It is a small adversarial consumer of the general
Language 1.0 text, collection, package-data, ownership, and budget contracts.
