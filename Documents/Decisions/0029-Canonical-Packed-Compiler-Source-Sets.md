# Decision 0029: Canonical packed compiler source sets

- Date: 2026-07-30
- Status: Accepted and implemented; cross-host qualification pending

## Context

The qualified lexer and parser consume one immutable source value. Semantic binding must additionally resolve imports and named declarations across the root and as many as 63 supplied dependencies. Seed has no general collection of `bytes`, and passing native paths, host objects, callbacks, or ambient filesystem state into the portable binder would make host behavior part of compiler semantics.

Repeatedly invoking a single-source binder from C# would also put import resolution and source-set ordering in a permanent host path. Adding general arrays, a heap, or a mutable compiler arena before the binder has proven their precise ownership requirements would enlarge the bootstrap loop substantially.

## Decision

Introduce Windvale Source Set 1 (`WVSS 1`), a compiler-owned canonical packed byte contract. Entry zero is the root source. Remaining entries are dependencies in strict ordinal order by declared module-name UTF-8 bytes. The envelope contains a fixed header and an eight-byte `(offset, length)` directory for random entry access; source payloads follow contiguously with no gaps, overlaps, padding, or trailing bytes.

The portable `Compilerˉsourceˉset` module scans the untrusted envelope before slicing any source. It then validates every source through the qualified declaration and body parsers, rejects duplicate module names, enforces canonical dependency order, and preserves the accepted dependency profile/shape rule: imported sources are portable, contain no capability or data declarations, and export every function.

The current packed value is bounded by Seed's existing 4 MiB immutable-`bytes` ceiling and contains one through 64 nonempty sources. This is sufficient for the real bootstrap frontend set and avoids changing WVB or runtime memory semantics merely to begin binding. It is not the final answer for Stage 0's existing 16 MiB aggregate source-character ceiling. Bootstrap closure requires either a proven larger immutable-value contract, a bounded source collection/arena, or a portable multi-buffer interface before the Windvale compiler can claim the complete Stage 0 input envelope.

Do not put native paths or timestamps in WVSS. A hosted shell may read explicit argument resources, rely on the first-read snapshot contract, construct the canonical value, and pass it to portable compiler logic. Import resolution, cycle/reachability validation, symbol binding, diagnostics, WIR, and WVB production remain compiler phases above this source-container boundary.

## Consequences

Windvale now has the first collection introduced directly by self-hosting pressure: a narrow, immutable, compiler-owned packed source set rather than a speculative general container library. A binder can revisit any module deterministically by index while remaining independent of Windows, Linux, native paths, and host collection iteration.

The source-set validator intentionally repeats bounded source/header passes. The real five-module compiler frontend set demonstrates that this remains practical, but its cost is now measurable. Semantic import and symbol passes may justify a packed index or arena; any generalization must remove demonstrated duplication or close the 16 MiB parity gap rather than merely rename WVSS as a general collection.

The cross-value ordinal comparison remains local to the compiler module because its two source spans reside in different immutable values. Decision 0021's Foundation operation is deliberately same-value and optimized for its assembler/linker consumers. A future shared cross-value operation requires another concrete consumer or measured compiler pressure.

## Verification gate

The exact candidate must pass the complete conformance and native CLI verifiers on Windows and Debian. Coverage includes every header field, truncation, bad magic/version/count/directory, noncanonical offsets, zero and oversized lengths, trailing bytes, entry access, strict source/body failures, duplicate names, dependency order/profile/shape/private-function failures, the exact 64-module boundary, and hosted snapshot reuse.

Both hosts must produce identical source-set core, demo, and tool WVB files. The tool must validate the real five-module frontend source set with identical counts and source bytes, and normalized conformance reports must match while every previously qualified direct artifact retains its identity.
