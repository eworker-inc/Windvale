# Decision 0542: Refresh segmented WVO staging after ABI 23

- Date: 2026-08-13
- Status: Implemented candidate with focused Windows reconstruction evidence
- Requires: [Decision 0540](0540-First-Abi-23-Storage-Call-Lowering.md)
- Refreshes: [Decision 0496](0496-Native-Segmented-Compiler-Toolset-Reconstruction.md)
- Retains: Compiler-image staging and canonical image-transport artifact identities

## Context

Decision 0540 changed the Windvale native x64 lowering source closure. The
segmented WVO-staging producer embeds that closure, but its checked-in candidate
and three launcher identities still described the previous compiler. GitHub's
Windows and Linux retirement suites therefore failed at the same exact
`segmented-compiler-toolset-reconstruction` owner after the earlier native
front-door, AOT, compiler reconstruction, bootstrap, and WebAssembly jobs
passed.

## Decision

- Reconstruct all nine segmented toolset artifacts from current source using
  the retained native construction path.
- Refresh only `Wvo-Staging-Producer.wvb`, `windows-x64-wvstage.exe`, and
  `linux-x64-wvstage.elf` plus their exact launcher and manifest identities.
- Require the other six artifacts to remain byte-for-byte equal to the existing
  compiler-image staging and canonical transport candidates.
- Do not change the staged object, image-manifest, canonical-transport, hosted
  package, or target-selection contracts.

## Evidence

Current Windows reconstruction produces:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `Wvo-Staging-Producer.wvb` | 482,611 | `4a79ffad86630a7bf1efed7f3c4c28f7d7586c0432bdb0c34a14c428d57a8ade` |
| `windows-x64-wvstage.exe` | 6,934,528 | `50ea8ba23182802f577b1adf3865950558c626865b45e212792fda44b358f0da` |
| `linux-x64-wvstage.elf` | 6,934,528 | `e147ec43acbaec07c88b7c549df1fc1cf4ca7d5fdc06a48865b31ec95110d92a` |

The WVB and both applications changed together. All three compiler-image
staging artifacts and all three canonical image-transport artifacts reproduce
their prior bytes and SHA-256 identities exactly.

## Consequences

- The segmented compiler launcher now consumes a candidate built from the same
  lowerer closure that the source and focused ABI-23 tests verify.
- This refresh makes no new storage-provider execution or bootstrap-stage
  claim; it repairs exact artifact provenance after an intentional compiler
  source change.
- Independent Linux reconstruction and the complete dual-host retirement suite
  remain the pushed qualification evidence.

## Reconsideration triggers

Replace this whole-family refresh when content-addressed compiler-closure keys
can identify and rebuild the affected candidate automatically, or when the
segmented producer stops embedding the complete lowerer source closure.
