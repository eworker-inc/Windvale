# Windvale OS firmware boot probe

## Status and purpose

Firmware Probe 37 is the cross-host-qualified contract for resolving process capabilities through one kernel-owned service endpoint. It composes ABI 22/context 7, WVA seam 11, admission 4/bridge 2, retained bridge 10, memory 14, paging 4, protected processes 16, endpoint 1, channel 4, interpreter profile 7, `WVRES006`, `WVBR002`, `WVRS 1`, and `WVDS 1`.

[Decision 0172](../Documents/Decisions/0172-First-Kernel-Owned-Service-Endpoint.md) owns Probe 37. Exact commit `2a1461b` passes all 87 Seed and 38 OS tests plus the golden compiler and native CLI gates on Windows and digest-pinned Debian in GitHub [Verify run 30819690110](https://github.com/eworker-inc/Windvale/actions/runs/30819690110). All five pinned-QEMU scenarios pass on Windows; Debian QEMU is not claimed.

The firmware ABI follows UEFI 2.11 x64 calling conventions and `GetMemoryMap`/`ExitBootServices`. These host mechanics do not define portable Windvale semantics.

## Artifact construction

The builder compiles the selected system-profile Hello source; portable admission, process-policy, init, exact `Tests/Fixtures/Source-Wvb/Function-Only.wv`, and retained-native modules; hosted `Bytecode-Interpreter.wv`; and the kernel/init/service/client WVA shims. Portable modules pass through canonical WVB, mandatory verification, ABI-22 native selection and fragment verification, and WVO. Stage 0 rewrites only verified link-facing symbols.

The existing Stage 0 and Windvale-written compilers must emit byte-identical canonical WVB for `Function-Only.wv`, and those bytes must equal the embedded admission identity. The portable process policy is a 12,398-byte canonical WVB with SHA-256 `42676fb558683a5a1a1b30d7f74c15fc0396f0e384bed86db0a3d1f3fb4c0bda`; its 84,836-byte ABI-22 WVO has SHA-256 `6d0e4e88d862438702da5b034fcaae1a9fcb9e7aac6044d6ebbd254c2f2c10f8`. Probe 37 does not change the compiler or ABI-22 backend.

Stage 0 creates the loader, memory, exception, paging, admission bridge, process machine, retained bridge, x64 byte adapter, canonical three-entry `WVRS 1` store, and canonical two-entry `WVDS 1` snapshot. It independently verifies both immutable values and their complete SHA-256 identities before machine publication. The linker reconstructs the base-zero image and passes verified code and read-only data to UEFI application writer 3.

The linked kernel payload fits the fixed 768 KiB supervisor RX window. Init fits two user RX pages; the client fits 110 user RX pages. Generated WVB, WVO, EFI, firmware maps, and virtual disks are not committed.

Candidate image identities are:

| Scenario | EFI bytes | SHA-256 | Expected host code |
| --- | ---: | --- | ---: |
| `normal` | 615,936 | `aeb2e4b781fb01f59dae6a9f588e84cfa14e7bb84acddfc1b593e9854faa2818` | 0 |
| `invalid-opcode` | 615,936 | `6113a667b3156bdf71650ee5147b27e843d65258849d1deca16adffb2075cfac` | 3 |
| `general-protection` | 615,936 | `96d0cca11543b92eebba7640cf1ca278c1693d2511e3b1f84503bbc4276bc364` | 3 |
| `user-fault` | 616,448 | `f14b75ef03c2c76a6c6456184baca4087e551a9f5b53be921952c431c0a9e742` | 0 |
| `service-fault` | 604,160 | `0b8e0e8f07f3465f28db713b711f81e014b979c761d9fb8d3361726cc38f3588` | 0 |

All five identities pass the Windows pinned-QEMU gate with complete exact serial markers. Cross-host construction qualification is pending; no Debian QEMU execution is claimed.

## Firmware exit and kernel entry

The loader validates UEFI tables, obtains a bounded memory map, and retries `ExitBootServices` at most three times. It uses no firmware service after successful exit and overlays exact `WVKHAND1` before kernel entry.

Memory 14 selects and clears a 2 MiB-aligned 147-page arena below 4 GiB, publishes `WVKMEM14`, copies the handoff, and switches to the four-page kernel stack. Page 5 becomes the IDT; paging 4 consumes pages 6 through 11 and activates the low-1-GiB W^X root with a 768 KiB supervisor RX window.

Admission 4 requires token 73 for the exact 815-byte canonical WVB. Protected-process execution then:

1. runs Windvale process policy and requires token 97;
2. allocates pages 12 through 24 for init and 25 through 146 for the client;
3. creates two roots, the two client-grant resources, independently attached init store and directory snapshot, absent client aliases, `WVPROC16`, `WVENDP01`, `WVCHAN04`, and four `WVRES006` records;
4. maps the exact 1,195-byte `WVRS 1` store and 3,184-byte `WVDS 1` snapshot RO/NX only in init, publishes checked descriptors, and maps dedicated RW/NX response pages into init and client;
5. enters init, which grants ordered set token `131073` and registers a bounded request window;
6. publishes both client aliases, service table 5, and `WVBR002` atomically;
7. enters client generation 1, completes the 55-byte dynamic resource lookup and 116-byte `WVRY 1` reply;
8. re-registers init, copies the exact 37-byte `WVDQ 1` request, constructs and copies the maximal 3,096-byte `WVDR 1` reply, and requires the client to validate all 3,072 `kernel.wv` bytes;
9. interprets `Sourceˉwvbˉfixture` for exactly 199 guest instructions and returns `6`;
10. clears channel message and destination state, records terminal peer status, cleans both client aliases and publications, reloads init's CR3, releases the exact 122-page tail, reopens a clean channel, rebuilds the same root as generation 2, and rebinds `WVENDP01` from client reference `65538` to `131074` after exactly eight resolutions;
11. repeats grant, resource lookup, directory read, interpretation, result, peer cleanup, and resource cleanup under generation 2;
12. lets init receive the second `6`, exit, and close `WVENDP01` exactly once at sixteen resolutions with provider status exited; and
13. runs the retained native probe and compiler-generated system-profile Main.

The user-fault scenario records a faulted client peer after client `CLI` and still completes cleanup. The service-fault scenario branches after generation 1's successful resource lookup: the client sends a 37-byte `WVDQ 1` whose total-length field declares 36, init rejects it and faults through CPL3 `CLI`, the kernel records init as the faulted peer, closes `WVENDP01` at six resolutions with provider status faulted, closes and clears the channel, wakes the waiting client once with exact result `-1`, requires its clean three-syscall exit, revokes its resources, and continues to shutdown without a generation-2 rebuild. CPL0 invalid-opcode and general-protection scenarios remain terminal after the complete successful normal prefix.

## Exact serial evidence

Normal success requires:

```text
windvale-os-boot 37
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
service-endpoint=bound
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

The user-fault case inserts `user-fault=contained` before `status=pass`. The service-fault case stops after the first client, omits the normal reuse/init/directory completion markers, and instead emits `service-fault=contained` plus `ipc=service-peer-loss`. Kernel panic writes value 1 to QEMU test port `0xF4`, producing host code 3; normal and both contained-fault successes use the Q35 shutdown leaf and produce code 0. Complete serial evidence is mandatory because host code alone is ambiguous.

## Boot harness

From the repository root:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Os-Boot.ps1 -Scenario normal
pwsh -NoProfile -File Tools/Verify/Verify-Os-Boot.ps1 -Scenario invalid-opcode
pwsh -NoProfile -File Tools/Verify/Verify-Os-Boot.ps1 -Scenario general-protection
pwsh -NoProfile -File Tools/Verify/Verify-Os-Boot.ps1 -Scenario user-fault
pwsh -NoProfile -File Tools/Verify/Verify-Os-Boot.ps1 -Scenario service-fault
```

The harness preflights pinned QEMU/OVMF, creates run-private media and variable state, launches `pc-q35-11.0,accel=tcg` with one CPU and 128 MiB, captures serial, validates scenario output and exit code, and rechecks input identities. Default timeout is 60 seconds. Temporary artifacts are deleted unless `-KeepRunDirectory` is supplied.

Harness diagnostics remain `WVOS3001` build failure, `WVOS3002` start failure, `WVOS3003` timeout, `WVOS3004` exit mismatch, `WVOS3005` serial mismatch, `WVOS3006` changed input, and `WVOS3007` cleanup-boundary failure.

## Deliberate non-claims

Probe 37 does not authenticate firmware CRCs, own general physical memory, release non-tail extents, load arbitrary WVB, provide complete verification, publish executable memory, JIT, schedule generally, name or discover endpoints, create them publicly, supervise or restart services, transfer capabilities, accept arbitrary stores or snapshots in its bounded guest seam, enumerate directories, implement nested paths/handles/mounts/packages/block storage/writable state/network/devices, handle concurrent calls, perform SMP shootdown, or qualify Hyper-V/physical hardware. Stage 0 machine emitters remain explicit replacement seams.
