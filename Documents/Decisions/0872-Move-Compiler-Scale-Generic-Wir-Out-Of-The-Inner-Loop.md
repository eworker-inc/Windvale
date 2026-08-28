# Decision 0872: move compiler-scale generic WIR out of the inner loop

## Status

Accepted on 2026-08-28.

## Context

The language 1 development front door compiled
`Projects/Tests/Windvale-Native-Test-Language-1-Generic-Wir.wvproj` twice,
compared the two WVB products, verified one product, and ran it. The project
embeds the complete Windvale compiler source only so the resulting program can
analyze one small generic identity fixture.

The current admitted source is 2,401,137 bytes. Both the preceding analyzer and
the Decision 0871 candidate reach the native carrier's fixed `2^37`
instruction allowance with status `2` before publishing a WIR or WVB product.
The comparison therefore isolates a compiler-scale cost boundary rather than a
generic semantic regression. Repeating the same whole-compiler work twice in
every changed-file front door also delays feedback without producing reusable
evidence.

The replacement must not weaken generic discovery, deterministic-output, or
serialized-WIR coverage merely to make the verifier finish.

## Decision

The development front door uses the candidate analyzer directly against three
small permanent source fixtures: generic identity, multiple specialization,
and nested specialization discovery. Each case performs two independent
analyses and requires byte-identical source-set, manifest, binding, and WIR
artifacts. The verifier also decodes each WIR header and directory and checks
the exact expected function, block, operation, and operand counts. This is 12
exact artifact comparisons plus structural validation of all three products.

The compiler-scale project and its source fixture remain in the repository as
a named performance and self-hosting workload. They are not part of the normal
changed-file front door until the compiler completes the workload within an
explicit bounded contract. Merely raising the carrier instruction allowance is
not a performance improvement and does not qualify the workload.

The Windows front door removes four compiler-scale cases and changes from 486
to 482 cases. The Linux front door changes from 470 to 466 cases. The complete
registry remains 114 owners and contains 5,568 cases in 18,829 LF-only bytes at
SHA-256
`08944441716282739f74a1362511944e437a757204eb92d3640d2489e5037b39`.

## Consequences

- Ordinary development no longer performs two complete compiler analyses,
  two complete emissions, one large verification, and one execution to test a
  small generic rule.
- The new oracle tests nested discovery and exact intermediate artifacts more
  directly than the old final-product-only check.
- The retained compiler-scale workload continues to expose overall analysis
  cost and self-hosting readiness, but its current failure cannot block
  unrelated changed-file feedback indefinitely.
- The first complete front-door rerun exposed an independent stale route: the
  482,767-byte WVB runner was still sent through the monolithic native lowerer.
  Windows and Linux now use the existing segmented package composition for
  that runner. The WVB is byte-identical under the preceding and candidate
  compilers, and the segmented Windows application executes the floating
  fixture with result `42`; the native lowerer's 64 MiB safety bound is not
  raised.
- The same rerun exposed a stale fixed-array oracle. The fixture now carries
  four transitive type descriptors because its Collections dependency imports
  Memory. The compiler pair emits the same 916-byte WVB 1.22 product; the
  oracle derives array type index `3`, validates `Array<i32, 3>`, and retains
  all six valid, trap, and malformed-input checks.
- The collection type fixture now supplies the WVB 1.26 memory-budget entry
  required by its owned Vector parameter. The preceding and candidate
  compilers publish the same 978-byte module. Its focused type, runtime,
  sequence-read, and Vector-read/freeze oracles pass 35 cases; the independent
  low-level runtime remains a 1,156-byte WVB 1.20 product.
- Focused `using` and callable owners pass 18 and 59 cases. Their settled
  compiler-derived evidence identities are a deterministic 406,715-byte WVB
  at SHA-256 `f81e973ac7ee7f7e3929d6d587a44efd35ff10e497177104eda41d17b851c184`
  and a 4,392,237-byte evidence stream at SHA-256
  `88ffc45c1f6aaf8ec38866458368559c0b3a6a30d06c289de01178a4dea1354b`.
- This decision does not claim that whole-compiler self-analysis passes, that
  the candidate is promoted, or that Slice 7 qualification is complete.

## Reconsideration triggers

Return the compiler-scale workload to a routine gate only after it has a named
input, elapsed-time and memory limits, visible phase progress, and a qualified
completion threshold. Reconsider the direct oracle if generic catalog or WIR
serialization changes require additional exact fixtures or structural fields.
