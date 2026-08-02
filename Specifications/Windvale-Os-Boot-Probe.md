# Windvale OS firmware boot probe

## Status and purpose

Firmware Probe 31 is the implemented same-WVB candidate. It composes ABI 17/context 7, WVA seam 8, admission 3/bridge 2, retained bridge 10, memory 8, paging 4, protected processes 10, interpreter profile 5, `WVRES004`, and `WVBR002`.

[Decision 0101](../Documents/Decisions/0101-First-Exact-Wvb-Across-Three-Environments.md) owns Probe 31. The focused Windows suite passes all 25 OS tests, and all four local Windows pinned-QEMU scenarios pass; complete committed Windows/Debian qualification remains pending. [Decision 0100](../Documents/Decisions/0100-First-Reclaimed-And-Reused-Process-Root.md) retains the fully qualified Probe-30 baseline.

The firmware ABI follows UEFI 2.11 x64 calling conventions and `GetMemoryMap`/`ExitBootServices`. These host mechanics do not define portable Windvale semantics.

## Artifact construction

The builder compiles system-profile `Hello-World.wv`; portable admission, process-policy, init, canonical `Examples/Seed/Sum-Data.wv`, and retained-native modules; hosted `Bytecode-Interpreter.wv`; and the kernel/init/resource/client WVA shims. Portable modules pass through canonical WVB, mandatory verification, ABI-17 native selection/fragment verification, and WVO. Stage 0 rewrites only verified link-facing symbols.

Stage 0 also creates the loader, memory, exception, paging, admission bridge, process machine, retained bridge, and x64 byte adapter. It independently reconstructs the exact WVA lookup leaf before publication. The linker reconstructs the base-zero image and passes verified code/read-only data to UEFI application writer 3.

The linked kernel payload must fit the fixed 768 KiB supervisor RX window. Init fits one user RX page; the client fits 98 user RX pages. Generated WVB, WVO, EFI, firmware maps, and virtual disks are not committed.

Candidate image identities are:

| Scenario | EFI bytes | SHA-256 | Expected host code |
| --- | ---: | --- | ---: |
| `normal` | 531,456 | `30acb028e44b6d12bc4d0e4d34232d86a43b83b40f070d3a48b7c56e505bc0bc` | 0 |
| `invalid-opcode` | 531,456 | `dec5a39be132a3e6f140425547097b450a38a7b62e6ac3fa3d20f7d3c457587b` | 3 |
| `general-protection` | 531,456 | `f0e9daacfa479945afec952692f69e3911b285e478f9ae8d12a3e14f0c091960` | 3 |
| `user-fault` | 531,968 | `795cb85aa599d2ead4e228bd0eb3da5ad28ecd8970955b294ab09f72c3f7ade7` | 0 |

These identities pass the local Windows pinned-QEMU gate with complete exact serial markers. They remain qualification candidates until the committed Windows/Debian gate passes.

## Firmware exit and kernel entry

The loader validates UEFI tables, obtains a bounded memory map, and retries `ExitBootServices` at most three times. It uses no firmware service after successful exit and overlays exact `WVKHAND1` before kernel entry.

Memory 8 selects and clears a 2 MiB-aligned 137-page arena below 4 GiB, publishes `WVKMEM08`, copies the handoff, and switches to the four-page kernel stack. Page 5 becomes the IDT; paging 4 consumes pages 6 through 11 and activates the low-1-GiB W^X root with a 768 KiB supervisor RX window.

Admission 3 requires token 73 for the exact 493-byte canonical WVB. Protected-process execution then:

1. runs Windvale process policy and requires token 97;
2. allocates pages 12 through 20 for init and 21 through 136 for the client;
3. creates two roots, two init-owned RO/NX resources, absent client aliases, `WVPROC10`, `WVCHAN01`, and two `WVRES004` records;
4. enters init, which grants ordered set token `131073` and waits;
5. publishes both aliases, service table 5, and `WVBR002` atomically;
6. enters client generation 1, which interprets `Sumˉdata` for exactly 203 guest instructions and returns `29`;
7. cleans both aliases/publications, reloads init's CR3, releases the exact 116-page tail, and rebuilds the same root as generation 2;
8. repeats grant, interpretation, result, and cleanup under generation 2;
9. lets init receive the second `29` and exit;
10. runs the retained native probe and compiler-generated system-profile Main.

The user-fault scenario contains vector 13 from client `CLI` after the result send. CPL0 invalid-opcode and general-protection scenarios remain terminal after the complete successful prefix.

## Exact serial evidence

Normal success requires:

```text
windvale-os-boot 31
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

Probe 31 does not authenticate firmware CRCs, own general physical memory, release non-tail extents, load arbitrary WVB, provide complete verification, publish executable memory, JIT, schedule generally, transfer capabilities, enumerate resources, provide independent resource lifetimes, implement filesystems/packages/network/devices, handle SMP shootdown, or qualify Hyper-V/physical hardware. Stage 0 machine emitters remain explicit replacement seams.
