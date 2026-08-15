# Windvale OS firmware boot probe

## Status and purpose

Firmware Probe 40 is the cross-host-qualified first generation-safe non-tail memory-object reclamation proof. It composes ABI 22/context 7, WVA 1, admission 4/bridge 2, retained bridge 10, memory 17, paging 5, protected processes 17, private `WVMEMO01`/`WVTHR001`/`WVTIME01` evidence, endpoint 1, channel 4, interpreter profile 7, `WVRES006`, `WVBR002`, `WVRS 1`, and `WVDS 1`.

[Decision 0196](../Documents/Decisions/0196-First-Generation-Safe-Non-Tail-Memory-Object-Reclamation.md) owns Probe 40. Exact implementation commit `c4008e75db061df375eb323d75a818863aee553f` passes GitHub [Verify run 30853255559](https://github.com/eworker-inc/Windvale/actions/runs/30853255559): Windows and digest-pinned Debian each complete a zero-warning Release build, all 87 Seed tests, all 39 OS tests, and the complete native CLI gate. All five pinned Windows QEMU scenarios pass. Debian QEMU execution is not claimed.

The firmware ABI follows UEFI 2.11 x64 calling conventions and `GetMemoryMap`/`ExitBootServices`. These host mechanics do not define portable Windvale semantics.

## Artifact construction

The builder compiles the selected system-profile Hello source; portable admission, process-policy, init/resource service, directory service, exact `Tests/Fixtures/Source-Wvb/Function-Only.wv`, and retained-native modules; hosted `Bytecode-Interpreter.wv`; and the kernel/service/client WVA shims. Portable modules pass through canonical WVB, mandatory verification, ABI-22 native selection and fragment verification, and WVO. Stage 0 rewrites only verified link-facing symbols.

The existing Stage 0 and Windvale-written compilers must emit byte-identical canonical WVB for `Function-Only.wv`, and those bytes must equal the embedded admission identity. The portable process policy is an 18,763-byte canonical WVB with SHA-256 `907d89aae0575d05306d4111c87f52f5a684085576a19d6425968ebe83afa3f4`; its 129,310-byte ABI-22 WVO has SHA-256 `483ba9c752862fa739dea5fb9c40ce747e3210797d39bc73ac3f8d22084f669a`. Probe 40 changes no source-language semantics or ABI-22 backend behavior; it extends the Windvale policy, WVA, and OS composition.

The separate directory service is a 473-byte WVB with SHA-256 `33b0e425bd6e2a1cd6ae8f95d4645748a6031b93684a9b1ac4d0e56e8408bef7`. Its WVA entry, exact preemption probe, and ABI-22 object link to a 3,911-byte image with SHA-256 `f4d047c6f311b1561a5621b98f3db2868a969c54bb81dac2f75d599b7207f3fb`. The machine seam pins and revalidates that complete image and exact preemption-function bytes before publication.

Stage 0 creates the loader, fixed-page memory leaf, exception, paging, admission bridge, process machine, retained bridge, x64 byte adapter, canonical three-entry `WVRS 1` store, and canonical two-entry `WVDS 1` snapshot. The separately assembled 2,538-byte memory-object WVA has SHA-256 `fe0a94461b743be58319d2e2f8b737840ec1216e61a98ee7e210f96f97f85bee`; the retained 1,202-byte timer WVA has SHA-256 `e331a1db404b8b8359d35d410792496683a63acee621ff64f128a6eae128c344`. Every immutable object is independently verified before machine publication. The linker reconstructs the base-zero image and passes verified code and read-only data to UEFI application writer 3.

The linked kernel payload fits the fixed 768 KiB supervisor RX window. Init and the directory service each fit two user RX pages; the client fits 110 user RX pages. Generated WVB, WVO, EFI, firmware maps, and virtual disks are not committed.

Probe-40 qualified image identities are:

| Scenario | EFI bytes | SHA-256 | Expected host code |
| --- | ---: | --- | ---: |
| `normal` | 681,984 | `4b9ba1425f3e892404fdab41fd6270b4c6f1ea04d96a05233539f45c4253f974` | 0 |
| `invalid-opcode` | 681,984 | `b9ecc7f13a50c4c5bbc4c259830554bcec14b64caf6f2d69a6f271bb4d1e11fd` | 3 |
| `general-protection` | 681,984 | `dbb5a213d3303ecf349166a1cb60114e0347a478227e11f1690bb2dc8b1e853a` | 3 |
| `user-fault` | 682,496 | `2dc3bbbb6d413499394840d2466c53a206142c6d62e1c7cdab6c490eb38507ad` | 0 |
| `service-fault` | 667,648 | `bf39b8f503c9eaab0437f58dec89a3eb9b011ea1b7d3affe4c9187f2a7ab128d` | 0 |

All five identities are deterministic in the focused Windows suite and pass pinned Windows QEMU 11.0/Q35/TCG with exact transcripts and exit results. The Windows/Linux qualification gate is recorded above; no Debian QEMU execution is claimed.

## Firmware exit and kernel entry

The loader validates UEFI tables, obtains a bounded memory map, and retries `ExitBootServices` at most three times. It uses no firmware service after successful exit and overlays exact `WVKHAND1` before kernel entry.

Memory 17 selects and clears a 2 MiB-aligned 157-page arena below 4 GiB, publishes `WVKMEM17`, copies the handoff, initializes bitmap/owner/object evidence, and switches to the four-page kernel stack. Page 5 becomes the IDT; paging 5 consumes pages 6 through 12 and activates the low-1-GiB W^X root, a 768 KiB supervisor RX window, and exact supervisor-only HPET/local-APIC MMIO windows.

Admission 4 requires token 73 for the exact 816-byte canonical WVB 1.11 module. Protected-process execution then:

1. runs Windvale process policy and requires token 97;
2. allocates `WVMEMO01` objects at pages 13 through 24 for init, 25 through 146 for client generation 1, and 147 through 156 for the directory provider;
3. creates three roots, two client-grant resources, an init-attached store, a directory-attached snapshot, absent client aliases, three `WVPROC17` records, two `WVENDP01` records, two `WVCHAN04` records, and four `WVRES006` records;
4. maps the exact 1,195-byte `WVRS 1` store RO/NX only in init, maps the exact 3,184-byte `WVDS 1` snapshot RO/NX only in the directory process, and maps a separate RW/NX service-response page into each process;
5. validates Q35 HPET/local-APIC state, calibrates a local-APIC one-shot over 500,000 HPET ticks, and forces four interrupts plus three root switches through the three CPU-bound WVA probes;
6. restores all three records to ready state, then uses the ready/wait dispatcher to start the directory provider so it registers its bounded request window and starts init to grant ordered set token `131073` and register the resource-service window;
7. publishes both client resource aliases, service table 5, and `WVBR002` atomically;
8. enters client generation 1, completes both service exchanges and interpretation, and returns `6`;
9. clears transient state, cleans aliases/publications, reloads init's CR3, releases client pages 25 through 146 while the later directory object remains live, and first-fits the identical root under generation 2;
10. rebinds both endpoints, repeats grant, service exchanges, interpretation, result, and cleanup under generation 2;
11. resumes and exits both providers with exact endpoint accounting; and
12. runs the retained native probe and compiler-generated system-profile Main.

The dispatcher still scans exactly three validated records from a persistent round-robin cursor. It selects only a ready thread in a ready or running process and updates the cursor to the following slot. Every workload entry and explicit wake uses that decision. Probe 39's retained timer experiment is private evidence over the same logical order, not a public run queue or a claim that the retained workload is generally preemptive.

The user-fault scenario records a faulted client peer after client `CLI` and still completes cleanup. The service-fault scenario branches after generation 1's successful resource lookup: the client sends a 37-byte `WVDQ 1` whose total-length field declares 36, the directory process rejects it and faults through CPL3 `CLI`, the kernel records process 3 as the faulted peer, closes its endpoint at two resolutions, closes and clears only the directory channel, wakes the waiting client once with exact result `-1`, requires its clean three-syscall exit, revokes its resources, and continues to shutdown with init still alive. No replacement or generation-2 rebuild is attempted. CPL0 invalid-opcode and general-protection scenarios remain terminal after the complete successful normal prefix.

## Exact serial evidence

Normal success requires:

```text
windvale-os-boot 40
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
memory-object-reuse=pass
wvb-runtime=interpreted
init-service=pass
directory-service=pass
ipc=resource-and-directory
Hello from Windvale
timer-preemption=pass
cpu-exceptions=armed
native-context=pass
native-wvb=pass
windvale-source=pass
status=pass
shutdown=poweroff
```

The user-fault case inserts `user-fault=contained` before `status=pass`. The service-fault case stops after the first client, omits the normal reuse/init/directory completion markers, and instead emits `service-fault=contained` plus `ipc=service-peer-loss`; both successful contained-fault cases retain `timer-preemption=pass`. Terminal invalid-opcode and general-protection cases do not emit the later timer success marker. Kernel panic writes value 1 to QEMU test port `0xF4`, producing host code 3; normal and both contained-fault successes use the Q35 shutdown leaf and produce code 0. Complete serial evidence is mandatory because host code alone is ambiguous.

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

Probe 40 does not authenticate firmware CRCs, own general physical memory, coalesce or scatter allocations, load arbitrary WVB, provide complete in-guest verification, publish executable memory, JIT, provide a public timer/time API, prioritize, handle delayed/lost/wrapped ticks, enter idle, guarantee general wake latency, schedule multiple threads, discover or publicly create endpoints, supervise or restart services, transfer capabilities, accept arbitrary stores or snapshots in its bounded guest seam, enumerate directories, implement nested paths/handles/mounts/packages/block storage/writable state/network/devices, handle concurrent calls, perform SMP shootdown, or qualify Hyper-V/physical hardware. Its three memory objects, four-tick timer sequence, and one-thread-per-process dispatcher remain fixed internal evidence. Stage 0 machine emitters remain explicit replacement seams.
