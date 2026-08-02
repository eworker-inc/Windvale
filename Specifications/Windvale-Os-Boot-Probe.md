# Windvale OS firmware boot probe

## Status and purpose

Firmware probe 30 is the implemented process-reclamation proof. It composes ABI 17/context 7, WVA seam 8, admission bridge 2, retained bridge 10, memory 7, paging 3, protected processes 9, interpreter profile 4, `WVRES004`, and `WVBR002`.

[Decision 0100](../Documents/Decisions/0100-First-Reclaimed-And-Reused-Process-Root.md) owns Probe 30. Focused Windows evidence passes all 25 OS tests and all four pinned-QEMU scenarios; cross-host qualification is pending. [Decision 0098](../Documents/Decisions/0098-First-Typed-Two-Resource-Lookup.md) retains the qualified Probe-29 baseline.

The firmware ABI follows UEFI 2.11 x64 calling conventions and `GetMemoryMap`/`ExitBootServices`. These host mechanics do not define portable Windvale semantics.

## Artifact construction

The builder compiles system-profile `Hello-World.wv`; portable admission, process-policy, init, embedded-program, and retained-native modules; hosted `Bytecode-Interpreter.wv`; and the kernel, init, typed-resource, and client WVA shims. Portable modules pass through canonical WVB, mandatory verification, ABI-17 native selection/fragment verification, and WVO. Stage 0 rewrites only verified link-facing symbols.

Stage 0 also creates the loader, memory, exception, paging, admission bridge, process machine, retained bridge, and x64 byte adapter. It independently reconstructs the exact 347-byte typed lookup leaf and requires byte equality with the WVA stencil before publishing it as code. The linker reconstructs the base-zero image and passes verified code/read-only data to UEFI application writer 3.

The linked kernel payload starts at entry offset zero and fits the fixed 256 KiB supervisor RX window. The init image fits one user RX page; the client image fits 33 user RX pages. Generated WVB, WVO, EFI, firmware, maps, and virtual disks are not committed.

Key qualified identities are recorded in [Windvale-Protected-Process.md](Windvale-Protected-Process.md) and [Windvale-Os-Bytecode-Interpreter.md](Windvale-Os-Bytecode-Interpreter.md). Complete image identities are:

| Scenario | EFI bytes | SHA-256 | Expected host code |
| --- | ---: | --- | ---: |
| `normal` | 261,120 | `5034c01a98f20344d96fa091fd9a55a303e72669d746a4b83df2900eed93992f` | 0 |
| `invalid-opcode` | 261,120 | `bb57ebf7e50eb56bf3d42d91b2213ed5b262554416fdf76609142eccba44cc55` | 3 |
| `general-protection` | 261,120 | `d56fe572fb7a7ff724f7b7c26aa5299a6c5cee4c203f009b63d651c1d3cd8fcc` | 3 |
| `user-fault` | 261,632 | `78dfa73a80a05021273cb44587f6b957d16d4cd4ebaec487f7b8a8f5427846ca` | 0 |

## Firmware exit and kernel entry

The loader validates the UEFI system/boot-service tables, obtains a bounded memory map, and retries `ExitBootServices` at most three times. A stale key permits only a bounded refresh. No firmware service is used after successful exit.

The loader overlays exact `WVKHAND1` and enters the kernel. Memory 7 selects and clears a 2 MiB-aligned 63-page arena below 4 GiB, publishes `WVKMEM07`, copies the handoff, and switches to the measured four-page stack. Page 5 becomes the IDT; paging 3 consumes pages 6 through 11 and activates the unchanged low-1-GiB W^X root.

Admission bridge 2 retains context budgets 8,944/2 and requires token 73. The protected-process path then:

1. runs Windvale process policy and requires token `97`;
2. reconstructs the aligned arena base from the owned stack and revalidates `WVKMEM07` after the generated policy call;
3. allocates pages 12 through 20 for init and 21 through 62 for the client;
4. creates two roots, two init-owned RO/NX resource pages, two absent client target PTEs, `WVPROC09`, `WVCHAN01`, and two `WVRES004` records;
5. enters init, which passes ordered set token `131073` to syscall `4` and then waits;
6. atomically installs both aliases and publishes service table 5 plus `WVBR002`;
7. enters the client, which reads `boot:main.wvb` and `boot:main.budget`, enforces budget `4`, interprets four opcodes to `29`, and sends the result;
8. on exit or contained fault, revalidates and clears both aliases and the complete publication, then reloads init's CR3;
9. zeroes and releases the exact 42-page allocator tail, reallocates the identical root, and rebuilds it as client generation `2`;
10. wakes init, which receives the first result, performs a generation-2 grant, and waits again;
11. enters generation 2, repeats interpretation/result `29`, and performs generation-matched cleanup after exit or contained fault; and
12. wakes init, requires its fifth-syscall exit result `29`, runs the retained native probe, and reaches compiler-generated Main.

