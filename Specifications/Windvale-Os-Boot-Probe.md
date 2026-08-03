# Windvale OS firmware boot probe

## Status and purpose

Firmware Probe 39 is the implemented candidate for the first HPET-calibrated local-APIC preemption proof across three protected process roots. It composes ABI 22/context 7, the expanded WVA 1 seam, admission 4/bridge 2, retained bridge 10, memory 16, paging 5, protected processes 17 plus private `WVTHR001`/`WVTIME01` evidence, endpoint 1, channel 4, interpreter profile 7, `WVRES006`, `WVBR002`, `WVRS 1`, and `WVDS 1`.

[Decision 0188](../Documents/Decisions/0188-First-Hpet-Calibrated-Local-Apic-Preemption-Proof.md) owns Probe 39. Probe 38 remains the cross-host-qualified baseline at exact implementation commit `aae6818e3226e9e7e88d205b4666fb9904e4735b` and GitHub [Verify run 30834243770](https://github.com/eworker-inc/Windvale/actions/runs/30834243770). Probe 39 has 39 passing focused OS tests, 13 passing focused assembler tests, and all five passing pinned-QEMU scenarios on Windows; independent Windows/Linux qualification remains pending.

The firmware ABI follows UEFI 2.11 x64 calling conventions and `GetMemoryMap`/`ExitBootServices`. These host mechanics do not define portable Windvale semantics.

## Artifact construction

The builder compiles the selected system-profile Hello source; portable admission, process-policy, init/resource service, directory service, exact `Tests/Fixtures/Source-Wvb/Function-Only.wv`, and retained-native modules; hosted `Bytecode-Interpreter.wv`; and the kernel/service/client WVA shims. Portable modules pass through canonical WVB, mandatory verification, ABI-22 native selection and fragment verification, and WVO. Stage 0 rewrites only verified link-facing symbols.

The existing Stage 0 and Windvale-written compilers must emit byte-identical canonical WVB for `Function-Only.wv`, and those bytes must equal the embedded admission identity. The portable process policy is a 16,812-byte canonical WVB with SHA-256 `4904b44715399048e920d126d8a49f15a6b437cd4c77a25b23c3b113b9e7655d`; its 115,198-byte ABI-22 WVO has SHA-256 `a4e218e5417e4ed605ddfbd7df2f92d9df7a9a154a41382f4e94f1ff9bc4c2ed`. Probe 39 changes no source-language semantics or ABI-22 backend behavior; it extends WVA and the OS composition.

The separate directory service is a 473-byte WVB with SHA-256 `33b0e425bd6e2a1cd6ae8f95d4645748a6031b93684a9b1ac4d0e56e8408bef7`. Its WVA entry, exact preemption probe, and ABI-22 object link to a 3,911-byte image with SHA-256 `f4d047c6f311b1561a5621b98f3db2868a969c54bb81dac2f75d599b7207f3fb`. The machine seam pins and revalidates that complete image and exact preemption-function bytes before publication.

Stage 0 creates the loader, memory, exception, paging, admission bridge, process machine, retained bridge, x64 byte adapter, canonical three-entry `WVRS 1` store, and canonical two-entry `WVDS 1` snapshot. A separately assembled 1,202-byte timer WVA object has SHA-256 `e331a1db404b8b8359d35d410792496683a63acee621ff64f128a6eae128c344`. Every immutable object is independently verified before machine publication. The linker reconstructs the base-zero image and passes verified code and read-only data to UEFI application writer 3.

The linked kernel payload fits the fixed 768 KiB supervisor RX window. Init and the directory service each fit two user RX pages; the client fits 110 user RX pages. Generated WVB, WVO, EFI, firmware maps, and virtual disks are not committed.

Probe-39 candidate image identities are:

| Scenario | EFI bytes | SHA-256 | Expected host code |
| --- | ---: | --- | ---: |
| `normal` | 665,088 | `415304780f360508f11cba337638aac4434746ee2e4a08133b06bf4a7f6e01df` | 0 |
| `invalid-opcode` | 665,088 | `81ca354906173d8c8909271a6ad027d6b302a968eb96c87687647b21fef26184` | 3 |
| `general-protection` | 665,600 | `ba35ae69e84950f682df45c8928ebe5f3e63841aa486e51945128e5b6fdfa27e` | 3 |
| `user-fault` | 665,600 | `bed13cd66bdee6051008dbbe35d71110afcb2b65037853883b221a8b66651aeb` | 0 |
| `service-fault` | 651,264 | `00c8e41e2b9bd0c9e918ac4233f520cebea6aebdbd4d235f2d22a1b53fa57de8` | 0 |

All five identities are deterministic in the focused Windows suite. Normal passes the Windows pinned-QEMU gate with the complete exact serial marker. The other four pinned scenarios and the independent Windows/Linux qualification gate remain pending; no Debian QEMU execution is claimed.

## Firmware exit and kernel entry

The loader validates UEFI tables, obtains a bounded memory map, and retries `ExitBootServices` at most three times. It uses no firmware service after successful exit and overlays exact `WVKHAND1` before kernel entry.

Memory 16 selects and clears a 2 MiB-aligned 157-page arena below 4 GiB, publishes `WVKMEM16`, copies the handoff, and switches to the four-page kernel stack. Page 5 becomes the IDT; paging 5 consumes pages 6 through 12 and activates the low-1-GiB W^X root, a 768 KiB supervisor RX window, and exact supervisor-only HPET/local-APIC MMIO windows.

Admission 4 requires token 73 for the exact 815-byte canonical WVB. Protected-process execution then:

1. runs Windvale process policy and requires token 97;
2. allocates pages 13 through 24 for init, 25 through 34 for the directory provider, and 35 through 156 for the client;
3. creates three roots, two client-grant resources, an init-attached store, a directory-attached snapshot, absent client aliases, three `WVPROC17` records, two `WVENDP01` records, two `WVCHAN04` records, and four `WVRES006` records;
4. maps the exact 1,195-byte `WVRS 1` store RO/NX only in init, maps the exact 3,184-byte `WVDS 1` snapshot RO/NX only in the directory process, and maps a separate RW/NX service-response page into each process;
5. validates Q35 HPET/local-APIC state, calibrates a local-APIC one-shot over 500,000 HPET ticks, and forces four interrupts plus three root switches through the three CPU-bound WVA probes;
6. restores all three records to ready state, then uses the ready/wait dispatcher to start the directory provider so it registers its bounded request window and starts init to grant ordered set token `131073` and register the resource-service window;
7. publishes both client resource aliases, service table 5, and `WVBR002` atomically;
8. enters client generation 1, completes both service exchanges and interpretation, and returns `6`;
9. clears transient state, cleans aliases/publications, reloads init's CR3, releases the exact 122-page tail, and reallocates the identical root as generation 2;
10. rebinds both endpoints, repeats grant, service exchanges, interpretation, result, and cleanup under generation 2;
11. resumes and exits both providers with exact endpoint accounting; and
12. runs the retained native probe and compiler-generated system-profile Main.

The dispatcher still scans exactly three validated records from a persistent round-robin cursor. It selects only a ready thread in a ready or running process and updates the cursor to the following slot. Every workload entry and explicit wake uses that decision. Probe 39's preceding timer experiment is private evidence over the same logical order, not a public run queue or a claim that the retained workload is generally preemptive.

The user-fault scenario records a faulted client peer after client `CLI` and still completes cleanup. The service-fault scenario branches after generation 1's successful resource lookup: the client sends a 37-byte `WVDQ 1` whose total-length field declares 36, the directory process rejects it and faults through CPL3 `CLI`, the kernel records process 3 as the faulted peer, closes its endpoint at two resolutions, closes and clears only the directory channel, wakes the waiting client once with exact result `-1`, requires its clean three-syscall exit, revokes its resources, and continues to shutdown with init still alive. No replacement or generation-2 rebuild is attempted. CPL0 invalid-opcode and general-protection scenarios remain terminal after the complete successful normal prefix.

## Exact serial evidence

Normal success requires:

```text
windvale-os-boot 39
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

Probe 39 does not authenticate firmware CRCs, own general physical memory, release non-tail extents, load arbitrary WVB, provide complete in-guest verification, publish executable memory, JIT, provide a public timer/time API, prioritize, handle delayed/lost/wrapped ticks, enter idle, guarantee general wake latency, schedule multiple threads, discover or publicly create endpoints, supervise or restart services, transfer capabilities, accept arbitrary stores or snapshots in its bounded guest seam, enumerate directories, implement nested paths/handles/mounts/packages/block storage/writable state/network/devices, handle concurrent calls, perform SMP shootdown, or qualify Hyper-V/physical hardware. The four-tick three-process sequence and one-thread-per-process dispatcher remain fixed internal evidence. Stage 0 machine emitters remain explicit replacement seams.
