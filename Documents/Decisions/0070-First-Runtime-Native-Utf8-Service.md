# Decision 0070: First runtime-native UTF-8 service

- Date: 2026-08-01
- Status: Implemented; cross-host qualification pending
- Refines: [Decision 0069](0069-Dynamic-Native-Text-And-Complete-Wvdump.md)'s ABI-10 service implementation
- Advances: The first ABI-10 service that executes without a managed callback or platform thunk

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

## Implementation evidence

The focused borrowed-bytes/native test builds the exact leaf twice, verifies its identity, rejects a corrupted byte, and executes 23 checked-in valid/invalid UTF-8 ranges through both the managed oracle and real Windows W^X code. The complete native `wvdump` dynamic-text test also passes through native strict conversion without the former delegate. The pre-commit Windows Standard pass builds Release with zero warnings and passes all 56 tests in 202.065 seconds; the unchanged Windvale OS suite passes all 15 tests.

Linux W^X execution, full Windows/Debian Qualification, portable-artifact comparison, and exact candidate identity remain pending. WVB/WVO and the Windvale OS image are expected to remain byte-identical because the change is confined to runtime-owned service code appended after the verified fragment.

## Consequences

One real runtime operation no longer crosses from generated machine code into a managed delegate or a platform calling convention. Windows and Linux use the same service bytes, making this a concrete dependency-removal pattern for other pure leaves.

Allocation-bearing enum names, integer formatting, concatenation, and quoting need a native text-arena ownership contract before they can follow. Hosted console, diagnostic, argument, and file services additionally require explicit native OS adapters. Those concerns remain separate.

## Reconsider when

- A Windvale-written/WVA service object can replace the Stage 0 byte builder while preserving exact bytes and recovery provenance.
- A self-contained native container owns service code and metadata outside the current in-process executor.
- SIMD validation is measured to matter and can retain deterministic bounded behavior plus a simple verified fallback.
- Descriptor provenance or host-buffer ownership changes require an explicit runtime range check inside the leaf.
