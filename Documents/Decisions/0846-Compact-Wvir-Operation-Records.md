# Decision 0846: Compact WVIR operation records

## Status

Accepted on 2026-08-24 with paired Windows/Linux focused development evidence.
This is an internal compiler-format replacement; canonical WVB and Language
1.0 source semantics do not change.

## Context

WVIR 1.3 through 1.8 stored every operation as eight `u32` values. Operation
kind and operand count used only a small bounded range, yet together occupied
eight bytes. The evolving source compiler was approaching the existing 4 MiB
WVIR evidence ceiling. Raising the ceiling would defer the same structural
waste and increase retained compiler memory, cache bytes, hashing work, and
verification input without improving the language.

WVIR is an internal validated phase boundary rather than a compatibility or
distribution format. The repository direction explicitly rejects obsolete
experimental formats unless a named compatibility case needs them. That makes
an atomic replacement preferable to a second decoder or a per-record variant.

## Decision

1. Every WVIR operation record is exactly 28 bytes. It contains owning block
   `u32` at offset `0`, kind `u16` at offset `4`, operand count `u16` at offset
   `6`, shape `u32` at offset `8`, result temporary `u32` at offset `12`, first
   operand `u32` at offset `16`, target `u32` at offset `20`, and auxiliary
   `u32` at offset `24`.
2. Construction checks that kind and operand count fit their independently
   specified bounds before narrowing. Independent validation rereads the exact
   `u16` fields and rejects any unsupported kind, arity, offset, or total
   length. There is no overlapping read that can reinterpret adjacent fields.
3. Ordinary WVIR advances from 1.3 to 1.9 and specialized WVIR from 1.4 to
   1.10. Memory operations 171/172 use 1.11 or specialized 1.12. Append
   operation 173 uses 1.13 or specialized 1.14. The feature-to-version mapping
   and even-version specialization envelope otherwise remain unchanged.
4. WVIR 1.1 through 1.8 are rejected. No compatibility decoder, record-size
   switch, conversion tool, or persisted cache promotion is retained.
5. The aggregate WVIR ceiling remains exactly 4 MiB. All active compiler,
   emitter, inspector, corruption-test, and verification consumers migrate as
   one source checkpoint. Historical decisions keep their historical sizes and
   identities.
6. WVB bytes and minimum bytecode versions remain selected by executable
   semantics, not by this compiler-only representation change.
7. The incompatible self-hosting transition retains one digest-pinned portable
   bridge emitter. It was built from baseline `269294c0` with only the compact
   WVIR reader/validator side applied, consumes WVIR 1.9, and builds the current
   emitter. It is development bootstrap provenance, not a legacy decoder or a
   release compiler, and is removed after a promoted current-format compiler
   checkpoint closes the cycle.

## Consequences

- The current compiler source graph publishes 3,853,556 WVIR bytes, leaving
  340,748 bytes under the unchanged ceiling. The saved four bytes per operation
  also reduce subsequent hashing and retained analysis traffic.
- The current generic nominal inspectors prove representative ordinary and
  specialized products: the main pipeline is 604-byte WVIR 1.9, function-body
  specialization is 980-byte WVIR 1.10, declaration dependency is 1,100-byte
  WVIR 1.10, and the generic variant is 1,708-byte WVIR 1.9. Their emitted WVB
  products remain 441, 600, 668, and 947 bytes respectively.
- Existing malformed-input checks now corrupt the narrow kind/count fields and
  the new entry size. An old minor, a 32-byte entry declaration, an unknown
  `u16` kind, truncation, and trailing bytes fail closed.
- This recovers compiler headroom without claiming a runtime optimization or
  making the 4 MiB boundary semantically unbounded.
- The bridge build itself measured 4,193,520 bytes of old-layout WVIR, only 784
  bytes below the existing ceiling. Its 1,146,083-byte WVB 1.11 product has
  SHA-256 `0d838b6d983320cf22b9094ef5a4692d6833f1834292863789577e034f6febdb`
  and is independently verified before packaging on either host.

## Reconsideration triggers

Replace this layout only when measured compiler-scale evidence again reaches a
stable bound or a later IR contract needs information that cannot be derived
from its canonical ranges. Do not widen every record, raise the global ceiling,
or retain parallel decoders without first measuring the specific field and
consumer cost.
