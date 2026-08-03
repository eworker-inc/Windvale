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
Reg8  = al cl dl bl spl bpl sil dil r8b r9b r10b r11b r12b r13b r14b r15b
Reg16 = ax cx dx bx sp bp si di r8w r9w r10w r11w r12w r13w r14w r15w
Reg32 = eax ecx edx ebx esp ebp esi edi r8d r9d r10d r11d r12d r13d r14d r15d
Reg64 = rax rcx rdx rbx rsp rbp rsi rdi r8 r9 r10 r11 r12 r13 r14 r15
```

The legacy high-byte names `ah`, `ch`, `dh`, and `bh` are deliberately not registers in WVA. Excluding them makes every admitted byte register compatible with the REX encoding needed by `spl`, `bpl`, `sil`, `dil`, and `r8b` through `r15b`.

WVA 1 code definitions support the original symbol-oriented and machine operations:

```text
nop
return
trap
call <Functionˉsymbol>
jump <Functionˉsymbol>
move_i32 <Reg32> <i32>
move_u32 <Reg32> <u32>
move_u8 <Reg8> <u8>
move_u16 <Reg16> <u16>
push_i32 <i32>
enable_page_protection
activate_page_table
syscall
cpuid
read_tsc
read_msr
swap_gs
interrupt_return
disable_interrupts
halt
in_u8
out_u8
out_u16
```

They also support definition-local control flow, register operations, and RIP-relative symbol access:

```text
label <Localˉname>
jump_label <Localˉname>
branch <Condition> <Localˉname>

move <Reg8|Reg16|Reg32|Reg64> <sameˉwidthˉregister>
add <Reg8|Reg16|Reg32|Reg64> <sameˉwidthˉregister>
subtract <Reg8|Reg16|Reg32|Reg64> <sameˉwidthˉregister>
and <Reg8|Reg16|Reg32|Reg64> <sameˉwidthˉregister>
or <Reg8|Reg16|Reg32|Reg64> <sameˉwidthˉregister>
xor <Reg8|Reg16|Reg32|Reg64> <sameˉwidthˉregister>
compare <Reg8|Reg16|Reg32|Reg64> <sameˉwidthˉregister>
test <Reg8|Reg16|Reg32|Reg64> <sameˉwidthˉregister>

push <Reg64>
pop <Reg64>
call_register <Reg64>
jump_register <Reg64>

load_u8 <Reg8> <Dataˉsymbol>
load_u16 <Reg16> <Dataˉsymbol>
load_u32 <Reg32> <Dataˉsymbol>
load_u64 <Reg64> <Dataˉsymbol>
store_u8 <Dataˉsymbol> <Reg8>
store_u16 <Dataˉsymbol> <Reg16>
store_u32 <Dataˉsymbol> <Reg32>
store_u64 <Dataˉsymbol> <Reg64>
load_address <Reg64> <Symbol>

multiply <Reg16|Reg32|Reg64> <sameˉwidthˉregister>
add_i8 <Reg8> <i8>
subtract_i8 <Reg8> <i8>
and_i8 <Reg8> <i8>
or_i8 <Reg8> <i8>
xor_i8 <Reg8> <i8>
compare_i8 <Reg8> <i8>
test_i8 <Reg8> <i8>
add_i16 <Reg16> <i16>
subtract_i16 <Reg16> <i16>
and_i16 <Reg16> <i16>
or_i16 <Reg16> <i16>
xor_i16 <Reg16> <i16>
compare_i16 <Reg16> <i16>
test_i16 <Reg16> <i16>
add_i32 <Reg32|Reg64> <i32>
subtract_i32 <Reg32|Reg64> <i32>
and_i32 <Reg32|Reg64> <i32>
or_i32 <Reg32|Reg64> <i32>
xor_i32 <Reg32|Reg64> <i32>
compare_i32 <Reg32|Reg64> <i32>
test_i32 <Reg32|Reg64> <i32>

rotate_left <Reg8|Reg16|Reg32|Reg64> <count>
rotate_right <Reg8|Reg16|Reg32|Reg64> <count>
shift_left <Reg8|Reg16|Reg32|Reg64> <count>
shift_right <Reg8|Reg16|Reg32|Reg64> <count>
shift_right_signed <Reg8|Reg16|Reg32|Reg64> <count>

