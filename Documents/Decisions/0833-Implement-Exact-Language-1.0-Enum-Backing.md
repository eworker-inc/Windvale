# Decision 0833: Implement exact Language 1.0 enum backing

## Status

Accepted on 2026-08-23. Candidate toolset qualification remains pending.

## Context

The frozen Language 1.0 source contract requires every enum to name one exact
fixed-integer backing type. The retained descriptorless Seed compiler sources
instead use an implicit, nonnegative `i32` form. The active compiler therefore
needed to admit the new syntax without forcing a simultaneous mechanical rewrite
of its own implementation sources, and without silently narrowing an `i64`,
`u32`, or `u64` tag into the current bytecode representation.

Adding exact enum validation enlarged the compiler closure beyond profile 7's
earlier 80,000,000,000-instruction envelope. This is a bootstrap capacity issue,
not evidence that broad optimization of the pre-1.0 seed is worthwhile. The
capacity change must be limited to the heavy compiler profile, reproducible from
bounded probes, and leave the ordinary hosted-tool profiles unchanged.

The profile geometry change also requires a coherent reconstruction of the
checked-in native hosted-container candidate. Most candidate applications can
be constructed through the ordinary retained native path. The publisher is a
special case because its WVB owns read-only admission while its platform
application also embeds the immutable atomic-publication shell.

## Decision

1. Edition-1 enums require one of `i8`, `i16`, `i32`, `i64`, `u8`, `u16`,
   `u32`, or `u64` after `:`. Each explicit tag must fit that exact backing.
   Unsuffixed literals are contextual; a suffix must match; negative values
   require signed backing; signed minima are valid; and `-0` is the same tag as
   `0` for duplicate detection.
2. The lexer carries a nonnegative integer literal as exact low/high `u32`
   limbs. The symbol phase owns backing, range, sign, suffix, and duplicate
   validation. Three appended symbol statuses distinguish missing backing,
   invalid backing, and invalid value without renumbering the retained duplicate
   name/value statuses.
3. Descriptorless Seed enums retain their implicit nonnegative `i32`
   compatibility path so the current compiler implementation need not be
   rewritten merely to cross this checkpoint. This path is implementation
   compatibility, not portable Language 1.0 syntax.
4. WIR enum operations continue to carry the nominal enum plus declaration-order
   member identity. They do not store a narrowed numeric tag. The current WVB
   1.20 writer serializes exact signed `i32` tags as two's-complement bits and
   rejects every other backing as `Unsupportedˉshape` before publication.
5. `Foundationˉmemory` now publishes the frozen `Allocationˉreason: u8` and
   `Allocationˉfailure` declarations. Their names, four tag values, and two
   `u64` amount fields match the accepted Foundation 1.0 contract exactly.
6. Compiler-family profile 7 advances to an instruction limit of `2^37`
   (137,438,953,472), a 224 MiB text/byte arena, and the retained 1 MiB name
   stride. Profiles 1 through 6 remain unchanged. The profile-7 Windows/Linux
   runtime and linker geometry advances with those bounds.
7. The 72-artifact hosted-container candidate is reconstructed and pinned by
   its complete `SHA256SUMS` inventory. The publisher's two platform
   applications are reconstructed only in an isolated, verified checkout of
   immutable release `stage0-recovery-e5a1a7473c57`; only binary products return
   to `main`, and managed source remains absent.
8. Broader compiler optimization waits for the 1.0 compiler to become the
   active seed. Before then, optimization is limited to measured blockers,
   verification waste, explicit capacity failures, and clear accidental
   complexity that directly impedes migration.
9. The accepted source identity advances through
   `Windvale-Language-1.0-Source-Amendment-0833-Candidate.txt`. It binds the two
   clarified core specifications while retaining the 0815 manifest as immutable
   provenance; the accepted source set remains 251 inputs.

## Consequences

- Analysis covers all eight backings. Five focused rejection cases cover a
  missing backing, suffix mismatch, out-of-range value, unsigned negative, and
  duplicate signed tag. A signed `i32` enum emits a 427-byte WVB that returns
  `42` and passes the current compiler-aligned verifier.
