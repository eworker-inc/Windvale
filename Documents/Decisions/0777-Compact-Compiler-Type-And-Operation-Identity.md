# Decision 0777: Compact compiler type and operation identity

- Status: Accepted
- Date: 2026-08-19

## Context

Decision 0776 left the source compiler only 71,690 bytes below the fixed 32 MiB
large-native object ceiling. Completing typed Foundation `Option<T>` and
`Result<T, E>` requires canonical specialization identity, substituted field
shapes, and deterministic WVB type emission. Adding that machinery directly to
the nearly full compiler would either exceed the existing bound or encourage a
limit increase without addressing duplicated compiler work.

The compiler also repeated the primitive-to-shape table in binding, symbol,
WIR, and WVB phases, repeated the token-to-source-type table in the declaration
parser and symbol binder, converted every private WIR operation enum value
through a 164-branch identity function, and encoded the WVB opcode table as a
long list of individual comparisons.

## Decision

1. The declaration parser owns one token-to-source-type conversion. Symbol
   binding consumes that exact conversion instead of maintaining a second
   table.
2. The symbol phase owns the canonical primitive and nominal binding-to-shape
   conversions and the declared-value shape predicate. Bindings, WIR, and WVB
   delegate to those helpers.
3. Private WIR operation identities are fixed `u32` constants. Emission stores
   that identity directly; the redundant enum-to-number conversion function is
   removed. The serialized operation values `0` through `164` do not change.
4. WVB opcode selection uses exact contiguous ranges and offsets where the
   existing table is arithmetic. Exceptional entries remain explicit. Opcode
   selection returns the already encoded one-byte value so callers do not wrap
   it repeatedly.
5. WVB operation-length selection groups entries with the same exact encoded
   size. Operation 64, shape-dependent floating constants, operands, and the
   optional shape field retain their prior rules.
6. No source syntax, source type, WIR identity, WVB opcode, format version,
   native output limit, diagnostic contract, or runtime behavior changes.

## Evidence

An independent comparison covers all 165 WIR operation identities and reports
the same WVB opcode for every value. A second comparison covers all 165
operation values with no shape, an ordinary shape, and the shape-15 floating
constant case and reports the same encoded operation length.

The complete compiler source set rebuilds successfully with 505 functions,
939,424 code bytes, and 1,132,084 module bytes. Native planning reports
33,187,051 machine-code bytes and 2,472 relocation bytes. Relative to Decision
0776, this removes 32,851 compiler-module bytes and 265,572 native machine-code
bytes. Applying the retained 30,119-byte object envelope gives an estimated
33,217,170-byte object and 337,262 bytes of headroom below 32 MiB. No admitted
limit was widened.

## Non-decision

This checkpoint does not implement generic declarations, specialization,
Foundation package publication, distinct-success-shape `try`, error adapters,
or the manual status migration required to complete Slice 3. It does not claim
the final Language 1.0 verification gate or paired-host qualification.

## Consequences

Type and operation identities now have one compiler owner, reducing the chance
that later Language 1.0 work updates one phase but leaves another stale. The
recovered space is useful safety margin, but the measurement proves that local
table compaction alone cannot fund bounded generics inside the current
near-limit monolithic native compiler image.

The next compiler-capacity boundary should therefore separate independently
useful source/type analysis evidence from WVB emission, with a versioned,
validated, bounded phase artifact. That split must reduce both native package
size and repeated verification work; it must not create a parallel semantic
compiler or make the intermediate artifact a distribution contract.

## Reconsideration triggers

Reconsider the arithmetic opcode ranges only if a versioned WIR or WVB change
breaks a contiguous mapping. Reconsider the phase split if measured native
code-generation improvements provide durable Language 1.0 implementation
headroom while preserving one coherent compiler and narrow verification
ownership.
