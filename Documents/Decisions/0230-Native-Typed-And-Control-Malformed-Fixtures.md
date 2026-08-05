# Decision 0230: Native typed and control malformed fixtures

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0218](0218-First-Native-Test-Orchestration.md), and [Decision 0229](0229-Native-Malformed-Wvb-Test-Fixtures.md)
- Contract: [Windvale native test plan](../../Specifications/Windvale-Native-Test-Plan.md)

## Context

Decision 0229 moves five malformed WVB envelope cases into the .NET-free test plan, but typed-execution and control-reachability corruptions remain executable only through the managed suite. The existing conformance test already names nine stable corruptions and proves that Stage 0 rejects them. Reimplementing its WVB decoder and mutation helpers in both host launchers would add duplicate test logic and make a candidate calculate its own expected values.

## Decision

### Retain the fixed-fixture model

Supersede the unqualified `WVNT 3` plan with fixed `WVNT 4` without changing the six-field schema or adding another launcher branch. Nine checked-in base64 fixtures decode to exact WVB values, and the existing `verify-failure` expectation requires the pinned qualified native verifier's exact phase, process result, standard output, and standard error.

The first eight nominal/control corruptions derive from the exact 1,782-byte native build of `Windvale-Native-Test-Nominal-Types.wvproj`, SHA-256 `b1c3543f8064732a0039d071f4e3a7da2bb901f8cfb890fb1de42193a228ff4b`. The capability corruption derives from the exact 850-byte native Project 1 build of `Tests/Fixtures/Source-Wvb/Hosted-Capabilities.wv`, SHA-256 `bad95ed62ed8406c169ddadaa8da8576825d9213af2faa74b945db44afdfd41f`.

The zero-based decoded offsets and replacements mirror the already-reviewed managed conformance cases:

| Fixture | Exact decoded change | Expected phase | Decoded SHA-256 |
| --- | --- | --- | --- |
| `Typed-Operator-Stack-Kind` | opcode byte 1,480: `i32.add` (`0x10`) to `bool.equal` (`0x26`) | `typed-execution` | `c6a5431f2f79165294b23409212d5c30cc6dc191051f248561af7fb2c919fcbb` |
| `Typed-Local-Store-Kind` | `local.store` index at 551: `11` to record local `2` | `typed-execution` | `bd5b097c685065a16adafad9c1e84bbfe010016648251964dfd9edc0d4a482df` |
| `Typed-Call-Argument-Identity` | pre-call `local.load` index at 576: `12` to record local `2` | `typed-execution` | `790846e2f2cba9df0c96a4c513cf6771935c18ec11ad6420dfdf71277d7b9a26` |
| `Typed-Record-Receiver-Identity` | record-receiver `local.load` index at 665: `17` to record local `2` | `typed-execution` | `699c2e735ca84621aa7170dd6befacdfb7f38480a0ba16b6968dea3024f68440` |
| `Typed-Enum-Operand-Identity` | enum-operand `local.load` index at 699: `19` to other-enum local `15` | `typed-execution` | `1d5f4418769be1aaebbf791dc683f7cbdb1773d3f6d51ceb10e80f38149b5c09` |
| `Typed-Branch-Condition-Kind` | branch-condition `local.load` index at 710: `20` to `i32` local `4` | `typed-execution` | `2110116c3d542df9b716977dcd877e39e1192ea036664f2ae5defa8b9de13e40` |
| `Typed-Declared-Maximum-Stack` | function maximum-stack `u32` at 371: `7` to `8` | `typed-execution` | `e4ed0f9aa8ee47de4fb22e89227d9367f1179a93581a88e269b51c607953d673` |
| `Typed-Capability-Argument-Kind` | capability-argument `local.load` index at 735: text local `2` to bytes local `3` | `typed-execution` | `e0204e16f5d64e559f15ab0cbb21b578f12177c98464d778085d7ac7b5d78acc` |
| `Control-Unreachable-Instruction` | jump-target `u32` at 720: instruction offset `282` to `298` | `control-reachability` | `4a76e7dbd5057efbf26b47c7edfb928eebc611da9857081bb8c03ed1b5f6c20c` |

These offsets document provenance only. They are not interpreted at test time. The launchers decode complete fixed values and bind them by SHA-256 before invoking the verifier.

### Keep verification proportional

Review the existing managed corruption definitions and update its native-wrapper expected report before execution. Run the direct Windows native plan once after the plan, fixtures, and launchers are coherent. Do not run the managed wrapper, Standard, Qualification, OS, or GitHub gates. Linux execution remains part of the final grouped dual-host gate.

## Consequences

- The .NET-free plan owns eight representative typed-execution rejection oracles and one control-reachability oracle without adding a test mutator or another parser.
- Expected phases and digests are fixed repository evidence; a candidate run cannot learn them from Stage 0 or rewrite them.
- `WVNT 3` was not qualified or distributed, so `WVNT 4` replaces it without a compatibility lane.
- Remaining semantic and structural limits, randomized malformed modules, the broader unsafe-bytecode corpus, WVO/linker corruption, OS, bootstrap, golden, and differential coverage remain in their existing lanes until separately transferred.

## Focused evidence

Review confirms 22 unique six-field rows, 14 fixed fixtures with exact decoded identities, and a managed-wrapper PASS sequence identical to the plan. The direct .NET-free Windows plan passes all 22 cases in 5.794 seconds. The managed wrapper, Linux adapter, Standard, Qualification, OS, and GitHub gates were not run; Linux and broader evidence remain part of the final grouped gate.

## Reconsideration triggers

Introduce a bounded Windvale-owned fixture container only when duplicating whole malformed modules materially harms reviewability or repository size. Preserve independent expected identities and never make arbitrary mutation commands part of the normal test plan.
