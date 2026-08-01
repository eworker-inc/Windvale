# Decision 0064: First shared native WVB in Windvale OS

- Date: 2026-07-31
- Status: Implemented on the pinned Windows QEMU environment; exact-candidate qualification pending
- Depends on: [Decision 0063](0063-Shared-Budget-Native-Calls-And-Static-Data.md)'s cross-host ABI-5 call/data boundary
- Refines: [Decision 0056](0056-Windvale-Owned-Post-Memory-Evidence.md)'s special kernel-native seam

## Context

Windvale OS already runs compiler-generated `.wv` after firmware shutdown on a kernel-owned stack, but that code uses the special `x86-64-kernel-entry-wvo-v2` target rather than the shared portable-WVB native backend. Decision 0063 now supplies enough general ABI coverage—typed internal calls, exact instruction/depth budgets, loops, immutable i32 arrays, bounds traps, and relocatable `.rodata`—to consume one ordinary portable module in the OS without waiting for capabilities, general memory, or a complete Windvale-written runtime.

The next proof must execute the selected code, not merely link unused bytes. It must preserve the existing handoff and source-owned kernel evidence, reject a native trap or wrong result before those markers can succeed, and avoid implying that the OS can already load or verify WVB at runtime.

## Decision

- Add `Operating-System/Kernel/Native-Wvb-Probe.wv` as an ordinary `portable` module. It loops over immutable i32 data `[3, 5, 8, 13]`, calls a two-parameter `Add` function for each element, and returns `29`.
- Compile that source through the ordinary Seed compiler into canonical verified WVB, then lower that verified module through the exact Decision 0063 `x86-64-wvb-baseline-v5` backend and WVO sink. Do not add an OS-specific source recognizer or second instruction selector.
- Require the selected program to execute in exactly 203 WVB instructions with maximum active call depth 2. Pin those values in the bridge contract and independently confirm the instruction count with the reference interpreter.
- Add one separately verified 57-byte x86-64 bridge. It preserves the copied-handoff pointer, supplies the exact instruction/depth budgets in `RDX` and `R9`, calls native export `Main`, accepts only packed `RAX == 29`, and otherwise returns failure. On success it restores the handoff and tail-transfers to the existing compiler export `Windvale_kernel_main`.
- Advance the WVA kernel seam to version 3. `Windvale_kernel_wva_main` now tail-transfers to the native-WVB bridge rather than directly to the special compiler Main; the outbound byte adapter remains WVA-owned.
- Advance the UEFI application adapter to version 3. Admit only code and immutable read-only-data input, retain at least one code section, and continue requiring base zero plus only resolved `relative-i32` relocations. Preserve the complete flat image in one non-writable readable/executable PE `.text` segment so existing relative references remain exact. Writable data, zero-fill, absolute relocations, and general PE section mapping remain rejected.
- Advance the firmware probe to version 7. Link the loader, special kernel object, ABI-5 native object, kernel memory object, WVA seam, native bridge, and x64 byte adapter as seven independently verified WVO inputs. Emit `native-wvb=pass` only after the aggregate kernel call returns zero; that return is unreachable unless the native probe produced packed result 29 and the existing system-profile Main also succeeded.
- Keep the existing pinned QEMU/OVMF environment, post-firmware failure path, deterministic PE verification, exact serial transcript, and guest-controlled completion gate.

## Initial evidence

The portable probe is a deterministic 502-byte WVB with SHA-256 `1f384f77c4e1c718a331aaa1a3c1f1e4173bbae9d870ec9023d70c7b15c1f7ef`. Its ABI-5 object is 2,296 bytes with SHA-256 `338d05395502cc34dc5ac1a99626e0507faf3503ae7d5a3016c52ac140139ee5`; it has separate `.text` and `.rodata`, one data relocation, internal calls, and bounded loops. The 269-byte bridge object has SHA-256 `b345f42813fb5a20829a28882e03820a05e815982689478dc2b17ac593dca88d`. The version-3 WVA seam is 291 bytes with SHA-256 `332a0158c51e81d1beb5d212f508649c8efe2874af712d6d8ef15929ffd438fc`.

All 15 focused OS tests pass with a zero-warning Release build, including deterministic WVB/WVO/bridge evidence, exact interpreter instruction count, UEFI read-only-data acceptance, unsupported-section rejection, and complete firmware-image reconstruction. The pinned QEMU 11.0 environment boots the 9,728-byte EFI image with SHA-256 `16c225916be855ca0aa27bcdacb56e38c08b79e2270f12e9040bffe343873fb3` and emits the complete version-7 success transcript. Exact candidate identity and final qualification evidence remain to be recorded.

## Consequences

One ordinary portable Windvale program now crosses source, WVB verification, the same ABI-5 native backend qualified on Windows and Linux, WVO linking, PE packaging, firmware exit, and execution on a kernel-owned Windvale OS stack. Its immutable data and internal calls are real runtime inputs, and its packed result gates continued boot evidence.

This is AOT consumption, not a bytecode runtime in the OS. C#/.NET still compiles and packages the image on the host; the guest does not decode or verify WVB, select instructions, allocate a process, expose capabilities, or load a module dynamically. The special system-profile compiler target still owns source-selected console lines after the native probe. Decision 0057's native-retirement gate remains open.

The single PE `.text` segment is readable/executable and not writable. Carrying immutable data inside that segment is a bounded version-3 adapter rule, not a general claim that data should be executable. A future section-aware PE/ELF/Windvale loader can split code and `.rdata` only with exact placement and relocation verification.

## Reconsider when

- The OS gains a WVB decoder/verifier and can select or load this module after boot rather than embedding its AOT result.
- Runtime services or capabilities let the portable module own observable output directly.
- General PE/ELF section mapping can preserve exact relative and future absolute relocations with separate page permissions.
- The special kernel target can be retired in favor of the shared ABI plus narrow WVA services.
