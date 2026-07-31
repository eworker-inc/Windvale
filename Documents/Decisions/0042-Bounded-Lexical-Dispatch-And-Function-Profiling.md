# Decision 0042: Bounded lexical dispatch and function profiling

- Date: 2026-07-31
- Status: Qualified at `5d67463d42fc81ca82825da7c3894e16193391f7`

## Context

Decision 0041 removed one complete successful-path body traversal, but the exact ten-module typed-WVIR self-lowering input still reached the fixed 4,000,000,000-instruction ceiling. Wall-clock comparisons could not identify which portable functions consumed the remaining instructions, and the runtime exposed only the aggregate successful-run count.

An opt-in function-level profile of the retained compiler showed that lexical work dominated the ceiling. Before this slice, `Compilerˉlexˉnextˉvalidated`, whitespace classification, span-to-keyword comparison, identifier-start classification, identifier continuation, and keyword classification together accounted for about 3.1 billion of the four billion executed instructions. The repeated cost came from general helper paths used for ordinary ASCII token starts, identifier bytes, and impossible keyword candidates.

## Decision

Keep lexical semantics and all existing limits unchanged while making the hot decisions bounded by information already available to the lexer.

- Dispatch the 28 exact keywords by byte length and first ASCII byte before performing a full text comparison. Identifiers shorter than two bytes or longer than ten bytes cannot be keywords.
- Classify ASCII letters and underscore directly in the identifier-continuation path instead of calling the exported identifier-start helper for every byte.
- Call the complete Unicode whitespace classifier only for ASCII control/space bytes and UTF-8 leading bytes `194` or `225..227`, the only leading bytes that can begin a currently accepted whitespace scalar.
- Preserve strict UTF-8 preflight, all accepted Unicode whitespace, U+02C9 identifier continuation, source positions, token identities, failure ordering, and the existing source/token ceilings.
- Add opt-in per-function instruction collection to the C# reference runtime. `Readˉfunctionˉsteps` returns only functions that executed, ordered by descending count then function index.
- Add CLI switch `--report-function-steps`. It writes deterministic `Function instructions=<count> index=<index> name=<name>` lines to standard error after success or failure. The default runtime allocates no counter array and default command output is unchanged.
- Keep the 4,000,000,000-instruction experimental ceiling unchanged.

The expanded lexer demo checks all exact keywords, a near-keyword identifier for every dispatch bucket, identifier lengths outside the keyword range, and representative accepted two- and three-byte Unicode whitespace.

## Consequences

The original fixed lexer demo workload falls from 822,959 to 590,813 executed instructions, a 28.2% reduction with identical result. The focused typed-WVIR fixture falls from 8,074,045 instructions at the qualified Decision 0041 baseline to 5,735,695, a 29.0% reduction. The expanded lexer demo now executes 1,438,364 instructions because it deliberately adds the new dispatch and Unicode boundary cases, so its total is not a like-for-like performance comparison.

The real nine-module binding closure remains valid and completes in 2,972,056,275 instructions. Its updated source-derived counts are 945 locals, 8,335 reads, 612 assignments, 1,479 calls, and 65,704 directory bytes.

The exact ten-module typed-WVIR closure still reaches bounded diagnostic `WVR3011` at exactly 4,000,000,000 instructions. A retained-state profile now exposes repeated symbol-directory decoding and ordinal span comparison as the next major structural cost in addition to the remaining lexer entry cost. This decision therefore improves and measures the compiler; it does not claim self-hosting or typed-WVIR closure.

Two broader experiments are rejected from this slice:

- Compact signature evidence increased the focused fixture cost and did not complete the exact closure.
- Cached nominal ordinals plus an expanded symbol-match record improved the focused fixture by only 0.2%, still hit the exact ceiling, and made the retained-host run slower. Its private format and record-shape cost did not earn retention.

## Verification gate

The candidate must preserve every existing lexical, parser, symbol, binding, WVIR, WVB, diagnostic, and differential fixture; prove deterministic function counts with profiling both disabled and enabled; keep default CLI output stable; retain the fixed experimental ceiling; pass Standard and Qualification on Windows; and pass exact-commit Debian qualification with matching normalized reports and byte-identical portable artifacts.

Exact commit `5d67463d42fc81ca82825da7c3894e16193391f7`, tree `bee0468d61f7d700989b16ffabcc180a1e961b0d`, passed Standard and complete Qualification on Windows x64 and exact-archive Qualification on Debian GNU/Linux 12 x64. Both hosts completed zero-warning Release builds, all 48 tests, and the native CLI verifier. Their normalized contracts match, and all 61 portable artifacts are byte-identical. The complete evidence is recorded in [Seed verification evidence](../Project/Seed-Verification-Evidence.md#bounded-lexical-dispatch-and-function-profiling-qualification).

Completing the exact ten-module input below the ceiling remains the entry gate for the later compiler self-hosting milestone.
