# Decision 0077: First Windvale-owned native stencil

- Date: 2026-08-01
- Status: Qualified at exact commit `da593126980d19aecacc354591eb888edc5da2c5`
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

Exact commit `da593126980d19aecacc354591eb888edc5da2c5`, tree `1d1d7d3866bb0e41ecf9426a2a875a98766dc793`, passes zero-warning Windows and isolated Debian GNU/Linux 12 x64 Qualification under .NET SDK `10.0.302`. Both hosts pass all 59 integrated Seed tests, including the concurrently added Windvale project-manifest parser, then complete the native CLI/reproduction gate. Their in-process suite times are 253.312 and 266.554 seconds; complete Qualification takes 499.750 and 512.050 seconds wall-clock. The stencil case takes 0.912 and 0.881 seconds, and the live native hosted-input case takes 0.174 and 0.104 seconds.

The 15,563-byte Windows report has SHA-256 `c34a2199e548631323b2186dda0dcf8ffcb0a3a3c6eb7d53d9a405c314837a4b`; its 12,398-byte timing report has SHA-256 `f7fc8fa08fe090d5bea3ce4bc03bf084ad975af1d403f5c78fa2e8fd9cddc56b`. The 15,473-byte Debian report has SHA-256 `0a8116b03185d7344dd47fb0996c1cc9402c3b9583522574a2a77b0e2fa1f5cf`; its 11,990-byte timing report has SHA-256 `04c612c9fe6fbc83994428da34d33afab2024cb5f995cf7f1e8d1eccca7fd1d3`. Their normalized contracts match exactly. All 61 established portable artifacts, totaling 7,752,647 bytes, match byte for byte and retain canonical manifest SHA-256 `11ac1d4a57fce3648004d7a6002e6124d6e2fbeefc108b31bfe305523b2de0de`.

The exact 2,944,228-byte source archive has SHA-256 `b547c5adb8377052c0df1db8262fd937862f7678c3e2eb198c8aa987e7010812`; the retrieved 2,585,001-byte Debian evidence bundle has SHA-256 `4c41d6b711e16b7941c2fe77e9fc278b1ca782e319fa3273442d45895c9d863d`. Both hosts pass all 15 OS tests. Pinned QEMU 11.0/Q35/TCG reproduces the unchanged 15,872-byte firmware-probe-16 image with SHA-256 `206a036f8cbe3198544b6878bf52c80ef8d489c14d5437c6c7004ff1d6599504`, emits the complete success transcript, and returns guest-controlled host exit code 1.

One earlier detached Windows launcher was discarded after its default console decoding replaced a macron in otherwise successful CLI output. The exact UTF-8-configured rerun passed the complete unmodified verifier with zero stderr. This was launcher evidence, not a Windvale semantic or artifact failure.

GitHub [Verify run 30706925533](https://github.com/eworker-inc/Windvale/actions/runs/30706925533) passes its independent classification, Windows, and Linux jobs for the exact commit. After report and bundle retrieval, both exact candidate QA directories, both transferred archives, and both remote evidence bundles were removed and confirmed absent.

## Consequences

This is the first active native-runtime artifact whose machine template and patch description are owned by Windvale assembly rather than a C# byte literal. It validates the copy-and-patch construction pattern without changing program semantics, native ABI, WVB, WVO, service-table layout, or final machine bytes.

It is not a general baseline JIT and does not retire .NET. C# still loads the embedded resource, validates and applies the patch, constructs all other leaves and tables, publishes W^X memory, owns arenas, invokes native code, maps traps, packages the OS image, and remains the independent reference/recovery implementation.

## Reconsider when

- A second measured stencil needs multiple patches, wider values, branches, calls, data references, or architecture selection.
- Loading a canonical WVO resource materially harms startup or package size compared with an independently verifiable generated representation.
- Windvale-owned object loading and patch application can replace the bounded C# consumer without losing recovery evidence.
