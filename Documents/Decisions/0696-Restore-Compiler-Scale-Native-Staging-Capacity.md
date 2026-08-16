# Decision 0696: Restore compiler-scale native staging capacity

- Date: 2026-08-16
- Status: Implemented with local Windows and Debian WSL2 execution evidence
- Advances: [Decision 0674](0674-Compiler-Scale-Project-Wvb-Checkpoint.md)
- Contracts: [source-to-WVB compiler](../../Specifications/Compiler-Source-Wvb.md), [native hosted-container packaging](../../Specifications/Windvale-Native-Hosted-Container-Packaging.md)

## Context

The optimized source-WVB reachability work added one analysis record to the
current compiler closure. The compiler build-driver WVB remained valid at
1,156,427 bytes, 519 functions, 155 data entries, six capabilities, and 89
nominal types, but 65 of those types were records. The native x64 runtime uses
one byte for nominal runtime identities and deliberately admits at most 64
records. Segmented staging therefore rejected the compiler as
`Unsupportedˉmodule` before function lowering.

This was not a package-size or digest failure. Raising a byte bound would not
make record identity 64 representable, while wrapping or silently aliasing the
65th identity would corrupt type safety.

The source encoder still carried a private two-field quotient/remainder record
from before Windvale implemented checked unsigned division and remainder. Its
only consumer split one Unicode scalar into base-64 groups for UTF-8 encoding.

## Decision

- Remove that obsolete private division record and its repeated-subtraction
  helper.
- Express UTF-8 base-64 partitioning with `u32 / 64u32` and `u32 % 64u32`.
  The divisor is a nonzero constant, all intermediate values retain the complete
  Unicode scalar domain, and the emitted UTF-8 byte contract is unchanged.
- Keep the public reachability evidence record and the native 64-record bound.
  Do not widen or wrap the one-byte nominal runtime representation for one
  compiler-private helper.
- Extend the paired segmented-compiler toolset reconstruction owner with one
  current-compiler staging case. Each host builds the exact project through its
  pinned native build driver into private temporary storage, requires the exact
  WVB identity, and invokes the reconstructed current-host staging producer.
- Do not execute, publish, cache as qualification evidence, or promote that
  private compiler WVB. The focused case owns source-build and staging capacity;
  general verifier, publisher, container, and front-door promotion remain
  independent gates.
- Select the staging owner whenever the source-WVB encoder, temporary-slot
  allocator, compiler build-driver root/project, or source-WVB product manifests
  change.

## Evidence

The resulting compiler build-driver WVB is 1,153,758 bytes at SHA-256
`31c7e5292f607b3b88153ef64a14c64bae438fcfbca75b8495ffba2a5cb991bb`.
It contains 88 nominal types: exactly 64 records and 24 enums. The existing
native staging producer accepts it and reports 30,148,873 object bytes across
39 chunks with a 492-byte manifest.

The paired four-case reconstruction owners build the same WVB and require the
same staging report on Windows and Debian WSL2. Their local elapsed times are
305,710 milliseconds on Windows and 341,000 milliseconds on Debian WSL2. After
the final upstream rebase, the 26-case seed owner also passed in 8,580
milliseconds on Windows and 105,000 milliseconds on Debian WSL2. These are
operational measurements, not portable thresholds or release qualification.

## Consequences

- Decision 0674's next recorded `Unsupportedˉmodule` boundary is closed without
  increasing memory, record, function, file, or chunk limits.
- UTF-8 encoding now consumes the language's already implemented division and
  remainder semantics in a production compiler path.
- Future source-compiler record growth still fails at the explicit native bound
  and selects a focused owner that exercises the complete current compiler.
- Full compiler packaging is not yet promoted. The ordinary verifier and
  publisher still have narrower pinned envelopes, and the independent metadata
  package migration still requires those front-door gates.

## Reconsideration triggers

Reconsider when a general native value representation can widen nominal
identities coherently across interpreter, verifier, JIT/AOT, ABI, malformed
input, and cross-host evidence, or when the compiler project no longer needs a
segmented bootstrap path. Do not reintroduce a nominal helper for a scalar pair
unless a measured consumer requires identity rather than bounded values.
