# Windvale OS WVB admission

## Status and scope

WVB admission version 1 is the implemented candidate owned by [Decision 0090](../Documents/Decisions/0090-First-In-Guest-Wvb-Admission.md). It proves that AOT Windvale code running inside the guest validates one embedded canonical WVB before the native derivative of those exact bytes executes.

This is a fixed bootstrap admission profile, not the general semantic WVB verifier, a loader, an interpreter, a JIT, or a stable public ABI.

## Admitted module

The sole admitted input is the canonical WVB 1.6 produced from `Operating-System/Kernel/Embedded-Wvb-Program.wv`:

| Property | Required value |
| --- | --- |
| Module identity | `Embeddedˉwvbˉprogram` |
| Profile | portable |
| Capabilities | 0 |
| Data declarations | 0 |
| Functions | one `Main() -> i32` |
| Code | reachable constant 29, local store/load, return |
| Exports | one function export `Main` |
| Nominal types | 0 |
| Bytes | 174 |
| SHA-256 | `7f08efbb20c6cc69c100f07407f759625b38c02a3f05bb4e8dabcc7bdd10c4e2` |

The exact bytes occur as immutable `Embeddedˉmodule` data in `Wvb-Admission.wv`. `Expectedˉmodule` is the Windvale-owned accepted identity. The Stage 0 builder recompiles `Embedded-Wvb-Program.wv` and refuses image construction unless its canonical output is byte-identical to the checked-in candidate data.

## Windvale admission policy

`Wvb-Admission.wv` is portable, capability-free Windvale compiled to canonical WVB and then AOT through the shared x86-64 backend. Its exported source entry returns:

- `73` after all admission checks succeed; or
- `0` when the candidate length, header, version, section count, any section envelope, or any canonical byte differs.

The verifier first requires exactly 174 bytes, `WVB1`, WVB version 1.6, and seven sections. It then requires the exact canonical kind/flags/reserved words and payload lengths at offsets 12, 47, 59, 71, 113, 137, and 162. Finally, a bounded loop compares all 174 candidate bytes with the accepted identity. Fixed input length is established before any read. The successful reference path executes exactly 8,944 WVB instructions with maximum dynamic call depth 2.

Changed magic, changed first-section length, changed code constant, and one-byte truncation must return rejection. The code-constant mutation remains structurally plausible and therefore proves that exact identity is checked in addition to the outer envelopes.

## AOT symbols and call order

The ordinary native backend accepts one source export named `Main`. Stage 0 rewrites only the verified WVO external symbol table, reorders symbols canonically, and remaps relocation symbol indices:

| Source module | Boot-image export |
| --- | --- |
| `Wvbˉadmission.Main` | `Windvale_kernel_wvb_admit` |
| `Embeddedˉwvbˉprogram.Main` | `Windvale_kernel_embedded_main` |

The 163-byte bridge body exported as `Windvale_kernel_x64_wvb_admission` performs this exact sequence:

1. Preserve the kernel handoff pointer.
2. Construct native execution context version 7 with instruction budget 8,948, call-depth budget 2, and every service/resource pointer and capacity zero.
3. Call `Windvale_kernel_wvb_admit`; continue only when `RAX == 73`.
4. Reload the context pointer and call `Windvale_kernel_embedded_main`; continue only when `RAX == 29`.
5. Restore the handoff and tail-transfer to retained `Windvale_kernel_x64_native_probe`.
6. On any mismatch or native trap, return failure 1 without making a later call.

The bridge is a Stage 0 machine emitter and replacement seam. Admission policy lives in Windvale. The later portable probe and system-profile kernel Main retain their prior contracts and run only after this sequence succeeds.

## Exact candidate artifacts

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Embedded program WVB | 174 | `7f08efbb20c6cc69c100f07407f759625b38c02a3f05bb4e8dabcc7bdd10c4e2` |
| Admission WVB | 2,786 | `231a4001dc316ae965a851aa27eabacaba7ef57d4f72d18ee0e7eaa4d90d2e54` |
| Embedded program WVO | 504 | `461361ba8853faa59d7b8f841308fd88b5e7ee837a2654ab3e534771c189a834` |
| Admission WVO | 24,445 | `5b11e97e5bb9746daa911559ea9a7a204419fe2cded44977163430185e7d150d` |
| Admission bridge WVO | 481 | `eb229f4fbf104c67e3402280016355da87a3bda51ffcb361c07d709815060f39` |

These identities are candidate evidence until exact Windows/Debian qualification is recorded.

## Non-claims and next boundary

Version 1 does not accept arbitrary valid WVB, produce general diagnostics, retain a decoded module model, validate capabilities or complex control flow generically, select cached native code, or publish executable pages. It does not isolate the admitted program: both verifier and program run as trusted AOT ring-0 boot components.

The next general-verifier revision must use checked offset arithmetic, bounded counts and strings, complete instruction decoding, branch-boundary validation, stack/type agreement, capability validation, canonical trailing-byte rejection, and deterministic diagnostics. The next OS architecture slice remains a protected process/thread/capability/IPC boundary so an admitted ordinary module can execute outside the kernel.
