# Windvale WVA scanner core

## Status and purpose

`Wvaˉscannerˉcore` is the first Windvale-written implementation slice of WVA 1. It consumes immutable source bytes, applies the accepted source and line limits, normalizes the accepted line endings, recognizes the exact version header, and exposes deterministic token-boundary evidence. Its portable functions do not use host text parsing or file APIs; the hosted shell supplies one explicit resource and prints a versioned report.

The scanner and its hosted report are cross-host qualified at `e5fd109` from one exact source archive on Windows x64 and Debian Linux x64.

This is a lexical foundation, not yet the Windvale assembler. It does not classify declarations or statements, validate names or numeric widths, resolve symbols, derive definition ranges, encode instructions, or emit WVO.

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

Horizontal whitespace and a trailing comment are allowed under the general WVA line rules. A missing meaningful line returns `missing_header`; any wrong, missing, or additional header word returns `bad_header`. Later words are counted but deliberately remain unclassified until the semantic-inspector gate.

## Scan result

`Wvaˉscan` contains:

- status;
- total source bytes;
- physical lines;
- meaningful lines;
- word-token count;
- failure or terminal byte offset;
- failure or terminal line and column.

Statuses are `valid`, `source_too_large`, `invalid_utf8`, `line_too_long`, `missing_header`, and `bad_header`. The ASCII report is:

```text
wvascan 1
status=<status> bytes=<u32> lines=<u32> meaningful-lines=<u32> tokens=<u32> offset=<u32> line=<u32> column=<u32>
```

Invalid scans write only the status line to diagnostics and return `2`. A valid hosted scan writes both lines to normal output and returns `0`. Incorrect argument count writes `Usage: wvascan <source.wva>` to diagnostics and returns `64`.

The current strict UTF-8 intrinsic returns validity rather than the first malformed offset, so `invalid_utf8` reports offset zero, line one, column one. The oversized-source status reports the first disallowed offset, 1,048,576, without walking hostile input.

## Hosted boundary and self-tests

The module declares `console.write_line`, `diagnostic.write_line`, `file.read_bytes`, `process.argument`, and `process.argument_count`. All must be explicitly authorized and supported before execution. With no program arguments, the module runs embedded tests without reading a file. Those tests cover comments, tabs, LF, CR, valid and extra header words, a missing header, invalid UTF-8, and a 4,097-byte line.

The host conformance suite additionally covers the exact 4,096-byte boundary, the 1 MiB source boundary, CRLF, capability refusal, the canonical `Hello-Object.wva`, deterministic module bytes, and a real CLI file scan on Windows and Debian.

## Replacement path

The next gate reuses these pure byte-cursor contracts for multi-pass declaration, section, definition, statement, name, ordering, integer, and reference validation. Object encoding follows only after accepted/rejected source classifications agree with the Stage 0 oracle. If real parser work proves that immutable repeated passes are impractical, the project may introduce a bounded collection or scanner module facility through a recorded decision; it must preserve this observable WVA contract and cross-host evidence.
