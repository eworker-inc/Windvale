# Decision 0077: First Windvale-owned native stencil

- Date: 2026-08-01
- Status: Implemented candidate; cross-host qualification pending
- Extends: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)'s copy-and-patch direction
- Preserves: Native ABI 14, execution-context version 6, service-table version 4, WVB 1.6, WVO 1.0, kernel bridge 9, and firmware probe 16

## Context

Decision 0076 qualifies native leaves for all eleven runtime service slots but leaves C# reconstructing every leaf. The next ownership step needs to be small enough to validate independently, active in ordinary execution, and byte-identical to already-qualified behavior. The five-byte `process.argument_count` leaf is the smallest live artifact with one meaningful execution-context offset.

## Decision

- Define the first deliberately bounded `WVSP 1` patch record in [the WVA native-stencil specification](../../Specifications/Wva-Native-Stencil.md).
- Author the five machine bytes and one typed patch record in `Compiler/Native/Stencils/Process-Argument-Count.wva`.
- Retain the canonical 166-byte WVO produced by the Windvale-written WVA assembler as an embedded native-compiler resource. Its SHA-256 is `e2057943b9c79e10a432ea20a77da5ed0a261e3effdd36511cbb34e77e55c10b`.
- Require the native owner to decode and verify the WVO, exact section and symbol layout, patch metadata, immutable opcode shell, and zero-valued hole before instantiation.
- Patch only the checked one-byte execution-context argument-count offset. The resulting five bytes remain the ABI-14 leaf with SHA-256 `2358e7e2c72d6476cfe05134db4f0eb5e6987fcca1b10894a8588a28d3929829`.
- Route the live `X64ˉnativeˉargumentˉservices.Build(Processˉargumentˉcount)` path through the embedded Windvale-assembled artifact. The native executor therefore consumes this construction during ordinary hosted-input execution.
- Keep the C# Stage 0 assembler as the independent byte oracle and the C# native owner as the bounded loader, patch applier, final identity verifier, W^X publisher, and executor for this slice.

## Evidence boundary

The focused conformance case compiles and runs the Windvale-written assembler twice over the committed WVA source, compares the two WVO results byte for byte, compares them with the Stage 0 oracle, validates the exact object, instantiates the patch, and compares the result with the qualified leaf size and digest. It also rejects a wrong patch kind, changed fixed opcode, mismatched requested kind, and nonzero patch hole.

The focused test and the existing real-file native hosted-input test pass on Windows with a zero-warning build. Full same-commit Windows and Debian qualification remains required before this candidate becomes qualified.

## Consequences

This is the first active native-runtime artifact whose machine template and patch description are owned by Windvale assembly rather than a C# byte literal. It validates the copy-and-patch construction pattern without changing program semantics, native ABI, WVB, WVO, service-table layout, or final machine bytes.

It is not a general baseline JIT and does not retire .NET. C# still loads the embedded resource, validates and applies the patch, constructs all other leaves and tables, publishes W^X memory, owns arenas, invokes native code, maps traps, packages the OS image, and remains the independent reference/recovery implementation.

## Reconsider when

- A second measured stencil needs multiple patches, wider values, branches, calls, data references, or architecture selection.
- Loading a canonical WVO resource materially harms startup or package size compared with an independently verifiable generated representation.
- Windvale-owned object loading and patch application can replace the bounded C# consumer without losing recovery evidence.
