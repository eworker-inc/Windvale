# WVA native-stencil contract

## Status

`WVSP 1` and the measured multi-patch `WVSP 2` extension are qualified bounded contracts for the two Windvale-owned process-input construction slices under Decisions 0077 and 0078. `WVSP 2` is qualified at exact integrated commit `50294d9d5cc24edc26a3e56994cb3aa28e16352c`; their Windvale-written exact consumer is qualified under Decision 0079 at exact commit `f3a4ba4ebf4d2e9de28dfa73e34e14362691919b`. Decision 0080 qualifies its bounded byte-result bridge and live consumption at exact commit `f547af8dcf8e257ab8ad8a76a49bbdd1b9136677` on Windows and Debian.

Neither version defines a general stencil container or complete baseline JIT.

## Purpose

The native-stencil artifacts move construction of existing x86-64 runtime leaves from C# byte literals into WVA source assembled by the Windvale-written assembler. The runtime consumes canonical WVO objects, validates their complete fixed shapes, applies named typed patches, and retains the already-qualified final leaf identities.

The objects remain read-only data until the native owner accepts and instantiates them. Loading an object never grants permission to execute arbitrary bytes.

## Common object boundary

Both accepted objects must be canonical WVO 1.0 for `X86ˉ64` with exactly one `.rodata` read-only-data section at alignment 1, exactly two symbols in declaration order, and no relocations. Metadata and template bytes occupy distinct named data definitions in that section.

Every patch record uses three little-endian `u32` fields:

| Record offset | Field |
| ---: | --- |
| 0 | Template-relative patch offset |
| 4 | Patch width in bytes |
| 8 | Closed `Nativeˉstencilˉpatchˉkind` value |

Patch locations are strictly increasing, non-overlapping, within the declared template, and zero-valued before instantiation. Every currently accepted patch has width one. A patch kind identifies the ABI meaning of its byte; repeated locations with the same meaning receive the same checked value.

## `WVSP 1`: `process.argument_count`

The object has:

- one 25-byte data/memory section;
- local data symbol `Argument_count_patch` at section 0, offset 0, size 20;
- exported data symbol `Process_argument_count_stencil` at section 0, offset 20, size 5; and
- no relocations.

The first 20 section bytes are five little-endian `u32` values:

| Offset | Value | Meaning |
| ---: | ---: | --- |
| 0 | `0x50535657` | ASCII `WVSP` magic |
| 4 | `1` | Single-patch format version |
| 8 | `3` | Patch offset relative to the stencil |
| 12 | `1` | Patch width |
| 16 | `1` | `Executionˉcontextˉu8ˉoffset` patch kind |

The five-byte stencil is `41 8B 47 00 C3`. Byte 3 receives the checked execution-context `ARGUMENT_COUNT_OFFSET`. The canonical 166-byte WVO has SHA-256 `e2057943b9c79e10a432ea20a77da5ed0a261e3effdd36511cbb34e77e55c10b`. The instantiated five-byte leaf retains SHA-256 `2358e7e2c72d6476cfe05134db4f0eb5e6987fcca1b10894a8588a28d3929829`.

## `WVSP 2`: `process.argument`

The object has:

- one 182-byte data/memory section;
- local data symbol `Process_argument_patches` at section 0, offset 0, size 112;
- exported data symbol `Process_argument_stencil` at section 0, offset 112, size 70; and
- no relocations.

The 112 metadata bytes begin with four little-endian `u32` values:

| Offset | Value | Meaning |
| ---: | ---: | --- |
| 0 | `0x50535657` | ASCII `WVSP` magic |
| 4 | `2` | Ordered-patch format version |
| 8 | `8` | Exact patch-record count |
| 12 | `70` | Exact template size |

Eight 12-byte patch records follow at metadata offset 16:

| Index | Metadata offset | Template offset | Width | Kind | Instantiated value |
| ---: | ---: | ---: | ---: | --- | ---: |
| 0 | 16 | 3 | 1 | `Executionˉcontextˉserviceˉfailureˉdetailˉu8ˉoffset` | 64 |
| 1 | 28 | 11 | 1 | `Executionˉcontextˉargumentˉcountˉu8ˉoffset` | 80 |
| 2 | 40 | 21 | 1 | `Executionˉcontextˉargumentˉtableˉpointerˉu8ˉoffset` | 72 |
| 3 | 52 | 40 | 1 | `Borrowedˉtextˉlengthˉu8ˉoffset` | 8 |
| 4 | 64 | 44 | 1 | `Borrowedˉtextˉlengthˉu8ˉoffset` | 8 |
| 5 | 76 | 48 | 1 | `Borrowedˉtextˉreservedˉu8ˉoffset` | 12 |
| 6 | 88 | 59 | 1 | `Executionˉcontextˉserviceˉfailureˉdetailˉu8ˉoffset` | 64 |
| 7 | 100 | 60 | 1 | `Argumentˉindexˉoutˉofˉrangeˉu8ˉdetail` | 3 |

The exact zero-hole template is:

```text
41 C7 47 00 00 00 00 00 45 3B 47 00 0F 83 26 00 00 00
49 8B 47 00 44 89 C1 48 C1 E1 04 48 01 C8 48 8B 08 49 89 09
8B 48 00 41 89 49 00 41 C7 41 00 00 00 00 00 31 C0 C3
41 C7 47 00 00 00 00 00 B8 01 00 00 00 C3
```

The canonical 321-byte WVO has SHA-256 `307e61dcb2a156eb0d4b77f7d93676d7b1ac24f9bb6fe1f31217837213352bad`. Instantiation must obtain every value through the named native contract, supply no missing or unused kinds, reuse one value for repeated kinds, and apply records only in their accepted order. The resulting exact 70-byte leaf retains SHA-256 `2253e1435f141df5b68f9f7e9e9aa0de448410c42dcf33ad76dcf131afea65d1`.

## Validation and reproduction

The native owner rejects a complete artifact if its architecture, section identity, alignment, sizes, symbol records, relocation count, magic, version, patch count, template size, patch order, patch offset, width, kind, fixed shell, or zero-valued holes differ. Instantiation revalidates version, bounds, strict ordering, widths, holes, required kinds, and exact value coverage before copying immutable template bytes.

The repository retains each `.wva` source and its Windvale-assembled WVO. The conformance test compiles the Windvale-written WVA assembler once, runs it twice over each source, compares deterministic outputs, compares them byte for byte with the Stage 0 recovery oracle and embedded object, validates both exact objects, instantiates every patch, and checks the final qualified leaf identities. Malformed header fields, patch records, fixed opcodes, holes, missing values, and duplicate locations are rejected.

## Windvale-owned consumer

`Compiler/Windvale/Native-Stencil-Core.wv` is the portable, capability-free consumer for these two contracts. It exposes closed status and patch-kind enums plus an immutable result containing the accepted bytes and a failure offset. It accepts only the complete 166-byte `WVSP 1` WVO and complete 321-byte `WVSP 2` WVO shapes described above, validates every byte rather than treating a digest as an input predicate, derives patch values from their named ABI meanings, and constructs the output without mutating the input.

`Examples/Compiler/Native-Stencil-Demo.wv` embeds the retained production objects and exercises successful construction, exact output locations, deterministic repetition, truncated input, corrupt object metadata, corrupt symbols, corrupt patch records, changed fixed shell bytes, and nonzero holes. The conformance harness proves that the embedded inputs remain byte-identical to the live production resources and executes the same compiled Windvale module through the reference interpreter, native JIT, and linked WVO/AOT routes.

`Compiler/Windvale/Native-Stencil-Bridge.wv` embeds both retained objects, calls the exact consumer, and returns their instantiated leaves as one immutable 75-byte bundle. Decision 0080's verified descriptor-return entry copies that bounded result before teardown, and the live process-input service path consumes the Windvale-produced bytes. C# remains the WVB loader, native compiler/verifier, publication and lifetime owner, and independent oracle; it no longer supplies the live leaf bytes. This is an integration boundary, not a language-semantics dependency.

## Deliberate limits

The accepted contracts describe two exact service leaves, one x86-64 architecture, one template per object, at most eight measured patches, and one-byte contract values. They do not admit arbitrary stencil discovery, multiple templates, wider values, branch-target patching, calls, data references, WVO relocations, user-supplied executable code, or an extensible patch-kind namespace. Those shapes require measured follow-up cases and a revised accepted contract rather than permissive parsing.
