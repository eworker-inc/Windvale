# Windvale WVA scanner and semantic core

## Status and purpose

`Wvaˉscannerˉcore` is the Windvale-written frontend foundation for WVA 1. It consumes immutable source bytes, applies the accepted source and line limits, normalizes accepted line endings, recognizes the exact version header, validates the complete initial WVA grammar and semantic model, and exposes deterministic structural evidence. Its portable functions do not use host text parsing or file APIs; the hosted shell supplies one explicit resource and prints versioned scan and semantic reports.

The lexical scanner was first cross-host qualified at `e5fd109`. The complete scanner and semantic inspector are cross-host qualified at `cc57bf9` from one exact source archive on Windows x64 and Debian Linux x64; both hosts produced the same verified bytecode module and observable semantic classifications.

This is a semantic inspector, not yet the Windvale assembler. It classifies declarations and statements, validates names and numeric widths, resolves declarations and references, and derives aggregate section/data/memory/relocation counts. It deliberately does not construct encoded definition ranges, instruction bytes, symbol records, relocations, or WVO output; those belong to the following encoder gate.

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

The implementation stores no hidden cursor or host object. It uses repeated source passes and byte spans because current Seed has no general bounded collection module. Those passes are deterministic and bounded by WVA limits, but some name/definition checks are quadratic in declaration count. The object-encoder and linker work must revisit that tradeoff if representative sources or qualification runtime make it impractical.

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

## Scan result

`Wvaˉscan` contains:

- status;
- total source bytes;
- physical lines;
- meaningful lines;
- word-token count;
- failure or terminal byte offset;
- failure or terminal line and column.

Scan statuses are `valid`, `source_too_large`, `invalid_utf8`, `line_too_long`, `missing_header`, and `bad_header`. A successfully scanned input receives both report records:

```text
wvascan 1
status=<status> bytes=<u32> lines=<u32> meaningful-lines=<u32> tokens=<u32> offset=<u32> line=<u32> column=<u32>
semantics status=<valid|WVA1001..WVA1011> sections=<u32> symbols=<u32> definitions=<u32> relocations=<u32> data-bytes=<u32> memory-bytes=<u32> offset=<u32> line=<u32> column=<u32>
```

An invalid lexical scan writes its `status=` line to diagnostics and returns `2`. An invalid semantic inspection writes only its `semantics status=` line to diagnostics and returns `2`. A valid hosted inspection writes all three lines to normal output and returns `0`. Incorrect argument count writes `Usage: wvascan <source.wva>` to diagnostics and returns `64`.

The current strict UTF-8 intrinsic returns validity rather than the first malformed offset, so `invalid_utf8` reports offset zero, line one, column one. The oversized-source status reports the first disallowed offset, 1,048,576, without walking hostile input.

## Hosted boundary and self-tests

The module declares `console.write_line`, `diagnostic.write_line`, `file.read_bytes`, `process.argument`, and `process.argument_count`. All must be explicitly authorized and supported before execution. With no program arguments, the module runs embedded tests without reading a file. Those tests cover comments, tabs, LF, CR, valid and extra header words, a missing header, invalid UTF-8, a 4,097-byte line, a valid semantic body, an unresolved reference, a statement in the wrong section, and a bad semantic header.

The host conformance suite additionally covers the exact 4,096-byte boundary, the 1 MiB source boundary, CRLF, capability refusal, the canonical `Hello-Object.wva`, the complete initial statement set, exact integer limits, all eleven diagnostic families, deliberately hostile structure/reference cases, 200 deterministic source mutations compared with Stage 0, deterministic module bytes, and real CLI inspection on Windows and Debian.

## Encoder path

The next gate reuses these validated byte spans and statement classifications to derive definition offsets/sizes, append exact instruction and data bytes, construct canonical section/symbol/relocation records, and emit WVO 1.0. It qualifies only when every canonical fixture is byte-for-byte identical to Stage 0 and passes the independently owned WVO verifier. A later refactor may split lexical, semantic, and encoding facilities into modules when the language gains an import/module-composition contract; it must preserve this observable WVA contract and cross-host evidence.
