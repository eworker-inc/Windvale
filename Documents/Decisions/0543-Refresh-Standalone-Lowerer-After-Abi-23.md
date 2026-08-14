# Decision 0543: Refresh standalone lowerer after ABI 23

- Date: 2026-08-13
- Status: Implemented candidate with focused Windows reconstruction evidence
- Requires: [Decision 0542](0542-Refresh-Segmented-Wvo-Staging-After-Abi-23.md)
- Refreshes: [Decision 0497](0497-Native-Wvb-To-Wvo-Application-Reconstruction.md)
- Retains: WVB 1.11, WVO 1.0, and exact ABI-22 `Return-42` output

## Context

The normal `Lower-Wvb-To-Wvo` launcher remained pinned to a standalone lowerer
built before Decision 0540 added ABI-23 storage calls. The next retirement owner
would therefore reject the intentional compiler closure change, and local ABI-23
work had to reconstruct and package a lowerer for every focused run.

## Decision

- Reconstruct the standalone lowerer WVB and both host applications through the
  refreshed segmented staging toolset.
- Refresh their exact candidate, launcher, reconstruction, downstream raw-tool,
  and specification identities together.
- Require the unchanged 174-byte `Return-42.wvb` to lower to the exact unchanged
  479-byte ABI-22 WVO.
- Keep all lowerer invocation, publication, WVO verification, and error contracts
  unchanged.

## Evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `Wvb-To-Wvo.wvb` | 457,041 | `15a91a965860c4a36ae114651e87b82e5cd31869f4852040bb428f19f9d0382a` |
| `Wvb-To-Wvo.exe` | 6,498,816 | `8e4656c9f478c6aecd58d7e3e5fda2a44d420562a5dc9d359795b15494922a89` |
| `Wvb-To-Wvo.elf` | 6,500,352 | `0ea1b8ff4bda963b40bb9fa8d62852530e0fc4945e059be135fc2ee829bfe4ac` |

`Return-42.wvb` remains 174 bytes at SHA-256
`7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31`.
Its emitted WVO remains 479 bytes at SHA-256
`0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5`.

## Consequences

- The ordinary digest-bound native lowerer now supports the ABI-23 storage
  fixture without rebuilding the compiler package in the edit loop.
- Existing ABI-22 oracle output remains byte-for-byte unchanged.
- Downstream reconstruction processes may consume the new raw lowerer identity
  while retaining their outputs when they lower only unchanged ABI-22 inputs.
- This refresh improves the local path but does not replace the planned
  content-addressed compiler/verification cache.

## Reconsideration triggers

Replace manual identity refresh when candidate construction records complete
source-closure keys and can publish affected artifacts automatically, or when a
new WVO version requires an explicit lowerer target transition.
