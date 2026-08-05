# Foundation ordinal byte-span ordering

## Status and scope

`Foundationˉbyteˉordering` is a portable, capability-free source module that owns unsigned lexicographic ordering for two validated spans of one immutable byte value. It is cross-host qualified at `4fdea22` under Decision 0021.

The contract is byte-oriented, not text-oriented. UTF-8 decoding, Unicode normalization, locale collation, case folding, machine-name grammar, and path semantics are outside this module.

## Public function

```text
Foundationˉbyteˉspansˉcompare(
    Input: bytes,
    Leftˉoffset: u32,
    Leftˉlength: u32,
    Rightˉoffset: u32,
    Rightˉlength: u32
) -> i32
```

Both `(offset, length)` pairs must describe ranges inside `Input`. The caller owns that validation. For valid spans, the result is exactly:

- `-1` when the left span is ordinally earlier;
- `0` when both spans contain identical bytes and lengths; or
- `1` when the left span is ordinally later.

Bytes are widened to unsigned `u32` values before comparison, so byte `127` sorts before byte `128`. The first unequal byte decides the result. If the common prefix is equal, the shorter span sorts first. Empty spans compare equal to each other and before non-empty spans.

The function reads at most the smaller declared length, allocates no byte value, invokes no capability, and observes no host state. Its work is linear in the common prefix and remains subject to the caller's runtime instruction budget.

## Ownership and consumers

The three current consumers are:

- `Object-Model/Windvale/Wvo-Object-Core.wv`, for canonical section and symbol order;
- `Assembler/Windvale/Wva-Assembler-Core.wv`, for declaration equality, lookup, and canonical order; and
- `Linker/Windvale/Wv-Linker-Core.wv`, for same-object name equality and canonical order.

Each consumer validates its spans at its parser boundary before comparison. The linker continues to own cross-object ordering and complete-image equality because those contracts accept two independent byte values and are not duplicated by the other tools.

## Verification

`Examples/Foundation/Byte-Ordering-Demo.wv` is the fixed portable boundary artifact. The complete object-core, assembler, and linker suites are the consumer conformance tests. Qualification must retain the 189-byte representative WVO, 218-byte canonical assembled WVO, 24-byte linked image, 1,721-byte canonical map, no-write failures, and the existing maximum image/map instruction ceilings.

At qualified commit `4fdea22`, the standalone module is 989 bytes with SHA-256 `194e4b5c4eb7f4641a39098abce3dabb93187af7149e184b56b76f978ed2f4f1`. The 2,445-byte demo has SHA-256 `0b41e8f615630e0734812ba8cd8e7c06e975592b86327c2fe8220f5e29c10cab` and returns `0`. Windows and Debian produced identical bytes for both artifacts and all three composed consumers.