The CPL0 invalid-opcode and general-protection scenarios occur after both processes and Main complete, so they retain terminal panic behavior. The user-fault scenario executes `CLI` in process `2`; vector 13/error 0 is contained and the same two-resource cleanup completes.

## Exact serial evidence

Normal success requires:

```text
windvale-os-boot 30
entry=pass
system-table=pass
memory-map=pass
boot-services=exited
memory-owned=pass
allocator=pass
kernel-stack=pass
paging=owned
wvb-admission=pass
processes=isolated
resource-grant=pass
typed-resources=pass
resource-revoked=pass
process-reuse=pass
wvb-runtime=interpreted
init-service=pass
ipc=cross-process
Hello from Windvale
cpu-exceptions=armed
native-context=pass
native-wvb=pass
windvale-source=pass
status=pass
shutdown=poweroff
```

The user-fault scenario inserts `user-fault=contained` before `status=pass`. The two CPL0 fault scenarios share the prefix through `Hello from Windvale` and end respectively with vector 6 or 13, error code 0, and `status=panic`.

Kernel panic writes value 1 to QEMU test port `0xF4`, producing host code 3. Normal and contained-user-fault success use the Q35 shutdown adapter and produce host code 0. The complete serial marker is mandatory because host code alone is ambiguous.

## Boot harness

From the repository root:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Os-Boot.ps1 -Scenario normal
pwsh -NoProfile -File Tools/Verify/Verify-Os-Boot.ps1 -Scenario invalid-opcode
pwsh -NoProfile -File Tools/Verify/Verify-Os-Boot.ps1 -Scenario general-protection
pwsh -NoProfile -File Tools/Verify/Verify-Os-Boot.ps1 -Scenario user-fault
```

The harness preflights the pinned QEMU/OVMF environment, creates run-private media and variable-store state, launches `pc-q35-11.0,accel=tcg` with one CPU and 128 MiB, captures serial, checks exact scenario output and exit code, and rechecks input identities. The default timeout is 60 seconds. Temporary artifacts are deleted unless `-KeepRunDirectory` is supplied.

Normal path-free report shape:

```text
windvale-os-boot-report 30
status=pass
scenario=normal
architecture=x86-64
application-format=pe32-plus-uefi-application-v3
probe-version=30
efi-bytes=261120
efi-sha256=5034c01a98f20344d96fa091fd9a55a303e72669d746a4b83df2900eed93992f
serial-marker=windvale-os-boot-30-entry-system-table-memory-map-boot-services-exited-memory-owned-allocator-kernel-stack-paging-owned-wvb-admission-processes-isolated-resource-grant-pass-typed-resources-pass-resource-revoked-pass-process-reuse-pass-wvb-runtime-interpreted-init-service-pass-ipc-cross-process-hello-cpu-exceptions-armed-native-context-native-wvb-windvale-source-status-pass-shutdown-poweroff
qemu-exit-code=0
```

## Harness failures

| Code | Meaning |
| --- | --- |
| `WVOS3001` | Host or probe build failed. |
| `WVOS3002` | QEMU could not start. |
| `WVOS3003` | Bounded timeout expired. |
| `WVOS3004` | Unexpected QEMU exit code. |
| `WVOS3005` | Missing, duplicate, or conflicting serial evidence. |
| `WVOS3006` | Generated EFI or installed firmware input changed. |
| `WVOS3007` | Temporary cleanup failed its absolute-path boundary. |

## Deliberate non-claims

Probe 30 does not authenticate firmware CRCs, own general physical memory, release non-tail extents, load arbitrary WVB, provide complete verification, publish executable memory, JIT, schedule generally, transfer capabilities, enumerate dynamic resources, provide independent resource lifetimes, implement filesystems/packages/network/devices, handle SMP shootdown, or qualify Hyper-V/physical hardware. The two names, kinds, owner, generations, order, and lifetime remain fixed; Stage 0 machine emitters remain explicit replacement seams.