- A declaration-only all-width fixture produces 848 source-set bytes, 96
  binding bytes, and 544 WIR bytes, then reaches the explicit non-`i32` WVB
  `Unsupportedˉshape` boundary with no output.
- The exact compiler self-emission consumes a 1,953,683-byte source set and
  produces 104 manifest bytes, 293,664 binding bytes, 3,902,856 WIR bytes, and
  a 1,046,456-byte WVB with 552 functions and 869,476 code bytes. Its SHA-256
  is `92fa90b0d942cbe5a74861af49f680efe3c69b19466a237893e21ad0dff3cd66`,
  and the independent native verifier accepts it.
- The maintained analyzer is 1,132,570 bytes at SHA-256
  `e3eef9e462f47cb88d4de174eb1e714106b346137538d9e6b396361b834d8471`;
  its Windows profile-7 package is 35,597,824 bytes at SHA-256
  `21d6ace08354a2b4154d8356ca9255fd288d2ae5c7c7d0292b0c90538270705a`.
  The emitter package is 22,945,280 bytes at SHA-256
  `51980614da75ef5e8e33cdd33fef91fa0cf74d7ee02cf1d978e2e14cf05f3701`.
- Bounded probes fail from instruction exhaustion through
  120,259,084,288 instructions and next reach text-arena exhaustion at
  124,554,051,584. The selected `2^37` plus 224 MiB envelope completes the
  exact compiler closure and leaves explicit, finite headroom.
- The candidate inventory remains 72 entries and 6,927 bytes, now at SHA-256
  `40af573f510861b375b1dac5216e5e622b6539656dfec188f6f4079f33040239`.
  The reconstructed publisher applications are 388,608 Windows bytes at
  SHA-256 `91c14c884cf552abc5927815b3095ec50134b58709db2fc2077825d1abc478a4`
  and 385,981 Linux bytes at SHA-256
  `022ad0fcf6dcbd3ed89eb2cba45533aacec275ff699c7bc034a54618cd7dd6b9`.
- The Language 1.0 owner advances from 417 to 427 cases. The 108-owner registry
  advances from 5,201 to 5,211 declared cases at SHA-256
  `39df2841962a0efa20541c5b2b2ecf5e15ec514d709756107f8bd5c8c5ef899b`.
- The 3,778-byte source-amendment manifest has SHA-256
  `1a48d58136e5200cdb6f5ae1e15638f554854a3764f61ce1f0d2222d9d8e0c13`.
  Its 251 inputs total 1,728,883 bytes and have aggregate entry-stream SHA-256
  `a6e6bf3617049a987b545a78e5f3fcef28b24a3fc2b82c45d620e58baed73fc9`.
- The generic-call parser self-test was already larger than the retained scalar
  runner's fixed one-million guest-instruction budget. Its unchanged WVB now
  executes through the existing bounded segmented profile-1 native route rather
  than raising that global runner budget; the three-fragment application returns
  `42`.
- The borrow-call fixture emits WVB 1.20 ownership operations outside the last
  promoted native scalar-runner artifact. It retains independent WVB
  verification and executes through the bounded import-free WebAssembly scalar
  interpreter, returning `42` in 93 guest instructions, rather than treating a
  stale development runner as the Language 1.0 oracle.
- The compiler-aligned generic-WIR closure grows deterministically to 1,295,691
  bytes at SHA-256
  `6afc2f4574158d5b151c7d4c0ec85eca132e26f88187f8d5fda8b2c866be9e6b`.
  Its two independent productions remain byte-identical.

## Reconsideration triggers

Define a new WVB enum representation before claiming executable support for any
backing other than `i32`. Reprofile profile 7 after the 1.0 compiler becomes the
active seed or after a cohesive compiler-phase refactor materially changes the
closure. Reduce or split the envelope only from reproducible instruction and
working-set evidence; do not infer a smaller safe bound from source size alone.
