# Decision 0070: First runtime-native UTF-8 service

- Date: 2026-08-01
- Status: Qualified on Windows and Debian x64
- Refines: [Decision 0069](0069-Dynamic-Native-Text-And-Complete-Wvdump.md)'s ABI-10 service implementation
- Advances: The first ABI-10 service that executes without a managed callback or platform thunk
- Advanced by: [Decision 0355](0355-Windvale-Owned-Native-Utf8-Service-Construction.md)

## Context

ABI 10 cross-host qualifies the complete Windvale-written `wvdump`, but its runtime-service table is still populated by C# delegates behind Windows/System V argument adapters. Removing all services at once would mix pure validation, text allocation, nominal metadata, host I/O, process arguments, and files into one risky migration.

Strict UTF-8 validation is the smallest useful leaf. It is deterministic, allocation-free, capability-free, and already receives a verifier-proven immutable byte range plus a verified Boolean result cell. Its accepted byte language is fully defined and independent of locale, host libraries, and operating-system APIs.

## Decision

- Keep target `x86-64-wvb-baseline-v10`, native ABI 10, execution-context version 2, service-table version 4, the existing `Textˉutf8ˉisˉvalid` slot, WVB, and WVO unchanged.
- Replace only the managed UTF-8 validation delegate and its Windows/System V adapter with one identical 800-byte x86-64 runtime leaf on both hosts.
- Retain the internal service convention: `R8` is the proven input address, `R9D` is its bounded length, and `RCX` is the verified Boolean output cell. The leaf writes normalized zero or one, returns status zero in `EAX`, and does not change instruction counter `R11`, call-depth counter `R10`, or context owner `R15`.
- Accept ASCII; canonical two-byte sequences from `C2 80` through `DF BF`; three-byte sequences excluding overlong encodings and UTF-16 surrogates; and four-byte sequences from `F0 90 80 80` through `F4 8F BF BF`. Reject stray continuations, `C0`/`C1`, `F5` through `FF`, truncation, bad continuations, overlong forms, surrogates, and values above `U+10FFFF`.
- Continue relying on the fragment verifier's descriptor provenance and the execution owner's immutable allocation boundary. Verified code cannot manufacture a pointer or length for this service; argument/file callbacks publish only execution-registered immutable buffers.
- Build the leaf with one bounded label/relative-branch builder, then require exact identity before W^X publication: 800 bytes with SHA-256 `4c3d2e370d62c8d2f54a3c453f39b94cf46ddabd6db3c2f3d6b65f0713b68aaf`.
- Preserve the managed strict decoder in the reference runtime as the semantic oracle. Differential tests cover every lead-byte family, minimum/maximum scalar boundaries, all exclusion boundaries, deterministic reconstruction, and corrupt native-service identity.
- Do not describe this as a Windvale-written native runtime. C# still constructs and publishes the leaf, owns executable memory and buffers, supplies every other service, and remains the recovery/reference implementation.

## Qualification evidence

The focused borrowed-bytes/native test builds the exact leaf twice, verifies its identity, rejects a corrupted byte, and executes 23 checked-in valid/invalid UTF-8 ranges through both the managed oracle and real Windows W^X code. The complete native `wvdump` dynamic-text test also passes through native strict conversion without the former delegate. The pre-commit Windows Standard pass builds Release with zero warnings and passes all 56 tests in 202.065 seconds; the unchanged Windvale OS suite passes all 15 tests.

Exact commit `53cee69bdd24e96bb42521d53e9d55b0c59c5876`, tree `2e6aa434d5956be489fa24f3df8da0e9fc25ec9d`, was archived as 2,877,521 bytes with SHA-256 `de6810995b67e7f8eb678f0ca228ca1c774c1b1ce901c104f8f42646bda55133`. Windows and Debian GNU/Linux 12 x64 with .NET SDK `10.0.302` pass zero-warning Release builds, all 56 tests, and the complete CLI verifier from independently extracted copies. The suite takes 214.004 seconds on Windows and 229.082 seconds on Debian. Their normalized reports match exactly.

All 61 portable artifacts, totaling 7,752,647 bytes, remain byte-identical with Decision 0069's retained canonical manifest SHA-256 `11ac1d4a57fce3648004d7a6002e6124d6e2fbeefc108b31bfe305523b2de0de`. The retrieved 2,297,339-byte Debian evidence bundle has SHA-256 `3f24b92447ea6f124a2fa1986beabaa52a623de7127a0595b647aa2629c2738c`. Both hosts pass all 15 OS tests. Pinned QEMU 11.0/Q35/TCG on Windows boots the unchanged exact 15,872-byte probe-12 image with SHA-256 `3010bc72b9c26386f062f78481c900cac841321b040b41447a0bbb65a9e392fe`; the Debian QA host does not provide QEMU. GitHub's independent Verify workflow also passes for the exact candidate. After evidence retrieval, the resolved exact QA directory, transferred archive, evidence bundle, and transient log were removed and confirmed absent.

## Consequences

One real runtime operation no longer crosses from generated machine code into a managed delegate or a platform calling convention. Windows and Linux use the same service bytes, making this a concrete dependency-removal pattern for other pure leaves.

Allocation-bearing enum names, integer formatting, concatenation, and quoting need a native text-arena ownership contract before they can follow. Hosted console, diagnostic, argument, and file services additionally require explicit native OS adapters. Those concerns remain separate.

## Reconsider when

- A Windvale-written/WVA service object can replace the Stage 0 byte builder while preserving exact bytes and recovery provenance.
- A self-contained native container owns service code and metadata outside the current in-process executor.
- SIMD validation is measured to matter and can retain deterministic bounded behavior plus a simple verified fallback.
- Descriptor provenance or host-buffer ownership changes require an explicit runtime range check inside the leaf.
