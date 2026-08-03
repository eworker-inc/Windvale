# Windvale textual assembly

## Status and purpose

Windvale Assembly version 1 (`WVA 1`) is the first textual source contract for constructing canonical x86-64 WVO 1.0 objects. It is intentionally a small semantic assembly language rather than a spelling of Intel or AT&T syntax. The assembler owns parsing, instruction encoding, definition offsets, symbol sizes, and relocation records. It does not choose final addresses, resolve imports, apply relocations, or produce PE, ELF, UEFI, or flat images; those are linker responsibilities.

WVA 1 is an early-development contract without a backward-compatibility promise. The dependency-free C# assembler remains the Stage 0 oracle and recovery path. The qualified Windvale-written assembler implements the same contract in verified bytecode.

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

Register operands use these exact names:

```text
Reg32 = eax ecx edx ebx esp ebp esi edi r8d r9d r10d r11d r12d r13d r14d r15d
Reg64 = rax rcx rdx rbx rsp rbp rsi rdi r8 r9 r10 r11 r12 r13 r14 r15
```

WVA 1 code definitions support the original symbol-oriented and machine operations:

```text
nop
return
trap
call <Functionˉsymbol>
jump <Functionˉsymbol>
move_i32 <Reg32> <i32>
move_u32 <Reg32> <u32>
push_i32 <i32>
enable_page_protection
activate_page_table
syscall
disable_interrupts
halt
out_u16
```

They also support definition-local control flow, register operations, and RIP-relative symbol access:

```text
label <Localˉname>
jump_label <Localˉname>
branch <Condition> <Localˉname>

move <Reg32|Reg64> <sameˉwidthˉregister>
add <Reg32|Reg64> <sameˉwidthˉregister>
subtract <Reg32|Reg64> <sameˉwidthˉregister>
and <Reg32|Reg64> <sameˉwidthˉregister>
or <Reg32|Reg64> <sameˉwidthˉregister>
xor <Reg32|Reg64> <sameˉwidthˉregister>
compare <Reg32|Reg64> <sameˉwidthˉregister>
test <Reg32|Reg64> <sameˉwidthˉregister>

push <Reg64>
pop <Reg64>
call_register <Reg64>
jump_register <Reg64>

load_u32 <Reg32> <Dataˉsymbol>
load_u64 <Reg64> <Dataˉsymbol>
store_u32 <Dataˉsymbol> <Reg32>
store_u64 <Dataˉsymbol> <Reg64>
load_address <Reg64> <Symbol>

multiply <Reg32|Reg64> <sameˉwidthˉregister>
add_i32 <Reg32|Reg64> <i32>
subtract_i32 <Reg32|Reg64> <i32>
and_i32 <Reg32|Reg64> <i32>
or_i32 <Reg32|Reg64> <i32>
xor_i32 <Reg32|Reg64> <i32>
compare_i32 <Reg32|Reg64> <i32>
test_i32 <Reg32|Reg64> <i32>

rotate_left <Reg32|Reg64> <count>
rotate_right <Reg32|Reg64> <count>
shift_left <Reg32|Reg64> <count>
shift_right <Reg32|Reg64> <count>
shift_right_signed <Reg32|Reg64> <count>

load_memory_u32 <Reg32> <Base64> <Index64|none> <scale> <i32ˉdisplacement>
load_memory_u64 <Reg64> <Base64> <Index64|none> <scale> <i32ˉdisplacement>
store_memory_u32 <Base64> <Index64|none> <scale> <i32ˉdisplacement> <Reg32>
store_memory_u64 <Base64> <Index64|none> <scale> <i32ˉdisplacement> <Reg64>
```

`Condition` is one of:

```text
overflow not_overflow below above_equal equal not_equal below_equal above
sign not_sign parity not_parity less greater_equal less_equal greater
```

The original encodings are:

| Statement | Bytes | Relocation |
| --- | --- | --- |
| `nop` | `90` | none |
| `return` | `C3` | none |
| `trap` | `CC` | none |
| `call Name` | `E8 00 00 00 00` | `relative-i32`, field offset after opcode, addend `-4` |
| `jump Name` | `E9 00 00 00 00` | `relative-i32`, field offset after opcode, addend `-4` |
| `move_i32 Reg Value` | `B8+rd imm32` | none |
| `move_u32 Reg Value` | `B8+rd imm32` | none |
| `push_i32 Value` | `68 imm32` | none |
| `enable_page_protection` | `B9 80 00 00 C0 0F 32 0F BA E8 0B 0F 30 0F 20 C0 48 0F BA E8 10 0F 22 C0` | none |
| `activate_page_table` | `0F 22 D8 0F 20 D8` | none |
| `syscall` | `0F 05` | none |
| `disable_interrupts` | `FA` | none |
| `halt` | `F4` | none |
| `out_u16` | `66 EF` | none |

