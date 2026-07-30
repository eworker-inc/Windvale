# Windvale WVA assembler core

## Status and purpose

`Wvaˉassemblerˉcore` is the first complete Windvale-written implementation of WVA 1. It consumes immutable source bytes, applies the accepted source and line limits, validates the complete initial grammar and semantic model, derives section and definition ranges, encodes x86-64 instruction/data bytes, constructs canonical WVO 1.0 records, and returns one immutable object value. Portable scanning, validation, measurement, and encoding do not use host text parsing or file APIs; the hosted shell supplies explicit input/output resources only at the boundary.

The lexical scanner was first cross-host qualified at `e5fd109`, and the complete semantic inspector at `cc57bf9`. The object encoder and hosted write path described below are cross-host qualified at `a689617`: the exact committed archive passed the same 28-test suite and real CLI flow on Windows and Debian, and both hosts produced byte-for-byte identical assembler modules, WVO output, and normalized conformance contracts. The current module imports `Foundationˉmachineˉcontracts` for the shared alignment and whole-value machine-name rules; the composed WVB candidate SHA-256 is `5043d1b47686af72c04209875845021862dc664f960dae0a2c1d7babe051d842` with cross-host requalification pending.

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

The accepted statements are the complete initial WVA 1 set: `nop`, `return`, `trap`, `call`, `jump`, `move_i32`, `move_u32`, `bytes`, `u32`, `i32`, `address_u32`, and `zero`. Decimal parsing is performed over bytes with explicit checked arithmetic, including exact `i32` minimum and maximum and `u32` maximum boundaries.

The implementation stores no hidden cursor or host object. It uses repeated source passes and byte spans because current Seed has no general bounded collection module. Those passes are deterministic and bounded by WVA limits, but some name, definition-range, and symbol-index checks are quadratic in declaration count. The linker or representative-source evidence must revisit that tradeoff if it becomes impractical.

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

The encoder covers every initial instruction and data statement. Fixed-width integers use the Foundation little-endian constructors. `bytes` values use a canonical embedded 0-through-255 byte table plus a one-byte immutable slice, avoiding a new narrowing intrinsic solely for this bootstrap stage. Register move opcodes use the same table at `0xB8 + register index`. All relocation fields are four encoded zero bytes.

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

The conformance suite compares complete Windvale-written output with Stage 0 for the canonical object, the complete statement set, exact numeric boundaries, all eight move-register opcodes, multiple definition ranges, empty objects, both relocation kinds, LF/CRLF/CR input, and every accepted deterministic mutation. Every output passes the independently owned WVO verifier. Rejected diagnostic fixtures and mutations must invoke the writer zero times; native verifier paths additionally require no output file.

The module now consumes the first two-consumer Foundation contract for alignment and machine-name validation. It still owns WVA token spans, exact diagnostics, scanner, semantics, object measurement, and encoding; further extraction requires another demonstrated consumer rather than a broad split by file size. The separately owned linker consumes completed WVO bytes and owns multi-object resolution, layout, relocation application, map evidence, and final images rather than extending this assembler.
