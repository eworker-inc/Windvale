# Decision 0506: Native Seed console AOT qualification smoke

- Status: Paired-host focused owner complete; broad native qualification cutover pending
- Date: 2026-08-10
- Scope: canonical capability-free WVB-to-WVO lowering, WVO verification, flat linking, paired version-1 console packaging, and current-host execution inside the broad Seed qualification commands
- Extends: Decisions 0119, 0122, 0213, 0305, 0457, 0458, and 0505

## Context

Decision 0505 moved the representative source/project build and WVB
verify/inspect smoke inside both broad Seed qualification commands to their
qualified native front doors. The next two managed calls in each script still
compiled `Examples/Seed/Sum-Data.wv` separately for the Windows and Linux
version-1 console targets, even though the preceding native helper had already
published the exact canonical WVB and the lower, verify, link, and package
front doors were available independently.

Those managed calls repeated source compilation and hid a coherent native AOT
path already narrow enough to verify directly.

## Decision

Windvale adds paired `Verify-Seed-Native-Console-Aot.ps1` and `.sh` helpers.
Each helper consumes the exact `Sum-Data.wvb` produced by the preceding native
front-door smoke and performs one deterministic chain:

1. lower the admitted WVB through `Lower-Wvb-To-Wvo`;
2. admit the complete object through `Verify-Wvo`;
3. link the object at base zero with `Main` as the entry;
4. package the same flat image for both version-1 console targets; and
5. execute the current-host application and require process result `29`.

The helper requires exact reports, the complete sixteen-line linker map, and
these products:

| Product | Bytes | SHA-256 |
| --- | ---: | --- |
| `Sum-Data.wvb` | 494 | `76b4fa3c4c0cc37e6f1350e8191ccd78c6272224f146ef9816b5f987114c15df` |
| `Sum-Data.wvo` | 3,288 | `4e4958f8f0d611e00e912b925b837aa968e06f85abb116b721e3d6e9b8eed4e1` |
| flat image | 3,104 | `8185a8893587d8d5a8d0430e53310c5e6725dea30a76073292864b90c5150c8a` |
| `Sum-Data-Windows.exe` | 5,120 | `5947c00a81f4cf94651d42d619f3173a622448d042f4fa20e3042940d4a56c77` |
| `Sum-Data-Linux.elf` | 8,304 | `8af8b46c290965cfc4475d882ac2d5fbdb0ffe4c493a19883a19c2683a319ec4` |

The linked `Main` entry is exactly 774. The Linux product must retain mode
`0755`. Both broad Seed scripts invoke the helper once immediately after the
native front-door helper and no longer invoke managed `compile --target` for
these two products. Together, Decisions 0505 and 0506 remove eleven managed
invocations from each host script.

The frozen `windvale compile` and `windvale aot` version-1 target contracts
remain Stage 0 recovery and differential surfaces. This decision transfers one
canonical qualification smoke; it does not claim that the general target
surface or complete backend is promoted.

## Evidence boundary

The current Windows helper passed its exact chain in 1.1 seconds. It reproduced
both established application identities and the generated Windows application
returned `29` without loading .NET.

The paired focused owner now constructs the exact input WVB through the native
Project 1 front door and invokes the host-specific helper in one private
directory. Windows and Debian execute the same lower, admit, link, paired
package, identity, mode, and current-host result contract. The fixed coordinator
owns this as `seed-native-console-aot`; all three changed-file mappings formerly
reporting that evidence gap now select the focused lane.

## Consequences

- N1, L1, and P1 now own the canonical capability-free AOT smoke through their
  native front doors.
- T2 remains `managed-normal`; both broad Seed scripts and the GitHub workflow
  remain in the direct managed-entry inventory.
- The direct managed-entry file count does not change. The fixed retirement
  plan advances to 45 suites and 3,206 cases.
- General native WVB execution, the remaining broad Seed transfers, GitHub
  orchestration cutover, grouped qualification, promotion, and recovery
  retirement remain open.

## Reconsider when

Reconsider this decision if the canonical Sum source or Project 1 closure, ABI,
WVO, link map, version-1 container contracts, native front-door reports,
qualification orchestration, or current-host execution result changes.
