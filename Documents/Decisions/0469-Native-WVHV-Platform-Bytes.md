# Decision 0469: Native WVHV platform bytes

- Status: Implemented current-host candidate; independent Linux execution and promotion pending
- Date: 2026-08-09
- Advances: [Decision 0468](0468-Native-WVHV-Startup-Composition.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [native hosted-verifier platform bytes](../../Specifications/Windvale-Native-Hosted-Verifier-Platform-Bytes.md)

## Context

After Decision 0468, Windvale owned the exact verifier runtime placement and
startup relocation. The remaining outer-container bytes were still emitted by
the managed Windows/Linux application builders. The generic hosted compiler
producer was not compatible: it describes format 3, reserves a much larger
runtime region, and includes a Windows import absent from the verifier.

## Decision

- Reuse the shared byte-construction primitives and admitted format-4 layout,
  not the incompatible compiler-family plan.
- Keep Windows PE/import/relocation construction and Linux ELF construction in
  separate focused portable modules.
- Emit a small versioned response containing only platform-owned regions. Do
  not join the startup, bundle, or runtime here.
- Package the small hosted wrapper through the ordinary native path. Add no C#
  writer, recovery target, or ordinary dispatch entry.

## Evidence and consequences

The native front door builds a 34,376-byte WVB with SHA-256
`03ad87aa7cef5d440fbd1ac94569aa9f07f979b625f81acbfa5405d9bc8a1fce`.
Its paired native-packaged applications are:

| Target | Bytes | SHA-256 |
| --- | ---: | --- |
| Windows x64 | 431,104 | `5288573a8eaedb5745f5b0aae733e2ba7dd89253bfb26d01beffe6279d3540c0` |
| Linux x64 | 430,080 | `b988c45fd1eada051e93243d506e33e0f15325c03ea83c1b9cdd2e994c322e07` |

The focused named test passes 1/1 in 6.190 seconds after the incremental build.
It matches the complete PE header, import page, relocation block, and ELF header
against the frozen application oracle. Invalid metadata and output alias cases
preserve resources. C# is differential evidence only.

The hosted candidate now binds 69 artifacts: 23 native-built WVBs and their
paired Windows/Linux applications. Its 6,625-byte inventory has SHA-256
`c01a37482d5eaaf3cfbcc5d89362ef5698edeb0b8689bf5952e4f9eff787fc59`;
all entries match. Including manifest and inventory, it contains 71 files
totaling 18,709,329 bytes. Targeted reconstruction reproduced both new
applications. The five unchanged packaging-smoke cases were not rerun.

Final verifier source-set assembly, durable publication, independent Linux
execution, grouped qualification, promotion, and recovery-source deletion
remain. No broad Seed, OS, Standard, Qualification, WebAssembly, or QEMU gate
ran.

## Reconsideration triggers

Version this boundary if format 4, PE/ELF policy, import ownership, section or
program-header layout, runtime geometry, image base, or relocation policy
changes.
