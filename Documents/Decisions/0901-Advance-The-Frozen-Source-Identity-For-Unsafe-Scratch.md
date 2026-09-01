# Decision 0901: advance the frozen source identity for unsafe scratch

## Status

Accepted on 2026-08-31 and implemented by the Language 1.0 source-amendment
candidate named below. This decision advances source-design identity; it does
not claim WVB, runtime, native, or paired-host execution of unsafe scratch.

## Context

Decision 0894 bound the accepted Language 1.0 source design before the
canonical Foundation unsafe type identities were published. Commit
`8571c93425eeee3631115ae1f55b261f1a970111` subsequently added those identities
to the frozen Foundation specification without advancing the amendment
manifest, so the front-door frozen-fixture check correctly rejected the tree.

Decision 0899 now adds the first exact producer, `Constructˉscratch`, at the
source-binding and typed-WVIR boundary. The Foundation specification must
distinguish that implemented producer from the remaining unimplemented unsafe
operations, and the migration record must describe its evidence and remaining
executable boundary. Leaving either file outside the accepted identity would
make the frozen design disagree with the compiler being implemented.

## Decision

1. Accept
   `Documents/Project/Windvale-Language-1.0-Source-Amendment-0901-Candidate.txt`
   as amendment version 9 over the immutable Decision-0894 manifest.
2. Bind the current exact bytes of all 13 core design inputs, with only
   `Specifications/Windvale-Language-1.0-Foundation.md` and
   `Documents/Project/Windvale-Language-1.0-Migration.md` changed relative to
   that base.
3. State in the Foundation contract that exact `Constructˉscratch` source
   binding and WVIR operation `186` are published while executable WVB,
   allocation, pointer production, borrowing, runtime, and native ABI remain
   unavailable.
4. Keep the 16 accepted source-design decisions and both paper corpora byte
   identical. The complete candidate remains 251 identity inputs.
5. Rebind the migration-fixture verifier and inventory to amendment version 9.
   Do not weaken, skip, or make the frozen-input comparison advisory.

## Consequences

- The frozen source identity again matches the repository and fails closed on
  an unrecorded change.
- The public Foundation text no longer says that every unsafe operation is
  unpublished after one typed producer has been implemented.
- This amendment does not make operation `186` executable. The source-WVB
  boundary continues to reject it until a later decision supplies one complete
  verified representation and containment path.
- Prior manifests remain immutable provenance and continue to identify their
  historical source states.

## Reconsideration triggers

Reconsider this decision if the exact `Constructˉscratch` signature or typed
WVIR semantics change, if another frozen core input must change, or if the WVB
and runtime boundary becomes executable. Any such change requires a new
append-only amendment rather than editing this manifest.
