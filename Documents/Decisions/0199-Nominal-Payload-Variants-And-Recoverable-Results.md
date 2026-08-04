# Decision 0199: Nominal payload variants and recoverable results

- Status: Accepted and implemented locally; coherent change-aware verification passes and independent cross-host qualification remains pending
- Date: 2026-08-03

## Context

Decision 0184 accepts exhaustive enum matching before payload-bearing variants and requires a focused decision for variant construction, matching, ownership, defaults, WVB encoding, verification, runtime representation, and malformed input. Windvale needs typed provider outcomes and optional values without implicit null, general exceptions, catchable traps, or hidden control flow.

## Decision

Windvale adds a distinct nominal `variant` declaration. It does not change the existing explicit-`i32` enum contract.

```text
variant Readˉresult {
    Success(Value: bytes);
    Failure(Error: Readˉerror);
}
```

A variant contains between 1 and 256 uniquely named cases. A case contains either no payload or exactly one named payload with an explicit non-`void` type. The first implementation admits scalar, text, bytes, record, and enum payload shapes; recursive or variant-containing payload graphs wait for a separate bounded-layout and ownership decision. Case and payload order is declaration order.

Construction is explicit and call-like:

```text
let Complete = Readˉresult.Success(Bytes);
let Empty = Optionalˉbytes.None();
```

The constructor name is the qualified nominal variant name followed by the case name. A payload case requires exactly one argument of its declared type. A no-payload case requires no arguments. Construction evaluates its payload once and produces one immutable nominal variant value.

Statement-form `match` extends to variants:

```text
match Result {
    case Readˉresult.Success(Value) {
        return Value;
    }
    case Readˉresult.Failure(Error) {
        return Recover(Error);
    }
}
```

Each case names the same nominal variant as the matched value. A payload case binds exactly one immutable arm-local name; a no-payload case binds no name. Bindings exist only inside that arm. Cases cannot fall through, cannot repeat, and must cover every declared case. There is no wildcard or integer-tag pattern. The discriminant evaluates once before dispatch.

Explicit `match` is the first recoverable-result flow. Libraries may define nominal result and optional variants, but the language adds no privileged generic `Result`, implicit conversion, general exception, or punctuation-only propagation operator. A later `try` expression requires a separate decision covering recognized success/failure cases, compatible returns, ownership transfer, cleanup, and diagnostics.

WVB 1.9 carries variants as follows:

- nominal type kind `3` is `variant`;
- value-shape kind `11` is a nominal variant followed by its `u32` Types-section index;
- the 1.9 Module payload appends a canonical metadata-present byte (`0` or `1`) before optional metadata so variant-bearing modules remain representable while repository source headers migrate;
- each variant type stores its ordered cases; each case stores its name, a payload-present byte, and, when present, the payload name and value shape;
- opcode `0x97 variant.create` carries a variant type index and case index, consumes the declared payload when present, and produces the nominal variant;
- opcode `0x98 variant.is_case` carries a variant type index and case index, consumes that exact nominal variant, and produces `bool`;
- opcode `0x99 variant.payload` carries a variant type index and case index, consumes that exact nominal variant, traps if the selected case differs, and produces the declared payload.

The verifier checks type and case indices, constructor arity and payload shape, exact nominal inputs, payload presence, control-flow stack agreement, bounded counts, unique names, initialized arrays, and canonical ordering. A runtime variant value contains the nominal type index, selected case index, and zero or one immutable payload value. The default value selects the first declared case and recursively defaults its payload when present.

During the current no-compatibility development window, 1.9 is the current variant-bearing contract rather than a compatibility promise. Older readers, dual encodings, aliases, and migration shims are not required; temporarily retained 1.6 through 1.8 paths exist only while repository inputs advance.

The Stage 0 and Windvale-written compilers, WVIR validation, canonical WVB writer/reader/verifier/inspector, and reference runtime implement this contract. Focused enum-match, variant-match, no-payload, payload, default-value, malformed-case, and cross-compiler byte-parity fixtures pass locally. The final change-aware gate also passes all 92 affected Seed tests; independent dual-host qualification remains pending.

## Consequences

Windvale can express typed recoverable provider outcomes and optional values while preserving explicit control flow and exact nominal identity. Variants add one new composite runtime value and require every compiler, verifier, interpreter, serializer, inspector, and supported backend to handle the new shape explicitly.

The single-payload rule avoids tuple layout, destructuring order, and partial binding complexity. Excluding recursive and variant-containing payload graphs keeps default construction, ownership evidence, native lowering, and bounded verification finite for the first slice.

## Reconsider when

- measured APIs require multiple payload fields strongly enough to justify record-like case payloads;
- nested result or optional values require acyclic variant payload graphs;
- explicit result matching causes repeated propagation code that a narrowly typed `try` expression can remove without hiding cleanup or authority; or
- immutable variant equality receives bounded derived-equality rules.
