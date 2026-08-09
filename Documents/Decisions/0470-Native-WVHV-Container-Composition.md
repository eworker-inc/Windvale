# Decision 0470: Native WVHV container composition

- Status: Implemented current-host candidate; pipeline wiring and independent Linux execution pending
- Date: 2026-08-09
- Advances: [Decision 0469](0469-Native-WVHV-Platform-Bytes.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [native hosted-verifier container](../../Specifications/Windvale-Native-Hosted-Verifier-Container.md)

## Context

Decisions 0461 through 0469 transferred the verifier's metadata, runtime,
evidence, bundle, startup, and platform-region construction into Windvale, but
the final PE/ELF byte join still belonged only to managed application builders.
The generic hosted-container pipeline could perform that join indirectly, but
its multi-segment source-set machinery obscured the small format-4 verifier
boundary and did not bind the complete service bundle back to `WVHV` digests.

## Decision

- Add one focused portable constructor for exact format-4 application placement.
- Admit the complete bundle in a separate module that verifies the native and
  six service SHA-256 values plus canonical alignment fills.
- Consume the already-versioned platform, startup, and bundle responses rather
  than duplicating their producers.
- Keep durable sibling/reread/atomic replacement in the existing native
  publisher. This slice owns construction, not filesystem transaction policy.
- Package only a small hosted wrapper. Add no managed product path or writer.

## Evidence and consequences

The native front door builds a 53,900-byte WVB with SHA-256
`78973e37b7baa2ab5befd83bfa8df5b6676e40ef58a218ffe7a7c7ce4e53a5fe`.
Its paired native-packaged applications are:

| Target | Bytes | SHA-256 |
| --- | ---: | --- |
| Windows x64 | 822,784 | `8394b3a76ed26401ac3c1b127dc548488d98d1af7295079feadf92fc5059ce1a` |
| Linux x64 | 823,296 | `e501594c90a2f8c0c2d3c4528aef2bafa1fff437af6f6320a276c5dc3df1e66c` |

The focused named test passes 1/1 in 15.169 seconds after the incremental build.
Both completed applications equal the current Stage 0 contracts byte for byte.
Corrupting the bundle's first payload byte is rejected through its native-image
digest while the existing destination remains unchanged; output aliasing also
preserves the runtime input. The first run exposed the distinct zero-filled
fragment alignment and `0x90` inter-service padding rules, which are now
explicitly enforced.

The hosted candidate now binds 72 artifacts. Its 6,927-byte inventory has
SHA-256 `a3566d34a19243aed706b1b2f972ace8698f859a21a91aa060bf78a2763057d5`;
all entries match. Including manifest and inventory, it contains 74 files
totaling 20,409,768 bytes. The previously passing platform, startup, bundle,
packaging, and broad suites were not rerun.

Pipeline wiring through the retained durable publisher, native execution of the
completed applications, independent Linux execution, grouped qualification,
promotion, and recovery-source deletion remain. No broad Seed, OS, Standard,
Qualification, WebAssembly, or QEMU gate ran.

## Reconsideration triggers

Version this boundary if format 4, response envelopes, service count, padding,
runtime placement, PE/ELF policy, or bundle digest ownership changes.
