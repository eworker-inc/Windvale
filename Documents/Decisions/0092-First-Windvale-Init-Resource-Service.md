# Decision 0092: First Windvale init/resource service

- Date: 2026-08-01
- Status: Accepted and implemented; focused Windows and pinned-QEMU evidence recorded, cross-host OS qualification pending
- Implements: Step 5 of [Decision 0084](0084-Minimal-Capability-Oriented-Windvale-Os-Architecture.md)
- Contract: [Protected process version 2](../../Specifications/Windvale-Protected-Process.md)

## Context

Decision 0091 proved one real CPL3 protection domain, a separate root, capability-checked syscall entry, and user-fault containment. Its capacity-one channel looped back to the same process, so it did not yet prove independent peers, rights reduction, blocking, wake-up, or a Windvale user service.

The smallest coherent step 5 is not a general scheduler or resource manager. It is one service that waits, one client that sends through a separately reduced endpoint, and a bounded coordinator that runs each under its own root. The service must be written in Windvale; WVA may own fixed entry instructions; C# may remain only at the already named Stage 0 machine-construction seam.

## Decision

- Advance the protected-process contract to version 2 and firmware to probe 23. Do not preserve a compatibility branch for the version-1 experimental record.
- Compile [`Init-Resource-Service.wv`](../../Operating-System/Kernel/Init-Resource-Service.wv) through canonical WVB and the shared ABI-16 backend. Bind its exact WVB SHA-256 `478dfcd36fed7c8063cfb3f53a6a1362bda5353656339b730be573a1be8f95b0` alongside the admitted client identity in Windvale policy token `92`.
- Construct init process/thread `1/1` and client process/thread `2/2` under distinct seven-page roots. Each receives user RX code plus RW/NX stack and context pages and a bounded instruction budget of `64`.
- Give init only receive right `2` and the client only send right `1`. Both use opaque slot `0`, generation `1`, reference `65536`; neither receives combined rights.
- Replace `WVPROC01` with two `WVPROC02` records and add one kernel-owned `WVCHAN01` record. Add explicit role, wait reason, shared-channel address, and saved user `RDX`; ABI-16 requires `RDX` to survive the block/wake context switch because it carries the native execution-context pointer.
- Run init first. Its receive on the empty channel records waiter `1`, leaves the process running, marks the thread waiting, and returns to a fixed kernel coordinator.
- Run the admitted client under the second root. It sends its Windvale result `29` and exits `29`; the fault scenario sends `29` then takes CPL3 general protection through `CLI`.
- After either admitted client outcome, reactivate init's root, consume the one message, record one receive and wake, restore the saved user context, run the Windvale service, and require its exact exit `29`.
- Preserve existing CPL0 terminal faults, memory/paging version 2, WVA seam version 8, admission bridge version 2, ABI 16/context 7, and retained bridge 10. No WVA grammar change is needed.
- Require deterministic artifact identities, all 25 focused OS tests, and all four live pinned-QEMU scenarios with explicit `processes=isolated`, `init-service=pass`, and `ipc=cross-process` evidence.

## Evidence

The focused Windows OS suite passes 25 of 25 tests. It independently verifies both roots and W^X mappings, both exact WVB identities, role-reduced records, malformed planner families including `WVOS6006`, deterministic WVA/WVO/linked images, the channel record, the process-machine object, and all four firmware images.

Pinned QEMU `pc-q35-11.0,accel=tcg` passes all scenarios:

| Scenario | Bytes | SHA-256 | Host code |
| --- | ---: | --- | ---: |
| normal | 80,896 | `5e2314ad4f3bbc3809c3027e56cf955b398d40d82657be597f8ca822ad6cdec8` | `0` |
| invalid opcode | 80,896 | `a5929f97d1ef7d1152c8f4783f8f5a0bf40ba7f6f87dda529519a6b76327a2bb` | `3` |
| general protection | 80,896 | `205b4dfc88f73f9ecec41f91242642528387e0ae0c55d1273cb50a46f14d2847` | `3` |
| contained client fault | 81,408 | `b2ed520486199104cad227f0bcbc863b428c9484400116dc88c2a55c159d2951` | `0` |

The contained-fault transcript proves that the client may fault after send while the independent init service still wakes, executes Windvale code, exits, and reaches clean shutdown.

GitHub run `30730151722` verifies exact commit `22e350b8965bbe70452261dabfc411d28cf7a1d5`: Windows and Linux each pass all 67 Seed qualification tests, and both jobs compile the OS projects successfully. That workflow does not execute the 25-test OS binary, so it is useful cross-host build evidence but does not promote probe 23 to cross-host OS qualification. A Linux OS-test run from the same exact source state remains required.

## Consequences

Windvale OS now has its first user-space service written in Windvale, two independent protection domains, real inter-process IPC, role-reduced endpoints, a waiting thread, and deterministic wake-up. This is materially more than the version-1 loopback proof without committing to a general scheduler prematurely.

The block/wake bug found by live testing establishes one important machine invariant: a context switch must preserve every register required by the Windvale native ABI, including `RDX` in ABI 16. The invariant is now explicit in the version-2 record and tests.

C# remains a Stage 0 constructor and coordinator, not the service implementation or semantic owner. Later Windvale/WVA facilities can replace it behind the named boundary.

## Deliberate limits

This decision does not add preemption, a timer, round-robin scheduling, a process-creation API, arbitrary WVB loading, capability transfer/revocation, general endpoint discovery, queued or byte IPC, resource enumeration, namespace policy, teardown, reclamation, filesystems, packages, networking, device services, Hyper-V evidence, or physical-hardware evidence.

## Reconsider when

- a third runnable thread requires a real scheduling policy or timer;
- resource discovery requires names, typed requests, or larger messages;
- capability transfer or revocation requires a general table lifecycle;
- arbitrary admitted modules require a loader and semantic verifier;
- another architecture needs a different saved-context representation; or
- system-profile Windvale can safely replace the fixed Stage 0 coordinator and record mutation.
