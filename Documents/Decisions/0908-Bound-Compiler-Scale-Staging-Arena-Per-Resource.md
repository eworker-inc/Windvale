# Decision 0908: bound compiler-scale staging arena per resource

## Status

Accepted and implemented locally on Windows on 2026-09-01.

This decision changes the allocation lifetime of the versioned WVO staging
producer. It does not change WVB, WVO, `WVOP`, chunk grouping, object bytes,
host capabilities, or the 4 MiB publication ceiling.

## Context

The Slice 8 analyzer is a 1,552,090-byte WVB. Its canonical native object is
50,761,605 bytes split into 50 published resources and a 624-byte manifest.
The existing staging producer could construct and publish the first 39
resources byte for byte, then terminate with text-arena exhaustion while
constructing the next resource.

Each publication remained within the existing 4 MiB value limit, and the
1.25 MiB greedy code-step coalescing policy was working as specified. The
failure came from lifetime rather than value size: `Main` repeatedly assigned
new immutable `Bytesˉconcat` results while retaining the function-entry arena
checkpoint until all resources and the manifest had been published. Completed
resource values therefore remained charged even though their bytes were no
longer needed by Windvale code.

Increasing the global hosted arena would postpone the same failure and make
the staging cost depend on total compiler output. Reducing the coalescing
target would increase resource count without establishing a lifetime bound.
Neither alternative addresses the ownership mismatch.

## Decision

1. Retain the existing cursor plan, 1.25 MiB greedy code-step coalescing,
   4 MiB per-resource publication ceiling, 62-resource staging limit, exact
   WVO positions, and `WVOP 1.0` manifest format.
2. Construct and publish exactly one resource inside a private
   `Writeˉnextˉchunk` function. Return only scalar status, cursor, position,
   index, and length evidence to `Main`.
3. Rely on the qualified ABI 22 function-entry checkpoint to reclaim that
   helper's dynamic text and byte allocations on every normal return. No
   resource byte value may remain live in `Main` after its write completes.
4. Keep manifest construction in `Main`; it retains only bounded scalar entry
   evidence and constructs the final manifest after every resource write has
   returned successfully.
5. Permit the helper to recompute the first step beyond a coalesced code
   resource when `Main` resumes at that cursor. The lookahead is bounded and
   pure, and the resource boundary, bytes, position, and following cursor must
   remain identical to the existing plan.
6. Do not add a capability, mutable buffer, retry, or publication shortcut.
   Existing write failure and manifest-last behavior remain unchanged.

## Evidence

The repaired staging producer contains 649 functions in 705,579 WVB bytes at
SHA-256 `257ac6389d5bf9647e989654cb41a1cd3af298caa5f6a12f425bb0764b0caa7e`.
Its sequentially packaged candidates are:

| Target | Bytes | SHA-256 |
| --- | ---: | --- |
| Windows x64 | 10,277,376 | `14e6bcab721fe9eb1f8afc6a362d57196afac8a9acfa5d8dd50fc67ce0eaf3d9` |
| Linux x64 | 10,276,864 | `701616a768c205fbb402a8e09d37c95ecffba1c0b93a4297e82d59b39fb6cc9a` |

On Windows, the repaired producer stages the exact compiler-scale analyzer to
50,761,605 WVO bytes in 50 resources and a 624-byte manifest. All 50 resources
and the manifest match the trusted predecessor byte for byte; the manifest
SHA-256 is
`e962868e0780c88c7b13dde98c0bbd2b655dbddeea6863865440c8b28af55780`.
It also self-stages its own WVB into 15 resources with every resource and the
manifest matching the predecessor. The exact segmented-toolset reconstruction
owner passes all five WVO staging, image staging, transport, identity, and
compiler-scale cases.

## Consequences

- Peak staging arena retention is bounded by one current resource plus the
  bounded manifest, rather than by cumulative object size.
- Output bytes and consumer contracts remain unchanged.
- A coalescing-boundary step can be analyzed twice, but it is bounded work and
  does not perform a second write.
- The Linux artifact is reconstructed and identity-checked locally; actual
  Linux execution remains part of paired-host qualification.
- This repair removes a development and bootstrap blocker. It does not expand
  Slice 8 language or runtime semantics.

## Reconsideration triggers

Reconsider this structure if the host gains a specified bounded mutable byte
builder, if publication becomes streaming, or if measurement shows the pure
boundary lookahead to be material. Any replacement must preserve exact WVO and
manifest bytes, manifest-last publication, explicit resource bounds, and
bounded cleanup and failure behavior.
