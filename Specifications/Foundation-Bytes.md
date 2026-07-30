# Windvale Foundation byte primitives

## Purpose

This contract is the first Foundation slice needed for Windvale-written binary tools. It lets portable source inspect bounded binary data without introducing ambient files, unsafe pointers, mutable buffers, or host-dependent integer behavior.

## Values

- `u8` represents one unsigned byte. A source literal uses the `u8` suffix and must be in the range 0 through 255.
- `u32` represents an unsigned 32-bit value. A source literal uses the `u32` suffix and must be in the range 0 through 4,294,967,295.
- `bytes` represents an immutable byte sequence. Module byte data is embedded canonically; slices preserve immutable view semantics.

`u32` addition, subtraction, and multiplication are checked. Overflow or underflow traps with `WVR3007`; arithmetic never wraps. `u32` supports equality and ordering. `u8` initially supports equality only. There are no implicit numeric conversions.

## Operations

The following names are compiler-recognized Foundation intrinsics and are reserved from source redefinition:

```text
Bytesˉlength(Value: bytes) -> u32
Bytesˉslice(Value: bytes, Offset: u32, Length: u32) -> bytes
Bytesˉreadˉu8(Value: bytes, Offset: u32) -> u8
Bytesˉreadˉu16ˉlittle(Value: bytes, Offset: u32) -> u32
Bytesˉreadˉu32ˉlittle(Value: bytes, Offset: u32) -> u32
```

Each operation validates its complete range before reading or constructing a slice. An invalid range traps with `WVR3008`. Reads always interpret multi-byte values as little-endian, independent of host architecture. A slice is semantically immutable; the reference runtime implements it as a zero-copy view sharing the original immutable storage.

## Portability boundary

These operations are pure and available to portable modules. They do not read files, inspect process memory, or depend on the host operating system. The first example embeds a canonical `.wvb` header as module data. A later hosted capability can provide file bytes to the same portable inspection logic without changing the byte operations.

## Deliberate limits

This slice does not provide mutable buffers, general arrays of `u8`, streaming, cursors, endian selection, signed binary reads, text decoding, file I/O, allocation APIs, or unsafe memory access. Those should be added only when a concrete self-hosting tool requires them.
