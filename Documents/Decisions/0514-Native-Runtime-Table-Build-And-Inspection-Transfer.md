# Decision 0514: Native runtime-table build and inspection transfer

## Status

Implemented current-Windows evidence. Independent Linux execution and grouped
qualification remain pending.

## Context

The broad Seed verification scripts still used the feature-frozen Stage 0 CLI
to build sixteen exact products across eight closely related runtime
construction families. They also used the managed inspector on each public
bridge. The output, file-output, file-input, service, execution-context,
argument, entry, and byte-result-admission products already have deterministic
portable source closures accepted by the native Project 1 front door.

Their bridge manifests remained at repository root even though every source in
each closure belongs to `Runtime/Windvale`. The preceding Foundation, service,
I/O, and fixed-service transfers established component-local manifests as the
normal ownership rule.

## Decision

Extend the paired `Verify-Seed-Native-Front-Door` helpers with these exact
Project 1 products:

| Product | Bytes | SHA-256 |
| --- | ---: | --- |
| output-table core | 4,710 | `ab51993aea2370d84b8fe116634e3da71882756bfa87822f1bce180bb01b04a8` |
| output-table bridge | 4,714 | `b5b20dc0213e55790e4f39e8a512a17e2a0304b0202d488a9342905ee35e80a8` |
| file-output-table core | 3,926 | `fb6fd67339561f517967b326cc4299132699dc6f098a38595bbb3aabbf1fbc7f` |
| file-output-table bridge | 3,930 | `94cc057b655c58be3ccd2db333cff4e7a755482c52983c4031196ab060a89e06` |
| file-input-table core | 5,078 | `0c6b66ae7fcef5a0b73df1d56bbfd0a5376ae2978f6ae762470abcf544b6a438` |
| file-input-table bridge | 5,084 | `e7d33fc579c0bc2d001a3e7e2ad68e6403091cae6bda270e51578e10f04c4bd9` |
| service-table core | 3,065 | `ca7388bf816e7d23d5a4cd3cb7cff488ba2cb3d96c0c1a0f511ced54b4296c26` |
| service-table bridge | 3,079 | `04c87116f12097c6efaeddc471c06ce831f6146c94b4cae0205a635f31bcd50b` |
| execution-context core | 5,530 | `dda77e9fd637746bf5b1179136deee0bbae2d8d6b57982323b868b98a8daa29b` |
| execution-context bridge | 5,531 | `86b9a139a387eb3c4fb86f43731e442a62af8ce3c7289cf914b31a9256d21a68` |
| argument-table core | 4,362 | `08df8569d091fc0c860988dceff1320d7a8e407b54ce571515af601c10120d75` |
| argument-table bridge | 4,374 | `080be2dea127948697222c23efe4be828410450b602dee5cf2a63abc11627788` |
| entry-bridge core | 3,385 | `8eab863c7b214e559c48c822381b822eef22bd852ce16252bb392ebdfbcefdae` |
| entry bridge | 3,401 | `d66a34430da6db3271103cfb9c2064a3a5a9de455c564ed87144cf4a0a4994c1` |
| byte-result-admission core | 7,078 | `eacc3c6bce78f9b07d11b13a46059e92cf8a34fc1f659b896d444e7e3c937c04` |
| byte-result-admission bridge | 7,057 | `9106356cf441c995b7c8478b3a5a779628328cd82acac87621de9a45bbb2becf` |

Native inspection binds the portable profile, zero declared capabilities,
one `Main(bytes) -> bytes` export, and the exact public bridge surface for all
eight families. The broad scripts consume the native-built bridge WVBs and
retain byte-for-byte agreement with every embedded bridge plus the exact
linked-fragment identities. They no longer repeat the sixteen managed builds
or eight managed inspections.

All sixteen manifests now live beside their `Runtime/Windvale` source. The
eight obsolete repository-root bridge manifests are removed. This relocation
changes neither Project 1 source order nor any WVB identity.

## Evidence

- All sixteen native builds reproduce the established WVB identities.
- Eight native inspections admit the exact intended bridge surfaces.
- `Verify-Seed-Native-Front-Door.ps1` passes its 100-case contract over 59
  artifacts in 40.9 seconds.
- The eight directly affected frozen behavioral owners pass 8/8 in 7.981 test
  seconds.
- The changed-file planner assigns every source, component-local manifest, and
  removed root manifest to the Seed native-front-door evidence boundary.

This removes twenty-four additional managed invocations from each broad host
script, 106 cumulatively across Decisions 0505, 0506, 0508, 0509, 0510, 0511,
0512, 0513, and 0514. It does not remove a direct managed entry file: the
inventory remains three normal direct files plus nine recovery files, and T2
remains `managed-normal`.

## Consequences

The paired native helper grows from 43 to 59 exact artifacts and from 76 to
100 owned cases. Construction of the complete bounded runtime-table and entry
metadata layer is native in both permanent-host scripts. The feature-frozen
behavioral oracle, capability-bearing execution, linked native products, and
broad managed harness remain separately owned.

Current evidence is Windows-host native build, inspection, and focused
differential evidence. It is not independent Linux execution, replacement of
the broad managed test harness, clean or previous-seed bootstrap, grouped
qualification, promotion, or recovery deletion.

## Reconsideration triggers

Keep the next transfer cohesive around hosted-tool metadata, startup, and
runtime-header construction. Do not merge runtime-table formats merely because
their current qualification ownership is shared.
