# Windvale OS firmware boot probe

## Status and purpose

Firmware Probe 34 is the current implemented candidate for an immutable guest resource store and terminal peer cleanup. It composes ABI 21/context 7, WVA seam 10, admission 4/bridge 2, retained bridge 10, memory 12, paging 4, protected processes 13, channel 3, interpreter profile 6, `WVRES005`, and `WVBR002`.

[Decision 0142](../Documents/Decisions/0142-Immutable-Guest-Resource-Store.md) owns Probe 34. The current candidate passes all 31 bounded OS tests and all four Windows pinned-QEMU scenarios; fresh dual-host qualification remains pending. [Decision 0133](../Documents/Decisions/0133-Frame-Owned-Direct-Native-Records.md) retains the latest cross-host-qualified Probe-32 history.

The firmware ABI follows UEFI 2.11 x64 calling conventions and `GetMemoryMap`/`ExitBootServices`. These host mechanics do not define portable Windvale semantics.

## Artifact construction

The builder compiles system-profile `Hello-World.wv`; portable admission, process-policy, init, exact `Tests/Fixtures/Source-Wvb/Function-Only.wv`, and retained-native modules; hosted `Bytecode-Interpreter.wv`; and the kernel/init/resource/client WVA shims. Portable modules pass through canonical WVB, mandatory verification, ABI-21 native selection and fragment verification, and WVO. Stage 0 rewrites only verified link-facing symbols.

The existing Stage 0 and Windvale-written compilers must emit byte-identical canonical WVB for `Function-Only.wv`, and those bytes must equal the embedded admission identity. The compiler remains at ABI 21.

Stage 0 also creates the loader, memory, exception, paging, admission bridge, process machine, retained bridge, x64 byte adapter, and canonical three-entry `WVRS 1` store. It independently verifies the exact store and complete-image SHA-256 before machine publication. The linker reconstructs the base-zero image and passes verified code and read-only data to UEFI application writer 3.

The linked kernel payload must fit the fixed 768 KiB supervisor RX window. Init fits two user RX pages; the client fits 109 user RX pages. Generated WVB, WVO, EFI, firmware maps, and virtual disks are not committed.

Candidate image identities are:

| Scenario | EFI bytes | SHA-256 | Expected host code |
| --- | ---: | --- | ---: |
| `normal` | 565,760 | `603c193ffacb5272c918d5c931598889cf58d12a80ff9292095b354b3541302c` | 0 |
| `invalid-opcode` | 565,760 | `aa50d9b3836ca4444a434bd0b7d55d230054680149031660ed2ce9288609472d` | 3 |
| `general-protection` | 565,760 | `169a8fffae25b2143a5971c1e1927ff49d3038597566ffe0173313ea0a78f43a` | 3 |
| `user-fault` | 566,272 | `19267825e530f0a950033f7e61602efd259e01c8fab45b049ee441da740b804a` | 0 |

All four identities pass the Windows pinned-QEMU gate with complete exact serial markers. Fresh Debian and complete cross-host qualification remain pending; no Debian QEMU execution is claimed.

## Firmware exit and kernel entry

The loader validates UEFI tables, obtains a bounded memory map, and retries `ExitBootServices` at most three times. It uses no firmware service after successful exit and overlays exact `WVKHAND1` before kernel entry.

Memory 12 selects and clears a 2 MiB-aligned 143-page arena below 4 GiB, publishes `WVKMEM12`, copies the handoff, and switches to the four-page kernel stack. Page 5 becomes the IDT; paging 4 consumes pages 6 through 11 and activates the low-1-GiB W^X root with a 768 KiB supervisor RX window.

Admission 4 requires token 73 for the exact 815-byte canonical WVB. Protected-process execution then:

1. runs Windvale process policy and requires token 97;
2. allocates pages 12 through 22 for init and 23 through 142 for the client;
3. creates two roots, the two client-grant resources, an independently attached init store, absent client aliases, `WVPROC13`, `WVCHAN03`, and three `WVRES005` records;
4. maps the exact 1,195-byte three-entry store RO/NX only in init and publishes a checked descriptor;
5. enters init, which grants ordered set token `131073` and registers a bounded request window;
6. publishes both client aliases, service table 5, and `WVBR002` atomically;
7. enters client generation 1, copies the exact `WVRQ 1` request, dynamically selects `boot:main.configuration` from the init-owned store, constructs and validates the `WVRY 1` response, interprets `Sourceˉwvbˉfixture` for exactly 199 guest instructions, and returns `6`;
8. clears channel message and destination state, records terminal peer status, cleans both client aliases and publications, reloads init's CR3, releases the exact 120-page tail, reopens a clean channel, and rebuilds the same root as generation 2;
9. repeats grant, dynamic request/reply, interpretation, result, peer cleanup, and resource cleanup under generation 2;
10. lets init receive the second `6` and exit;
11. runs the retained native probe and compiler-generated system-profile Main.

The user-fault scenario records a faulted peer after client `CLI` and still completes cleanup. CPL0 invalid-opcode and general-protection scenarios remain terminal after the complete successful prefix.

## Exact serial evidence

Normal success requires:

```text
windvale-os-boot 34
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
ipc=dynamic-resource-store
Hello from Windvale
cpu-exceptions=armed
native-context=pass
native-wvb=pass
windvale-source=pass
status=pass
shutdown=poweroff
```

The user-fault case inserts `user-fault=contained` before `status=pass`. Kernel panic writes value 1 to QEMU test port `0xF4`, producing host code 3; normal and contained-fault success use the Q35 shutdown leaf and produce code 0. Complete serial evidence is mandatory because host code alone is ambiguous.

## Boot harness

From the repository root:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Os-Boot.ps1 -Scenario normal
pwsh -NoProfile -File Tools/Verify/Verify-Os-Boot.ps1 -Scenario invalid-opcode
pwsh -NoProfile -File Tools/Verify/Verify-Os-Boot.ps1 -Scenario general-protection
pwsh -NoProfile -File Tools/Verify/Verify-Os-Boot.ps1 -Scenario user-fault
```

The harness preflights pinned QEMU/OVMF, creates run-private media and variable state, launches `pc-q35-11.0,accel=tcg` with one CPU and 128 MiB, captures serial, validates scenario output and exit code, and rechecks input identities. Default timeout is 60 seconds. Temporary artifacts are deleted unless `-KeepRunDirectory` is supplied.

Harness diagnostics remain `WVOS3001` build failure, `WVOS3002` start failure, `WVOS3003` timeout, `WVOS3004` exit mismatch, `WVOS3005` serial mismatch, `WVOS3006` changed input, and `WVOS3007` cleanup-boundary failure.

## Deliberate non-claims

Probe 34 does not authenticate firmware CRCs, own general physical memory, release non-tail extents, load arbitrary WVB, provide complete verification, publish executable memory, JIT, schedule generally, transfer capabilities, accept arbitrary stores in its bounded guest seam, recompute store-entry payload digests in the guest, enumerate resources, implement path components/directories/handles/mounts/packages/block storage/writable state/network/devices, handle SMP shootdown, or qualify Hyper-V/physical hardware. Stage 0 machine emitters remain explicit replacement seams.
