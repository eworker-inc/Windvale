# Decision 0313: Fixed native console-packager rejections

- Date: 2026-08-06
- Status: Implemented current-host candidate; grouped Windows/Linux qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0218](0218-First-Native-Test-Orchestration.md), [Decision 0303](0303-Digest-Bound-Native-Console-Packager-Candidate.md), [Decision 0307](0307-Native-Console-Application-Publication.md), and [Decision 0311](0311-Fixed-Native-Linker-Rejections.md)
- Contract: [Windvale native console packager](../../Specifications/Windvale-Native-Console-Packager.md)

## Context

The fixed AOT chain proves successful packaging, while the managed package test
retains detailed rejection and output-preservation checks. Repeating source
building, lowering, and linking for deterministic packager rejection would waste
time and obscure the packager's ownership.

The managed rejection used a dedicated six-byte image, not the AOT chain's
406-byte linked image. That distinction must remain explicit rather than changing
the expected entry while a test runs or deriving a packager fixture from another
tool.

## Decision

- Store the exact six-byte return-42 native image as a base64 fixture with its
  complete decoded identity.
- Add no-argument `Test-Console-Packager-Rejections.cmd` and `.sh` coordinators.
  They invoke only the digest-bound current-host package launcher and inbox
  decoding/digest utilities.
- Fix three public-launcher cases: entry equal to the six-byte image length,
  non-decimal entry text, and a zero-byte native image.
- Before every case, copy the existing bad-magic WVO to the requested `.exe` or
  `.elf` destination. Require exit `2`, empty standard output, the complete
  host-target diagnostic identity, and byte-for-byte sentinel preservation.
- Keep unsupported target names as launcher usage errors. Do not bypass the
  digest-bound launcher merely to reproduce a raw-application-only case.
- Keep complete construction, verification, limits, publication faults,
  concurrency, and hostile-input coverage in the managed evidence lane until
  separately transferred.

## Evidence and consequences

- The fixture is six bytes at SHA-256
  `11db5348e275fb704be582e8005ee7d604f7f17b154d6cc644d240eef29d456a`.
  Direct Windows execution passes all three cases in about 1.2 seconds.
- The reviewed focused selection
  `native console-packager rejections preserve existing output without .NET`
  passes 1/1 in 0.813 test seconds after an 11.59-second zero-warning Release
  build; the complete command takes 16.9 seconds.
- Windows report identities were measured through the real batch redirection
  path. Linux identities are the SHA-256 of the same exact LF-terminated contract
  with the required `linux-x64-console-v1` target name.
- The permanent command invokes no .NET process and does not rebuild, lower,
  link, package successfully, or execute an application before its rejection
  cases.
- No packager semantics, native artifacts, WebAssembly implementation, or
  compiler source changed. Linux execution, grouped qualification, promotion,
  broader packager-corpus transfer, and native host-container construction remain.

## Reconsideration triggers

Add a fixed case only when it proves a distinct durable public-launcher boundary.
If generated malformed requests become necessary, specify their limits and
deterministic oracle separately rather than turning this coordinator into a
general test language or bypassing artifact admission.
