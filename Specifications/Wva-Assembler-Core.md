# Windvale WVA assembler core

## Status and purpose

`Wvaˉassemblerˉcore` is the first complete Windvale-written implementation of WVA 1. It consumes immutable source bytes, applies the accepted source and line limits, validates the complete initial grammar and semantic model, derives section and definition ranges, encodes x86-64 instruction/data bytes, constructs canonical WVO 1.0 records, and returns one immutable object value. Portable scanning, validation, measurement, and encoding do not use host text parsing or file APIs; the hosted shell supplies explicit input/output resources only at the boundary.

The implementation source is owned by `Assembler/Windvale/Wva-Assembler-Core.wv`, and `Projects/Assembler/Windvale-Wva-Assembler.wvproj` is its explicit source-to-WVB construction contract. The independent C# Stage 0 oracle and recovery implementation is owned by `Assembler/Reference/`; canonical WVA input examples remain under `Examples/Assembler/`. The paired native application contract is specified separately in [Windvale native WVA assembler](Windvale-Native-Wva-Assembler.md).

Decision 0519 makes this Project 1 manifest the normal broad-script
construction contract and requires independent native verification plus exact
inspection before execution. The current 101-function product contains 145,748
code bytes in its exact 180,071-byte WVB. Self-test, hosted assembly, all
rejection and existing-output-preservation families, canonical WVO comparison,
and Stage 0 differential behavior remain separate execution contracts.

Decision 0520 consumes the qualified digest-bound assembler to construct the
canonical WVO read-only fixture in both broad host scripts, removing that
managed assembly call. Historical application bytes remain owned by the native
front-door manifest and launchers. The current Stage 0 application-writer test
instead requires repeated byte equality, independent profile verification, CLI
equality, accepted/rejected current-host execution, and no CLR loading; an
evolving recovery writer is not required to reproduce an older qualified
container.

The lexical scanner was first cross-host qualified at `e5fd109`, and the complete semantic inspector at `cc57bf9`. The object encoder and hosted write path described below were first cross-host qualified at `a689617`: the exact committed archive passed the same 28-test suite and real CLI flow on Windows and Debian, and both hosts produced byte-for-byte identical assembler modules, WVO output, and normalized conformance contracts. The machine-contract composition was cross-host requalified at `d46af86`, ordinal byte ordering at `4fdea22`, bounded decimal parsing at `6d2a351`, and bounded long-line fixture construction at `26e2fd1`. Exact commit `12e9e2e` cross-host qualifies the clean-shutdown instructions `disable_interrupts`, `halt`, and `out_u16` plus `push_i32` for exact 64-bit exception-frame cells in a 99,102-byte composed WVB with SHA-256 `e1869d1ca62196328d0311fb0c42dc8789e00f2a90e041db2872e155128f4173`. Exact commit `860c69c` qualifies the retained operations plus named compound operations `enable_page_protection` and `activate_page_table`; its composed WVB has SHA-256 `e32d237127b07de73a639f47292c7cfeb3f7cb88f233c107ad3f852d9781d03b`. Protected-process version 1 introduced the no-operand `syscall` statement with exact x86-64 bytes `0F 05`; versions 2 and 3 reuse it without a grammar change. The first expanded register/local-control/RIP-relative implementation produced SHA-256 `dbdbae4fae2c19ec67e7f06824b91488ceccb65f1668520a759d62c7577521b7`. The subsequent scalar-immediate, multiply, shift/rotate, and deterministic SIB-memory implementation produced SHA-256 `6c96bc45cd9ce17016773391e1b39da27953355d2d462c29d90887f0b510a0fc`. The retained typed scalar implementation adds both sixteen-register 8- and 16-bit families, byte/word register and RIP/SIB-memory operations, exact signed 8-/16-bit immediates, width-bounded shifts, condition materialization, zero/sign extension, and byte port input/output. The current timer/interrupt-boundary implementation additionally admits exact no-operand `cpuid`, `read_tsc`, `read_msr`, `swap_gs`, and `interrupt_return` mechanics. Its locally verified composed WVB 1.11 module is 180,071 bytes with SHA-256 `a50e261fb690b1b2836b7b05da2d94ec7f023ef531ddd2432fc6a9001ae7049c`; dual-host native-package qualification remains pending. The unchanged 218-byte canonical WVO remains `992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85`.

This module is an assembler, not a linker. Imports remain unresolved, relocation placeholders remain zero, and no final address, image layout, entry point, PE/ELF/UEFI structure, or host ABI is selected.

## Preflight boundary

Scanning is ordered so no later byte operation can bypass an earlier source rule:

