# Decision 0447: Native Probe 40 admission source producer

- Status: Implemented current-host native-build candidate; Linux execution pending
- Date: 2026-08-09
- Advances: [Decision 0446](0446-Native-Probe-40-Windvale-Source-Producer.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale WVO export renamer](../../Specifications/Windvale-Wvo-Export-Renamer.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

The ordinary Probe 40 build still consumed the frozen Stage 0
`02-wvb-admission-native.wvo`, although its implementation already existed as
canonical Windvale source. The native compiler and lowerer reproduced its code
and object structure, but exported `Main`; the boot link contract requires
`Windvale_kernel_wvb_admit`. Extending WVA with raw code bytes would have widened
the language and assembler contracts merely to reproduce this object, so that
route was rejected.

## Decision

- Add one focused Windvale-native WVO export-renaming tool. It admits the whole
  input object, requires exactly one matching exported function, rewrites only
  that string, and admits the complete result before writing it.
- Bind the Windows and Linux launchers to exact paired native packages and refuse
  an existing destination. Keep the tool general enough for a named WVO export,
  without making it an arbitrary object mutator.
- Compile `Operating-System/Kernel/Wvb-Admission.wv` through the native Project 1
  front door, lower it natively, rename `Main` to
  `Windvale_kernel_wvb_admit`, and require the exact retained link-facing WVO.
- Remove `02-wvb-admission-native.wvo` from the frozen Probe 40 seed. Keep the C#
  compiler/backend and recovery builder frozen for regeneration and differential
  evidence; no source semantics move back into Stage 0.
- Add four focused cases: exact positive output, missing-export rejection,
  invalid-name rejection, and existing-output preservation.

## Evidence and consequences

The ordinary native path produces these exact admission identities:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Admission WVB | 4,071 | `69727bb8151aea164690be4f69adcda481532b965d9ae02ec92db21087f3d669` |
| Unrenamed admission WVO | 20,316 | `676a91062e7f1b4483ca9f332b17614a6b75988d21f9ff99caabcbfd51839568` |
| Link-facing admission WVO | 20,337 | `37e47bd2fed0242ad5cae9c9cc684927dc17041d4cd1d154658616be8b140c32` |

The retained renamer package identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| WVB | 37,036 | `7429577711817c534b17bfcb083fd136468a3c33b2fd692e28bf6c3bb1642395` |
| Windows x64 application | 391,680 | `2cf43335af7782676e21ecdd5cb946cb3c9a7309572e21eadac5c7f5d33d2244` |
| Linux x64 application | 393,216 | `c27787ee970d551ad0d85026ee7f9c0ac9de72d933e563398ac356d5561ed0ae` |

After reviewing the affected tests, the Windows `wvo-export-renamer` filter
passes 4/4 in 1.187 seconds and the unchanged `os-probe` filter passes 2/2; the
combined focused run takes 12.006 seconds. The final EFI remains 683,008 bytes
at SHA-256
`080b4d669e9a11fdc802bf7197ae5a044978b6ba39741b2b1c832296987f74d9`.
The seed now contains nine frozen WVOs totaling 665,007 bytes and the ordinary
build supplies two objects from Windvale source plus three native WVA objects.

Linux execution and every broad Seed, OS, QEMU, Standard, and Qualification gate
remain pending. This slice retires the second frozen source producer from the
ordinary build; nine frozen producers and the non-normal scenarios remain.

## Reconsideration triggers

Reconsider this boundary if the object format gains a canonical link-time symbol
alias contract, the compiler can declare the required external export directly,
or cross-host execution fails to reproduce the retained object and EFI identities.
