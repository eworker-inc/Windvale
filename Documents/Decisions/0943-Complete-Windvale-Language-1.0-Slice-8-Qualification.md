# Decision 0943: complete Windvale Language 1.0 Slice 8 qualification

## Status

Accepted on 2026-09-04 and qualified by source commit
`47dd3d69fef8a0ac5b894885b0a1917e21033622` and the linked paired-host
evidence. This decision completes the frozen Language 1.0 compiler track; it
does not qualify unfinished Libraries 1.0 profiles or the Windvale 1.0 product.

## Context

The Language 1.0 migration divided the frozen source design into eight bounded
slices. Slice 8 added explicit unsafe-memory containment and authenticated
Foreign calls without ambient addresses or undeclared authority. Its last gates
were to carry the exact source-produced call through a real system boundary,
reconstruct the current compiler deterministically, and pass the complete
Windows and Linux qualification selection.

`Runtime/Windvale/Foreign-Record-Consumer.wv` is the real boundary. It imports
the canonical Foundation Memory, Result, and Unsafe modules, constructs bounded
scratch storage, derives a contained write pointer, and consumes it immediately
through the registered Linux SysV record provider. The production admission
owner authenticates and compiles that repository source through WVB 1.38,
verification, native lowering, assembly, linking, packaging, and Linux
execution.

Earlier complete runs exposed two verifier defects rather than source-semantic
failures: a Linux process could disappear between `/proc` discovery and stat
reading, and Windows could transiently retain a file-read test directory during
cleanup. Both boundaries now fail or recover explicitly and their repaired
cases pass inside the final qualification.

## Decision

1. Accept commit `47dd3d69fef8a0ac5b894885b0a1917e21033622` as the qualified
   implementation identity for the frozen Language 1.0 compiler and Slice 8.
2. Accept the exact WVB 1.38 registered-Foreign path, affine pointer
   containment, typed native ABI lowering, and real system-profile consumer as
   the completed bounded Slice 8 contract.
3. Accept the [paired-host qualification evidence](../Evidence/2026-09-04-Language-1.0-Slice-8-Qualification.json): each host passed 126 native
   owners and 5,981 cases, both hosts reproduced the compiler deterministically,
   both declared WebAssembly subsets passed, and the aggregate workflow gate
   passed.
4. Keep Seed and its qualified WVB 1.11 recovery contract frozen and separate.
   The Language 1.0 compiler is the forward compiler; this decision creates no
   backward-compatibility requirement for obsolete experimental formats.
5. Treat the Language 1.0 compiler track as complete only within the frozen
   source design and its explicitly declared target subsets. New source
   semantics, WVB behavior, authority, or ABI surface requires a separately
   versioned contract and new evidence.
6. Continue the product critical path with the required Libraries 1.0 profiles.
   Do not repeat complete compiler qualification unless a later change
   invalidates one of its declared inputs or contracts.

## Consequences

- Compiler Slice 8 and the bounded Language 1.0 compiler track are Qualified,
  not merely Candidate.
- The compiler has no known remaining implementation or conformance gap inside
  this frozen scope. This is not a `v1.0.0` release claim.
- Windows, Linux, and WebAssembly remain explicit target promises rather than
  one undifferentiated portability claim.
- The long-running qualification suite is retained as a final gate, while
  ordinary library development continues to use focused causal owners and
  reusable evidence.
- The database-storage owner is correct at this identity but its cold duration
  remains a named verification-workflow performance defect to reduce.

## Reconsideration triggers

Reopen or supersede this decision if the frozen Language 1.0 source contract,
WVB 1.38 behavior, compiler reconstruction identity, unsafe-memory containment,
registered Foreign ABI, or a dependency declared by the qualification gate
changes. Do not reopen it for a documentation-only closure commit, an unrelated
library implementation, or a narrower passing focused check.
