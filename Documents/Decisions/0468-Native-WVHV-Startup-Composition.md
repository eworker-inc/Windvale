# Decision 0468: Native WVHV startup composition

- Status: Implemented current-host candidate; independent Linux execution and promotion pending
- Date: 2026-08-09
- Advances: [Decision 0467](0467-Native-WVHV-Service-Bundle-Process.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [native hosted-verifier startup](../../Specifications/Windvale-Native-Hosted-Verifier-Startup.md)

## Context

Windvale already owned exact verifier metadata, runtime-header construction,
the six-service bundle, and the generic WVO relocation instantiator. Stage 0
still selected the verifier's format-4 layout and supplied the ordered startup
relocation targets. Reusing the compiler-family format-3 plan would have hidden
different runtime and service placement rules behind an incompatible contract.

## Decision

- Add one focused portable layout module for the compiler-verifier format-4
  runtime. Derive platform placement only from admitted `WVHV 1` fields and
  fixed format rules.
- Retain the paired canonical WVA sources and their exact native-assembled WVO
  products. Admit their byte length and digest before constructing a request.
- Map relocation ordinals to semantic runtime, service, import, and native-entry
  addresses in a separate request module. Feed the resulting `WVSI 1` to the
  existing portable startup instantiator; do not add another patcher.
- Keep the hosted wrapper small, preserve input/output alias safety, and report
  request versus response rejection phases.
- Package the wrapper through the ordinary native hosted-container path. Add no
  C# product writer, recovery target, or ordinary dispatch entry.

## Evidence and consequences

The native front door builds a 63,636-byte WVB with SHA-256
`435d464bef51cfa0c4154dbdaee24b34c8dd7fc6ef3ee8f39204edb4774358f0`.
The paired native-packaged applications are:

| Target | Bytes | SHA-256 |
| --- | ---: | --- |
| Windows x64 | 684,032 | `b84a4fa6ee8127d9bd040fa601ccae0d3c959b85389097c00b99992ce19f6495` |
| Linux x64 | 684,032 | `29cde262ea0218cd857f729bf3bf04684caefda1e6b2da3e783ae0ddc24ff7f1` |

The focused named test passes 1/1. Its body completes in 8.105 seconds after
the incremental build. Windows produces the exact 1,275-byte startup and Linux
the exact 668-byte startup compared with the frozen C# oracle. A changed WVO
returns request rejection without overwriting the destination; an output alias
returns usage and preserves the runtime input. C# participates only as
differential evidence and does not compile or package a production artifact.

The hosted candidate now binds 66 artifacts: 22 native-built WVBs and their
paired Windows/Linux applications. Its 6,329-byte inventory has SHA-256
`b457d99c99f84cf608fe3fd3f1e4177cbbf30b8d4d369eb25b0659ebf6dd2008`;
all entries match. Including manifest and inventory, it contains 68 files
totaling 17,813,314 bytes. Targeted current-host reconstruction reproduced the
new Windows and cross-target Linux applications exactly. The five unchanged
packaging-smoke cases were not rerun.

This closes canonical verifier startup relocation and its format-4 placement
plan. Outer platform bytes, final publication, independent Linux execution,
grouped qualification, promotion, and recovery-source deletion remain. No
broad Seed, OS, Standard, Qualification, WebAssembly, or QEMU gate ran.

## Reconsideration triggers

Version this boundary if the verifier profile, runtime geometry, service order,
startup WVO identity, import table, relocation ordering, or format-4 outer
layout changes. Do not merge it into the compiler-family startup contract until
one versioned layout can represent both profiles without implicit branching.
