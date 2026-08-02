# Windvale OS firmware boot probe

## Status and purpose

Firmware probe 29 is the current cross-host-qualified typed-resource proof. It composes ABI 16/context 7, WVA seam 8, admission bridge 2, retained bridge 10, memory 6, paging 3, protected processes 8, interpreter profile 4, `WVRES003`, and `WVBR002`.

[Decision 0098](../Documents/Decisions/0098-First-Typed-Two-Resource-Lookup.md) owns Probe 29. Exact implementation commit `3fd9ef7535d7536ed084144e4f697cda548bf35c` passes all 67 Seed tests, all 25 OS tests, and the complete non-Fast verifier on Windows and digest-pinned Debian 12 in GitHub [Verify run 30745623111](https://github.com/eworker-inc/Windvale/actions/runs/30745623111). All four pinned-QEMU scenarios pass on Windows.

The firmware ABI follows UEFI 2.11 x64 calling conventions and `GetMemoryMap`/`ExitBootServices`. These host mechanics do not define portable Windvale semantics.

## Artifact construction

The builder compiles system-profile `Hello-World.wv`; portable admission, process-policy, init, embedded-program, and retained-native modules; hosted `Bytecode-Interpreter.wv`; and the kernel, init, typed-resource, and client WVA shims. Portable modules pass through canonical WVB, mandatory verification, ABI-16 native selection/fragment verification, and WVO. Stage 0 rewrites only verified link-facing symbols.

Stage 0 also creates the loader, memory, exception, paging, admission bridge, process machine, retained bridge, and x64 byte adapter. It independently reconstructs the exact 347-byte typed lookup leaf and requires byte equality with the WVA stencil before publishing it as code. The linker reconstructs the base-zero image and passes verified code/read-only data to UEFI application writer 3.

The linked kernel payload starts at entry offset zero and fits the fixed 256 KiB supervisor RX window. The init image fits one user RX page; the client image fits 33 user RX pages. Generated WVB, WVO, EFI, firmware, maps, and virtual disks are not committed.

Key qualified identities are recorded in [Windvale-Protected-Process.md](Windvale-Protected-Process.md) and [Windvale-Os-Bytecode-Interpreter.md](Windvale-Os-Bytecode-Interpreter.md). Complete image identities are:

| Scenario | EFI bytes | SHA-256 | Expected host code |
| --- | ---: | --- | ---: |
| `normal` | 258,048 | `a8a14581eab4c1a6d67aba7af0cec1baa956574a410a4cd0de1121e1f843ee67` | 0 |
| `invalid-opcode` | 258,048 | `35ee08e97aff4f6a2c0018c962960d5c7ee8af58fe6d5b36565613a99292ad0f` | 3 |
| `general-protection` | 258,048 | `92ae33986ab53245f57dc9263179e6dcd2c66cf79b634dcedaee51e93f915ca7` | 3 |
| `user-fault` | 258,560 | `35a3dece4e64463bc9df7ef73c83ec5f5fff3b0daedd7176f77f1c2ef5525484` | 0 |

## Firmware exit and kernel entry

The loader validates the UEFI system/boot-service tables, obtains a bounded memory map, and retries `ExitBootServices` at most three times. A stale key permits only a bounded refresh. No firmware service is used after successful exit.

The loader overlays exact `WVKHAND1` and enters the kernel. Memory 6 selects and clears a 2 MiB-aligned 63-page arena below 4 GiB, publishes `WVKMEM06`, copies the handoff, and switches to the measured four-page stack. Page 5 becomes the IDT; paging 3 consumes pages 6 through 11 and activates the unchanged low-1-GiB W^X root.

Admission bridge 2 retains context budgets 8,944/2 and requires token 73. The protected-process path then:

1. runs Windvale process policy and requires token `97`;
2. reconstructs the aligned arena base from the owned stack and revalidates `WVKMEM06` after the generated policy call;
3. allocates pages 12 through 20 for init and 21 through 62 for the client;
4. creates two roots, two init-owned RO/NX resource pages, two absent client target PTEs, `WVPROC08`, `WVCHAN01`, and two `WVRES003` records;
5. enters init, which passes ordered set token `131073` to syscall `4` and then waits;
6. atomically installs both aliases and publishes service table 5 plus `WVBR002`;
7. enters the client, which reads `boot:main.wvb` and `boot:main.budget`, enforces budget `4`, interprets four opcodes to `29`, and sends the result;
8. on exit or contained fault, revalidates and clears both aliases and the complete publication, then reloads init's CR3;
9. wakes init, requires its exit result `29`, runs the retained native probe, and reaches compiler-generated Main.

The CPL0 invalid-opcode and general-protection scenarios occur after both processes and Main complete, so they retain terminal panic behavior. The user-fault scenario executes `CLI` in process `2`; vector 13/error 0 is contained and the same two-resource cleanup completes.

## Exact serial evidence

Normal success requires:

```text
windvale-os-boot 29
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
windvale-os-boot-report 29
status=pass
scenario=normal
architecture=x86-64
application-format=pe32-plus-uefi-application-v3
probe-version=29
efi-bytes=258048
efi-sha256=a8a14581eab4c1a6d67aba7af0cec1baa956574a410a4cd0de1121e1f843ee67
serial-marker=windvale-os-boot-29-entry-system-table-memory-map-boot-services-exited-memory-owned-allocator-kernel-stack-paging-owned-wvb-admission-processes-isolated-resource-grant-pass-typed-resources-pass-resource-revoked-pass-wvb-runtime-interpreted-init-service-pass-ipc-cross-process-hello-cpu-exceptions-armed-native-context-native-wvb-windvale-source-status-pass-shutdown-poweroff
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

Probe 29 does not authenticate firmware CRCs, own general physical memory, reclaim pages or roots, load arbitrary WVB, provide complete verification, publish executable memory, JIT, schedule generally, transfer capabilities, enumerate dynamic resources, provide independent resource lifetimes, implement filesystems/packages/network/devices, handle SMP shootdown, or qualify Hyper-V/physical hardware. The two names, kinds, owner, borrower, order, and lifetime remain fixed; Stage 0 machine emitters remain explicit replacement seams.
