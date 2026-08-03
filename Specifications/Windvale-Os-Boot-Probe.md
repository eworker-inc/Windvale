# Windvale OS firmware boot probe

## Status and purpose

Firmware Probe 35 is the cross-host-qualified implementation of the first live guest directory service. It composes ABI 22/context 7, WVA seam 11, admission 4/bridge 2, retained bridge 10, memory 14, paging 4, protected processes 14, channel 3, interpreter profile 7, `WVRES006`, `WVBR002`, `WVRS 1`, and `WVDS 1`.

[Decision 0159](../Documents/Decisions/0159-First-Guest-Directory-Service.md) owns Probe 35. Exact implementation commit `a797e31dbe404267622f409b6c45da9b680ec8b5` passes all 87 Seed tests and all 37 OS tests on Windows and digest-pinned Debian in GitHub [Verify run 30808267999](https://github.com/eworker-inc/Windvale/actions/runs/30808267999). All four pinned-QEMU scenarios pass on Windows; the workflow does not claim Debian QEMU execution.

The firmware ABI follows UEFI 2.11 x64 calling conventions and `GetMemoryMap`/`ExitBootServices`. These host mechanics do not define portable Windvale semantics.

## Artifact construction

The builder compiles system-profile `Hello-World.wv`; portable admission, process-policy, init, exact `Tests/Fixtures/Source-Wvb/Function-Only.wv`, and retained-native modules; hosted `Bytecode-Interpreter.wv`; and the kernel/init/service/client WVA shims. Portable modules pass through canonical WVB, mandatory verification, ABI-22 native selection and fragment verification, and WVO. Stage 0 rewrites only verified link-facing symbols.

The existing Stage 0 and Windvale-written compilers must emit byte-identical canonical WVB for `Function-Only.wv`, and those bytes must equal the embedded admission identity. The portable process policy is an 8,001-byte canonical WVB with SHA-256 `73f67a7c8294b7a2d3e2633fab482fa8eabe53a14dc8883821dacd7812b822aa`; its 53,456-byte ABI-22 WVO has SHA-256 `edf5d9a767a46b91577b739f6dbcf6c57a963c91c18cab5b8364746b3451dd44`. Probe 35 does not change the compiler or ABI-22 backend.

Stage 0 creates the loader, memory, exception, paging, admission bridge, process machine, retained bridge, x64 byte adapter, canonical three-entry `WVRS 1` store, and canonical two-entry `WVDS 1` snapshot. It independently verifies both immutable values and their complete SHA-256 identities before machine publication. The linker reconstructs the base-zero image and passes verified code and read-only data to UEFI application writer 3.

The linked kernel payload fits the fixed 768 KiB supervisor RX window. Init fits two user RX pages; the client fits 110 user RX pages. Generated WVB, WVO, EFI, firmware maps, and virtual disks are not committed.

Candidate image identities are:

| Scenario | EFI bytes | SHA-256 | Expected host code |
| --- | ---: | --- | ---: |
| `normal` | 582,144 | `a1157d2f367cee2755120264621c9d9f5d5f410ade3d54fd27bca5a50ded9b9f` | 0 |
| `invalid-opcode` | 582,144 | `c1ada91a1928e380166ba87b4d454415e8c0256218f4b98cad3feee57d674af3` | 3 |
| `general-protection` | 582,144 | `bbc472e29d38ff327dbb90f63ae994731fd07c97aa09bc8b3f36fd179744a946` | 3 |
| `user-fault` | 582,656 | `3ad6ab38405d78ee8af73758a203633a00c2a5e8e4d07cd6a964b7a24f446c16` | 0 |

All four identities pass the Windows pinned-QEMU gate with complete exact serial markers. The exact construction is cross-host qualified; no Debian QEMU execution is claimed.

## Firmware exit and kernel entry

The loader validates UEFI tables, obtains a bounded memory map, and retries `ExitBootServices` at most three times. It uses no firmware service after successful exit and overlays exact `WVKHAND1` before kernel entry.

Memory 14 selects and clears a 2 MiB-aligned 147-page arena below 4 GiB, publishes `WVKMEM14`, copies the handoff, and switches to the four-page kernel stack. Page 5 becomes the IDT; paging 4 consumes pages 6 through 11 and activates the low-1-GiB W^X root with a 768 KiB supervisor RX window.

Admission 4 requires token 73 for the exact 815-byte canonical WVB. Protected-process execution then:

1. runs Windvale process policy and requires token 97;
2. allocates pages 12 through 24 for init and 25 through 146 for the client;
3. creates two roots, the two client-grant resources, independently attached init store and directory snapshot, absent client aliases, `WVPROC14`, `WVCHAN03`, and four `WVRES006` records;
4. maps the exact 1,195-byte `WVRS 1` store and 3,184-byte `WVDS 1` snapshot RO/NX only in init, publishes checked descriptors, and maps dedicated RW/NX response pages into init and client;
5. enters init, which grants ordered set token `131073` and registers a bounded request window;
6. publishes both client aliases, service table 5, and `WVBR002` atomically;
7. enters client generation 1, completes the 55-byte dynamic resource lookup and 116-byte `WVRY 1` reply;
8. re-registers init, copies the exact 37-byte `WVDQ 1` request, constructs and copies the maximal 3,096-byte `WVDR 1` reply, and requires the client to validate all 3,072 `kernel.wv` bytes;
9. interprets `Sourceˉwvbˉfixture` for exactly 199 guest instructions and returns `6`;
10. clears channel message and destination state, records terminal peer status, cleans both client aliases and publications, reloads init's CR3, releases the exact 122-page tail, reopens a clean channel, and rebuilds the same root as generation 2;
11. repeats grant, resource lookup, directory read, interpretation, result, peer cleanup, and resource cleanup under generation 2;
12. lets init receive the second `6` and exit; and
13. runs the retained native probe and compiler-generated system-profile Main.

The user-fault scenario records a faulted peer after client `CLI` and still completes cleanup. CPL0 invalid-opcode and general-protection scenarios remain terminal after the complete successful resource-and-directory prefix.

## Exact serial evidence

Normal success requires:

```text
windvale-os-boot 35
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
directory-service=pass
ipc=resource-and-directory
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

Probe 35 does not authenticate firmware CRCs, own general physical memory, release non-tail extents, load arbitrary WVB, provide complete verification, publish executable memory, JIT, schedule generally, transfer capabilities, accept arbitrary stores or snapshots in its bounded guest seam, enumerate directories, implement nested paths/handles/mounts/packages/block storage/writable state/network/devices, handle concurrent calls, perform SMP shootdown, or qualify Hyper-V/physical hardware. Stage 0 machine emitters remain explicit replacement seams.
