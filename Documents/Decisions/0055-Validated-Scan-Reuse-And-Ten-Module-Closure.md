# Decision 0055: Validated scan reuse and ten-module closure

- Date: 2026-07-31
- Status: Qualified on Windows and Debian at `1a4fca7e295545b3b815bbf187fc048f1a885c74`

## Context

Decision 0050 removed repeated nominal reranking and most packed-directory materialization, but the exact ten-module typed-WVIR workload still exhausted the fixed 4,000,000,000-instruction acceptance ceiling. Per-function profiling showed that the remaining cost was dominated by repeated lexical and declaration/body traversal. The compiler often rechecked cursor shape after advancing from an already accepted token, reparsed declarations before parsing bodies, and tokenized complete function bodies merely to locate their closing braces.

The acceptance ceiling is part of the engineering pressure, not a value to raise until the implementation happens to pass. The next slice therefore had to remove repeated proven work while preserving strict UTF-8 validation, public checked entry points, deterministic failures, nesting containment, independent packed-evidence validation, and all existing semantic and serialization results.

## Decision

Retain checked standalone boundaries and add explicit internal boundaries for callers that already possess the required evidence:

- `Compilerˉlexˉnextˉvalidated` continues to check cursor shape and then delegates to `Compilerˉlexˉnextˉafterˉscan`. Compiler loops use the latter only after a complete source scan or a validated parser/symbol boundary established the cursor.
- The checked whitespace and identifier-continuation helpers retain their original signatures. Internal variants accept the already known total byte length, and the lexer directly classifies its common ASCII paths without temporary records.
- Source-set validation passes its existing declaration summary to `Compilerˉparseˉsourceˉbodiesˉfromˉdeclarations` instead of parsing all declarations a second time.
- Declaration parsing uses a bounded byte scanner to find the end of a lexically validated function body. It tracks braces, line comments, escaped strings, lines, columns, and the 64-level block limit. Ambiguous or rejected shapes fall back to the original token-based scanner so established diagnostics remain authoritative.
- The checked `Compilerˉparseˉbodyˉspan` performs the iterative block-shape preflight before recursive statement parsing. This contains deliberately over-deep input as `Nestingˉlimit` instead of allowing host-stack exhaustion. Already validated compiler paths do not repeat that preflight.
- Nominal lookup uses the existing private WVSI reverse canonical table. It restricts record and enum searches to the requested first-byte range, rejects unequal lengths before ordinal comparison, and preserves record-then-enum canonical identity.

Frequency-ordered whitespace, identifier, and punctuation dispatch is retained where measurement showed a reduction. The language grammar, accepted Unicode whitespace, identifier alphabet, token values, public WVSS/WVSD/WVLB/WVIR/WVB formats, diagnostics, and generated semantic outputs do not change.

## Rejected alternatives

A binary search over canonical nominal names was implemented and measured. With the compiler's long shared `Compilerˉ` name prefix, repeated midpoint ordinal comparisons cost more than the bounded first-byte range scan, so the experiment was removed.

A helper returning a record for each identifier-tail step and an earlier broad lexer-helper inlining experiment also increased the focused instruction count. The accepted lexer keeps only the measured scalar/direct branches and avoids allocating a result record in its hot identifier loop.

The exact workload is not moved into Fast or Standard verification. It remains a deliberate milestone experiment because one run takes several minutes and its portable instruction count, not host elapsed time, is the acceptance evidence.

## Consequences

The focused typed-WVIR fixture falls from Decision 0050's 5,715,847 instructions to 3,626,693, a reduction of 2,089,154 instructions or 36.55%. Its semantic result and independently validated WVIR bytes remain unchanged.

The exact ten-module compiler input now completes at 3,912,239,584 instructions, leaving 87,760,416 instructions of margin under the unchanged ceiling. It reports:

```text
source wir status=Valid modules=10 functions=315 blocks=5658 operations=27377 temporaries=25094 operands=21869 directory-bytes=1720804
Result: 0
```

This completes the exact typed-WVIR performance entry gate. It does not yet prove that the Windvale compiler compiles itself to WVB, that Stage 1 and Stage 2 are identical, that the 4 MiB WVSS envelope matches Stage 0's 16 MiB source envelope, or that Phase 8 is complete.

## Verification

Before the exact-commit qualification, the candidate passed the focused 24-test compiler area, focused nesting-containment and golden-contract reruns, and the complete 48-test Windows Standard suite. The exact ten-module run completed successfully at the instruction count above.

Exact candidate commit `1a4fca7e295545b3b815bbf187fc048f1a885c74`, tree `00eef5249581f48e57e20a92b4524af1e2b54420`, was archived as `windvale-scan-reuse-1a4fca7e2955.tar.gz`, 2,744,521 bytes with SHA-256 `767906a4f4e114c595ae92bfab3dbb6caf1914dc29ec25b213eec6413c092158`. The same digest was verified on the isolated Debian QA host. Windows x64 and Debian GNU/Linux 12 x64 with .NET SDK `10.0.302` both completed zero-warning Release builds, all 48 tests, and the complete native verifier.

Windows Qualification completed in approximately 449.0 seconds with a 220.663-second suite; Debian completed in approximately 469.5 seconds with a 227.434-second suite. The 15,563-byte Windows report has SHA-256 `24a1b6eb0096e9fba642b0f1284f287e3d43c1d0e4a49f27157b71bae04f7efa`; the 15,473-byte Debian report has SHA-256 `2fa2b75b8e6def4183925850fb0a4df8b9da751e0d966f147b5609e1afc3f4f3`. Their normalized contracts matched exactly with canonical SHA-256 `3e0d97b8a3d68c150545f69e7c7761be61bd39443dde27b83d8288f9e7dd93d5`.

All 61 directly retrieved portable artifacts, totaling 7,733,603 bytes, matched Windows byte for byte. The Debian evidence bundle was 2,287,803 bytes with SHA-256 `7aa03b7145ca5777ab646d4a7043ea4c391ecf81638fea7a32f1722b6e2bb760`. After retrieval and comparison, the exact Debian QA directory, source archive, and evidence bundle were removed and confirmed absent. This qualifies the performance entry gate without claiming compiler self-hosting or Stage 1/Stage 2 convergence.
