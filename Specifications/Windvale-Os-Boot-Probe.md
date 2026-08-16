# Windvale OS firmware boot probe

## Status and purpose

Firmware Probe 40 is the cross-host-qualified first generation-safe non-tail memory-object reclamation proof. The current native construction additionally composes resource-domain policy 1, application-launch policy 1, application-machine-construction policy 1, and boot-service-composition policy 1 around both sequential client generations; that newer composition is current-host candidate evidence until cross-host qualification. It composes ABI 22/context 7, WVA 1, admission 4/bridge 2, retained bridge 10, memory 17, paging 6, protected processes 17, private `WVMEMO01`/`WVTHR001`/`WVTIME01` evidence, endpoint 1, channel 4, interpreter profile 7, `WVRES006`, `WVBR002`, `WVRS 1`, and `WVDS 1`.

[Decision 0196](../Documents/Decisions/0196-First-Generation-Safe-Non-Tail-Memory-Object-Reclamation.md) owns Probe 40. Exact implementation commit `c4008e75db061df375eb323d75a818863aee553f` passes GitHub [Verify run 30853255559](https://github.com/eworker-inc/Windvale/actions/runs/30853255559): Windows and digest-pinned Debian each complete a zero-warning Release build, all 87 Seed tests, all 39 OS tests, and the complete native CLI gate. All five pinned Windows QEMU scenarios pass. Debian QEMU execution is not claimed.

The firmware ABI follows UEFI 2.11 x64 calling conventions and `GetMemoryMap`/`ExitBootServices`. These host mechanics do not define portable Windvale semantics.

## Artifact construction

`Tools/Native/Build-Os-Probe.cmd` is the current construction owner. It builds the kernel markers, WVB admission policy, native WVB probe, process policy, and process object through pinned native front doors; assembles the memory-object, timer, and kernel shims through the native WVA assembler; produces the remaining bounded OS objects through the native Probe object producer; links the base-zero payload; and packages the UEFI application. Every intermediate with a retained identity is checked before publication.

The canonical 816-byte `Function-Only.wv` WVB remains the embedded admission identity. Current construction builds the 4,071-byte admission WVB, lowers it to a verified 20,316-byte WVO, and uses the verified export renamer to publish the 20,337-byte link-facing object. The process-policy owner publishes the current 699,394-byte ABI-22 WVO at SHA-256 `dea015f8cafac002eddb9383691e2de10cbdcd0c0a589a88d88fbef95241f5b5`. Its portable source composes resource-domain, versioned application-start, application-launch, machine-construction, and boot-service-composition policy 1, returning token 97 only after the fixed filesystem/operation/network envelope is selected and both sequential client generations prove an authorized caller, a generation-safe executable publication, checked object sets, exact page partitions, W^X mappings, bounded capability tables, reserve, private construction, charge-backed publication, and complete failed-construction rollback. The full domain transcript still proves current/peak use, post-stop rejection, and idempotent zero-charge finish. The portable policy returns 97 in 21,917 instructions; the reviewed native context admits the exported entry frame as step 21,918 at depth 5. Paging 6 expands only the fixed supervisor RX window from 768 KiB to 772 KiB; the kernel stack and ABI-22 backend are unchanged. Probe 40 changes no source-language semantics; it extends Windvale policy and OS composition.

The separate directory service is a 473-byte WVB with SHA-256 `33b0e425bd6e2a1cd6ae8f95d4645748a6031b93684a9b1ac4d0e56e8408bef7`. Its WVA entry, exact preemption probe, and ABI-22 object link to a 3,911-byte image with SHA-256 `f4d047c6f311b1561a5621b98f3db2868a969c54bb81dac2f75d599b7207f3fb`. The machine seam pins and revalidates that complete image and exact preemption-function bytes before publication.

The native Probe object and process-object producers create the loader, fixed-page memory leaf, exception, paging, admission bridge, process machine, retained bridge/support, canonical three-entry `WVRS 1` store, and canonical two-entry `WVDS 1` snapshot. The separately assembled 2,538-byte memory-object WVA has SHA-256 `fe0a94461b743be58319d2e2f8b737840ec1216e61a98ee7e210f96f97f85bee`; the retained 1,202-byte timer WVA has SHA-256 `e331a1db404b8b8359d35d410792496683a63acee621ff64f128a6eae128c344`. The immutable Stage 0 recovery release preserves the former construction path and historical differential provenance; it is not invoked by ordinary `main` construction.

The linked kernel payload fits the fixed 772 KiB supervisor RX window. Init and the directory service each fit two user RX pages; the client fits 110 user RX pages. Generated WVB, WVO, EFI, firmware maps, and virtual disks are not committed.

Current native `main` image identities are:

| Scenario | EFI bytes | SHA-256 | Expected host code |
| --- | ---: | --- | ---: |
| `normal` | 1,693,184 | `4d25d5105149b6b819ff35e41e027ecf351d04ba865dc7c6f3bd6c18a77bcbce` | 0 |
| `invalid-opcode` | 1,693,184 | `05ac3cc2d3a6337a2838f507e5a15b10755e3e6775fc6160850fc3ea4a338f31` | 3 |
| `general-protection` | 1,693,184 | `34862977a74940f31829c7b5d9aee684bb5d9fdade4ce9e3eba9e830811c3a8d` | 3 |

The current native builder does not construct `user-fault` or `service-fault`; changes to their retained source seams are therefore an explicit changed-file verification gap rather than falsely covered by the normal probe owner.

The historically qualified Probe-40 image identities at implementation commit `c4008e75db061df375eb323d75a818863aee553f` are:

| Scenario | EFI bytes | SHA-256 | Expected host code |
| --- | ---: | --- | ---: |
| `normal` | 681,984 | `4b9ba1425f3e892404fdab41fd6270b4c6f1ea04d96a05233539f45c4253f974` | 0 |
| `invalid-opcode` | 681,984 | `b9ecc7f13a50c4c5bbc4c259830554bcec14b64caf6f2d69a6f271bb4d1e11fd` | 3 |
| `general-protection` | 681,984 | `dbb5a213d3303ecf349166a1cb60114e0347a478227e11f1690bb2dc8b1e853a` | 3 |
| `user-fault` | 682,496 | `2dc3bbbb6d413499394840d2466c53a206142c6d62e1c7cdab6c490eb38507ad` | 0 |
| `service-fault` | 667,648 | `bf39b8f503c9eaab0437f58dec89a3eb9b011ea1b7d3affe4c9187f2a7ab128d` | 0 |

All five historical identities were deterministic in the focused Windows suite and passed pinned Windows QEMU 11.0/Q35/TCG with exact transcripts and exit results. The Windows/Linux qualification gate is recorded above; no Debian QEMU execution is claimed. Current native construction and live verification cover the three scenarios in the first table.

## Firmware exit and kernel entry

The loader validates UEFI tables, obtains a bounded memory map, and retries `ExitBootServices` at most three times. It uses no firmware service after successful exit and overlays exact `WVKHAND1` before kernel entry.

Memory 17 selects and clears a 2 MiB-aligned 157-page arena below 4 GiB, publishes `WVKMEM17`, copies the handoff, initializes bitmap/owner/object evidence, and switches to the four-page kernel stack. Page 5 becomes the IDT; paging 6 consumes pages 6 through 12 and activates the low-1-GiB W^X root, a 772 KiB supervisor RX window, and exact supervisor-only HPET/local-APIC MMIO windows.

Admission 4 requires token 73 for the exact 816-byte canonical WVB 1.11 module. Protected-process execution then:

1. runs Windvale process policy, requires both versioned start requests to admit the authorized caller and generation-safe executable publication plus complete machine-object layouts, reserve and privately construct before charge-backed publication, requires the failed-construction rollback transcript, requires the exact resource-domain transcript to finish at current 0 with peaks 3/144/2, and then requires token 97;
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
resource-domain=pass current=0 peak=3/144/2
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
$efi = Join-Path $env:TEMP 'Windvale-Probe40-normal.efi'
Tools\Native\Build-Os-Probe.cmd $efi normal
pwsh -NoProfile -File Tools/Verify/Verify-Os-Boot.ps1 `
    -EfiPath $efi `
    -ExpectedEfiSha256 4d25d5105149b6b819ff35e41e027ecf351d04ba865dc7c6f3bd6c18a77bcbce `
    -Scenario normal
```

Use a fresh output path and the identity from the current-native table for `invalid-opcode` or `general-protection`. Verification of a retained `user-fault` or `service-fault` EFI requires an independently supplied, identity-pinned historical artifact; `Build-Os-Probe.cmd` intentionally rejects those scenario names.

The harness preflights pinned QEMU/OVMF, creates run-private media and variable state, launches `pc-q35-11.0,accel=tcg` with one CPU and 128 MiB, captures serial, validates scenario output and exit code, and rechecks input identities. Default timeout is 60 seconds. Temporary artifacts are deleted unless `-KeepRunDirectory` is supplied.

Harness diagnostics remain `WVOS3001` build failure, `WVOS3002` start failure, `WVOS3003` timeout, `WVOS3004` exit mismatch, `WVOS3005` serial mismatch, `WVOS3006` changed input, and `WVOS3007` cleanup-boundary failure.

## Deliberate non-claims

Probe 40 does not authenticate firmware CRCs, own general physical memory, coalesce or scatter allocations, load arbitrary WVB, provide complete in-guest verification, publish executable memory, JIT, provide a public timer/time API, prioritize, handle delayed/lost/wrapped ticks, enter idle, guarantee general wake latency, schedule multiple threads, expose a general dynamic resource-domain or application-launch API, discover or publicly create endpoints, supervise or restart services, move capabilities, accept arbitrary page totals or images in machine-construction policy 1, accept arbitrary stores or snapshots in its bounded guest seam, launch a filesystem or network provider, enumerate directories, implement nested paths/handles/mounts/packages/block storage/writable state/network devices or packet transport, handle concurrent calls, perform SMP shootdown, or qualify Hyper-V/physical hardware. Operation 8 now provides a fixed init-only entry, checked request snapshot, and fail-closed publication of the already constructed generation-1 child as reference `65538`; the retained init does not invoke it yet, and no arbitrary machine is allocated or entered. Its fixed boot-service envelope, two fixed versioned launch requests, bounded variable machine layout, domain transcript, three memory objects, four-tick timer sequence, and one-thread-per-process dispatcher remain internal evidence. The recovery release preserves the replaced Stage 0 emitters for historical reconstruction only.
