# Decision 0799: Pack fixed-length keyword dispatch

- Status: Accepted
- Date: 2026-08-20

## Context

Language 1.0 adds several seven- and eight-byte English keywords. The lexer
already dispatches by byte length, but these two groups compared every
same-length identifier against four or six text values in sequence. Each
comparison materialized the expected UTF-8 value and then compared bytes. The
compiler closure contains 12,369 seven-byte and 15,546 eight-byte ASCII-word
occurrences before repeated parser traversal, so ordinary identifiers exercise
this rejection path far more often than the small keyword set suggests.

The complete compiler remains dominated by lexer/parser traversal and immutable
record pressure. A change at this boundary must therefore have bounded local
evidence and exact output comparison; a noisy whole-process wall-clock sample is
not enough to justify a semantic or limit change.

## Decision

1. Retain byte-length dispatch and every existing token identity.
2. For a seven-byte candidate, read the overlapping little-endian words at
   offsets 0 and 3. For an eight-byte candidate, read the adjacent words at
   offsets 0 and 4.
3. Compare those words with the exact packed keyword bytes. Length plus both
   words covers the complete candidate and is collision-free; this is not a
   hash or locale-dependent comparison.
4. Perform the reads only after the lexer has proved the complete token span.
   Do not weaken UTF-8, cursor, identifier, diagnostic, or source-size checks.
5. Retain exact comparison for every other keyword length.
6. Add direct keyword and near-miss cases to the existing value-front-end
   fixture. Do not add a separate broad verifier or rerun storage, OS, or
   Qualification gates for this compiler-local change.

## Evidence

The real-lexer profiling fixture classifies `Compile` and `Compiler` in a
bounded loop. With the same current Windows scalar runner and exact
4,096-instruction ceiling:

| Implementation | Passing iterations | Rejected iterations | WVB code bytes | WVB bytes |
| --- | ---: | ---: | ---: | ---: |
| Original exact text comparisons | 2 | 4 | 10,221 | 15,856 |
| Packed exact word comparisons | 8 | 16 | 10,727 | 16,096 |

Both rejection runs stop at `WVR3011` after exactly 4,096 instructions. This is
an instruction-bound hot-path result, not a claim of a fourfold compiler-wide
speedup.

The representative analyzer probe used Windows 11 build 26200 on an AMD Ryzen
9 3900X. Its retained three-run baseline median is 53,553.067 milliseconds. One
candidate run took 55,178.412 milliseconds with a sampled 551,317,504-byte peak
working set, so no complete-compiler wall-time improvement is claimed. The old
and candidate analyzers nevertheless publish byte-identical WVSS, WVCA, WVLB,
and WVIR artifacts for the exact changed 14-source analysis-driver input.

The bounded 39-assertion value-front-end fixture calls the keyword classifier
directly for all ten packed keywords plus case-distinct and tail-distinct near
misses. The registered Language 1.0 owner remains the single final local gate
for the change. Its Windows run passed all 11 phases and 155 declared cases. The
deterministic target-aware emitter is 838,654 bytes with SHA-256
`707c3aec27b481745ae599206960bc6f9c0be0053aaae73b359cd20cd2cc4876`.

## Consequences

Common same-length non-keywords avoid repeated text construction and byte loops
while source semantics and diagnostics remain unchanged. The analyzer grows by
240 WVB bytes and still fits the existing native package limit without raising
any compiler, evidence, instruction, or image bound.

This does not solve the larger repeated-traversal or token-record construction
cost. Those remain the next structural compiler-performance work; packing more
keyword lengths is justified only by the same bounded measurement.

## Reconsideration triggers

Replace this dispatch only if a generated table, localized lexicon path, or
Unicode keyword policy preserves exact byte identity with smaller measured work.
Broaden packed comparison to other lengths only when representative source
frequencies and executable-size evidence show a net benefit.
