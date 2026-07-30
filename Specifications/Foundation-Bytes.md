# Windvale Foundation binary and text primitives

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
Bytesˉreadˉi32ˉlittle(Value: bytes, Offset: u32) -> i32
U32ˉfromˉu8(Value: u8) -> u32
Textˉutf8ˉisˉvalid(Value: bytes) -> bool
Textˉfromˉutf8(Value: bytes) -> text
Textˉquote(Value: text) -> text
```

Each binary operation validates its complete range before reading or constructing a slice. An invalid range traps with `WVR3008`. Reads always interpret multi-byte values as little-endian, independent of host architecture, and the signed read interprets its four bytes as two's-complement `i32`. `U32ˉfromˉu8` preserves the numeric value while making the width change explicit. A slice is semantically immutable; the reference runtime implements it as a zero-copy view sharing the original immutable storage.

UTF-8 validation is strict and returns `false` rather than trapping for malformed input. `Textˉfromˉutf8` uses the same strict definition and traps with `WVR3014` when decoding invalid bytes. `Textˉquote` produces an ASCII JSON-style quoted form: quote, reverse solidus, and controls are escaped; printable ASCII is preserved; all non-ASCII UTF-16 code units use uppercase `\uXXXX`. Quoting is intended for safe deterministic reports, not for Unicode normalization. Decoded and quoted values remain subject to the 1 MiB text bound, with oversized quoting rejected as `WVR3012`.

## Portability boundary

These operations are pure and available to portable modules. They do not read files, inspect process memory, or depend on the host operating system. The first example embeds a canonical `.wvb` header as module data. `Wv-Dump-Core.wv` uses the same operations to validate the complete envelope, decode every payload, and report instruction streams; its hosted shell supplies real file bytes to those pure inspection functions through `file.read_bytes`.

## Deliberate limits

This slice does not provide mutable buffers, general arrays of `u8`, streaming, cursors, endian selection, text encoding, normalization, file I/O, allocation APIs, or unsafe memory access. Those should be added only when a concrete self-hosting tool requires them.
