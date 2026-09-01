# Decision 0900: add a compiler-scale hosted geometry and artifact readers

## Status

Accepted and implemented as a local Windows development checkpoint on
2026-08-31. This decision changes compiler construction and hosted packaging;
it does not change source semantics, canonical WVB, native ABI, or a promoted
cross-host tool identity.

## Context

The growing Language 1.0 analyzer and emitter crossed two development limits at
the same time. The analyzer needed more bounded hosted input and name capacity,
while the emitter's complete source closure approached the immutable 4 MiB
Windvale value ceiling even though emission uses only artifact readers and
validators from many compiler modules. Rebuilding monolithic source sets and
repeating already completed analysis also made small compiler changes appear to
stall in packaging and hashing work.

Increasing every hosted application profile would waste address space and
quietly broaden unrelated products. Splitting compiler files into numbered
fragments would reduce review quality without reducing semantic work. The
development path instead needs one compiler-only geometry, deterministic
target-aware source closure, and reusable phase evidence.

## Decision

1. Advance Profile 7 to the measured general compiler-emission geometry: a
   301,465,600-byte arena, 64 file inputs, an 8,192-byte name stride, name base
   303,636,480, data base 304,160,768, and scratch base 572,596,224. It retains
   the existing Profile-7 instruction and outer-runtime limits.
2. Add hosted container Profile 8 for compiler-analysis applications only. It has
   a 435,945,472-byte arena, 32 file inputs, an 8,192-byte name stride,
   name base 438,116,352, data base 438,378,496, and scratch base 572,596,224.
   It retains Profile 7's outer Windows and Linux runtime extents rather than
   widening every hosted profile.
3. Keep ordinary current-split targets on Profile 7. Select Profile 8 only for
   the analyzer and other explicitly compiler-scale products.
4. Extend hosted metadata, planning, construction, startup, runtime, admission,
   and platform consumers to recognize the exact Profile-8 geometry. Reject an
   unknown or mixed profile rather than deriving offsets from host defaults.
5. Preserve the established atomic Profile-7 publisher. For a Profile-8
   construction, the planner may publish a separately validated Profile-7
   publication-plan header while the real Profile-8 plan continues to own
   platform, startup, source-set, and container geometry. This bridge changes
   no payload offsets or final application identity.
6. Generate focused compiler artifact readers from their authoritative source
   modules. Retain only project-reachable functions plus the complete transitive
   closure of local helpers. Parse local function declarations independently of
   exported-name prefixes so helpers such as `Findˉlifetime` cannot be dropped.
7. Make generation deterministic and provide `--check`; generated readers are
   reviewable repository inputs, not ambient build-cache state.
8. Publish and cache explicit symbol, manifest, binding, and WVIR analysis
   checkpoints. Reuse a checkpoint only when its source set, producer identity,
   command contract, and prior evidence agree exactly.
9. Read only service-bundle segments that intersect the source-set digest
   region currently being reconstructed. Validate every used response; in the
   compiler-scale layout, region zero covers the complete segment inventory.
   Do not perform a redundant pre-read pass before the digest regions.
10. Keep construction progress visible at phase, region, chunk, and segment
   boundaries. A long operation must show bounded forward movement.

## Evidence

The generator deterministically reproduces 14 artifact-reader files and its
check reports `status=Passed files=14`. The optimized emitter closure completes
symbol analysis, full typed-WVIR analysis, and emission with 1,557,184 source
bytes, 3,556,880 WVIR bytes, 738 functions, and 1,249,322 code bytes. Its
1,479,716-byte WVB has SHA-256
`8d2ca39f2792210699a2ae11be33b28f44136722b606459b9e1a7fc86d2b98c1`.

The Profile-8 Windows analyzer application is 52,659,712 bytes at SHA-256
`211caf31790087d81537be5a29700097e57ed87333d7696691cbbb83dd3c3ac0`.
The local emitter package uses eight bounded application segments and produces
30,899,712 bytes at SHA-256
`713007426ffca090f1981647b09d22464138f849f5513990ec8ed979b5682c53`.
The current verifier accepts the WVB, while the older pinned verifier does not;
that discrepancy is an explicit promotion dependency, not ignored evidence.

## Consequences

- Compiler growth no longer forces a wider default hosted profile.
- The emitter compiles the code it consumes instead of carrying producer-only
  compiler paths into every build.
- Analysis and emission can resume from exact immutable phase evidence when
  their declared inputs are unchanged.
- Profile 8 remains development infrastructure until both hosts reconstruct and
  promote the exact toolset and the current verifier front door.

## Reconsideration triggers

Reconsider this geometry if a compiler-scale product exceeds any explicit
capacity, if an unrelated application begins selecting Profile 8, if generated
readers diverge from their authoritative modules, if a cache accepts mismatched
producer or input evidence, or if the Profile-7 publication bridge changes final
bytes or weakens atomic publication.
