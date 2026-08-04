# Windvale Foundation binary and text primitives

## Purpose

This contract is the first Foundation slice needed for Windvale-written binary tools. It lets portable source inspect bounded binary data without introducing ambient files, unsafe pointers, mutable buffers, or host-dependent integer behavior.

## Values

- `u8` represents one unsigned byte. A source literal uses the `u8` suffix and must be in the range 0 through 255.
- `u32` represents an unsigned 32-bit value. A source literal uses the `u32` suffix and must be in the range 0 through 4,294,967,295.
- `u64` represents an unsigned 64-bit value. A source literal uses the `u64` suffix and must be in the range 0 through 18,446,744,073,709,551,615.
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
Bytesˉreadˉi32ˉlittle(Value: bytes, Offset: u32) -> i32
Bytesˉreadˉu64ˉlittle(Value: bytes, Offset: u32) -> u64
Bytesˉconcat(Left: bytes, Right: bytes) -> bytes
Bytesˉfromˉu8(Value: u8) -> bytes
Bytesˉfromˉu16ˉlittle(Value: u32) -> bytes
Bytesˉfromˉu32ˉlittle(Value: u32) -> bytes
Bytesˉfromˉi32ˉlittle(Value: i32) -> bytes
Bytesˉfromˉu64ˉlittle(Value: u64) -> bytes
Bytesˉsha256ˉhex(Value: bytes) -> text
U32ˉfromˉu8(Value: u8) -> u32
Textˉutf8ˉisˉvalid(Value: bytes) -> bool
Textˉfromˉutf8(Value: bytes) -> text
Textˉtoˉutf8(Value: text) -> bytes
Textˉquote(Value: text) -> text
```

Each binary read validates its complete range before reading or constructing a slice. An invalid range traps with `WVR3008`. Reads always interpret multi-byte values as little-endian, independent of host architecture, and the signed read interprets its four bytes as two's-complement `i32`. The fixed-width construction operations are the exact inverse representations: one byte, two little-endian bytes, four little-endian bytes, or eight little-endian bytes. The `u16` encoder accepts a `u32` so the narrowing remains explicit and traps with `WVR3016` above 65,535. `Bytesˉconcat` returns a new immutable sequence and traps with `WVR3015` before construction when the combined result would exceed 4 MiB. `Bytesˉsha256ˉhex` hashes exactly the supplied sequence or slice and returns the standard SHA-256 digest as 64 lowercase ASCII hexadecimal characters. `U32ˉfromˉu8` preserves the numeric value while making the width change explicit. A slice is semantically immutable; the reference runtime implements slices and concatenations as structurally shared, height-balanced persistent byte sequences. Contiguous native bytes are materialized only at strict UTF-8, hashing, or hosted file-output boundaries.

UTF-8 validation is strict and returns `false` rather than trapping for malformed input. `Textˉfromˉutf8` uses the same strict definition and traps with `WVR3014` when decoding invalid bytes. `Textˉtoˉutf8` performs the inverse strict encoding and uses the same trap for an invalid Unicode value. `Textˉquote` produces an ASCII JSON-style quoted form: quote, reverse solidus, and controls are escaped; printable ASCII is preserved; all non-ASCII UTF-16 code units use uppercase `\uXXXX`. Quoting is intended for safe deterministic reports, not for Unicode normalization. Decoded and quoted values remain subject to the 1 MiB text bound, with oversized quoting rejected as `WVR3012`.

## Portability boundary

These operations are pure and available to portable modules. They do not read files, inspect process memory, or depend on the host operating system. The `u64` codecs currently require the WVB 1.12 Stage 0 compiler and reference runtime; native, WebAssembly, Windvale OS, and Windvale-written compiler adoption remain separate profiles. The first example embeds a canonical `.wvb` header as module data. `Wv-Dump-Core.wv` uses the read operations to inspect complete modules. `Wvo-Object-Core.wv` uses the construction operations to encode the canonical WVO 1.0 sample; its hosted shell persists the already-validated bytes through the separate `file.write_bytes` capability.

## Deliberate limits

This slice does not provide mutable buffers, general arrays of `u8`, streaming, cursors, endian selection, normalization, file I/O, allocation APIs, unsafe memory access, a general cryptography API, or configurable hash algorithms. SHA-256 identity is included because the accepted linker-map contract requires it; it can move into Windvale Foundation code after the language has the required bitwise and bounded-collection facilities. The source-level [bounded byte-construction contract](Foundation-Byte-Construction.md) now owns measured repeat and range-replacement needs while retaining immutable values; it is not a mutable or general-purpose builder.
