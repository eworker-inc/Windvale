# Decision 0465: Retained native WVHV request containers

- Status: Implemented current-host candidate; native package reconstruction and dual-host promotion pending
- Date: 2026-08-09
- Advances: [Decision 0464](0464-Native-WVHV-Evidence-Process.md), [Decision 0415](0415-Managed-Hosted-Tool-Aot-Recovery-Lane.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [native hosted-verifier metadata request](../../Specifications/Windvale-Native-Hosted-Verifier-Metadata-Request.md)

## Context

Decision 0464 introduced the small hosted `WVVE` to `WVVR` wrapper, but only
its WVB/native-fragment execution was retained. The verifier packaging pipeline
needs fixed Windows and Linux process artifacts before the existing native
host-container machinery can reconstruct and compose them without .NET.

## Decision

- Use the established compiler-family read/write hosted-tool service profile.
  The wrapper retains the same six capabilities and uses enum naming, text
  concatenation, and `u32` formatting only for canonical status output; this
  adds no authority.
- Freeze exact Windows and Linux format-3 application identities for the
  native-built wrapper WVB.
- Register the two new targets only under `recovery-aot`. Ordinary `compile`
  and `aot` do not accept this Stage 0 packaging route.
- Keep the new C# contract limited to target names, immutable identities, and
  deletion-bound Stage 0 writer selection. Request validation, construction,
  file behavior, and diagnostics remain Windvale-owned.
- Require focused evidence to rebuild the selected host container through the
  recovery CLI, compare its bytes with the frozen identity, execute that real
  application, and preserve exact Windows/Linux `WVVR` results.

## Evidence and consequences

The canonical wrapper is a 17,204-byte native-built WVB with SHA-256
`c5aeb2ff6f50760bd01843d43a307fb23988d9fe6c8865b4c549d21f52486f25`.
Its retained applications are:

| Target | Bytes | SHA-256 |
| --- | ---: | --- |
| Windows x64 | 187,904 | `dc42cd573e26ba8617a7323089f2c140f0488ec0cb3b9a6e4b77d5c4d7fbd4d5` |
| Linux x64 | 188,416 | `cfd0071c3d103ca0feedb33370b81bc3edb0b41e8bb95ee9744d1b53342fb6bd` |

One reviewed focused test passes. It reconstructs both WVBs through the native
front door, constructs both exact containers, rebuilds the current-host
container through `recovery-aot`, executes that retained application after the
native hashing process, and reproduces the exact frozen-oracle request for both
verifier targets.

This closes retained Stage 0 container identities and current-host execution
for the request wrapper. Native reconstruction of those containers, the
six-service verifier bundle process, startup, final verifier container,
independent Linux execution, and promotion remain. No broad Seed, OS,
Standard, Qualification, WebAssembly, or QEMU gate ran.

## Reconsideration triggers

Delete this managed writer and target registration after the native packaging
path reconstructs and promotes both artifacts and the final recovery archive
has retained their provenance.
