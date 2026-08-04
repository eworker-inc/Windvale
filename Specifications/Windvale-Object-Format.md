# Windvale object format

## Status and purpose

Windvale Object version 1.0 (`WVO1`) is the first structured native-object contract shared by the Stage 0 oracle, Windvale-written assembler, and linker. It is deliberately smaller than ELF, COFF, or Mach-O and does not replace those host formats. Its job is to preserve named sections, symbols, and unresolved relocations deterministically until a linker chooses final layout and an output image format.

This is an early-development format without a backward-compatibility promise. Readers must reject unsupported versions rather than guessing.

## Encoding

- All integers are little-endian.
- Counts, indices, sizes, alignments, and offsets use `u32`; relocation addends use `i32`.
- Names are length-prefixed strict UTF-8 but must also satisfy the ASCII machine-name grammar below.
- Every reserved or flags field must be zero.
- A complete object is limited by the caller-selected admission profile. Standard admission is 4 MiB; explicit large-native admission is 32 MiB.
- Readers use checked range arithmetic and reject trailing bytes.

The admission profile is not encoded in WVO. A file therefore cannot elevate its own limits: every reader, writer, verifier, and downstream linker must receive large-native authority explicitly. Both profiles use the same canonical WVO 1.0 encoding, so choosing large-native admission does not create a second object format or change bytes that already fit standard admission.

## Header

```text
4 bytes  magic: 57 56 4F 31 (ASCII WVO1)
u16      major version: 1
u16      minor version: 0
u8       architecture: 1 x86-64
u8       flags: 0
u16      reserved: 0
u32      section count
u32      symbol count
u32      relocation count
```

The three record groups immediately follow the 24-byte header in this order: all sections including their inline contents, all symbols, then all relocations.

## Sections

```text
repeat section count:
  u8     kind: 1 code, 2 read-only data, 3 writable data, 4 zero-fill
  u8     flags: 0
  u16    reserved: 0
  u32    alignment
  u32    memory size
  u32    data length
  string name
  bytes  data
```

Sections are strictly ordered by kind, then ordinal name, and names are unique. Alignment is a power of two from 1 through 4,096. Code, read-only-data, and writable-data sections require `memory size == data length`. A zero-fill section has no encoded data and a nonzero memory size. Under standard admission, total encoded section data is limited to 4 MiB and materialized plus zero-fill memory is limited to 16 MiB. Under large-native admission, both limits are 32 MiB. Version 1.0 derives access policy from the kind and has no arbitrary section flags.

## Symbols

```text
repeat symbol count:
  u8     binding: 1 local, 2 export, 3 import
  u8     kind: 1 function, 2 data
  u16    reserved: 0
  u32    section index
  u32    offset within section memory
  u32    size
  string name
```

Symbols are strictly ordered by binding, then ordinal name, and names are globally unique. A local or exported symbol must identify an existing section and a complete range inside that section. Function symbols belong to code; data symbols belong to a non-code section. An imported symbol uses section index `0xFFFFFFFF`, offset zero, and size zero. Imports are unresolved definitions, not ambient dynamic-library lookups; a later link contract decides how they are satisfied.

## Relocations

```text
repeat relocation count:
  u8     kind: 1 absolute-u32, 2 relative-i32
  u8     flags: 0
  u16    reserved: 0
  u32    section index
  u32    offset within encoded section data
  u32    symbol index
  i32    addend
```

Every relocation identifies four zero placeholder bytes completely inside a materialized section. Relocations are strictly ordered by section index and offset, and their four-byte patch ranges cannot overlap. The symbol index addresses the canonical symbol table.

For final symbol address `S` and relocation-field address `P`:

```text
absolute-u32 = S + addend
relative-i32 = S + addend - P
```

The linker must reject a result outside the destination width; relocation arithmetic never wraps. For an x86-64 `call rel32`, an assembler normally uses `relative-i32` with addend `-4`, because the processor adds the encoded displacement to the address after the four-byte field.

## Machine names

Section and symbol names are 1 through 255 ASCII bytes. The first byte is an ASCII letter, `_`, `.`, or `$`; following bytes may additionally be decimal digits. This namespace is deliberately separate from U+02C9 Windvale source identifiers. Assemblers and compilers must map source declarations to these external names explicitly.

## Limits

- Standard object bytes: 4 MiB
- Standard total object memory: 16 MiB
- Large-native object bytes: 32 MiB
- Large-native total object memory: 32 MiB
- Sections: 64
- Symbols: 4,096
- Relocations: 65,536
- Name: 255 ASCII bytes
- Alignment: 4,096

Large-native admission exists for measured Stage 0 native artifacts such as the exact ABI-22 compiler. It does not raise Windvale's ordinary 4 MiB `bytes` value, does not widen the qualified portable assembler/linker profile, and does not imply that one portable byte value can carry a large object. A later Windvale-owned large-object path must use bounded segmented or sparse transport while reproducing these same WVO bytes.

## Deliberate omissions

WVO 1.0 does not define final virtual addresses, an entry point, COMDAT selection, weak symbols, thread-local storage, dynamic linking, debug information, exceptions, unwind tables, arbitrary section flags, more architectures, or PE/ELF emission. Add those only when the assembler, linker, native backend, or boot path presents a concrete requirement.
