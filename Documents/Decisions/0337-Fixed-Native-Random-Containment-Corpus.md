# Decision 0337: Fixed native random containment corpus

- Date: 2026-08-06
- Status: Implemented current-host evidence; Linux execution pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0174](0174-Portable-Compiler-Memory-Contract-And-Wasm-Bytes-Entry.md), [Decision 0218](0218-First-Native-Test-Orchestration.md), and [Decision 0330](0330-Manifest-Driven-Native-Retirement-Test-Suite.md)
- Contract: [Native random containment tests](../../Specifications/Windvale-Native-Random-Containment-Tests.md)

## Context

One managed Seed test creates 500 arbitrary source strings, presents each to the
source compiler and assembler, then continues the same framework PRNG stream
for 1,000 arbitrary WVB values and 500 arbitrary WVO values. The test proves
that no value escapes the established diagnostic boundaries, but it must start
.NET and regenerate the framework-specific sequence on every run.

The ordinary project-aware native compiler cannot faithfully replace this test
by itself because WVSS and the hosted source tool reject a zero-length source
before it reaches the compiler core. The portable compiler memory adapter owns
the required bytes-entry rejection boundary and already exists as an exact,
import-free direct WebAssembly artifact.

## Decision

- Reproduce the exact `0x00575642` managed sequence once at commit `d964660`,
  preserving all 2,000 inputs and Stage 0 results in one digest-bound compact
  archive. Do not port or reimplement framework `Random` in permanent code.
- Split permanent execution into selectable source, WVB, and WVO lanes over one
  shared validated corpus. This keeps local checks focused without changing the
  original continued sequence.
- Drive nonempty source through one-module canonical WVSS and the sole empty
  value through the memory adapter's documented empty-input rejection. Use a
  fresh import-free ABI-4 compiler instance and a fixed instruction budget for
  every case.
- Present the same source bytes to the digest-bound native assembler, requiring
  exact Stage 0 rejection-code agreement plus complete input and destination
  preservation.
- Present arbitrary WVB and WVO bytes to the corresponding digest-bound native
  verifiers. Require bounded structured rejection, empty standard output, and
  input preservation. Retain Stage 0 code/offset as corpus provenance rather
  than requiring unrelated diagnostic taxonomies to share names.
- Use focused corpus, host-process, source, binary, and orchestration modules
  instead of one very large test source. Generate nothing and start no .NET
  process during permanent execution.

## Evidence and consequences

- The archive is 617,645 bytes at SHA-256
  `c3d17ee927d8c485fc98b85c4b50d5fb6110532b8a2d02b818d7018903f2edc6`.
  Its 240,966-byte manifest at SHA-256
  `d7076c44f43192db832796553cbe605c20829361d7249e111a270ff22458186c`
  binds 131,149 source bytes, 248,979 WVB bytes, and 124,722 WVO bytes.
- Stage 0 rejects every value. Source diagnostics span `WVC1002`, `WVC1004`,
  `WVC1100`, and `WVC1104`; all assembler results are `WVA1001`. WVB evidence
  spans `WVB1002` and `WVB1018`; WVO evidence spans `WVO1002` and `WVO1016`.
- Reviewed direct Windows execution passes source 500/500 in 27.6 seconds, WVB
  1,000/1,000 in 27.8 seconds, and WVO 500/500 in 22.0 seconds. Every source
  compiler response is a bounded `WVCO 1` diagnostic, every native command
  rejects structurally, and all inputs and assembler destinations remain exact.
- One exploratory attempt to start WVB and WVO lanes concurrently stopped at
  `Wvb-0188` with a noncontract exit. That exact input and its four-case neighbor
  group immediately returned the required exit, and the complete WVB and WVO
  lanes then passed separately. The permanent retirement coordinator is
  deliberately sequential and does not make cross-lane concurrency a contract.
- The 1,364-byte LF-only retirement plan now has SHA-256
  `6019c0f0577e096476d479a4de9a65a919a230064d1051c7bd587342aa5fa89a`;
  it fixes 17 suites and 2,986 declared cases.
- Passing children are not rerun through the changed coordinator. Linux
  execution, hostile-size WVO, remaining source/PE/ELF differential data,
  promotion, and the grouped end-of-goal gate remain pending.

This slice changes no source semantics, WVB/WVA/WVO format, product compiler,
assembler, verifier, candidate artifact, managed reference, or WebAssembly
implementation. Node.js is an explicit .NET-free host for the import-free
compiler artifact and native child coordination, not a semantic owner.

## Reconsideration triggers

Revise the corpus version and identities if the legacy sequence, any retained
input or Stage 0 result, direct compiler ABI/artifact, native assembler or
verifier artifact, instruction/process bound, report shape, or preservation
contract changes. Replace Node.js only when an equally bounded shared
Windows/Linux host can execute the same import-free compiler and native tools
without moving Windvale semantics into that host.
