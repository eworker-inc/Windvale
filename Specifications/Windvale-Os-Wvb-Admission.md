# Windvale OS WVB admission

## Status and scope

WVB admission version 1 is the cross-host-qualified fixed policy owned by [Decision 0090](../Documents/Decisions/0090-First-In-Guest-Wvb-Admission.md). It proves that AOT Windvale code running inside the guest validates one embedded canonical WVB before any accepted execution path consumes it. Admission bridge version 1 historically executed the program's native derivative directly at ring 0. Bridge version 2 invokes the [protected-process contract](Windvale-Protected-Process.md) and is cross-host qualified through probe 25. Candidate probe 26 maps the same admitted bytes as a separate RO/NX boot resource consumed by [Windvale interpreter profile 3](Windvale-Os-Bytecode-Interpreter.md).

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

Qualified bridge version 1 has a 163-byte body and performs the direct ring-0 call recorded by Decision 0090. Probes 22 through 25 use bridge version 2 with a 162-byte body exported as `Windvale_kernel_x64_wvb_admission`:

1. Preserve the kernel handoff pointer.
2. Construct native execution context version 7 with instruction budget 8,944, call-depth budget 2, and every service/resource pointer and capacity zero.
3. Call `Windvale_kernel_wvb_admit`; continue only when `RAX == 73`.
4. Reload the handoff pointer and call `Windvale_kernel_x64_process_enter`; that path independently requires the exact admitted identity and continues only after CPL3 returns exact result 29 or its explicitly admitted contained-fault result. Probe 26 obtains that result after the user-space interpreter fetches the admitted bytes from a separate RO/NX boot-resource page; neither `Windvale_kernel_embedded_main` nor the complete WVB is present in the linked client RX image.
5. Restore the handoff and tail-transfer to retained `Windvale_kernel_x64_native_probe`.
6. On any mismatch or native trap, return failure 1 without making a later call.

The bridge is a Stage 0 machine emitter and replacement seam. Admission policy and process policy live in Windvale; WVA owns user entry and syscall encoding. The later portable probe and system-profile kernel Main retain their prior contracts and run only after this sequence succeeds.

## Exact qualified artifacts

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Embedded program WVB | 174 | `7f08efbb20c6cc69c100f07407f759625b38c02a3f05bb4e8dabcc7bdd10c4e2` |
| Admission WVB | 2,786 | `231a4001dc316ae965a851aa27eabacaba7ef57d4f72d18ee0e7eaa4d90d2e54` |
| Embedded program WVO | 504 | `461361ba8853faa59d7b8f841308fd88b5e7ee837a2654ab3e534771c189a834` |
| Admission WVO | 24,445 | `5b11e97e5bb9746daa911559ea9a7a204419fe2cded44977163430185e7d150d` |
| Admission bridge WVO | 481 | `eb229f4fbf104c67e3402280016355da87a3bda51ffcb361c07d709815060f39` |

Exact commit `860c69c` reproduces these identities under all 21 OS tests on Windows and Debian and all three pinned-QEMU scenarios. Independent GitHub Windows/Linux verification also passes.

Probes 22 through 26 preserve the first four artifact identities and replace only the bridge composition. Bridge version 2 is 484 bytes with SHA-256 `7b53fc11e4e99966386994c247c3a2a19f99ef8da751dbd9dc53f5575871a00d`; its 162 code bytes contain three exact relative calls/transfers to admission, protected process, and the retained native probe. Exact commit `190174a01299369fb855e27ea676d34062e09c5b` cross-host qualifies the bridge with all 67 Seed tests and all 25 OS tests. Probe 26 retains it byte-identically.

## Non-claims and next boundary

Version 1 does not accept arbitrary valid WVB, produce general diagnostics, retain a decoded module model, validate capabilities or complex control flow generically, select cached native code, or publish executable pages. Probe 26 isolates the interpreter and Windvale service under separate CPL3 roots, supplies the admitted WVB through a separate immutable boot resource, and derives WVB sections inside the interpreter, but the fixed admission verifier remains a trusted AOT ring-0 boot component and Stage 0 still constructs the process images and resource mapping. The admitted program's AOT derivative is built as deterministic reference evidence but is no longer linked into its guest execution path.

The next general-verifier revision must use checked offset arithmetic, bounded counts and strings, complete instruction decoding, branch-boundary validation, stack/type agreement, capability validation, canonical trailing-byte rejection, and deterministic diagnostics. The next OS slice should replace one fixed assumption with measured evidence: move resource ownership/transfer toward the init or package-service boundary, broaden interpreter semantics only when a real module requires them, or add a third runnable only when it creates scheduling pressure. JIT publication remains a separate capability and W^X decision.
