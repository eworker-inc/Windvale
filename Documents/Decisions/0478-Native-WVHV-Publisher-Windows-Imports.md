# Decision 0478: Native WVHV publisher Windows imports

- Status: Implemented current-host candidate; outer PE/ELF materialization pending
- Date: 2026-08-09
- Advances: [Decision 0477](0477-Native-WVHV-Publisher-Object-Instantiation.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [native hosted-verifier publisher Windows imports](../../Specifications/Windvale-Native-Hosted-Verifier-Publisher-Windows-Imports.md)

## Context

Decision 0477 instantiated every publisher WVO, but the Windows final writer
still called a frozen C# helper to create its publisher-only 17-function Win32
import page. The ordinary hosted-verifier import profile is smaller and cannot
be reused without silently omitting publication operations.

## Decision

- Add a focused service-free Windvale constructor for the exact 4,096-byte
  publisher import page at admitted address 253,952.
- Construct three import descriptors, lookup tables, IAT tables, hint/name
  records, and library names directly; do not retain an opaque copied page.
- Keep this Windows-only prerequisite separate from the final PE/ELF byte
  materializer. Linux has no corresponding import-page input.
- Admit one small versioned request and emit a versioned response so target
  address changes cannot silently repurpose the constructor.

## Evidence and consequences

The digest-bound native front door builds a service-free 9,310-byte WVB with
SHA-256
`8d233b54d0387e9a1348447f9095e683415075da31104f4b80c935b09c960831`.
The reviewed focused test passes 1/1 in 2.014 seconds. Interpreter and native
execution agree, and the result equals the complete canonical publisher page
with SHA-256
`ff9b9a84ea0d74386337ab605a4d1afc76bd426bff49d6dfd96845b06207bee5`.
Truncated, wrong-magic, wrong-version, and wrong-address requests reject.

This removes the Windows publisher import table from the remaining C#
construction gap. It does not yet perform the final PE/ELF copies and header
mutations or claim independent Linux qualification. No broad Seed, OS,
Standard, Qualification, WebAssembly, QEMU, or Linux process gate ran.

## Reconsideration triggers

Version the request and response if the import address, library set, function
set/order, hint/name layout, IAT geometry, or PE directory contract changes.
