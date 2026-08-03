# Decision 0176: Third protected service and ready/wait dispatcher

- Date: 2026-08-03
- Status: Implemented candidate with focused Windows and pinned-QEMU evidence; cross-host qualification pending
- Extends: [Decision 0172](0172-First-Kernel-Owned-Service-Endpoint.md) and [Decision 0173](0173-Windvale-Process-Service-And-Driver-Architecture.md)
- Contracts: [`WVKMEM15`](../../Specifications/Windvale-Kernel-Memory.md), [`WVPROC17`](../../Specifications/Windvale-Protected-Process.md), and firmware Probe 38

## Context

Probe 37 proved one generation-safe kernel endpoint, but init still combined resource, directory, and lifecycle work in one process. The machine path also selected the next exact process through scenario-specific choreography rather than a reusable runnable-thread decision. Decision 0173 selected a fixed third directory provider, a second endpoint, and the smallest state-driven dispatcher as the next pressure before timers, dynamic launch, or supervision.

The process record needs a second client capability and endpoint. Widening it without moving adjacent state would overlap the private GDT and first channel header, so this slice also requires an explicit non-overlap invariant for every state-page record.

## Decision

- Advance firmware to Probe 38, kernel memory to `WVKMEM15`, and protected processes to `WVPROC17`. Retain ABI 22, context 7, WVA seam 11, paging 4, `WVCHAN04`, `WVENDP01`, interpreter profile 7, `WVRES006`, `WVBR002`, `WVRS 1`, `WVDS 1`, and the five existing scenarios.
- Build process/thread `3/3`, generation 1, reference `65539`, role `Directoryˉservice`, in a separate ten-page root. Map its 3,184-byte `WVDS 1` snapshot RO/NX and its own RW/NX request/reply page. Init no longer maps the snapshot or implements directory protocol policy.
- Retain the resource provider in init behind capability reference `65536`. Add client capability reference `65537` for a second `WVENDP01` whose provider is the directory process. Each endpoint owns an independent capacity-one `WVCHAN04`; client generations bind both endpoint addresses and rights explicitly.
- Expand the fixed arena to 156 pages. Allocate the 12-page init extent, ten-page directory extent, and recyclable 122-page client extent in that order. Client generation 2 still proves exact tail release, zeroing, identical-root reallocation, and endpoint rebind.
- Expand `WVPROC17` to 288 bytes for the second capability reference, rights, and endpoint address. Move the GDT/GDTR/TSS and all following channel, endpoint, resource, and directory-process records so no live interval overlaps. Reject process-machine construction if the checked layout no longer fits disjointly in the state page. Capability resolution also revalidates the selected channel magic, version, record size, and capacity while preserving live syscall arguments.
- Add one portable Windvale reference dispatcher and one matching x86-64 machine dispatcher over exactly three process records. A persistent cursor selects the next process in bounded round-robin order only when its process state is ready or running and its sole thread state is ready. Waiting, exited, faulted, malformed, wrongly identified, and stale-generation records are skipped or rejected. Every initial entry and explicit wake passes through this dispatcher.
- Keep wake causes exact and deterministic. This slice has no timer, involuntary preemption, priority, multiple threads, SMP, dynamic run queue, or public scheduling ABI. The coordinator still owns the expected scenario sequence after each selected process returns.
- Move the contained service-fault scenario to the directory provider. A malformed live directory request faults only process 3, closes and scrubs only the directory endpoint/channel, wakes the client once with peer-loss result `-1`, and permits client cleanup and clean shutdown while init remains alive. No restart or replacement is attempted.

## Consequences

Windvale now has three independently protected AOT/interpreter processes and two independently resolved service endpoints. The immutable directory snapshot belongs to the process that interprets it, and a directory-service fault no longer terminates the resource provider. The scheduler boundary is now state-driven and reusable even though the surrounding proof remains a fixed deterministic scenario.

This is still not a general scheduler, process manager, service manager, registry, dynamic endpoint API, capability-transfer system, or recovery supervisor. Process roles remain policy metadata rather than authority. The kernel still does not parse resource or directory wire formats, and Stage 0 still owns checked record serialization, page-table mutation, and x86-64 orchestration as named replacement seams.

## Local evidence

The portable process policy is 16,023 WVB bytes with SHA-256 `319a7fb7f3ea08ff3c7c4aba8b37ee90106f5360f62abcc529fd51286bee34ad`; its ABI-22 WVO is 109,340 bytes with SHA-256 `860e893dab8b170a9a9d49cdcda2d8997e351a3e6e13b03b7d92f1ad38f7cf74`. The directory provider is 473 WVB bytes with SHA-256 `33b0e425bd6e2a1cd6ae8f95d4645748a6031b93684a9b1ac4d0e56e8408bef7`; its linked 3,831-byte image has SHA-256 `bf25040b4925a13c4a919ffd5a53de8ff281e4452132a9f7cd9bb3624740c883`.

The normal process-machine WVO is 502,697 bytes with SHA-256 `6435782bc20b63b187e31a28634022d8f910ed92f49889ecfe1cb6e829de7dd2`; its 38,551 code bytes have SHA-256 `73e46e135ba08ea56090bf40cba6e3ff6186c894a23b6722fff911ebebd173bb`. A zero-warning Release build and all 38 focused OS tests pass locally on Windows. All five pinned QEMU/TCG scenarios pass with exact Probe-38 transcripts. The normal EFI image is 649,728 bytes with SHA-256 `534d73d391b155f53d70a01b770478d1f10818ea57566f6b60aa80cdf1941e68`; the other exact identities are recorded in the boot-probe specification.

This is local candidate evidence. Cross-host construction and the broader repository gates remain pending and must not be inferred from the Windows QEMU result.

## Rejected alternatives

Keeping the directory snapshot mapped in init while adding only another endpoint was rejected because it would preserve the combined authority and failure domain that selected this slice.

Adding a timer or dynamic process creation at the same time was rejected because either would obscure whether the third fixed process, independent endpoint, state model, or new interrupt/lifetime mechanism caused a failure.

Treating the fixed coordinator as a scheduler was rejected because its next target was encoded by scenario position rather than selected from process/thread state. Treating the new dispatcher as preemptive scheduling was also rejected because it runs only at explicit kernel transitions.

## Reconsider when

- a monotonic interrupt source and starvation evidence are ready for the first fixed-quantum preemption slice;
- independently lived memory objects replace exact tail-only client reclamation;
- a concrete fourth process or dynamic launch plan requires a general run queue or object table;
- service replacement requires observable new-generation binding and supervisor policy; or
- a second architecture cannot reproduce the same bounded ready/wait decision without changing process semantics.
