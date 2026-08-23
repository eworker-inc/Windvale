# Decision 0835: Represent `u8`-backed enums as WVB 1.22

## Status

Accepted on 2026-08-23. Candidate toolset qualification remains pending.

## Context

Decision 0833 gave every edition-1 fixed-width enum an exact source and WIR
identity while initially limiting executable WVB publication to the existing
signed-`i32` representation. Decision 0834 then allowed a completely unused
nominal declaration family to disappear from optimized output. That removed the
launcher-budget blocker but did not let real Foundation `u8` enums execute.

Changing the existing kind-2 descriptor would reinterpret every historical
enum byte stream. Treating `u8` tags as `i32` would erase an explicit source
representation promise. A new value shape is unnecessary because enum
operations are nominal and already resolve their exact Types entry.

The source-built runner also crossed its per-function bytecode limit and then
the native lowerer's 2,048-physical-cell function limit while adding the new
descriptor path. Those are measured migration blockers. Broad optimization of
the transitional compiler remains deferred under Decision 0834 until the
Language 1.0 compiler becomes the active seed.

## Decision

1. WVB 1.22 appends nominal descriptor kind `7` for an edition-1 `u8`-backed
   enum. The descriptor contains its name, source backing identity byte `6`,
   member count, and each declared member name followed by its exact one-byte
   value.
2. Existing kind `2` remains the exact signed-`i32` descriptor and retains all
   historical bytes. Kinds `2` and `7` share one enum ordering category, are
   sorted together by ordinal name, and both use the existing enum value shape
   `8` and enum instruction family.
3. Kind `7` is valid only in WVB 1.22. Every WVB 1.22 module contains at least
   one kind-7 descriptor, preventing an earlier vocabulary from being published
   under an unnecessarily high minor version.
4. Shape `25` remains valid in WVB 1.21 and may also appear in WVB 1.22 when a
   module combines the launcher-owned budget entry with a `u8` enum. WVB 1.21
   still requires the budget entry. WVB 1.22 otherwise uses ordinary
   `Main() -> i32`; if shape `25` appears, its exact single-parameter export,
   transfer, non-copyability, and teardown rules remain unchanged.
5. Verification rejects the wrong backing identity, duplicate values, truncated
   backing or member values, kind `7` under another version, WVB 1.22 without
   kind `7`, an unknown type kind, and every invalid nominal enum reference.
6. The scalar runner normalizes either descriptor's declared tag into its
   existing internal enum cell only after exact validation. Enum constants,
   equality, inequality, and name lookup therefore remain one semantic path.
7. The runner is split along function-directory, request, data/local/bytes,
   collection, aggregate, and extended-operation boundaries. This keeps the
   implementation within existing WVB and native-lowering limits without
   raising a bound or creating a second interpreter.
8. Retained `i8`, `i16`, `i64`, `u16`, `u32`, and `u64` enum backings remain
   explicit later representation work. No value is narrowed to `i32` or `u8`.

## Consequences

- `Enum-U8-Used-Main.wv` deterministically emits a 415-byte WVB 1.22 at
  SHA-256
  `961ba417955a523b9fc21e0b71df7a8d99613252b7450700dd4381aa94e825ed`.
  Its exact kind-7 descriptor encodes `Deliveryˉstate`, backing identity `6`,
  `Pending = 1`, and `Complete = 2`. The compiler-aligned verifier accepts it
  and the source-built runner returns `42`.
- A bounded corruption suite rejects nine malformed or non-canonical variants:
  old and future minor versions, wrong backing, duplicate value, truncated
  backing, truncated value, missing kind-7 feature, unknown type kind, and an
  out-of-range enum shape index.
- The current emitter is 1,055,285 bytes at SHA-256
  `bd87930696685475920bdc73dcf72dde01ae0eb5dae94579e28b9a79d018d606`.
  It retains 554 functions and 877,444 code bytes.
- The current compiler-aligned verifier is 248,741 bytes at SHA-256
  `f401d89796c48b4d6890a465d6f47c1a21c864cb48383ce54c8ec9bc1a0c3e08`.
- The refactored source-built runner is 257,017 bytes at SHA-256
  `269130ea87bba7504af0d7d8337a7d1b8748d61671611ffb816d7ca5f7fa2e02`,
  with 98 functions and 232,834 code bytes. Native lowering produces 2,836,720
  code bytes and a 2,842,043-byte object without changing the 2,048-cell bound.
- The focused Windows Language 1.0 owner passes all 449 cases in 745,820 ms;
  its coordinator completes in 746,630 ms. The 108-owner registry declares
  5,233 cases at SHA-256
  `e40651f750eddb420500561ad0969cec233261f2666c47f383e958e28744a5b8`.

## Reconsideration triggers

Add another exact descriptor kind or a separately versioned general backing
field when a retained `i8`, `i16`, `i64`, `u16`, `u32`, or `u64` enum reaches
the executable migration path. Revisit the runner's helper boundaries only
from measured compile, lower, or execution evidence. Reprofile broad compiler
optimization after Language 1.0 becomes the active seed, or earlier only when a
reproducible blocker prevents migration progress.