The expanded encodings use the standard x86-64 REX, ModRM, and near-control forms:

| Statement | Encoding | Relocation |
| --- | --- | --- |
| `label Name` | no bytes | none |
| `jump_label Name` | `E9 rel32` | none; resolved within the definition |
| `branch Condition Name` | `0F 80+cc rel32` | none; resolved within the definition |
| `move Dest Source` | `[REX.W/R/B] 89 /r` | none |
| `add Dest Source` | `[REX.W/R/B] 01 /r` | none |
| `subtract Dest Source` | `[REX.W/R/B] 29 /r` | none |
| `and Dest Source` | `[REX.W/R/B] 21 /r` | none |
| `or Dest Source` | `[REX.W/R/B] 09 /r` | none |
| `xor Dest Source` | `[REX.W/R/B] 31 /r` | none |
| `compare Dest Source` | `[REX.W/R/B] 39 /r` | none |
| `test Dest Source` | `[REX.W/R/B] 85 /r` | none |
| `push Reg64` | `[REX.B] 50+rd` | none |
| `pop Reg64` | `[REX.B] 58+rd` | none |
| `call_register Reg64` | `[REX.B] FF /2` | none |
| `jump_register Reg64` | `[REX.B] FF /4` | none |
| `load_u32 Reg Symbol` | `[REX.R] 8B /r rip+disp32` | `relative-i32`, displacement field, addend `-4` |
| `load_u64 Reg Symbol` | `REX.W/R 8B /r rip+disp32` | `relative-i32`, displacement field, addend `-4` |
| `store_u32 Symbol Reg` | `[REX.R] 89 /r rip+disp32` | `relative-i32`, displacement field, addend `-4` |
| `store_u64 Symbol Reg` | `REX.W/R 89 /r rip+disp32` | `relative-i32`, displacement field, addend `-4` |
| `load_address Reg Symbol` | `REX.W/R 8D /r rip+disp32` | `relative-i32`, displacement field, addend `-4` |
| `multiply Dest Source` | `[REX.W/R/B] 0F AF /r` | none |
| `add_i32 Reg Value` | `[REX.W/B] 81 /0 imm32` | none |
| `subtract_i32 Reg Value` | `[REX.W/B] 81 /5 imm32` | none |
| `and_i32 Reg Value` | `[REX.W/B] 81 /4 imm32` | none |
| `or_i32 Reg Value` | `[REX.W/B] 81 /1 imm32` | none |
| `xor_i32 Reg Value` | `[REX.W/B] 81 /6 imm32` | none |
| `compare_i32 Reg Value` | `[REX.W/B] 81 /7 imm32` | none |
| `test_i32 Reg Value` | `[REX.W/B] F7 /0 imm32` | none |
| `rotate_left Reg Count` | `[REX.W/B] C1 /0 imm8` | none |
| `rotate_right Reg Count` | `[REX.W/B] C1 /1 imm8` | none |
| `shift_left Reg Count` | `[REX.W/B] C1 /4 imm8` | none |
| `shift_right Reg Count` | `[REX.W/B] C1 /5 imm8` | none |
| `shift_right_signed Reg Count` | `[REX.W/B] C1 /7 imm8` | none |
| `load_memory_u32 Reg Base Index Scale Disp` | `[REX.R/X/B] 8B /r sib disp32` | none |
| `load_memory_u64 Reg Base Index Scale Disp` | `REX.W/R/X/B 8B /r sib disp32` | none |
| `store_memory_u32 Base Index Scale Disp Reg` | `[REX.R/X/B] 89 /r sib disp32` | none |
| `store_memory_u64 Base Index Scale Disp Reg` | `REX.W/R/X/B 89 /r sib disp32` | none |

Destination registers are written first in WVA syntax. `move`, arithmetic, logical, comparison, and test operands must have identical widths. The register-to-register arithmetic and logical statements leave the architectural flags defined by the corresponding x86-64 instruction; `compare` and `test` exist specifically to establish those flags for `branch`. Conditions use the complete ordinary x86 condition-code order from `overflow` (`cc=0`) through signed `greater` (`cc=15`).

The `_i32` operations always carry one exact signed 32-bit immediate. A 32-bit operation consumes its two's-complement bit pattern; a 64-bit operation uses the x86-64 sign-extended `imm32` form. `multiply` is two-operand signed `IMUL`: it writes the low-width product to the destination and sets overflow/carry when the mathematical product is not representable. Rotate and shift counts are explicit decimal values from zero through 31 for `Reg32` and zero through 63 for `Reg64`; WVA rejects values that x86 would otherwise mask implicitly.

