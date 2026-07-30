# Decision 0008: WvDump payload decoding and safe reports

- Date: 2026-07-29
- Status: Accepted and qualified on Windows and Debian Linux at `a829fc8`

## Context

The envelope-only Windvale `wvdump` proved bounded binary walking and explicit hosted files, but it could not identify declarations or explain executable code. Implementing payload inspection in the C# shell would make the demonstration misleading: Windvale would control only the outer workflow while the host performed the useful parsing. Implementing a second semantic verifier in Windvale would create a competing execution gate and substantially increase the bootstrap loop.

Useful decoding also exposed four missing pure operations: signed little-endian `i32`, strict UTF-8 validation and decoding, safe deterministic quoting, and explicit widening of `u8` opcode tags to `u32` for range classification.

## Decision

- Advance the early-development bytecode contract to WVB 1.4 without a backward reader.
- Add `Bytesˉreadˉi32ˉlittle`, `Textˉutf8ˉisˉvalid`, `Textˉfromˉutf8`, `Textˉquote`, and `U32ˉfromˉu8` as pure typed Foundation intrinsics.
- Keep validation and reporting in `Wv-Dump-Core.wv`; the C# CLI supplies only declared arguments, bounded file bytes, and output adapters.
- Validate the complete envelope and every payload before emitting normal output.
- Decode all declaration representations and instruction widths, while leaving semantic execution safety to the mandatory reference verifier.
- Emit a versioned ASCII line report with fixed field order, LF endings, invariant numbers, and quoted untrusted names.
- Compare the complete hosted report for a real golden module in the portable Windows/Debian conformance contract.
- Treat the report as an early Seed contract that may advance deliberately with WVB; do not promise permanent compatibility yet.

## Consequences

- Windvale code now performs the useful module inspection rather than delegating it to Stage 0.
- Report consumers can compare exact bytes across hosts and parse one record per line without interpreting terminal controls or host locale.
- Strict UTF-8 behavior and signed decoding are portable bytecode semantics, not accidental .NET behavior.
- `Textˉquote` uses UTF-16 `\uXXXX` units, including surrogate pairs, because Windvale text currently follows the reference runtime's immutable string representation. This is deterministic but is not a normalization contract.
- WvDump rejects structurally malformed payloads cleanly, but acceptance does not imply executable validity; `windvale verify` remains mandatory before execution.
- Co-location remains large because Seed has no module/package composition. Splitting source by copying parser logic is still prohibited.

## Reconsider when

- Windvale gains module composition and the pure WVB decoder can become a reusable Foundation package.
- Report volume requires streaming, filters, or explicit truncation records.
- A stable public interchange format is required and warrants a compatibility policy independent from WVB.
- A self-hosted verifier is pursued as an explicit bootstrap milestone rather than growing accidentally inside the inspector.
