# Decision 0454: Native Probe 40 system-kernel target

- Status: Implemented current-host native-build candidate; Linux execution pending
- Date: 2026-08-09
- Advances: [Decision 0453](0453-Native-Probe-40-Loader-Object-Producer.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contracts: [Windvale system-kernel target](../../Specifications/Windvale-System-Kernel-Target.md), [x86-64 kernel target](../../Specifications/Windvale-X64-Kernel-Target.md), and [WVO object construction](../../Specifications/Windvale-Wvo-Object-Construction.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

The ordinary Probe 40 build still consumed the 12,134-byte frozen Stage 0
`01-kernel.wvo`. Unlike the loader code fixture, that object has canonical
Windvale source: `Operating-System/Kernel/Hello-World.wv`. The C# special
kernel target parsed and type-checked that source, then emitted one entry
wrapper and one byte-output call for every source `console.write_line` byte.

Keeping only the object would lose the real source-to-WVB-to-WVO path. Copying
its machine code into another producer would remove a managed invocation but
would not transfer compiler ownership to Windvale.

## Decision

- Add a minimal Project 1 manifest that compiles the canonical system source
  into its exact verified WVB 1.11 module.
- Implement a bounded Windvale WVB reader for exactly the special kernel-target
  subset. It validates the module envelope, system profile, capability, data,
  function, local, instruction, and output limits before constructing a plan.
- Keep that cohesive parser in a 386-line source and the hosted x64/WVO emitter
  in a separate 118-line source. Do not merge them into a catch-all merely to
  reduce file count.
- Package one digest-bound target for Windows and Linux. The public launchers
  verify arbitrary input WVB first, reject unsupported verified modules, admit
  the produced WVO independently, and preserve an existing destination.
- Build `Hello-World.wvb` and lower it inside the ordinary Probe 40 private work
  directory, then remove `01-kernel.wvo` from the frozen seed.
- Retain `X64-Kernel-Compiler.cs` only in the frozen Stage 0
  recovery/differential lane.

## Evidence and consequences

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Canonical kernel WVB | 1,484 | `7a0ef0dedba2a72177239c54fd670be82968e7c5156855bf36be7412da6d656c` |
| Kernel-target WVB | 57,129 | `9a7149ee7e0cb7533ef95baa199af24c36b5819217e634e362dd4f70e92bd3e8` |
| Windows x64 application | 613,888 | `af00f5bdb8934b07e9cbfec6881446d9e7fdc19264c2248e96e2a5df5566c027` |
| Linux x64 application | 614,400 | `ca3730b7da3dcc645d353743cc14771a9bee9d669ecef89111d0342dabbf0147` |

Current-host execution reproduces the former seed object byte for byte at
12,134 bytes and SHA-256
`bf13c1b103c297e87f4aa14f5bf7eba57ef2a30caa21b4c67dba34abc0a7f7a8`.
After affected-test review, the new kernel-target lane passes 7/7 in 3.9
seconds and the normal `os-probe` lane passes 2/2 in 13.2 seconds. The final
EFI remains 683,008 bytes at SHA-256
`080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9`.

The frozen seed now contains two WVOs totaling 642,288 bytes. Nine ordinary
objects come from Windvale-native producers totaling 50,362 bytes, three more
come from native WVA, and the fourteen-object link order remains unchanged. The
retirement plan is 2,403 LF-only bytes at SHA-256
`4327d606866d7be6dd0107ac9a78466b5e5b64596e1ae3d7c33c5f2daaeba497`
and contains 29 suites with 3,143 fixed cases.

Linux execution and every broad Seed, OS, QEMU, Standard, Qualification, and
complete retirement gate remain pending. No maintained Stage 0 artifact was
produced in this slice.

## Reconsideration triggers

Retire this special target when the shared Windvale-native backend supports the
system profile and kernel ABI directly. Until then, expand the accepted subset
only with an explicit source/WVB/object contract and focused malformed-input
evidence; do not fall back silently to the C# target.
