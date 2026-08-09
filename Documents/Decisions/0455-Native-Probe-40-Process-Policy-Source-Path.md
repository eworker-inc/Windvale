# Decision 0455: Native Probe 40 process-policy source path

- Status: Implemented current-host native-build candidate; Linux execution pending
- Date: 2026-08-09
- Advances: [Decision 0454](0454-Native-Probe-40-System-Kernel-Target.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contracts: [OS process-policy object build](../../Specifications/Windvale-Os-Process-Policy-Object.md), [accepted-subset WVB lowering](../../Specifications/Windvale-Native-X64-Lowering.md), and [WVO export rename](../../Specifications/Windvale-Wvo-Export-Renamer.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

The ordinary Probe 40 build still consumed the 129,310-byte frozen Stage 0
`04-process-policy.wvo`. Its canonical owner is already the portable Windvale
source `Operating-System/Kernel/Process-Foundation.wv`. Stage 0 compiled that
source to WVB, lowered the verified module through the general x64 backend, and
renamed exported `Main` to `Windvale_kernel_process_policy`.

The current Windvale-native front door, accepted-subset lowerer, and verified
export renamer already implement that complete chain. A new special target or
machine-code fixture would duplicate established tools.

## Decision

- Add one Project 1 manifest for the existing process-policy source.
- Compose the qualified native builder, digest-bound general lowerer, verified
  export renamer, independent WVO verifier, and native WVO publisher behind
  paired focused launchers.
- Pin the source WVB, unrenamed WVO, and final WVO identities at each boundary.
- Build the final object in the ordinary Probe 40 private work directory and
  remove it from the frozen seed.
- Add a two-case lane for exact construction and existing-output preservation.
- Retain the Stage 0 policy compilation only as frozen recovery/differential
  evidence; do not create a second process-policy implementation.

## Evidence and consequences

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Process-policy WVB | 18,764 | `c46c6b3780cad8d292607ed687a7e511e2e3c47fbc6fc21526ecc0ffeb937895` |
| Unrenamed WVO | 129,284 | `11e1796c176dcdeb2f643108b646363751347707ca4b16b0e914b8c0b384987e` |
| Final process-policy WVO | 129,310 | `35d751147a7285fb926ba68e77da4ef554bcf68a58963520153f23ea3e8c4678` |

Current-host construction is byte-for-byte identical to the former seed
object. After affected-test review, `os-process-policy` passes 2/2 in 3.1
seconds and `os-probe` passes 2/2 in 15.6 seconds. The final EFI remains
683,008 bytes at SHA-256
`080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9`.

The frozen seed now contains one 512,978-byte WVO. Ten ordinary objects come
from Windvale-native producers totaling 179,672 bytes, three more come from
native WVA, and the fourteen-object link order remains unchanged. The
retirement plan is 2,484 LF-only bytes at SHA-256
`5138c5e7fb517f269cab07e3cd2840577114b822b51ce77fd1d9850bdd620c4d`
and contains 30 suites with 3,145 fixed cases.

Linux execution and every broad Seed, OS, QEMU, Standard, Qualification, and
complete retirement gate remain pending. No maintained Stage 0 artifact was
produced in this slice.

## Reconsideration triggers

Change this source-specific composition only if the general lowerer or export
contract changes. Keep the three pinned identities explicit so such a change
cannot silently alter the kernel ABI or Probe 40 image.
