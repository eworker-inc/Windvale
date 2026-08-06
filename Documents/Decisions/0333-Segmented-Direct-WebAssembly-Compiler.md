# Decision 0333: Segmented direct WebAssembly compiler

Status: Accepted

Date: 2026-08-06

## Context

The browser playground previously executed the 919,577-byte Windvale compiler WVB inside the scalar interpreter Wasm. That route was .NET-free but required about 1.4 billion outer interpreter instructions and roughly 64 to 105 seconds per small source compilation. Direct lowering of the complete compiler was already semantically available, but one immutable `bytes` result is bounded to 4 MiB and the compiler-scale WebAssembly artifact is larger than that bound.

Seed records permit 64 fields while the typed direct-lowering stack deliberately admits at most 36 live values. A segment protocol therefore cannot use an unbounded or 54-field return record. The artifact must also remain reconstructable without host imports, mutable shared state, or a server compiler.

## Decision

Add a fixed 34-segment artifact contract carried by a 36-field record: validity, aggregate length, and 34 byte segments. Segment zero owns the WebAssembly prefix, segment 33 owns the data section, and 32 code segments are packed from 16-function lowering batches according to actual emitted byte length. Every segment is bounded to 4 MiB and the reconstructed artifact is bounded to 64 MiB. The ABI-4 wrapper publishes the record as a 288-byte output-kind-3 manifest containing the aggregate length and 34 descriptor slots.

Pin the import-free segmented generator WVB and Wasm under `Artifacts/WebAssembly-Segmented-Backend`. Normal direct-compiler publication runs that generator in Node.js and verifies all identities, imports, ABI globals, ranges, segment bounds, aggregate length, and reconstructed WebAssembly before an atomic publication. Reconstructing the pinned generator Wasm remains an explicit Stage 0 recovery operation; it is not part of normal build, verification, deployment, or browser execution.

The browser package consumes the resulting direct compiler Wasm through ABI 4 output kind 1. The small scalar interpreter Wasm remains only for admitting and executing the compiler-produced WVB. Compilation and execution stay inside a disposable client worker with no host imports, server compiler, Blazor, or .NET runtime.

## Consequences

- The complete compiler publishes as 18,349,927 import-free WebAssembly bytes with SHA-256 `05dcee4e37cdd8db2e7321b01f0b9cde4d13662ba1f154830c95fda753b825e8`.
- On the Windows development host, Node.js compiled the Wasm module in 15.5 ms, instantiated it in 0.9 ms, and compiled the pinned browser sample to the exact 183-byte WVB in 991.3 ms.
- Compiler-artifact generation is still an 88-second maintenance operation, but it is removed from the browser request path and does not affect playground users.
- The direct compiler is a larger cached static asset and currently reserves a fixed 2,497-page WebAssembly memory. Later work may reduce download and memory cost without changing the source, WVB, or segment semantics.
- The segment count and bounds are versioned publication contracts, not a general replacement for Windvale collection semantics.

## Reconsideration triggers

Reconsider this decision when the ordinary pinned direct backend can self-host the segmented generator, when compiler output can stream through a verified bounded builder contract, when a smaller direct compiler artifact preserves exact output, or when measured browser memory compatibility requires a lower fixed-memory profile.
