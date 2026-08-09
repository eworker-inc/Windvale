# Decision 0448: Native Probe 40 exception object producer

- Status: Implemented current-host candidate; package consolidated by [Decision 0449](0449-Native-Probe-40-Admission-Bridge-Producer.md)
- Date: 2026-08-09
- Advances: [Decision 0447](0447-Native-Probe-40-Admission-Source-Producer.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contracts: [WVO object construction](../../Specifications/Windvale-Wvo-Object-Construction.md) and [x64 exception object producer](../../Specifications/Windvale-X64-Exception-Object-Producer.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

The ordinary Probe 40 build still consumed the 483-byte frozen Stage 0
`09-exceptions.wvo`. Its recipe is small, but it contains privileged x64
descriptor-table instructions and two link relocations. The normal Windvale
compiler currently rejects the kernel's system profile, and WVA intentionally
does not provide a raw arbitrary-instruction escape. Weakening either boundary
to copy one historical object would confuse source semantics, assembly, and
object construction.

A trial generic hosted dispatcher also exposed an existing native source-binding
limit. Repeating compiler-extension loops would not improve this retirement
slice. The cohesive boundary is instead one small hosted producer over a
reusable portable WVO constructor.

## Decision

- Add a portable WVO 1.0 construction module that encodes primitive section,
  symbol, and relocation records and admits the complete result through the
  shared WVO verifier before returning bytes.
- Add one focused hosted Windvale x64 exception-object recipe under
  `Operating-System/Tools`. Keep process arguments and file publication out of
  kernel runtime source, and keep the shared format logic under `Object-Model`.
- Retain exact paired Windows/Linux native packages and digest-bound launchers.
  Refuse an existing destination and remove an invalid newly created result.
- Generate `09-exceptions.wvo` inside the ordinary Probe 40 private work path,
  require its historical exact identity, and remove the object from the frozen
  seed. Keep the C# generator frozen only for recovery and differential evidence.
- Add three fixed cases for exact independently admitted output,
  existing-destination preservation, and invalid-extension rejection. Continue
  to defer the complete retirement and dual-host gates until the goal's end.

## Evidence and consequences

The retained producer identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Producer WVB | 35,685 | `dd20dfbcaa0cd9749da77116e8ec38fc48b4f7175fd646489221414c51c4358c` |
| Windows x64 application | 387,584 | `80dd0c525f4bf8cf97743852b4e874eddcea7799a5dc98cff4845b97b409580a` |
| Linux x64 application | 389,120 | `fa385758a5e167e5cf489e84a50efd34f85be9fbdeefb391e3292285554ba945` |
| Generated WVO | 483 | `9caeb7ce353bca33e3bbac729ecca0423d59f8ce6b65ccd6b54fa53c381d617c` |

After reviewing the affected tests, the Windows producer filter passes 3/3 and
the normal `os-probe` filter passes 2/2. The generated WVO is byte-identical to
the removed frozen object, and the final EFI remains 683,008 bytes at SHA-256
`080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9`.

The frozen seed now contains eight WVOs totaling 664,524 bytes. Three ordinary
objects come from Windvale-native producers, three more come from native WVA,
and the fourteen-object link order remains unchanged. The retirement plan now
contains 28 suites and 3,130 fixed cases.

Linux execution and every broad Seed, OS, QEMU, Standard, Qualification, and
complete retirement gate remain pending. The initial system-profile compilation
and generic-dispatcher experiments used Stage 0 only as a diagnostic oracle;
Stage 0 produced no maintained artifact in this slice.

## Reconsideration triggers

Reconsider the focused producer when the native compiler accepts the complete
kernel system profile, when WVA gains a deliberately specified privileged x64
instruction contract for independent reasons, or when cross-host execution does
not reproduce the retained object and EFI identities.
