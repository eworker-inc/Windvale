# Windvale OS WVB admission

## Status and scope

WVB admission version 4 is the exact Probe-32 contract owned by [Decision 0103](../Documents/Decisions/0103-Second-Exact-Wvb-And-Broader-Scalar-Control-Flow.md). The original WVB 1.6 identity was qualified at commit `da938979ae9fe59e5f752bdb81359ded58a0e6ac` in GitHub [Verify run 30758910402](https://github.com/eworker-inc/Windvale/actions/runs/30758910402). The current candidate preserves that narrow semantic profile while rebinding it to the sole canonical WVB 1.11 encoding; new cross-host qualification is required before the updated bytes inherit the earlier claim.

Admission version 3 and exact `Sum-Data.wv` remain qualified history under [Decision 0101](../Documents/Decisions/0101-First-Exact-Wvb-Across-Three-Environments.md). Admission bridge version 2 remains the process-entry composition introduced by Probe 22. This is fixed bootstrap admission, not the general semantic WVB verifier, loader, interpreter, JIT, or public ABI.

## Admitted module

The sole admitted input is canonical WVB 1.11 produced from `Function-Only.wv`:

| Property | Required value |
| --- | --- |
| Module identity | `Sourceˉwvbˉfixture` |
| Profile | portable |
| Capabilities/types/data | none |
| Functions | `Add`, `Main`, `Probe`, and `Select` |
| Code | bool/u8/u32/i32 values, locals, calls, comparisons, and forward branches |
| Export | exactly function `Main` |
| Result/instructions | `6` after exactly 199 guest instructions |
| Bytes | 816 |
| SHA-256 | `28d215b982a7b7185cfa80c4cc5346666bd0181582fe80bec8b7035d514da936` |

The exact bytes occur as immutable `Embeddedˉmodule` data in `Operating-System/Kernel/Wvb-Admission.wv`. `Expectedˉmodule` is the Windvale-owned accepted identity. The Stage 0 builder compiles the existing cross-compiler fixture and refuses image construction unless Stage 0 output, Windvale-compiler output, and embedded data are byte-identical.

## Windvale admission policy

`Wvb-Admission.wv` is portable, capability-free Windvale compiled to canonical WVB and AOT through the shared ABI-17 x86-64 backend. Its exported entry returns token `73` only after exact success and `0` for any candidate mismatch.

The verifier requires exactly 816 bytes, magic `WVB1`, WVB version 1.11, absent module metadata, and seven canonical sections. It checks ordered kind, flags, reserved words, and these exact payload envelopes:

| Kind | Section offset | Payload bytes |
| ---: | ---: | ---: |
| 1 | 12 | 26 |
| 2 | 46 | 4 |
| 3 | 58 | 4 |
| 4 | 70 | 161 |
| 5 | 239 | 532 |
| 6 | 779 | 17 |
| 7 | 804 | 4 |

A bounded loop then compares all candidate bytes with the accepted identity. Fixed length is established before any read. The successful admission path executes exactly 39,760 native-WVB instructions with maximum dynamic call depth 2.

Changed magic, section shape, code, and one-byte truncation must reject. The code mutation at offset `396` remains structurally valid and executes to `9` under the ordinary runtime, proving that admission binds exact program identity rather than only the envelope.

## AOT symbols and call order

Stage 0 renames only verified WVO export symbols:

| Source module | Boot-image export |
| --- | --- |
| `Wvbˉadmission.Main` | `Windvale_kernel_wvb_admit` |
| `Sourceˉwvbˉfixture.Main` | `Windvale_kernel_embedded_main` |

The retained 162-code-byte admission bridge:

1. builds native context 7 with budget 39,760, call depth 2, and no services;
2. calls `Windvale_kernel_wvb_admit` and requires `73`;
3. calls the protected-process entry, which independently requires exact program identity and result `6` for both client generations;
4. tail-transfers to the retained native probe; and
5. returns failure 1 on any mismatch or native trap.

The canonical program's AOT derivative is retained only as deterministic differential evidence. It is not linked into either client execution path.

## Deterministic qualified artifacts

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Canonical program WVB | 816 | `28d215b982a7b7185cfa80c4cc5346666bd0181582fe80bec8b7035d514da936` |
| Admission WVB | 4,068 | `f8f92352abed3c042c6ca6e5cbfd65b650a87837dd252802014b3a787cdb75cf` |
| Canonical program WVO | 6,913 | `269b48d32569b12b0b414c62cc50f70784fc8d90496c520aa58098fbb5f5ba8e` |
| Admission WVO | 25,727 | `cf819ee2f3f108a82ceca2b262b0e5ba6953594b177fef993efe20e41fe3b619` |
| Admission bridge WVO | 484 | `2b5b67bfe04ba87c473d7a9c9fbcc213e864a9bcf39bbd58d0c10f5314aad606` |

Windows and digest-pinned Debian 12 each pass the complete qualification gate with these identities.

## Non-claims and next boundary

Version 4 does not accept arbitrary valid WVB, produce general diagnostics, retain a general decoded module model, select cached native code, or publish executable pages. The exact admission verifier remains a trusted AOT ring-0 boot component, while the admitted program is separately validated by interpreter profile 6 at CPL3.

A future general-verifier revision still requires checked offset arithmetic, bounded counts and strings, complete instruction decoding, branch-boundary validation, stack/type agreement, capability validation, canonical trailing-byte rejection, and deterministic diagnostics. Another real program should choose the next semantic extension.
