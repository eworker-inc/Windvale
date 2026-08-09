# Decision 0462: Native WVHV runtime-header ownership

- Status: Implemented current-host candidate; native process composition and dual-host promotion pending
- Date: 2026-08-09
- Advances: [Decision 0461](0461-Native-WVHV-Metadata-Ownership.md), [Decision 0213](0213-Stage0-Semantic-Freeze-And-Native-Front-Door.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [native hosted-verifier runtime header](../../Specifications/Windvale-Native-Hosted-Verifier-Runtime-Header.md)
- Metadata: [native hosted-verifier metadata](../../Specifications/Windvale-Native-Hosted-Verifier-Metadata.md)

## Context

Decision 0461 transferred exact `WVHV 1` metadata construction and admission,
but the compiler verifier still received its initial runtime header from the
managed Stage 0 writer. The compiler-family Windvale constructor cannot be
reused directly: its numeric profile 2 means build driver, its instruction
budget is 48 billion, it allocates 64 file snapshots, and it includes a
file-output table. The read-only verifier instead requires 16 billion
instructions, one file snapshot, and an 80-byte zero file-output region.

## Decision

- Reuse the stable `WVHR 1` request and `WVHS 1` response envelopes in a
  verifier-specific executable rather than inventing another envelope.
- Require exact target, profile 2, reserved zero, and verifier-specific
  `WVHV 1` admission before constructing any successful response.
- Construct context 7, service table 5, output table 1, the one-snapshot
  file-input table, the zero file-output region, exact metadata, and zero tail
  in portable Windvale.
- Keep metadata admission, runtime-header construction, and the byte-input
  bridge as separate focused modules.
- Build the production WVB through the native project front door. Retain C#
  only as a byte-for-byte differential oracle after native construction.

## Evidence and consequences

The native project front door builds 22 functions and 15,893 code bytes into a
17,941-byte WVB with SHA-256
`cf27254409ab5d574f6b6b19feb5958d97c3076a5f3b0806208437cfde04114e`.

One reviewed focused test passes. For both Windows and Linux, the Windvale
interpreter and native backend return identical 4,096-byte headers, those
headers equal the frozen C# builder byte for byte, and the independent existing
runtime verifier accepts them. Ten size, envelope, request, and metadata
corruptions return identical bounded failures in both execution modes.

This closes portable compiler-verifier runtime-header construction only.
Native request/service-bundle composition, verifier startup, container
layout/plan, platform bytes, publication, independent Linux execution, and
promotion remain. No broad Seed, OS, Standard, Qualification, WebAssembly, or
QEMU gate ran.

## Reconsideration triggers

Add another verifier-family profile only with its exact authority, service,
runtime-layout, and malformed-input contract. Do not infer verifier layout from
the compiler-family profile number.