1. Read the immutable input length.
2. Reject more than 1,048,576 bytes as `source_too_large` without walking the contents.
3. Validate the complete value as strict UTF-8; reject malformed input as `invalid_utf8`.
4. Walk physical lines and reject a line containing more than 4,096 bytes as `line_too_long`.
5. Tokenize only after all three checks succeed.

LF, CRLF, and CR each end one physical line. A trailing terminator does not create an additional reported line. The line-byte limit excludes the terminator bytes, matching WVA 1 and the Stage 0 oracle.

## Token model

The scanner recognizes three token kinds:

- `Word` is a maximal non-empty byte sequence delimited by ASCII space, tab, `#`, CR, LF, or the end of input.
- `Newline` represents one normalized LF, CRLF, or CR line ending.
- `End` marks the exact source length.

ASCII space and tab are skipped. `#` skips bytes through, but not including, the next line ending. Blank and comment-only lines therefore produce only `Newline` tokens. Token offsets and lengths are zero-based byte values; reported lines and columns are one-based. Columns are byte columns. Accepted WVA keywords, numbers, registers, and WVO machine names are ASCII, so their byte and character columns are identical.

Each token carries its offset, length, next cursor, line, column, next line, and next column. This makes repeated bounded passes possible without hidden cursor state or mutable token collections.

## Version recognition

The first meaningful line must contain exactly two words:

```text
windvale-assembly 1
```

Horizontal whitespace and a trailing comment are allowed under the general WVA line rules. A missing meaningful line returns `missing_header`; any wrong, missing, or additional header word returns `bad_header`.

## Semantic passes

`Inspectˉwvaˉsemantics(Input: bytes)` first requires a valid scan, then performs bounded immutable passes over the same source bytes:

1. Validate line shapes, keywords, declaration/section/definition nesting, machine names, canonical order, alignment, register names, integer widths, statement contexts, and aggregate limits.
2. Detect globally duplicated symbol and section names, including duplicates that canonical adjacent-order checks alone cannot reveal.
3. Resolve every non-import symbol's section and enforce function/code and data/non-code ownership.
4. Resolve every definition to one non-import symbol in its declared section and reject duplicate definitions.
5. Resolve statement references, require function targets for `call` and `jump`, and require exactly one definition for every non-import symbol.

The accepted statements include the original WVA 1 set plus local control, same-width register ALU/test, register stack/indirect control, RIP-relative symbol access, two-operand `multiply`, seven signed-`i32` immediate ALU/test forms, five bounded immediate rotate/shift forms, typed SIB memory loads/stores with explicit base, optional index, scale, and fixed `disp32`, and the two exact interrupt-boundary operations. Register validation distinguishes all sixteen 32-bit GPR spellings from all sixteen 64-bit spellings. Memory validation separately requires a 64-bit base, rejects `rsp` as an index while admitting `r12` through REX.X, canonicalizes `none` to scale 1, and accepts only scales 1/2/4/8. Unsigned token accumulation uses `Foundationˉu32ˉdecimalˉparse`; the assembler retains token ownership, signed `i32` handling, width and shift-count checks, and exact status/diagnostic selection. Exact `i32` minimum and maximum and `u32` maximum boundaries remain part of WVA conformance.

The implementation stores no hidden cursor or host object. It uses repeated source passes and byte spans because current Seed has no general bounded collection module. Those passes are deterministic and bounded by WVA limits, but some name, definition-range, symbol-index, and local-label checks are quadratic in declaration or definition-statement count. The expanded differential fixture therefore uses a separately bounded 50-million-instruction interpreter envelope. Representative-source evidence must drive a bounded symbol/label table and byte builder before this cost becomes a normal tool limit.

Semantic status families correspond to the Stage 0 diagnostic codes:

- `WVA1001`: source encoding or version header;
- `WVA1002`: unexpected or unknown structure, keyword, kind, or statement;
- `WVA1003`: line/operand shape;
- `WVA1004`: machine name;
- `WVA1005`: alignment, register, or numeric width/value;
- `WVA1006`: duplicate or noncanonical declaration;
- `WVA1007`: section/symbol ownership or required section;
- `WVA1008`: statement used in the wrong section kind;
- `WVA1009`: definition/reference resolution or target kind;
- `WVA1010`: unclosed definition or section;
- `WVA1011`: source, line, count, data, memory, or relocation limit.

## Object measurement and encoding

`Encodeˉwva(Input: bytes) -> Wvaˉobjectˉencoding` first requires a valid semantic inspection. It then measures the complete WVO value before construction: the 24-byte header, every inline section/name/data record, every symbol/name record, and every 20-byte relocation. Measurement rejects a result beyond the 4 MiB object-value limit before immutable concatenation can trap or a host write can occur.

Encoding uses three deterministic record passes after measurement:

1. Walk sections in their already-validated canonical order. Track definition bodies, encode materialized statements, derive each section's memory size, and emit its inline data. Zero-fill advances memory without producing bytes.
2. Walk symbol declarations in canonical order. Imports receive section index `0xFFFFFFFF`, offset zero, and size zero. Each defined symbol triggers a bounded body pass that derives its section index, definition offset, and size from preceding statement widths.
3. Walk section bodies in source order to emit relocations. `call` and `jump` create `relative-i32` records at the four-byte field with addend `-4`; `address_u32` creates `absolute-u32` with addend zero. A declaration pass supplies the canonical target symbol index.

The encoder covers every accepted instruction and data statement. Fixed-width integers use the Foundation little-endian constructors. `bytes` values use a canonical embedded 0-through-255 byte table plus a one-byte immutable slice, avoiding a new narrowing intrinsic solely for this bootstrap stage. Register encodings derive low register fields and REX.W/R/X/B bits from typed validated operands. Immediate operations use fixed `imm32` fields; rotate/shift forms use validated `imm8`; general memory operations always emit one SIB byte and four displacement bytes rather than host-dependent shortening. Local branches use deterministic near displacements found by bounded definition passes and create no WVO records. RIP-relative load, store, and address operations create `relative-i32` records over their four zero placeholder bytes. The two paging operations construct their fixed semantic instruction sequences from the same immutable byte primitives; `syscall`, `cpuid`, `read_tsc`, and `read_msr` construct their exact two-byte machine operations; `swap_gs` constructs `0x0F, 0x01, 0xF8`; and `interrupt_return` constructs `0x48, 0xCF`.

The measured length must equal the constructed length. A mismatch returns `WVA1011` with no object bytes. A successful result contains the complete WVO value plus exact section, symbol, relocation, and byte counts.

## Hosted boundary and report

The module declares `console.write_line`, `diagnostic.write_line`, `file.read_bytes`, `file.write_bytes`, `process.argument`, and `process.argument_count`. All must be explicitly supported and granted. With no arguments, `Main` runs embedded lexical, semantic, encoding, and rejection checks without reading or writing a hosted resource. With exactly two arguments, it reads `<source.wva>`, completes validation, measurement, and encoding in memory, and only then replaces `<output.wvo>` through the bounded host adapter.

A successful hosted run writes the object once, emits:

```text
wvasm 1
assembly status=valid object-bytes=<u32> sections=<u32> symbols=<u32> relocations=<u32> offset=<u32> line=<u32> column=<u32>
```

and returns `0`. Rejected input writes no object, sends the `assembly status=WVAxxxx ...` line to diagnostics, and returns `2`. Incorrect argument count writes `Usage: wvasm <source.wva> <output.wvo>` and returns `64`. Native resource failures remain stable runtime diagnostics and cannot expose a partially constructed in-memory object.

The current strict UTF-8 intrinsic returns validity rather than the first malformed offset, so invalid UTF-8 reports offset zero, line one, column one. Oversized source reports the first disallowed offset, 1,048,576, without walking hostile input.

## Qualification boundary

The conformance suite compares complete Windvale-written output with Stage 0 for the canonical object, the complete original statement set, all register spellings, representative low/extended REX/ModRM combinations, every condition code, forward and backward local fixups, stack/indirect control, RIP-relative symbols, every immediate ALU/test and rotate/shift family, signed multiply, no-index and indexed SIB addressing, all four scales, fixed displacement boundaries, and the REX.X distinction between no index and `r12`. It also compares malformed immediate, count, width, base, index, scale, displacement, target-kind, label, and section-context rejection. Every output passes the independently owned WVO verifier. Rejected diagnostic fixtures and mutations must invoke the writer zero times; native verifier paths additionally require no output file.

The module consumes the shared Foundation contracts for alignment, machine names, ordinal byte-span comparison, bounded unsigned decimal accumulation, and repeated immutable byte construction. It still owns WVA token spans, signed numbers, width rules, exact diagnostics, scanner, semantics, object measurement, and encoding; further extraction requires another demonstrated consumer rather than a broad split by file size. The separately owned linker consumes completed WVO bytes and owns multi-object resolution, layout, relocation application, map evidence, and final images rather than extending this assembler.

Decision 0521 transfers the native-equivalent execution block to the paired
digest-bound applications. On both Windows and Linux, the native front-door
owner now runs the no-argument self-test, constructs and independently verifies
the canonical 218-byte WVO, rejects invalid semantics without creating a new
destination or changing an existing one, and constructs the exact 91-byte
provider WVO consumed by the linker case. The broad Seed scripts retain hosted
capability refusal, missing-output-parent adapter failure, and live Stage 0
differential/oracle behavior because those are not native-equivalent execution
claims.
