# Windvale textual assembly

## Status and purpose

Windvale Assembly version 1 (`WVA 1`) is the first textual source contract for constructing canonical x86-64 WVO 1.0 objects. It is intentionally a small semantic assembly language rather than a spelling of Intel or AT&T syntax. The assembler owns parsing, instruction encoding, definition offsets, symbol sizes, and relocation records. It does not choose final addresses, resolve imports, apply relocations, or produce PE, ELF, UEFI, or flat images; those are linker responsibilities.

WVA 1 is an early-development contract without a backward-compatibility promise. The dependency-free C# assembler is the Stage 0 oracle. The same contract is intended for a verified Windvale-written assembler before Phase 6 is complete.

## Source encoding and lines

- Source is strict UTF-8 without a byte-order mark and is limited to 1 MiB.
- A physical line is limited to 4 KiB of UTF-8.
- LF, CRLF, and CR line endings have the same meaning.
- ASCII space and tab separate tokens. Each statement occupies one logical line.
- `#` begins a comment through the end of its line.
- Blank and comment-only lines are ignored.
- Keywords are lowercase ASCII and case-sensitive.
- Section, symbol, definition, and reference names use the WVO ASCII machine-name grammar.
- Decimal integers have no separators. Signed values use an optional leading `-`; unsigned values do not use a sign.

The first meaningful line is exactly:

```text
windvale-assembly 1
```

## Symbol declarations

All symbol declarations precede the first section:

```text
symbol local  <function|data> <Name> in <Sectionˉname>
symbol export <function|data> <Name> in <Sectionˉname>
symbol import <function|data> <Name>
```

Declarations are already in canonical WVO order: binding order `local`, `export`, `import`, then strict ordinal name order within one binding. Names are globally unique. Local and exported symbols must have exactly one later definition in their declared section; imports must not have definitions. Functions belong to code sections and data belongs to non-code sections.

Requiring canonical declaration order keeps the first Windvale implementation bounded and streaming-friendly. A future source convenience layer may sort declarations without changing WVO ordering.

## Sections and definitions

Sections follow the symbol declarations and are already in WVO kind/name order:

```text
section <code|rodata|data|bss> <Name> align <powerˉofˉtwo>
define <Symbolˉname>
    <body statements>
end define
end section
```

Alignment is 1 through 4,096. Definition order determines layout within its section. The assembler derives each definition's offset and encoded size. Code sections contain function definitions; read-only, writable, and zero-fill sections contain data definitions. Every definition must correspond to one declared non-import symbol in that exact section.

## x86-64 code statements

WVA 1 code definitions support:

```text
nop
return
trap
call <Functionˉsymbol>
jump <Functionˉsymbol>
move_i32 <eax|ecx|edx|ebx|esp|ebp|esi|edi> <i32>
move_u32 <eax|ecx|edx|ebx|esp|ebp|esi|edi> <u32>
```

Their encodings are:

| Statement | Bytes | Relocation |
| --- | --- | --- |
| `nop` | `90` | none |
| `return` | `C3` | none |
| `trap` | `CC` | none |
| `call Name` | `E8 00 00 00 00` | `relative-i32`, field offset after opcode, addend `-4` |
| `jump Name` | `E9 00 00 00 00` | `relative-i32`, field offset after opcode, addend `-4` |
| `move_i32 Reg Value` | `B8+rd imm32` | none |
| `move_u32 Reg Value` | `B8+rd imm32` | none |

The move instructions write a 32-bit register and carry the exact little-endian bit pattern of the declared value. WVA does not define an ABI, stack discipline, calling convention, or function prologue. Those require a separate contract before generated calls are considered executable across a boundary.

## Data statements

Materialized `rodata` and `data` definitions support:

```text
bytes <u8> [<u8> ...]
u32 <u32>
i32 <i32>
address_u32 <Symbol>
```

Numeric fields are little-endian. `address_u32` emits four zero bytes and one `absolute-u32` relocation with addend zero. It is a format exercise and a constrained low-address representation, not the eventual general x86-64 pointer model.

Zero-fill `bss` definitions support only:

```text
zero <positiveˉu32ˉbyteˉcount>
```

The section stores no encoded bytes. Definition offsets and sizes advance through its declared memory extent.

## Validation and determinism

Assembly fails before output when source structure, names, ordering, integer widths, section ownership, definitions, references, instruction contexts, limits, or WVO invariants are invalid. `call` and `jump` require a declared function target; `address_u32` accepts any declared symbol. The resulting object passes the independent WVO verifier before bytes are returned.

Identical WVA text and assembler version produce identical WVO bytes on every host. Source paths, timestamps, comments, whitespace, and line-ending choice are not serialized.

## Limits

- Source: 1 MiB
- Physical line: 4 KiB UTF-8
- Sections, symbols, relocations, names, alignments, object bytes, and object memory: WVO 1.0 limits
- Bytes on one `bytes` statement: 4,096
- Encoded data and code remain subject to the 4 MiB object-data limit

## Deliberate omissions

WVA 1 has no labels inside definitions, conditional branches, memory operands, 64-bit immediates, SIMD, floating point, privileged operations, macros, includes, expressions, constants, debug records, ABI aliases, automatic section creation, or final-image directives. Add an operation only when the linker, native backend, or boot path provides a concrete use and exact verification rule.
