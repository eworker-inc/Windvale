# Decision 0873: complete bounded compiler-scale analysis

## Status

Accepted on 2026-08-28. Candidate promotion and cross-host qualification
remain pending.

## Context

Decision 0872 removed a complete compiler-scale build from the ordinary generic
development oracle, but the named self-analysis workload still exhausted the
profile-7 native carrier's fixed `2^37` instruction allowance. Phase probes
showed symbol construction and generic-signature discovery completing before
ordinary function-body WIR planning stopped. A first signature-only reduction
crossed the instruction boundary and then exposed the unchanged 224 MiB
dynamic text/byte arena. The compiler therefore still contained enough
avoidable immutable-byte construction to prevent a bounded source-built pair.

The repair must preserve exact names, declaration validation, WIR order,
diagnostics, evidence limits, and final WVB bytes. Neither a larger carrier nor
a relaxed value bound is evidence of a faster compiler.

## Decision

1. Symbol lookup constructs each canonical emitted name once before nominal
   ordering and reuses its bounded directory/payload slice for every comparison.
   Exact UTF-8 byte ordering remains the oracle.
2. Local lookup rejects a qualified name as soon as it observes `.` and scans
   the function-private table from newest to oldest. The compiler already
   rejects duplicate local names across one function, and the existing
   declaration-before-use and exact-byte checks remain unchanged.
3. A requested callable-parameter signature parses every parameter to advance
   the source cursor but performs concrete or specialized type resolution only
   for the requested parameter. Complete declaration validation remains owned
   by the earlier source phases, and the return type is still resolved exactly.
4. WIR operation, temporary, and operand streams accumulate small entries in
   bounded local chunks and merge complete function payloads through bounded
   64 KiB publication chunks. Materialization preserves declaration and
   operation order; all existing 4 MiB product limits remain enforced over the
   combined complete and pending lengths.
5. Profile 7 retains its finite `2^37` instruction allowance, 224 MiB dynamic
   text/byte arena, 1 MiB name stride, and every existing per-value and evidence
   bound.

## Evidence

The analyzer closure consumes 2,101,294 source bytes and publishes 104 manifest
bytes, 316,740 binding bytes, and 4,110,500 WIR bytes. Its 758-function portable
WVB is 1,515,372 bytes at SHA-256
`9876f178f4ac06872a44f44085de5d72f17777abf462985300f6e453e4b625d9`.
The current-host Windows package is 49,985,024 bytes at SHA-256
`3318d1fc3f454381831cfbc97fe4f81c368fc66d673b02805db60195b00a8ad1`.

The emitter closure, built by that analyzer and the pinned WVIR 1.9 bridge,
consumes 2,035,424 source bytes and publishes 104 manifest bytes, 318,492
binding bytes, and 3,951,708 WIR bytes. Its 738-function WVB is 1,523,605 bytes
at SHA-256
`a0beb624dcc225b0ccdac848d808af1faef63cdb66eb650faf0bb9216e0815c9`.
The Windows package is 33,315,840 bytes at SHA-256
`711e5c1b51d76b6f0049f8c770d92360742290956a6d9eca55727e84a13194c1`.

The source-built pair completes the formerly failing compiler-scale target in
the unchanged bounded carrier. It analyzes 2,087,629 source bytes into 104
manifest bytes, 314,968 binding bytes, and 3,723,236 current-WIR bytes, then
emits the exact retained 1,380,487-byte WVB at SHA-256
`b2a83f05c5079fb71dc365b751888049b02f022a899c7cf63e1293da7332f3a6`.
The retained analyzer reports the same 1,262 functions, 19,174 blocks, 89,614
operations, 81,969 temporaries, and 72,168 operands. Its older 32-byte operation
form is exactly `89,614 * 4` bytes larger than current WIR's 28-byte form; the
matching emitters converge on the byte-identical WVB.

A same-generation comparison exercises multiple publication flushes over the
bindings compiler closure. Both analyzers publish the exact same 2,034,560-byte
WIR at SHA-256
`776a5583dc08ddc48071444c8564fd537b94fbf76a74b8e36c7e906bc1e6fdf5`.
The incremental generic oracle also retains 12 exact artifact comparisons.

The selected Windows Development evidence passes editor support, 26 Seed, 20
unsafe-WVB, 500 compiler-containment, 59 generic-binding, 21 generic-layout, 28
generic-materialization, 20 generic-WVLB-carrier, 482 Language 1 front-door, 59
callable-semantics, 2 lowerer-rejection, and 2 console-packager reconstruction
cases. The callable evidence is 4,394,089 bytes at SHA-256
`b0d9f00087d04695004dfc04c331c646a192cd9291741dd78b7bf395acd8ff80`.
The 114-owner, 5,568-case registry remains structurally unchanged and advances
to SHA-256
`a2852df9feab3e8e79a80562db3df6eebffc0f321d65f44bf417048960cfb6c5`.

## Consequences

- Whole-compiler analysis now finishes within the existing native instruction
  and dynamic-value bounds instead of publishing no artifact.
- Large WIR construction copies bounded chunks rather than the complete prefix
  for every operation and function merge.
- The current analyzer and emitter can be reconstructed as one exact pair
  without making the retained seed a second evolving compiler.
- This is local Windows Development evidence. It does not promote artifacts,
  claim parallel-capable execution, or replace the final dual-host Slice 7
  Qualification gate.

## Reconsideration triggers

Reconsider the chunk sizes when a native byte builder provides lower measured
copy cost with the same deterministic failure behavior. Reconsider signature
queries when the symbol product carries complete canonical signatures directly.
Reprofile the full compiler after promotion; do not infer a permanent speed or
memory threshold from this migration workload alone.
