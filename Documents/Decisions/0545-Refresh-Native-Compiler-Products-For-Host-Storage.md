# Decision 0545: Refresh native compiler products for host storage

- Date: 2026-08-14
- Status: Implemented candidate with focused Windows construction evidence
- Requires: [Decision 0544](0544-First-Native-Durable-Storage-Provider.md)
- Refreshes: [Decision 0542](0542-Refresh-Segmented-Wvo-Staging-After-Abi-23.md), [Decision 0543](0543-Refresh-Standalone-Lowerer-After-Abi-23.md)
- Retains: WVB 1.11, WVO 1.0, ABI 23, and exact ABI-22 `Return-42` output

## Context

Decision 0544 closes the record-storage admission gap needed by the real native
database host. The segmented staging producer and standalone lowerer embed that
source closure, so their checked-in candidates and digest-bound launchers must
advance together before the durable-storage owner can verify the final tree.

## Decision

- Reconstruct all nine segmented compiler-toolset artifacts through the retained
  native path. Refresh only the source-closure-dependent staging WVB and paired
  host applications; require the other six products to remain byte-identical.
- Reconstruct the standalone lowerer WVB and paired applications through that
  refreshed segmented toolset.
- Refresh every ordinary launcher, reconstruction owner, downstream raw-lowerer
  consumer, manifest, and specification identity together.
- Require canonical `Return-42.wvb` and its ABI-22 WVO to remain byte-identical.

## Evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| `Wvo-Staging-Producer.wvb` | 482,789 | `35c7be9284e3a9f180e13f3b20b1c0fe72f7b0b227c94f18b4dd47c56d86eeea` |
| `windows-x64-wvstage.exe` | 6,935,552 | `292514068284da40eb0f288bfb4564ab7802d1fc8b03f80e6a5a3d45a311f7b4` |
| `linux-x64-wvstage.elf` | 6,934,528 | `812671adda682daf49902eb1679b49a40811a113cc6b951de727926ee8348d66` |
| `Wvb-To-Wvo.wvb` | 457,219 | `d5b17c84889dab959bd992083a06325149f746d2ff611445df1fb7f0102680b4` |
| `Wvb-To-Wvo.exe` | 6,499,840 | `a8041f1053fa04598a762998d7820ffc0b704b92494d3ae87ebb8d95ac94450e` |
| `Wvb-To-Wvo.elf` | 6,500,352 | `de7bdb40637208ee05a7987aba0ea88366638e132fb3f7ba5d9730befde316b5` |

The segmented construction took 158.726 seconds and reproduced all six
unaffected products exactly. Standalone lowerer construction took 115.058
seconds. `Return-42.wvb` remains 174 bytes at SHA-256
`7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31`;
its WVO remains 479 bytes at SHA-256
`0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5`.

## Consequences

- The ordinary native lowerer now contains the exact source closure used by the
  first real durable-storage host.
- Later database-only edits can consume the retained current lowerer without
  reconstructing it locally unless compiler inputs change.
- Independent Linux reconstruction and the complete dual-host qualification
  gate remain pushed evidence rather than claims made by this Windows refresh.

## Reconsideration triggers

Replace this manual family refresh when a content-addressed compiler cache can
derive complete source-closure keys, publish immutable outputs, and resume from
validated phase manifests without weakening clean qualification.
