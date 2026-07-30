# Foundation bounded byte construction

## Status and purpose

`Foundationˉbyteˉconstruction` provides total, portable construction operations over Windvale's immutable byte values. It owns efficient repeated-byte creation and checked range replacement for the assembler, linker, and future self-hosted bytecode encoder. Cross-host qualification of the current implementation is pending.

## Result contract

```text
record Foundationˉbytesˉresult {
    Valid: bool;
    Value: bytes;
}
```

Success returns `Valid = true` and the exact constructed value. Rejection returns `Valid = false` and an empty byte value. Callers never need to catch a range or size trap for these APIs.

## Repetition

```text
Foundationˉbytesˉrepeat(Value: u8, Count: u32) -> Foundationˉbytesˉresult
```

Counts from zero through 4,194,304 inclusive are valid. The result contains exactly `Count` copies of `Value`; zero returns an empty value. A larger count is rejected.

Construction doubles the current immutable value while a full doubling fits, then appends the exact remaining prefix. The number of concatenations grows logarithmically with `Count`. Structural sharing and balancing remain runtime-owned implementation details; callers observe only immutable bytes.

## Range replacement

```text
Foundationˉbytesˉreplace(
    Input: bytes,
    Offset: u32,
    Removedˉlength: u32,
    Replacement: bytes
) -> Foundationˉbytesˉresult
```

The removed span must be wholly inside `Input`; a zero-length span at the end is valid. The final length is `input length - removed length + replacement length` and must not exceed 4,194,304. Validation uses `Offset <= total`, `Removedˉlength <= total - Offset`, and a pre-addition final-size check.

Success is the immutable concatenation of the prefix before `Offset`, `Replacement`, and the suffix after the removed span. No input value is modified. Invalid offset, range, or final size returns the rejected result before any unsafe intrinsic call.

## Consumer boundary

`Wvaˉassemblerˉcore` uses repetition to construct the 4,097-byte long-line rejection fixture. `Wvˉlinkerˉcore` uses repetition for alignment/BSS materialization and replacement for four-byte relocation fields and one-byte verifier mutations. Domain-specific measurement, status selection, independent reconstruction, and publication remain with those tools.

The future Windvale WVB encoder can use replacement for measured branch and table backpatching without acquiring mutable storage. This contract does not itself define bytecode layout or compiler policy.

## Qualification

`Examples/Foundation/Byte-Construction-Demo.wv` is the fixed portable boundary artifact and includes the exact 4 MiB value. The complete assembler and linker suites are the consumer tests. Qualification must retain the 218-byte canonical WVO, 24-byte linked image, 1,721-byte canonical map, all no-write failures, and the fixed 200,000,000-instruction maximum link cases.

The candidate standalone WVB is 2,000 bytes with SHA-256 `6f26865069333c02b15ab83d48f2a0cb0e3a05db98bcd841f31e232485b76207`. The candidate composed demo is 5,068 bytes with SHA-256 `a9b577dc08ac6e4a0d786f04d6667eb0347c57a0c1abbd81f3481fb0e0bc6c29` and returns `0`.
