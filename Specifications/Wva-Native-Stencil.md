# WVA native stencil experiment

## Status

Experimental bounded contract for the first Windvale-owned native-runtime construction slice. It does not yet define a general stencil container or complete baseline JIT.

## Purpose

The first `WVSP 1` artifact moves construction of the existing `process.argument_count` x86-64 runtime leaf from a C# byte literal into WVA source assembled by the Windvale-written assembler. The runtime consumes a canonical WVO object, validates its complete shape, applies one typed patch, and retains the already-qualified leaf identity.

## Exact object contract

The object must be canonical WVO 1.0 for `X86ˉ64` with:

- exactly one `.rodata` read-only-data section, alignment 1, 25 data and memory bytes;
- exactly two symbols in declaration order;
- local data symbol `Argument_count_patch` at section 0, offset 0, size 20;
- exported data symbol `Process_argument_count_stencil` at section 0, offset 20, size 5; and
- no relocations.

The first 20 section bytes are five little-endian `u32` values:

| Offset | Value | Meaning |
| ---: | ---: | --- |
| 0 | `0x50535657` | ASCII `WVSP` magic |
| 4 | `1` | Patch-record version |
| 8 | `3` | Patch offset relative to the stencil |
| 12 | `1` | Patch width in bytes |
| 16 | `1` | `Executionˉcontextˉu8ˉoffset` patch kind |

The five-byte stencil is `41 8B 47 00 C3`, which decodes as the fixed Windvale x86-64 service convention's load of a 32-bit value from the execution context followed by return. Byte 3 is the required zero-valued hole.

The canonical 166-byte WVO has SHA-256 `e2057943b9c79e10a432ea20a77da5ed0a261e3effdd36511cbb34e77e55c10b`.

## Instantiation and validation

The native owner must reject the complete artifact if architecture, section count, section identity, alignment, sizes, symbol count, symbol records, relocation count, magic, version, patch offset, patch width, patch kind, fixed stencil bytes, or the zero-valued hole differs.

Instantiation accepts only the declared patch kind, validates the offset and width again, copies the immutable template, and replaces the one-byte hole with the checked execution-context `ARGUMENT_COUNT_OFFSET`. The resulting five bytes must retain size 5 and SHA-256 `2358e7e2c72d6476cfe05134db4f0eb5e6987fcca1b10894a8588a28d3929829` before W^X publication.

The repository retains both `Process-Argument-Count.wva` and its Windvale-assembled WVO. Tests run the Windvale assembler twice, compare both outputs to each other and the Stage 0 recovery oracle, validate malformed variants, instantiate the patch, and compare the result with the qualified leaf identity and live construction path.

## Deliberate limits

`WVSP 1` currently describes one exact leaf, one exact patch, one architecture, and one-byte execution-context offsets. It does not admit multiple templates, branches, calls, arbitrary relocations, user-supplied executable code, or an extensible patch-kind namespace. Those shapes require measured follow-up cases and a revised accepted contract rather than permissive parsing.
