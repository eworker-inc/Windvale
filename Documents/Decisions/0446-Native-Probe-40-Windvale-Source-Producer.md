# Decision 0446: Native Probe 40 Windvale source producer

- Status: Implemented current-host native-producer candidate; Linux execution pending; second source producer advanced by [Decision 0447](0447-Native-Probe-40-Admission-Source-Producer.md)
- Date: 2026-08-09
- Advances: [Decision 0445](0445-Digest-Bound-Native-Probe-40-Object-Seed.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)
- Native-test contract: [Windvale native retirement test suite](../../Specifications/Windvale-Native-Retirement-Test-Suite.md)

## Context

Decision 0445 gives the normal Probe 40 image a `.NET`-free ordinary build by
freezing eleven Stage 0 WVOs. One of them, `03-native-wvb-probe.wvo`, already
has a small canonical Windvale source. Stage 0 only compiles that source to WVB
and lowers the verified WVB to WVO. The native Project 1 compiler and native
WVB-to-WVO lowerer now implement this exact accepted subset.

Keeping the WVO frozen would leave an unnecessary generated artifact in the
seed and would miss the simplest source-owned producer cutover.

## Decision

- Add `Windvale-Os-Native-Wvb-Probe.wvproj` as the exact one-source Project 1
  contract for `Operating-System/Kernel/Native-Wvb-Probe.wv`.
- In the ordinary Windows and Linux Probe 40 build, compile that project with
  the digest-bound native build driver and lower its verified WVB with the
  digest-bound native WVB-to-WVO path.
- Require the exact 930-byte WVB and 7,306-byte WVO identities before linking.
- Remove `03-native-wvb-probe.wvo` from the frozen object seed. Version the
  manifest to distinguish ten frozen objects from the one native source
  producer while retaining the complete fourteen-object link contract.
- Keep the C# compiler/backend path frozen in Stage 0 for regeneration and
  differential evidence. Do not add new source semantics there.
- Reuse the existing two-case `os-probe` retirement lane because its successful
  construction now necessarily crosses the new source-to-WVO producer.

## Evidence and consequences

The native Project 1 build produces a 930-byte WVB at SHA-256
`af5f93c881f006be06565f15857efb72b201b8f694a6c7e40a90deeaa86cd2c2`.
Native lowering produces the exact former seed object, 7,306 bytes at SHA-256
`046f4fa32293b4f02bdc51a3ec71d562d7a064b31056ca77a43e2083b281cd2c`.
The initial direct comparison completed in 0.632 seconds.

After composition into the ordinary build, the focused lane passes in 9.383
seconds:

```text
PASS  suite os-probe cases=2
Suites: 1, Passed: 1, Failed: 0, Cases: 2
```

The resulting EFI remains exactly 683,008 bytes at SHA-256
`080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9`.
The seed now retains ten WVOs totaling 685,344 bytes; the complete native build
still supplies fourteen link inputs. No broad retirement, Seed, OS, QEMU, or
Qualification suite ran. Linux execution remains pending.

This retires one real Stage 0 producer from the ordinary build rather than only
retaining its output. Ten frozen producers and the non-normal scenarios remain.

## Reconsideration triggers

Update the project or exact identities when the canonical Windvale source,
source semantics, WVB format, ABI, or native lowering contract changes. Return
the producer to the recovery lane only if the native source-to-WVO path cannot
reproduce an explicitly accepted new baseline on both hosts.
