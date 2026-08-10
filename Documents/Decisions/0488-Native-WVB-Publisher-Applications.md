# Decision 0488: Native WVB publisher applications

- Status: Accepted
- Date: 2026-08-10
- Contract: [native WVB publisher](../../Specifications/Windvale-Native-Wvb-Publisher.md)

## Context

The general WVB publisher already owned semantic candidate admission and the
durable publication transaction in Windvale, but its current 159,328-byte WVB
still depended on the frozen C# application writer for the exact Windows and
Linux publisher containers. Decisions 0475 through 0487 established a native
publisher-overlay pipeline for the smaller hosted-verifier publisher and its
promoter. That pipeline admitted exact WVB/WVO identity and private transaction
geometry, so extending one explicit role was smaller and safer than keeping a
second target-specific writer.

## Decision

- Extend the publisher-overlay records with exact role 2 for the general WVB
  publisher while preserving every role-0 and role-1 byte.
- Admit the 159,328-byte WVB and 1,292,411-byte native WVO, including `Main`
  at 0, transaction begin at 5,475, and transaction apply at 4,686.
- Emit distinct `WVPB 1` metadata for role 2. Do not reuse `WVVP` or store a
  caller-selected digest.
- Reuse the existing five-byte startup, immutable-snapshot publication
  adapters, SHA-256 object, six-service base constructor, target bindings,
  import page, and final PE/ELF materializers with exact role-2 geometry.
- Add one ordinary paired constructor and a separate candidate directory. Keep
  the retained `Native-Front-Door` publisher unchanged until dual-host and
  grouped qualification authorize promotion.

## Evidence

The digest-bound native build front door reproduces the exact WVB, and the
native lowerer reproduces the exact WVO. The shared pipeline constructs:

- Windows: 1,313,792 bytes, SHA-256
  `e95676eabf80e5230d39241a9967b47bf61b4c96bddca0280ff0abb772bae1d1`;
- Linux: 1,311,685 bytes, SHA-256
  `3bb76b7ab4f5f5a00d9f949e70a65d49aac7b0973856e6a6148f2a9a5ca38c72`.

Those identities equal the independent frozen Stage 0 writer. The focused
managed publisher test passes one selected case, and the native publisher-file
owner now contains fifteen fixed cases covering exact source/object/fragment
identity, all three overlay roles on both targets, current-host execution,
admission, rejection preservation, and the existing promoter-to-publisher-to-
verifier chain. No broad local qualification suite was run for this slice.

## Consequences

The normal candidate construction of both general WVB publisher applications
no longer needs a C# PE/ELF writer. The C# writer remains frozen differential
and recovery evidence. The old and new publishers both still reject the current
compiler/build-driver candidates at the same semantic-verifier boundary, so
this decision does not claim compiler/build-driver self-convergence or front-
door promotion.

## Reconsideration

Reconsider this decision if either host cannot reproduce the pinned bytes, if
role 0 or 1 changes, if the transaction ABI changes, or if the semantic
verifier gains a new canonical WVB format that requires a new exact publisher
role.