Memory operations encode exactly `[Base64 + Index64 * scale + i32 displacement]` with one SIB byte and an unconditional four-byte displacement. `scale` is 1, 2, 4, or 8. `none` selects no index and requires scale 1. `rsp` cannot be an index; `r12` is valid because REX.X distinguishes it from the SIB no-index field. Any `Reg64` is a valid base, including `rbp` and `r13`, because the fixed `disp32` form removes their zero-displacement ambiguity. WVA performs no pointer, bounds, alignment, alias, or capability proof for these system-level machine addresses; the owning ABI or unsafe boundary must supply it.

Labels use the WVO machine-name grammar, are scoped to one definition, and emit no WVO symbol. A label name must be unique within that definition. `jump_label` and `branch` require exactly one matching local label and always encode deterministic near `rel32` displacements; the assembler does not shorten branches. Existing `jump` remains a symbol-oriented inter-definition operation with a WVO relocation.

RIP-relative loads and stores require a declared data symbol. `load_address` accepts a declared function or data symbol. All five forms leave a four-byte zero placeholder and one canonical `relative-i32` relocation, so WVO 1.0 and the existing linker remain sufficient. A 32-bit destination write follows x86-64 rules and clears its register's high 32 bits.

The move instructions write a 32-bit register and carry the exact little-endian bit pattern of the declared value. In 64-bit mode, `push_i32` decrements `RSP` by eight and stores the immediate sign-extended to one 64-bit stack cell. It exists to construct exact machine-entry records such as normalized exception frames; it does not define a general ABI, stack discipline, calling convention, function prologue, or balanced-stack policy. Those require a separate contract before generated calls are considered executable across a boundary.

`enable_page_protection` selects EFER MSR `0xC0000080`, sets NXE bit 11, writes EFER, reads CR0, sets WP bit 16, and writes CR0. It clobbers `EAX`/`RAX`, `ECX`/`RCX`, and `EDX`/`RDX`. `activate_page_table` loads CR3 from `RAX` and reads active CR3 back into `RAX`. These are semantic compound operations for [kernel paging version 1](Windvale-Kernel-Paging.md), not general MSR or control-register access. Their caller must prove processor support, table validity, address reachability, and ABI preservation before use.

`syscall` enters the x86-64 syscall target configured by privileged kernel policy. It has no implicit Windvale operation number, capability semantics, buffer convention, or authorization: the owning versioned OS boundary defines the complete register and state contract. Portable and ordinary hosted Windvale source cannot emit this WVA-only machine instruction.

`disable_interrupts` clears the x86 interrupt flag. `halt` stops instruction execution until an admitted wake event; it is not a process exit or permanent loop by itself. `out_u16` writes the low 16 bits of `EAX`/`AX` to the I/O port selected by the low 16 bits of `EDX`/`DX`. These statements expose privileged architecture mechanics deliberately. Their caller owns register initialization, authorization, hardware selection, and any terminal fallback loop. Ordinary Windvale source receives no ambient port-I/O authority from their presence in WVA.

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

Assembly fails before output when source structure, names, ordering, integer widths, register widths, memory base/index/scale/displacement shape, shift count, section ownership, definitions, references, instruction contexts, limits, or WVO invariants are invalid. `call` and `jump` require a declared function target; `address_u32` accepts any declared symbol. The resulting object passes the independent WVO verifier before bytes are returned.

Identical WVA text and assembler version produce identical WVO bytes on every host. Source paths, timestamps, comments, whitespace, and line-ending choice are not serialized.

## Limits

- Source: 1 MiB
- Physical line: 4 KiB UTF-8
- Sections, symbols, relocations, names, alignments, object bytes, and object memory: WVO 1.0 limits
- Bytes on one `bytes` statement: 4,096
- Encoded data and code remain subject to the 4 MiB object-data limit

## Deliberate omissions

WVA 1 still has no 8- or 16-bit general register or memory operations, division, variable-count shifts, double-width multiply results, conditional moves, condition-result materialization, short-branch or displacement relaxation, 64-bit immediates or absolute addresses, segment/address-size overrides, SIMD, floating point, other privileged operations, macros, includes, expressions, constants, debug records, ABI aliases, automatic section creation, or final-image directives. Memory operands deliberately require a base and fixed `disp32`; absolute, RIP-relative-with-addend, base-less index, and address-expression syntax remain separate future contracts. Add further operations with an exact operand, encoding, validation, and verification rule rather than an opaque executable-byte escape.
