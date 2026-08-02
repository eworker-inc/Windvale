# Windvale OS WVB admission

## Status and scope

WVB admission version 3 is the qualified Probe-31 contract owned by [Decision 0101](../Documents/Decisions/0101-First-Exact-Wvb-Across-Three-Environments.md). It proves that AOT Windvale code running inside the guest validates the exact canonical WVB compiled from [`Examples/Seed/Sum-Data.wv`](../Examples/Seed/Sum-Data.wv) before protected execution consumes it. Exact implementation commit `f3eca7c8dab290e3916fbf33dcabc41d685a91bb` passes complete Windows/Debian qualification in GitHub [Verify run 30753663882](https://github.com/eworker-inc/Windvale/actions/runs/30753663882).

Version 1 and the 174-byte historical fixture remain qualified under [Decision 0090](../Documents/Decisions/0090-First-In-Guest-Wvb-Admission.md). Admission bridge version 2 remains the process-entry composition introduced by Probe 22.

This is fixed bootstrap admission, not the general semantic WVB verifier, loader, interpreter, JIT, or public ABI.

## Admitted module

The sole admitted input is canonical WVB 1.6 produced from `Examples/Seed/Sum-Data.wv`:

| Property | Required value |
| --- | --- |
| Module identity | `Sumˉdata` |
| Profile | portable |
| Capabilities/types | none |
| Data | immutable `Values = [3, 5, 8, 13]` |
| Functions | `Add(i32, i32) -> i32`; `Main() -> i32` |
| Code | bounded loop, data load, four internal calls, integer add/compare, branches, return |
| Export | exactly function `Main` |
| Result/instructions | `29` after exactly 203 guest instructions |
| Bytes | 493 |
| SHA-256 | `6f3a272d37dd8893995c7f85c236414ed2864bf59de2f3775c08afd426013f8c` |

The exact bytes occur as immutable `Embeddedˉmodule` data in `Operating-System/Kernel/Wvb-Admission.wv`. `Expectedˉmodule` is the Windvale-owned accepted identity. The Stage 0 builder compiles the canonical example and refuses image construction unless its output is byte-identical to that data.

## Windvale admission policy

`Wvb-Admission.wv` is portable, capability-free Windvale compiled to canonical WVB and AOT through the shared ABI-17 x86-64 backend. Its exported entry returns token `73` only after exact success and `0` for any candidate mismatch.

The verifier requires exactly 493 bytes, magic `WVB1`, WVB version 1.6, and seven canonical sections. It checks ordered kind/flags/reserved words and these payload envelopes:

| Kind | Section offset | Payload bytes |
| ---: | ---: | ---: |
| 1 | 12 | 14 |
| 2 | 34 | 4 |
| 3 | 46 | 35 |
| 4 | 89 | 81 |
| 5 | 178 | 270 |
| 6 | 456 | 17 |
| 7 | 481 | 4 |

A bounded loop then compares all 493 candidate bytes with the accepted identity. Fixed length is established before any read. The successful admission path executes exactly 24,256 native-WVB instructions with maximum dynamic call depth 2.

Changed magic, section shape, data value, and one-byte truncation must reject. The data mutation remains structurally valid and executes to `28` under the ordinary runtime, proving that admission binds exact program identity rather than only the envelope.

## AOT symbols and call order

Stage 0 renames only verified WVO export symbols:

| Source module | Boot-image export |
| --- | --- |
| `Wvbˉadmission.Main` | `Windvale_kernel_wvb_admit` |
| `Sumˉdata.Main` | `Windvale_kernel_embedded_main` |

The retained 162-code-byte admission bridge:

1. builds native context 7 with budget 24,256, call depth 2, and no services;
2. calls `Windvale_kernel_wvb_admit` and requires `73`;
3. calls the protected-process entry, which independently requires exact program identity and result `29` for both client generations;
4. tail-transfers to the retained native probe; and
5. returns failure 1 on any mismatch or native trap.

The canonical program's AOT derivative is retained only as deterministic differential evidence. It is not linked into either client execution path.

## Deterministic qualified artifacts

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Canonical program WVB | 493 | `6f3a272d37dd8893995c7f85c236414ed2864bf59de2f3775c08afd426013f8c` |
| Admission WVB | 3,424 | `fde22db922a283c11c56b6802587398172e0b03e7580d99e91fcf95e189f8629` |
| Canonical program WVO | 3,585 | `fa67f716dd1b64d8a78fe0f67ae8deef1785e34c99344574b0ce207b57a9cf9e` |
| Admission WVO | 25,083 | `798d10bb9c6219b8d299d459d202f13a8fdd7e195d95c1cb80db8d06183634de` |
| Admission bridge WVO | 484 | `cc03a88843382f2eba5e0de6d8b88af156c214ca707887a222fed65448032d33` |

## Non-claims and next boundary

Version 3 does not accept arbitrary valid WVB, produce general diagnostics, retain a general decoded module model, select cached native code, or publish executable pages. The exact admission verifier remains a trusted AOT ring-0 boot component, while the admitted program is separately validated by interpreter profile 5 at CPL3.

A future general-verifier revision still requires checked offset arithmetic, bounded counts and strings, complete instruction decoding, branch-boundary validation, stack/type agreement, capability validation, canonical trailing-byte rejection, and deterministic diagnostics. Another real program should choose the next semantic extension.
