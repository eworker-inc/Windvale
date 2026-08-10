# Decision 0512: Native I/O-service build and inspection transfer

## Status

Implemented current-Windows evidence. Independent Linux execution and grouped
qualification remain pending.

## Context

The broad Seed verification scripts still built eleven exact native I/O
service products through the feature-frozen Stage 0 CLI: the paired console
and diagnostic output cores plus bridge; the shared file-output code, paired
platform cores, and bridge; and the corresponding file-input products. The
three public bridge WVBs were then inspected through the managed CLI even
though the native Project 1 builder and WVB inspector already accepted the
complete source closures.

The three bridge manifests also lived at repository root despite owning only
`Runtime/Windvale` sources. That placement obscured ownership and added to the
root manifest inventory without serving a cross-component aggregate.

## Decision

Extend the paired `Verify-Seed-Native-Front-Door` helpers with the following
exact component-local Project 1 builds:

| Product | Bytes | SHA-256 |
| --- | ---: | --- |
| Windows output core | 9,435 | `a072c3dc92b9675d00ac833860c0c7ef7b44cf98d15a3fead38955921d321983` |
| Linux output core | 8,908 | `d3d8c8b660694af7aed52b3f78a650fc6030bfe4ad6d8adc25396ee64ed608ad` |
| output bridge | 14,930 | `209b3fad1d03c6f9d08a20e4cfce2511c3af3ed894e1e70e3b32f05ad067ceed` |
| shared file-output code | 6,576 | `7ed9baf3a21912933045b99cb82d22d73620a318a716931db86670e5ea2212c6` |
| Linux file-output core | 18,658 | `834d0c45b85b26ffd3ee43e49a85c8c4ffa08f36581c02785729b276eeccdb48` |
| Windows file-output core | 21,129 | `9ca03bf6f5b8678389c81e281438160ff4c96c86f11a048aba90238fdc81a45d` |
| file-output bridge | 33,437 | `441db0e0e5a90f98c7e4b12b17086f56487e7d754d7b6378a0eb2972591e64f6` |
| shared file-input code | 7,869 | `e2bfd4521b8f22529f3747eef196bdf7fa7aa0e97644db23ed45939aa10a1a7a` |
| Linux file-input core | 26,718 | `04533e8ecade1f29e0b706c75ec949f5b4c300074cfd65feacb86f5107dcaeba` |
| Windows file-input core | 32,085 | `6155c4ebb8f4ea76a5d1f22c1bb788aec51e731ceb4a1c5a4ceb7551ba8f409a` |
| file-input bridge | 51,341 | `09f73787a909ae35ebc1aefb05bd88e4282ff8db7152d196f83b2798ea7c2234` |

The helpers bind the exact native build reports and natively inspect the three
bridges for the portable profile, one exported byte-result `Main`, and their
exact export sections. The broad scripts consume those native-built WVBs and
retain byte-for-byte comparison with the embedded bridge WVBs plus every exact
platform service leaf. They no longer repeat eleven managed compiles or three
managed inspections.

All eleven manifests live beside their `Runtime/Windvale` source closures.
The three former root bridge manifests are removed. Root manifests remain
reserved for genuine cross-component aggregates; a future workspace or
package-reference layer may remove that remaining pressure without weakening
Project 1 containment.

## Evidence

- All eleven native builds reproduce the established Stage 0 WVB identities.
- All three native inspections admit the exact portable profile, single
  byte-result `Main`, and one-entry export section.
- `Verify-Seed-Native-Front-Door.ps1` passes its 53-case ownership contract
  over 31 artifacts in 31 seconds.
- The focused frozen output/file-output/file-input differential passes 3/3 in
  9.276 test seconds.
- The broad Windows and Linux scripts retain the independent embedded-WVB and
  platform-leaf identity comparisons after the managed calls are removed.

This removes fourteen additional managed invocations from each broad host
script, fifty-nine cumulatively across Decisions 0505, 0506, 0508, 0509,
0510, 0511, and 0512. It does not remove a direct managed entry file: the
inventory remains three normal direct files plus nine recovery files, and T2
remains `managed-normal`.

## Consequences

The native helper grows from twenty to 31 exact artifacts and from 39 to 53
owned cases. The selected I/O-service build and inspection closure is native
on both permanent-host scripts, while execution behavior, retained binary
leaf identity, and the frozen differential implementation remain separately
owned.

Current evidence is Windows-host native build and inspection evidence. It is
not independent Linux execution, complete capability-bearing execution,
replacement of the broad managed test harness, clean or previous-seed
bootstrap, grouped qualification, promotion, or recovery deletion.

## Reconsideration triggers

Transfer the next coherent managed call cluster rather than moving isolated
calls. Introduce workspace or package-reference semantics only when they have
an exact source-identity, containment, and changed-file ownership contract.
