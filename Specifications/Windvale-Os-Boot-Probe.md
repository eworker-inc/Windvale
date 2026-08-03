# Windvale OS firmware boot probe

## Status and purpose

Firmware Probe 38 is the locally implemented candidate for three protected processes, two independent service endpoints, and the first bounded state-driven ready/wait dispatcher. It composes ABI 22/context 7, WVA seam 11, admission 4/bridge 2, retained bridge 10, memory 15, paging 4, protected processes 17, endpoint 1, channel 4, interpreter profile 7, `WVRES006`, `WVBR002`, `WVRS 1`, and `WVDS 1`.

[Decision 0176](../Documents/Decisions/0176-Third-Protected-Service-And-Ready-Wait-Dispatcher.md) owns Probe 38. All 38 focused OS tests and all five pinned Windows QEMU scenarios pass locally. Cross-host construction and repository qualification remain pending; the cross-host-qualified baseline remains Probe 37 at exact commit `2a1461b` and GitHub [Verify run 30819690110](https://github.com/eworker-inc/Windvale/actions/runs/30819690110). Debian QEMU is not claimed.

The firmware ABI follows UEFI 2.11 x64 calling conventions and `GetMemoryMap`/`ExitBootServices`. These host mechanics do not define portable Windvale semantics.

## Artifact construction

The builder compiles the selected system-profile Hello source; portable admission, process-policy, init/resource service, directory service, exact `Tests/Fixtures/Source-Wvb/Function-Only.wv`, and retained-native modules; hosted `Bytecode-Interpreter.wv`; and the kernel/service/client WVA shims. Portable modules pass through canonical WVB, mandatory verification, ABI-22 native selection and fragment verification, and WVO. Stage 0 rewrites only verified link-facing symbols.

The existing Stage 0 and Windvale-written compilers must emit byte-identical canonical WVB for `Function-Only.wv`, and those bytes must equal the embedded admission identity. The portable process policy is a 16,023-byte canonical WVB with SHA-256 `319a7fb7f3ea08ff3c7c4aba8b37ee90106f5360f62abcc529fd51286bee34ad`; its 109,340-byte ABI-22 WVO has SHA-256 `860e893dab8b170a9a9d49cdcda2d8997e351a3e6e13b03b7d92f1ad38f7cf74`. Probe 38 does not change the compiler or ABI-22 backend.

The separate directory service is a 473-byte WVB with SHA-256 `33b0e425bd6e2a1cd6ae8f95d4645748a6031b93684a9b1ac4d0e56e8408bef7`. Its WVA entry and ABI-22 object link to a 3,831-byte image with SHA-256 `bf25040b4925a13c4a919ffd5a53de8ff281e4452132a9f7cd9bb3624740c883`. The machine seam pins and revalidates that complete image before publication.

Stage 0 creates the loader, memory, exception, paging, admission bridge, process machine, retained bridge, x64 byte adapter, canonical three-entry `WVRS 1` store, and canonical two-entry `WVDS 1` snapshot. It independently verifies both immutable values and their complete SHA-256 identities before machine publication. The linker reconstructs the base-zero image and passes verified code and read-only data to UEFI application writer 3.

The linked kernel payload fits the fixed 768 KiB supervisor RX window. Init and the directory service each fit two user RX pages; the client fits 110 user RX pages. Generated WVB, WVO, EFI, firmware maps, and virtual disks are not committed.

Candidate image identities are:

| Scenario | EFI bytes | SHA-256 | Expected host code |
| --- | ---: | --- | ---: |
| `normal` | 649,728 | `534d73d391b155f53d70a01b770478d1f10818ea57566f6b60aa80cdf1941e68` | 0 |
| `invalid-opcode` | 649,728 | `27403e2945a6e81ca334e2e13bb9da3d180f4a0eb97c15f62d2447c77e0159ae` | 3 |
| `general-protection` | 649,728 | `5055bceefe4eb522ac236b3c75bf2abe9d2bd024df2d3dbd839a6d5c4debfafb` | 3 |
| `user-fault` | 650,240 | `4a4bf8f9d6aed34830c2a796c483179f4d1085d9ceea8085414acf3e772fb3bb` | 0 |
| `service-fault` | 635,904 | `523b65ac608f0f9457eda07304332496473f37f43328deeb8d8f4e5560d3683e` | 0 |

All five identities pass the local Windows pinned-QEMU gate with complete exact serial markers. Cross-host construction qualification is pending; no Debian QEMU execution is claimed.

## Firmware exit and kernel entry

The loader validates UEFI tables, obtains a bounded memory map, and retries `ExitBootServices` at most three times. It uses no firmware service after successful exit and overlays exact `WVKHAND1` before kernel entry.

Memory 15 selects and clears a 2 MiB-aligned 156-page arena below 4 GiB, publishes `WVKMEM15`, copies the handoff, and switches to the four-page kernel stack. Page 5 becomes the IDT; paging 4 consumes pages 6 through 11 and activates the low-1-GiB W^X root with a 768 KiB supervisor RX window.

Admission 4 requires token 73 for the exact 815-byte canonical WVB. Protected-process execution then:

1. runs Windvale process policy and requires token 97;
2. allocates pages 12 through 23 for init, 24 through 33 for the directory provider, and 34 through 155 for the client;
3. creates three roots, two client-grant resources, an init-attached store, a directory-attached snapshot, absent client aliases, three `WVPROC17` records, two `WVENDP01` records, two `WVCHAN04` records, and four `WVRES006` records;
4. maps the exact 1,195-byte `WVRS 1` store RO/NX only in init, maps the exact 3,184-byte `WVDS 1` snapshot RO/NX only in the directory process, and maps a separate RW/NX service-response page into each process;
5. uses the ready/wait dispatcher to start the directory provider so it registers its bounded request window, then starts init to grant ordered set token `131073` and register the resource-service window;
6. publishes both client resource aliases, service table 5, and `WVBR002` atomically;
7. enters client generation 1, completes the 55-byte dynamic resource lookup and 116-byte `WVRY 1` reply through capability `65536`;
8. calls independent capability `65537`, copies the exact 37-byte `WVDQ 1` request to process 3, copies back the maximal 3,096-byte `WVDR 1` reply, and requires the client to validate all 3,072 `kernel.wv` bytes;
9. interprets `Sourceˉwvbˉfixture` for exactly 199 guest instructions and returns `6`;
10. clears both channels' transient message and destination state, records terminal peer status, cleans both client aliases and publications, reloads init's CR3, releases the exact 122-page tail, and reallocates the identical root as generation 2;
11. rebinds the resource endpoint from client reference `65538` to `131074` after six resolutions and the directory endpoint after four, then repeats grant, both service exchanges, interpretation, result, and cleanup under generation 2;
12. resumes the directory provider after its final reply so it exits and closes its endpoint exactly once at six resolutions, then lets init receive the second `6`, exit, and close the resource endpoint exactly once at twelve resolutions; and
13. runs the retained native probe and compiler-generated system-profile Main.

The dispatcher scans exactly three validated records from a persistent round-robin cursor. It selects only a ready thread in a ready or running process and updates the cursor to the following slot. Every initial entry and explicit wake uses that decision. No timer or involuntary preemption exists.

The user-fault scenario records a faulted client peer after client `CLI` and still completes cleanup. The service-fault scenario branches after generation 1's successful resource lookup: the client sends a 37-byte `WVDQ 1` whose total-length field declares 36, the directory process rejects it and faults through CPL3 `CLI`, the kernel records process 3 as the faulted peer, closes its endpoint at two resolutions, closes and clears only the directory channel, wakes the waiting client once with exact result `-1`, requires its clean three-syscall exit, revokes its resources, and continues to shutdown with init still alive. No replacement or generation-2 rebuild is attempted. CPL0 invalid-opcode and general-protection scenarios remain terminal after the complete successful normal prefix.

## Exact serial evidence

Normal success requires:

```text
windvale-os-boot 38
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
directory-process=isolated
dispatcher=ready-wait
service-endpoints=2
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

Probe 38 does not authenticate firmware CRCs, own general physical memory, release non-tail extents, load arbitrary WVB, provide complete in-guest verification, publish executable memory, JIT, preempt, time-slice, prioritize, schedule multiple threads, discover or publicly create endpoints, supervise or restart services, transfer capabilities, accept arbitrary stores or snapshots in its bounded guest seam, enumerate directories, implement nested paths/handles/mounts/packages/block storage/writable state/network/devices, handle concurrent calls, perform SMP shootdown, or qualify Hyper-V/physical hardware. The three-process sequence and one-thread-per-process dispatcher remain fixed internal evidence. Stage 0 machine emitters remain explicit replacement seams.