load_memory_u8 <Reg8> <Base64> <Index64|none> <scale> <i32ˉdisplacement>
load_memory_u16 <Reg16> <Base64> <Index64|none> <scale> <i32ˉdisplacement>
load_memory_u32 <Reg32> <Base64> <Index64|none> <scale> <i32ˉdisplacement>
load_memory_u64 <Reg64> <Base64> <Index64|none> <scale> <i32ˉdisplacement>
store_memory_u8 <Base64> <Index64|none> <scale> <i32ˉdisplacement> <Reg8>
store_memory_u16 <Base64> <Index64|none> <scale> <i32ˉdisplacement> <Reg16>
store_memory_u32 <Base64> <Index64|none> <scale> <i32ˉdisplacement> <Reg32>
store_memory_u64 <Base64> <Index64|none> <scale> <i32ˉdisplacement> <Reg64>

set_condition <Condition> <Reg8>
zero_extend_u8 <Reg32|Reg64> <Reg8>
zero_extend_u16 <Reg32|Reg64> <Reg16>
sign_extend_i8 <Reg32|Reg64> <Reg8>
sign_extend_i16 <Reg32|Reg64> <Reg16>
```

`Condition` is one of:

```text
overflow not_overflow below above_equal equal not_equal below_equal above
sign not_sign parity not_parity less greater_equal less_equal greater
```

The fixed machine encodings are:

| Statement | Bytes | Relocation |
| --- | --- | --- |
| `nop` | `90` | none |
| `return` | `C3` | none |
| `trap` | `CC` | none |
| `call Name` | `E8 00 00 00 00` | `relative-i32`, field offset after opcode, addend `-4` |
| `jump Name` | `E9 00 00 00 00` | `relative-i32`, field offset after opcode, addend `-4` |
| `move_i32 Reg Value` | `B8+rd imm32` | none |
| `move_u32 Reg Value` | `B8+rd imm32` | none |
| `move_u8 Reg Value` | `[REX.B/forced] B0+rb imm8` | none |
| `move_u16 Reg Value` | `66 [REX.B] B8+rw imm16` | none |
| `push_i32 Value` | `68 imm32` | none |
| `enable_page_protection` | `B9 80 00 00 C0 0F 32 0F BA E8 0B 0F 30 0F 20 C0 48 0F BA E8 10 0F 22 C0` | none |
| `activate_page_table` | `0F 22 D8 0F 20 D8` | none |
| `syscall` | `0F 05` | none |
| `cpuid` | `0F A2` | none |
| `read_tsc` | `0F 31` | none |
| `read_msr` | `0F 32` | none |
| `swap_gs` | `0F 01 F8` | none |
| `interrupt_return` | `48 CF` | none |
| `disable_interrupts` | `FA` | none |
| `halt` | `F4` | none |
| `in_u8` | `EC` | none |
| `out_u8` | `EE` | none |
| `out_u16` | `66 EF` | none |

The expanded encodings use the standard x86-64 REX, ModRM, and near-control forms:

| Statement | Encoding | Relocation |
| --- | --- | --- |
| `label Name` | no bytes | none |
| `jump_label Name` | `E9 rel32` | none; resolved within the definition |
| `branch Condition Name` | `0F 80+cc rel32` | none; resolved within the definition |
| `move Dest Source` | byte: `[REX.R/B/forced] 88 /r`; word: `66 [REX.R/B] 89 /r`; dword/qword: `[REX.W/R/B] 89 /r` | none |
| `add Dest Source` | byte: `[REX.R/B/forced] 00 /r`; word: `66 [REX.R/B] 01 /r`; dword/qword: `[REX.W/R/B] 01 /r` | none |
| `subtract Dest Source` | byte: `[REX.R/B/forced] 28 /r`; word: `66 [REX.R/B] 29 /r`; dword/qword: `[REX.W/R/B] 29 /r` | none |
| `and Dest Source` | byte: `[REX.R/B/forced] 20 /r`; word: `66 [REX.R/B] 21 /r`; dword/qword: `[REX.W/R/B] 21 /r` | none |
| `or Dest Source` | byte: `[REX.R/B/forced] 08 /r`; word: `66 [REX.R/B] 09 /r`; dword/qword: `[REX.W/R/B] 09 /r` | none |
| `xor Dest Source` | byte: `[REX.R/B/forced] 30 /r`; word: `66 [REX.R/B] 31 /r`; dword/qword: `[REX.W/R/B] 31 /r` | none |
| `compare Dest Source` | byte: `[REX.R/B/forced] 38 /r`; word: `66 [REX.R/B] 39 /r`; dword/qword: `[REX.W/R/B] 39 /r` | none |
| `test Dest Source` | byte: `[REX.R/B/forced] 84 /r`; word: `66 [REX.R/B] 85 /r`; dword/qword: `[REX.W/R/B] 85 /r` | none |
| `push Reg64` | `[REX.B] 50+rd` | none |
| `pop Reg64` | `[REX.B] 58+rd` | none |
| `call_register Reg64` | `[REX.B] FF /2` | none |
| `jump_register Reg64` | `[REX.B] FF /4` | none |
| `load_u8 Reg Symbol` | `[REX.R/forced] 8A /r rip+disp32` | `relative-i32`, displacement field, addend `-4` |
| `load_u16 Reg Symbol` | `66 [REX.R] 8B /r rip+disp32` | `relative-i32`, displacement field, addend `-4` |
| `load_u32 Reg Symbol` | `[REX.R] 8B /r rip+disp32` | `relative-i32`, displacement field, addend `-4` |
| `load_u64 Reg Symbol` | `REX.W/R 8B /r rip+disp32` | `relative-i32`, displacement field, addend `-4` |
| `store_u8 Symbol Reg` | `[REX.R/forced] 88 /r rip+disp32` | `relative-i32`, displacement field, addend `-4` |
| `store_u16 Symbol Reg` | `66 [REX.R] 89 /r rip+disp32` | `relative-i32`, displacement field, addend `-4` |
| `store_u32 Symbol Reg` | `[REX.R] 89 /r rip+disp32` | `relative-i32`, displacement field, addend `-4` |
| `store_u64 Symbol Reg` | `REX.W/R 89 /r rip+disp32` | `relative-i32`, displacement field, addend `-4` |
| `load_address Reg Symbol` | `REX.W/R 8D /r rip+disp32` | `relative-i32`, displacement field, addend `-4` |
| `multiply Dest Source` | word: `66 [REX.R/B] 0F AF /r`; dword/qword: `[REX.W/R/B] 0F AF /r` | none |
| `_i8 Reg Value` ALU/compare | `[REX.B/forced] 80 /group imm8` | none |
| `test_i8 Reg Value` | `[REX.B/forced] F6 /0 imm8` | none |
| `_i16 Reg Value` ALU/compare | `66 [REX.B] 81 /group imm16` | none |
| `test_i16 Reg Value` | `66 [REX.B] F7 /0 imm16` | none |
| `add_i32 Reg Value` | `[REX.W/B] 81 /0 imm32` | none |
| `subtract_i32 Reg Value` | `[REX.W/B] 81 /5 imm32` | none |
| `and_i32 Reg Value` | `[REX.W/B] 81 /4 imm32` | none |
| `or_i32 Reg Value` | `[REX.W/B] 81 /1 imm32` | none |
| `xor_i32 Reg Value` | `[REX.W/B] 81 /6 imm32` | none |
| `compare_i32 Reg Value` | `[REX.W/B] 81 /7 imm32` | none |
| `test_i32 Reg Value` | `[REX.W/B] F7 /0 imm32` | none |
| `rotate_left Reg Count` | byte: `[REX.B/forced] C0 /0 imm8`; word: `66 [REX.B] C1 /0 imm8`; dword/qword: `[REX.W/B] C1 /0 imm8` | none |
| `rotate_right Reg Count` | byte: `[REX.B/forced] C0 /1 imm8`; word: `66 [REX.B] C1 /1 imm8`; dword/qword: `[REX.W/B] C1 /1 imm8` | none |
| `shift_left Reg Count` | byte: `[REX.B/forced] C0 /4 imm8`; word: `66 [REX.B] C1 /4 imm8`; dword/qword: `[REX.W/B] C1 /4 imm8` | none |
| `shift_right Reg Count` | byte: `[REX.B/forced] C0 /5 imm8`; word: `66 [REX.B] C1 /5 imm8`; dword/qword: `[REX.W/B] C1 /5 imm8` | none |
| `shift_right_signed Reg Count` | byte: `[REX.B/forced] C0 /7 imm8`; word: `66 [REX.B] C1 /7 imm8`; dword/qword: `[REX.W/B] C1 /7 imm8` | none |
| `load_memory_u8 Reg Base Index Scale Disp` | `[REX.R/X/B/forced] 8A /r sib disp32` | none |
| `load_memory_u16 Reg Base Index Scale Disp` | `66 [REX.R/X/B] 8B /r sib disp32` | none |
| `load_memory_u32 Reg Base Index Scale Disp` | `[REX.R/X/B] 8B /r sib disp32` | none |
| `load_memory_u64 Reg Base Index Scale Disp` | `REX.W/R/X/B 8B /r sib disp32` | none |
| `store_memory_u8 Base Index Scale Disp Reg` | `[REX.R/X/B/forced] 88 /r sib disp32` | none |
| `store_memory_u16 Base Index Scale Disp Reg` | `66 [REX.R/X/B] 89 /r sib disp32` | none |
| `store_memory_u32 Base Index Scale Disp Reg` | `[REX.R/X/B] 89 /r sib disp32` | none |
| `store_memory_u64 Base Index Scale Disp Reg` | `REX.W/R/X/B 89 /r sib disp32` | none |
| `set_condition Condition Reg8` | `[REX.B/forced] 0F 90+cc /0` | none |
| `zero_extend_u8 Dest Source` | `[REX.W/R/B/forced] 0F B6 /r` | none |
| `zero_extend_u16 Dest Source` | `[REX.W/R/B] 0F B7 /r` | none |
| `sign_extend_i8 Dest Source` | `[REX.W/R/B/forced] 0F BE /r` | none |
| `sign_extend_i16 Dest Source` | `[REX.W/R/B] 0F BF /r` | none |

Destination registers are written first in WVA syntax. `move`, arithmetic, logical, comparison, and test operands must have identical widths. The register-to-register arithmetic and logical statements leave the architectural flags defined by the corresponding x86-64 instruction; `compare` and `test` exist specifically to establish those flags for `branch`. Conditions use the complete ordinary x86 condition-code order from `overflow` (`cc=0`) through signed `greater` (`cc=15`).

The `_i8`, `_i16`, and `_i32` operations carry exact signed immediates of their declared widths. A 32-bit operation consumes its two's-complement bit pattern; a 64-bit operation uses the x86-64 sign-extended `imm32` form. `multiply` is two-operand signed `IMUL` for 16-, 32-, and 64-bit operands: it writes the low-width product to the destination and sets overflow/carry when the mathematical product is not representable. There is no admitted 8-bit two-operand `IMUL`. Rotate and shift counts are explicit decimal values from zero through one less than the operand width; WVA rejects values that x86 would otherwise mask implicitly.

Memory operations encode exactly `[Base64 + Index64 * scale + i32 displacement]` with one SIB byte and an unconditional four-byte displacement. `scale` is 1, 2, 4, or 8. `none` selects no index and requires scale 1. `rsp` cannot be an index; `r12` is valid because REX.X distinguishes it from the SIB no-index field. Any `Reg64` is a valid base, including `rbp` and `r13`, because the fixed `disp32` form removes their zero-displacement ambiguity. WVA performs no pointer, bounds, alignment, alias, or capability proof for these system-level machine addresses; the owning ABI or unsafe boundary must supply it.

Labels use the WVO machine-name grammar, are scoped to one definition, and emit no WVO symbol. A label name must be unique within that definition. `jump_label` and `branch` require exactly one matching local label and always encode deterministic near `rel32` displacements; the assembler does not shorten branches. Existing `jump` remains a symbol-oriented inter-definition operation with a WVO relocation.

RIP-relative loads and stores require a declared data symbol. `load_address` accepts a declared function or data symbol. Every RIP-relative form leaves a four-byte zero placeholder and one canonical `relative-i32` relocation, so WVO 1.0 and the existing linker remain sufficient. A 32-bit destination write follows x86-64 rules and clears its register's high 32 bits.

`set_condition` materializes the selected architectural flag condition as byte value zero or one. The extension statements accept only a 32- or 64-bit destination and an exact 8- or 16-bit source. Zero extension preserves the source bit pattern; sign extension replicates its high bit. A 32-bit extension destination clears the architectural high 32 bits.

The move instructions write a 32-bit register and carry the exact little-endian bit pattern of the declared value. In 64-bit mode, `push_i32` decrements `RSP` by eight and stores the immediate sign-extended to one 64-bit stack cell. It exists to construct exact machine-entry records such as normalized exception frames; it does not define a general ABI, stack discipline, calling convention, function prologue, or balanced-stack policy. Those require a separate contract before generated calls are considered executable across a boundary.

`enable_page_protection` selects EFER MSR `0xC0000080`, sets NXE bit 11, writes EFER, reads CR0, sets WP bit 16, and writes CR0. It clobbers `EAX`/`RAX`, `ECX`/`RCX`, and `EDX`/`RDX`. `activate_page_table` loads CR3 from `RAX` and reads active CR3 back into `RAX`. These are semantic compound operations for [kernel paging version 1](Windvale-Kernel-Paging.md), not general MSR or control-register access. Their caller must prove processor support, table validity, address reachability, and ABI preservation before use.

`syscall` enters the x86-64 syscall target configured by privileged kernel policy. It has no implicit Windvale operation number, capability semantics, buffer convention, or authorization: the owning versioned OS boundary defines the complete register and state contract. Portable and ordinary hosted Windvale source cannot emit this WVA-only machine instruction.

`cpuid` consumes `EAX`/`ECX` and writes `EAX`/`EBX`/`ECX`/`EDX`. `read_tsc` writes the current architectural timestamp-counter value to `EDX:EAX`. `read_msr` consumes the model-specific-register index in `ECX` and writes its value to `EDX:EAX`. These operations provide feature, clocksource, and local-APIC evidence to an owning kernel machine contract; WVA supplies no leaf selection, serialization, availability, privilege, calibration, or monotonicity guarantee by itself.

`swap_gs` exchanges `GS.base` with `IA32_KERNEL_GS_BASE`. `interrupt_return` executes 64-bit `IRETQ` and consumes the exact privilege-transition frame at `RSP`. Neither statement validates privilege, model-specific-register state, selectors, frame shape, addresses, flags, or the destination stack. They exist only for an owning kernel interrupt boundary that proves those invariants before use; ordinary Windvale source receives no authority to emit them.

`disable_interrupts` clears the x86 interrupt flag. `halt` stops instruction execution until an admitted wake event; it is not a process exit or permanent loop by itself. `in_u8` reads one byte from the I/O port selected by `DX` into `AL`; `out_u8` writes `AL` to that port; and `out_u16` writes `AX`. These statements expose privileged architecture mechanics deliberately. Their caller owns register initialization, authorization, hardware selection, and any terminal fallback loop. Ordinary Windvale source receives no ambient port-I/O authority from their presence in WVA.

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

WVA 1 still has no legacy high-byte registers, division, variable-count shifts, double-width multiply results, conditional moves, short-branch or displacement relaxation, general 64-bit immediates or absolute addresses, segment/address-size overrides, SIMD, floating point, generic port widths, other privileged operations, macros, includes, expressions, constants, debug records, ABI aliases, automatic section creation, or final-image directives. Memory operands deliberately require a base and fixed `disp32`; absolute, RIP-relative-with-addend, base-less index, and address-expression syntax remain separate future contracts. Add further operations with an exact operand, encoding, validation, and verification rule rather than an opaque executable-byte escape.
